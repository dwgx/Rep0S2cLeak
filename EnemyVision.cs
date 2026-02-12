using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class EnemyVision : MonoBehaviour
{
	private Enemy Enemy;

	internal bool HasVision;

	internal float DisableTimer = 0.1f;

	internal float StandOverrideTimer;

	private float VisionTimer;

	private float VisionCheckTime = 0.25f;

	public Transform VisionTransform;

	[Header("Base")]
	public float VisionDistance = 10f;

	public Dictionary<int, int> VisionsTriggered = new Dictionary<int, int>();

	public Dictionary<int, bool> VisionTriggered = new Dictionary<int, bool>();

	[Header("Close")]
	public float VisionDistanceClose = 3.5f;

	public float VisionDistanceCloseCrouch = 2f;

	[Header("Dot")]
	public float VisionDotStanding = 0.4f;

	public float VisionDotCrouch = 0.6f;

	public float VisionDotCrawl = 0.9f;

	[Header("Phys Object Vision")]
	public bool PhysObjectVision = true;

	private float PhysObjectVisionRadius = 10f;

	public float PhysObjectVisionRadiusOverride = -1f;

	public float PhysObjectVisionDot = 0.4f;

	[Header("Triggers")]
	public int VisionsToTrigger = 4;

	public int VisionsToTriggerCrouch = 10;

	public int VisionsToTriggerCrawl = 20;

	[Header("Events")]
	public UnityEvent onVisionTriggered;

	internal int onVisionTriggeredID;

	internal PlayerAvatar onVisionTriggeredPlayer;

	internal bool onVisionTriggeredCulled;

	internal bool onVisionTriggeredNear;

	internal float onVisionTriggeredDistance;

	private bool VisionLogicActive;

	private void Awake()
	{
		Enemy = GetComponent<Enemy>();
		StartCoroutine(Vision());
		VisionLogicActive = true;
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		VisionLogicActive = false;
	}

	private void OnEnable()
	{
		if (!VisionLogicActive)
		{
			DisableVision(0.5f);
			StartCoroutine(Vision());
			VisionLogicActive = true;
		}
	}

	public void PlayerAdded(int photonID)
	{
		VisionsTriggered.TryAdd(photonID, 0);
		VisionTriggered.TryAdd(photonID, value: false);
	}

	public void PlayerRemoved(int photonID)
	{
		VisionsTriggered.Remove(photonID);
		VisionTriggered.Remove(photonID);
	}

	private IEnumerator Vision()
	{
		VisionLogicActive = true;
		if (GameManager.Multiplayer() && !PhotonNetwork.IsMasterClient)
		{
			yield break;
		}
		while (VisionsTriggered.Count == 0)
		{
			yield return new WaitForSeconds(VisionCheckTime);
		}
		while (true)
		{
			if (DisableTimer > 0f || EnemyDirector.instance.debugNoVision)
			{
				DisableTimer -= Time.deltaTime;
				foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
				{
					int viewID = player.photonView.ViewID;
					VisionTriggered[viewID] = false;
					VisionsTriggered[viewID] = 0;
				}
				yield return null;
				continue;
			}
			if (!Enemy.HasStateChaseBegin || Enemy.CurrentState != EnemyState.ChaseBegin)
			{
				HasVision = false;
				bool[] array = new bool[GameDirector.instance.PlayerList.Count];
				if (PhysObjectVision)
				{
					float radius = PhysObjectVisionRadius;
					if (PhysObjectVisionRadiusOverride > 0f)
					{
						radius = PhysObjectVisionRadiusOverride;
					}
					Collider[] array2 = Physics.OverlapSphere(VisionTransform.position, radius, SemiFunc.LayerMaskGetPhysGrabObject());
					foreach (Collider collider in array2)
					{
						if (!collider.CompareTag("Phys Grab Object"))
						{
							continue;
						}
						PhysGrabObject componentInParent = collider.GetComponentInParent<PhysGrabObject>();
						if (!componentInParent || componentInParent.playerGrabbing.Count <= 0)
						{
							continue;
						}
						Vector3 direction = componentInParent.centerPoint - VisionTransform.position;
						RaycastHit[] array3 = Physics.RaycastAll(VisionTransform.position, direction, direction.magnitude, Enemy.VisionMask, QueryTriggerInteraction.Ignore);
						bool flag = true;
						if (array3.Length != 0)
						{
							RaycastHit[] array4 = array3;
							for (int j = 0; j < array4.Length; j++)
							{
								RaycastHit raycastHit = array4[j];
								if (raycastHit.transform.CompareTag("Phys Grab Object") || raycastHit.transform.CompareTag("Enemy"))
								{
									PhysGrabObject componentInParent2 = raycastHit.transform.GetComponentInParent<PhysGrabObject>();
									if ((bool)componentInParent2 && (componentInParent2 == componentInParent || (Enemy.HasRigidbody && raycastHit.transform.GetComponentInParent<EnemyRigidbody>() == Enemy.Rigidbody)))
									{
										continue;
									}
								}
								flag = false;
							}
						}
						if (!flag || !(Vector3.Dot(VisionTransform.forward, direction.normalized) >= PhysObjectVisionDot))
						{
							continue;
						}
						int num = 0;
						foreach (PlayerAvatar player2 in GameDirector.instance.PlayerList)
						{
							if (player2 == componentInParent.playerGrabbing[0].playerAvatar)
							{
								array[num] = true;
							}
							num++;
						}
					}
				}
				int num2 = 0;
				foreach (PlayerAvatar player3 in GameDirector.instance.PlayerList)
				{
					bool flag2 = false;
					if (player3.isDisabled)
					{
						continue;
					}
					int viewID2 = player3.photonView.ViewID;
					if (player3.enemyVisionFreezeTimer > 0f)
					{
						if (VisionTriggered[viewID2])
						{
							VisionTrigger(viewID2, player3, culled: false, playerNear: false);
						}
						num2++;
						continue;
					}
					VisionTriggered[viewID2] = false;
					float num3 = Vector3.Distance(VisionTransform.position, player3.PlayerVisionTarget.VisionTransform.position);
					if (!array[num2] && num3 > VisionDistance)
					{
						continue;
					}
					bool flag3 = player3.isCrawling;
					bool flag4 = player3.isCrouching;
					if (player3.isTumbling)
					{
						flag4 = true;
						flag3 = false;
					}
					if (StandOverrideTimer > 0f)
					{
						flag4 = false;
						flag3 = false;
					}
					Transform transform = null;
					Transform transform2 = null;
					Vector3 direction2 = player3.PlayerVisionTarget.VisionTransform.transform.position - VisionTransform.position;
					Collider[] array5 = Physics.OverlapSphere(VisionTransform.position, 0.01f, Enemy.VisionMask);
					if (array5.Length != 0)
					{
						Collider[] array2 = array5;
						foreach (Collider collider2 in array2)
						{
							if (!collider2.transform.CompareTag("Enemy"))
							{
								if (collider2.transform.CompareTag("Player"))
								{
									transform = collider2.transform;
								}
								if ((bool)collider2.transform.GetComponentInParent<PlayerTumble>())
								{
									transform = collider2.transform;
								}
								if (!collider2.transform.GetComponentInParent<EnemyRigidbody>())
								{
									transform2 = collider2.transform;
								}
							}
						}
					}
					if (!transform2)
					{
						RaycastHit[] array6 = Physics.RaycastAll(VisionTransform.position, direction2, VisionDistance, Enemy.VisionMask, QueryTriggerInteraction.Ignore);
						float num4 = 1000f;
						RaycastHit[] array4 = array6;
						for (int i = 0; i < array4.Length; i++)
						{
							RaycastHit raycastHit2 = array4[i];
							if (raycastHit2.transform.CompareTag("Enemy"))
							{
								continue;
							}
							if (raycastHit2.transform.CompareTag("Player"))
							{
								transform = raycastHit2.transform;
							}
							if (!raycastHit2.transform.GetComponentInParent<EnemyRigidbody>())
							{
								if ((bool)raycastHit2.transform.GetComponentInParent<PlayerTumble>())
								{
									transform = raycastHit2.transform;
								}
								float num5 = Vector3.Distance(VisionTransform.position, raycastHit2.point);
								if (num5 < num4)
								{
									num4 = num5;
									transform2 = raycastHit2.transform;
								}
							}
						}
					}
					if (array[num2] || ((bool)transform && transform == transform2))
					{
						float num6 = Vector3.Dot(VisionTransform.forward, direction2.normalized);
						bool flag5 = false;
						if (flag4)
						{
							if (num3 <= VisionDistanceCloseCrouch)
							{
								flag5 = true;
							}
						}
						else if (num3 <= VisionDistanceClose)
						{
							flag5 = true;
						}
						if (flag5)
						{
							VisionsTriggered[viewID2] = VisionsToTrigger;
						}
						bool flag6 = false;
						if (flag3 && Enemy.CurrentState != EnemyState.LookUnder)
						{
							if (num6 >= VisionDotCrawl)
							{
								flag6 = true;
							}
						}
						else if (flag4 && Enemy.CurrentState != EnemyState.LookUnder)
						{
							if (num6 >= VisionDotCrouch)
							{
								flag6 = true;
							}
						}
						else if (num6 >= VisionDotStanding)
						{
							flag6 = true;
						}
						if (array[num2] || flag6 || flag5)
						{
							flag2 = true;
							bool flag7 = false;
							if (flag3 && Enemy.CurrentState != EnemyState.LookUnder)
							{
								if (VisionsTriggered[viewID2] >= VisionsToTriggerCrawl)
								{
									flag7 = true;
								}
							}
							else if (flag4 && Enemy.CurrentState != EnemyState.LookUnder)
							{
								if (VisionsTriggered[viewID2] >= VisionsToTriggerCrouch)
								{
									flag7 = true;
								}
							}
							else if (VisionsTriggered[viewID2] >= VisionsToTrigger)
							{
								flag7 = true;
							}
							bool culled = false;
							if (Enemy.HasOnScreen)
							{
								if (GameManager.instance.gameMode == 0)
								{
									if (Enemy.OnScreen.CulledLocal)
									{
										culled = true;
									}
								}
								else if (Enemy.OnScreen.CulledPlayer[player3.photonView.ViewID])
								{
									culled = true;
								}
							}
							if (flag7 || flag5)
							{
								VisionTrigger(viewID2, player3, culled, flag5);
							}
						}
					}
					if (flag2)
					{
						VisionsTriggered[viewID2]++;
					}
					else
					{
						VisionsTriggered[viewID2] = 0;
					}
					num2++;
				}
			}
			if (StandOverrideTimer > 0f)
			{
				StandOverrideTimer -= VisionCheckTime;
			}
			yield return new WaitForSeconds(VisionCheckTime);
		}
	}

	public void VisionTrigger(int playerID, PlayerAvatar player, bool culled, bool playerNear)
	{
		VisionTriggered[playerID] = true;
		VisionsTriggered[playerID] = Mathf.Max(VisionsTriggered[playerID], VisionsToTrigger);
		onVisionTriggeredID = playerID;
		onVisionTriggeredPlayer = player;
		onVisionTriggeredCulled = culled;
		onVisionTriggeredNear = playerNear;
		onVisionTriggered.Invoke();
	}

	public void DisableVision(float time)
	{
		DisableTimer = time;
	}

	public void StandOverride(float time)
	{
		StandOverrideTimer = time;
	}
}
