using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Photon.Pun;
using Steamworks;
using UnityEngine;
using UnityEngine.AI;

public static class SemiFunc
{
	public enum emojiIcon
	{
		drone_heal,
		drone_zero_gravity,
		drone_indestructible,
		drone_feather,
		drone_torque,
		drone_battery,
		orb_heal,
		orb_zero_gravity,
		orb_indestructible,
		orb_feather,
		orb_torque,
		orb_battery,
		orb_magnet,
		grenade_explosive,
		grenade_stun,
		weapon_baseball_bat,
		weapon_sledgehammer,
		weapon_frying_pan,
		weapon_sword,
		weapon_inflatable_hammer,
		item_health_pack_S,
		item_health_pack_M,
		item_health_pack_L,
		item_gun_handgun,
		item_gun_shotgun,
		item_gun_tranq,
		item_valuable_tracker,
		item_extraction_tracker,
		item_grenade_human,
		item_grenade_duct_taped,
		item_rubber_duck,
		item_mine_explosive,
		item_grenade_shockwave,
		item_mine_shockwave,
		item_mine_stun
	}

	public enum itemVolume
	{
		small,
		medium,
		large,
		large_wide,
		power_crystal,
		large_high,
		upgrade,
		healthPack,
		large_plus
	}

	public enum itemType
	{
		drone,
		orb,
		cart,
		item_upgrade,
		player_upgrade,
		power_crystal,
		grenade,
		melee,
		healthPack,
		gun,
		tracker,
		mine,
		pocket_cart,
		tool
	}

	public enum itemSecretShopType
	{
		none,
		shop_attic
	}

	public enum User
	{
		None = -1,
		Walter,
		Axel,
		Robin,
		Jannek,
		Ruben,
		Builder,
		Monika,
		Jenson
	}

	public static void EnemyCartJumpReset(Enemy enemy)
	{
		if (enemy.HasJump)
		{
			enemy.Jump.CartJump(0f);
		}
	}

	public static void EnemyCartJump(Enemy enemy)
	{
		if (enemy.HasJump)
		{
			enemy.Jump.CartJump(0.1f);
		}
	}

	public static Vector3 EnemyGetNearestPhysObject(Enemy enemy)
	{
		PhysGrabObject physGrabObject = null;
		float num = 9999f;
		Collider[] array = Physics.OverlapSphere(enemy.CenterTransform.position, 3f, LayerMask.GetMask("PhysGrabObject"));
		for (int i = 0; i < array.Length; i++)
		{
			PhysGrabObject componentInParent = array[i].GetComponentInParent<PhysGrabObject>();
			if ((bool)componentInParent && !componentInParent.GetComponent<EnemyRigidbody>())
			{
				float num2 = Vector3.Distance(enemy.CenterTransform.position, componentInParent.centerPoint);
				if (num2 < num)
				{
					num = num2;
					physGrabObject = componentInParent;
				}
			}
		}
		if ((bool)physGrabObject)
		{
			return physGrabObject.centerPoint;
		}
		return Vector3.zero;
	}

	public static bool EnemySpawn(Enemy enemy)
	{
		float minDistance = 18f;
		float maxDistance = 35f;
		if (EnemyDirector.instance.debugSpawnClose)
		{
			minDistance = 0f;
			maxDistance = 999f;
		}
		LevelPoint levelPoint = enemy.TeleportToPoint(minDistance, maxDistance);
		if ((bool)levelPoint)
		{
			if ((!enemy.HasRigidbody) ? (!EnemyPhysObjectSphereCheck(levelPoint.transform.position, 1f)) : (!EnemyPhysObjectBoundingBoxCheck(enemy.transform.position, levelPoint.transform.position, enemy.Rigidbody.rb)))
			{
				enemy.EnemyParent.firstSpawnPointUsed = true;
				return true;
			}
			EnemyDirector.instance.FirstSpawnPointAdd(enemy.EnemyParent);
		}
		enemy.EnemyParent.Despawn();
		enemy.EnemyParent.DespawnedTimerSet(UnityEngine.Random.Range(2f, 3f), _min: true);
		return false;
	}

	public static bool EnemyLookUnderCondition(Enemy _enemy, float _stateTimer, float _stateTimerThreshold, PlayerAvatar _targetPlayer)
	{
		if (_stateTimer > _stateTimerThreshold && _targetPlayer.isCrawling && !_targetPlayer.isTumbling && Vector3.Distance(_enemy.NavMeshAgent.GetPoint(), _targetPlayer.transform.position) > 0.5f && Vector3.Distance(_targetPlayer.transform.position, _targetPlayer.LastNavmeshPosition) < 3f && Mathf.Abs(_targetPlayer.transform.position.y - _enemy.transform.position.y) <= 1f)
		{
			return true;
		}
		return false;
	}

	public static void EnemyLeaveStart(Enemy enemy)
	{
		if (enemy.HasVision)
		{
			enemy.Vision.DisableVision(5f);
		}
	}

	public static bool EnemyRoamPoint(Enemy enemy, out Vector3 _destination)
	{
		bool result = false;
		_destination = Vector3.zero;
		LevelPoint levelPoint = LevelPointGet(enemy.transform.position, 10f, 25f);
		if (!levelPoint)
		{
			levelPoint = LevelPointGet(enemy.transform.position, 0f, 999f);
		}
		if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + UnityEngine.Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
		{
			_destination = hit.position;
			result = true;
		}
		return result;
	}

	public static bool EnemyLeavePoint(Enemy enemy, out Vector3 _destination)
	{
		bool result = false;
		_destination = Vector3.zero;
		LevelPoint levelPoint = LevelPointGetPlayerDistance(enemy.transform.position, 30f, 50f);
		if (!levelPoint)
		{
			levelPoint = LevelPointGetFurthestFromPlayer(enemy.transform.position, 5f);
		}
		if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + UnityEngine.Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
		{
			_destination = hit.position;
			result = true;
		}
		return result;
	}

	public static Camera MainCamera()
	{
		return GameDirector.instance.MainCamera;
	}

	public static bool EnemySpawnIdlePause()
	{
		if (EnemyDirector.instance.spawnIdlePauseTimer > 0f)
		{
			return true;
		}
		return false;
	}

	public static bool EnemyForceLeave(Enemy enemy)
	{
		if (enemy.EnemyParent.forceLeave)
		{
			enemy.EnemyParent.forceLeave = false;
			return true;
		}
		return false;
	}

	public static bool EnemyDirectorEndingHeadToTruck()
	{
		if (RoundDirector.instance.allExtractionPointsCompleted && EnemyDirector.instance.extractionsDoneState == EnemyDirector.ExtractionsDoneState.StartRoom)
		{
			return true;
		}
		return false;
	}

	public static bool EnemyDirectorEndingHeadToPlayers()
	{
		if (RoundDirector.instance.allExtractionPointsCompleted && EnemyDirector.instance.extractionsDoneState == EnemyDirector.ExtractionsDoneState.PlayerRoom)
		{
			return true;
		}
		return false;
	}

	public static void EnemyOvercharge(PhysGrabObject _physGrabObject, EnemyParent.Difficulty _enemyDifficulty, float _multiplier)
	{
		int num = _physGrabObject.playerGrabbing.Count;
		if (num > 1)
		{
			num *= 2;
		}
		int num2 = (int)(_enemyDifficulty + 1);
		float num3 = 0.08f * _multiplier;
		if (_physGrabObject.grabbedLocal)
		{
			float amount = num3 * (float)num2 / (float)num;
			PhysGrabber.instance.PhysGrabOverCharge(amount, _multiplier);
		}
	}

	public static bool OnGroundCheck(Vector3 _position, float _distance, PhysGrabObject _notMe = null)
	{
		RaycastHit[] array = Physics.RaycastAll(_position, Vector3.down, _distance, LayerMask.GetMask("Default", "PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "Enemy", "Player"), QueryTriggerInteraction.Ignore);
		foreach (RaycastHit raycastHit in array)
		{
			if (!_notMe)
			{
				return true;
			}
			PhysGrabObject componentInParent = raycastHit.collider.GetComponentInParent<PhysGrabObject>();
			if (!componentInParent || componentInParent != _notMe)
			{
				return true;
			}
		}
		return false;
	}

	public static PlayerAvatar PlayerGetFromSteamID(string _steamID)
	{
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player.steamID == _steamID)
			{
				return player;
			}
		}
		return null;
	}

	public static Transform PlayerGetFaceEyeTransform(PlayerAvatar _player)
	{
		if (!_player.isLocal)
		{
			return _player.playerAvatarVisuals.headLookAtTransform;
		}
		return _player.localCamera.transform;
	}

	public static Vector3 PlayerGetObservedPosition()
	{
		return AudioListenerFollow.instance.TargetPositionTransform.position;
	}

	public static PlayerAvatar PlayerGetFromName(string _name)
	{
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player.playerName == _name)
			{
				return player;
			}
		}
		return null;
	}

	public static Color PlayerGetColorFromSteamID(string _steamID)
	{
		PlayerAvatar playerAvatar = PlayerGetFromSteamID(_steamID);
		if ((bool)playerAvatar)
		{
			return playerAvatar.playerAvatarVisuals.color;
		}
		return Color.black;
	}

	public static void ItemAffectEnemyBatteryDrain(EnemyParent _enemyParent, ItemBattery _itemBattery, float tumbleEnemyTimer, float _deltaTime, float _multiplier = 1f)
	{
		if (!_enemyParent || !_itemBattery)
		{
			return;
		}
		Rigidbody componentInChildren = _enemyParent.GetComponentInChildren<Rigidbody>();
		if (!componentInChildren)
		{
			return;
		}
		Enemy componentInChildren2 = _enemyParent.GetComponentInChildren<Enemy>();
		if (!componentInChildren2)
		{
			return;
		}
		switch ((int)_enemyParent.difficulty)
		{
		case 0:
		{
			float value3 = componentInChildren.mass * 0.5f;
			value3 = Mathf.Clamp(value3, 1f, 2f) * _multiplier;
			_itemBattery.batteryLife -= value3 * _deltaTime;
			if (tumbleEnemyTimer > 1.5f && componentInChildren2.HasStateStunned)
			{
				componentInChildren2.StateStunned.Set(1f);
			}
			break;
		}
		case 1:
		{
			float value2 = componentInChildren.mass * 0.85f;
			value2 = Mathf.Clamp(value2, 1f, 2f) * _multiplier;
			_itemBattery.batteryLife -= value2 * _deltaTime;
			if (tumbleEnemyTimer > 3f && componentInChildren2.HasStateStunned)
			{
				componentInChildren2.StateStunned.Set(1f);
			}
			break;
		}
		case 2:
		{
			float value = componentInChildren.mass * 1f;
			value = Mathf.Clamp(value, 1f, 2f) * _multiplier;
			_itemBattery.batteryLife -= value * _deltaTime;
			if (tumbleEnemyTimer > 4f && componentInChildren2.HasStateStunned)
			{
				componentInChildren2.StateStunned.Set(1f);
			}
			break;
		}
		}
	}

	public static void EnemyInvestigate(Vector3 position, float range, bool pathfindOnly = false)
	{
		EnemyDirector.instance.SetInvestigate(position, range, pathfindOnly);
	}

	public static int EnemyGetIndex(Enemy _enemy)
	{
		int result = -1;
		if ((bool)_enemy)
		{
			foreach (EnemyParent item in EnemyDirector.instance.enemiesSpawned)
			{
				if ((bool)item && item.Enemy == _enemy)
				{
					result = EnemyDirector.instance.enemiesSpawned.IndexOf(item);
					break;
				}
			}
		}
		return result;
	}

	public static Enemy EnemyGetFromIndex(int _enemyIndex)
	{
		Enemy result = null;
		if (_enemyIndex == -1)
		{
			return result;
		}
		foreach (EnemyParent item in EnemyDirector.instance.enemiesSpawned)
		{
			if (EnemyDirector.instance.enemiesSpawned.IndexOf(item) == _enemyIndex)
			{
				result = item.Enemy;
				break;
			}
		}
		return result;
	}

	public static Enemy EnemyGetNearest(Vector3 _position, float _maxDistance, bool _raycast)
	{
		Enemy result = null;
		float num = _maxDistance;
		foreach (EnemyParent item in EnemyDirector.instance.enemiesSpawned)
		{
			if (!item || item.DespawnedTimer > 0f)
			{
				continue;
			}
			Vector3 direction = item.Enemy.CenterTransform.position - _position;
			if (!(direction.magnitude < num))
			{
				continue;
			}
			if (_raycast)
			{
				bool flag = false;
				RaycastHit[] array = Physics.RaycastAll(_position, direction, direction.magnitude, LayerMaskGetVisionObstruct(), QueryTriggerInteraction.Ignore);
				foreach (RaycastHit raycastHit in array)
				{
					if (raycastHit.collider.gameObject.CompareTag("Wall"))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			num = direction.magnitude;
			result = item.Enemy;
		}
		return result;
	}

	public static bool EnemyPhysObjectSphereCheck(Vector3 _position, float _radius)
	{
		if (Physics.OverlapSphere(_position, _radius, LayerMaskGetPhysGrabObject()).Length != 0)
		{
			return true;
		}
		return false;
	}

	public static bool EnemyPhysObjectBoundingBoxCheck(Vector3 _currentPosition, Vector3 _checkPosition, Rigidbody _rigidbody, bool _checkDefault = false, float _boundsScale = 1.2f)
	{
		Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
		Collider[] componentsInChildren = _rigidbody.GetComponentsInChildren<Collider>(includeInactive: true);
		foreach (Collider collider in componentsInChildren)
		{
			if (!collider.isTrigger)
			{
				if (bounds.size == Vector3.zero)
				{
					bounds = collider.bounds;
				}
				else
				{
					bounds.Encapsulate(collider.bounds);
				}
			}
		}
		Vector3 vector = _currentPosition - _rigidbody.transform.position;
		Vector3 vector2 = bounds.center - _rigidbody.transform.position;
		bounds.center = _checkPosition - vector + vector2;
		bounds.size *= _boundsScale;
		LayerMask layerMask = LayerMaskGetPhysGrabObject();
		if (_checkDefault)
		{
			layerMask = (int)layerMask + LayerMask.GetMask("Default");
		}
		componentsInChildren = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, layerMask);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].GetComponentInParent<Rigidbody>() != _rigidbody)
			{
				return true;
			}
		}
		return false;
	}

	public static Bounds CalculateBounds(GameObject go, bool useColliders = false)
	{
		Bounds result = new Bounds(Vector3.zero, Vector3.zero);
		if (useColliders)
		{
			Collider[] componentsInChildren = go.GetComponentsInChildren<Collider>(includeInactive: true);
			foreach (Collider collider in componentsInChildren)
			{
				if (!collider.isTrigger)
				{
					if (result.size == Vector3.zero)
					{
						result = collider.bounds;
					}
					else
					{
						result.Encapsulate(collider.bounds);
					}
				}
			}
			if (result.size != Vector3.zero)
			{
				return result;
			}
		}
		Renderer[] componentsInChildren2 = go.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren2)
		{
			if (result.size == Vector3.zero)
			{
				result = renderer.bounds;
			}
			else
			{
				result.Encapsulate(renderer.bounds);
			}
		}
		if (!(result.size != Vector3.zero))
		{
			return new Bounds(go.transform.position, Vector3.one);
		}
		return result;
	}

	public static void DebugDrawBounds(Bounds _bounds, Color _color, float _time)
	{
		Vector3 vector = new Vector3(_bounds.min.x, _bounds.min.y, _bounds.min.z);
		Vector3 vector2 = new Vector3(_bounds.max.x, _bounds.min.y, _bounds.min.z);
		Vector3 vector3 = new Vector3(_bounds.max.x, _bounds.min.y, _bounds.max.z);
		Vector3 vector4 = new Vector3(_bounds.min.x, _bounds.min.y, _bounds.max.z);
		Debug.DrawLine(vector, vector2, _color, _time);
		Debug.DrawLine(vector2, vector3, _color, _time);
		Debug.DrawLine(vector3, vector4, _color, _time);
		Debug.DrawLine(vector4, vector, _color, _time);
		Vector3 vector5 = new Vector3(_bounds.min.x, _bounds.max.y, _bounds.min.z);
		Vector3 vector6 = new Vector3(_bounds.max.x, _bounds.max.y, _bounds.min.z);
		Vector3 vector7 = new Vector3(_bounds.max.x, _bounds.max.y, _bounds.max.z);
		Vector3 vector8 = new Vector3(_bounds.min.x, _bounds.max.y, _bounds.max.z);
		Debug.DrawLine(vector5, vector6, _color, _time);
		Debug.DrawLine(vector6, vector7, _color, _time);
		Debug.DrawLine(vector7, vector8, _color, _time);
		Debug.DrawLine(vector8, vector5, _color, _time);
		Debug.DrawLine(vector, vector5, _color, _time);
		Debug.DrawLine(vector2, vector6, _color, _time);
		Debug.DrawLine(vector3, vector7, _color, _time);
		Debug.DrawLine(vector4, vector8, _color, _time);
	}

	[HideInCallstack]
	public static bool DebugUser(User _user, bool editorOnly = true)
	{
		if (DebugDev() && _user != User.None && SteamManager.instance.developerUser == _user && Debug.isDebugBuild)
		{
			if (editorOnly)
			{
				return Application.isEditor;
			}
			return true;
		}
		return false;
	}

	public static bool Axel()
	{
		return DebugUser(User.Axel);
	}

	public static bool Jannek()
	{
		return DebugUser(User.Jannek);
	}

	public static bool Robin()
	{
		return DebugUser(User.Robin);
	}

	public static bool Ruben()
	{
		return DebugUser(User.Ruben);
	}

	public static bool Walter()
	{
		return DebugUser(User.Walter);
	}

	public static bool Monika()
	{
		return DebugUser(User.Monika);
	}

	public static bool Jenson()
	{
		return DebugUser(User.Jenson);
	}

	public static bool DebugKeyDown(User _user, KeyCode _input)
	{
		if (DebugUser(_user))
		{
			return Input.GetKeyUp(_input);
		}
		return false;
	}

	public static bool KeyDownAxel(KeyCode _input)
	{
		return DebugKeyDown(User.Axel, _input);
	}

	public static bool KeyDownJannek(KeyCode _input)
	{
		return DebugKeyDown(User.Jannek, _input);
	}

	public static bool KeyDownRobin(KeyCode _input)
	{
		return DebugKeyDown(User.Robin, _input);
	}

	public static bool KeyDownRuben(KeyCode _input)
	{
		return DebugKeyDown(User.Ruben, _input);
	}

	public static bool KeyDownWalter(KeyCode _input)
	{
		return DebugKeyDown(User.Walter, _input);
	}

	public static bool KeyDownMonika(KeyCode _input)
	{
		return DebugKeyDown(User.Monika, _input);
	}

	public static bool KeyDownJenson(KeyCode _input)
	{
		return DebugKeyDown(User.Jenson, _input);
	}

	public static bool DebugKey(User _user, KeyCode _input)
	{
		if (DebugUser(_user))
		{
			return Input.GetKey(_input);
		}
		return false;
	}

	public static bool KeyAxel(KeyCode _input)
	{
		return DebugKey(User.Axel, _input);
	}

	public static bool KeyJannek(KeyCode _input)
	{
		return DebugKey(User.Jannek, _input);
	}

	public static bool KeyRobin(KeyCode _input)
	{
		return DebugKey(User.Robin, _input);
	}

	public static bool KeyRuben(KeyCode _input)
	{
		return DebugKey(User.Ruben, _input);
	}

	public static bool KeyWalter(KeyCode _input)
	{
		return DebugKey(User.Walter, _input);
	}

	public static bool KeyMonika(KeyCode _input)
	{
		return DebugKey(User.Monika, _input);
	}

	public static bool KeyJenson(KeyCode _input)
	{
		return DebugKey(User.Jenson, _input);
	}

	[HideInCallstack]
	public static bool DebugDev()
	{
		if ((bool)SteamManager.instance)
		{
			return SteamManager.instance.developerMode;
		}
		return false;
	}

	public static bool DebugTester()
	{
		return Debug.isDebugBuild;
	}

	public static void DebugCube(Vector3 _position, Vector3 _scale, Quaternion _rotation, Color _color, float _time, Vector3 _pivotOffset)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(AssetManager.instance.debugCube, _position, _rotation);
		gameObject.transform.localScale = _scale;
		DebugCube component = gameObject.GetComponent<DebugCube>();
		component.color = _color;
		component.gizmoTransform.localPosition = _pivotOffset;
		UnityEngine.Object.Destroy(gameObject, _time);
	}

	public static void DebugSphere(Vector3 _position, float _radius, Color _color, float _time)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(AssetManager.instance.debugSphere, _position, Quaternion.identity);
		gameObject.transform.localScale = new Vector3(_radius, _radius, _radius);
		gameObject.GetComponent<DebugSphere>().color = _color;
		UnityEngine.Object.Destroy(gameObject, _time);
	}

	public static float UIMulti()
	{
		return HUDCanvas.instance.rect.sizeDelta.y;
	}

	public static string MessageGeneratedGetLeftBehind()
	{
		List<string> list = new List<string> { "You", "They", "My team", "Everyone", "My friends", "The squad", "The group", "All of them", "Those I trusted", "My companions" };
		List<string> list2 = new List<string> { "left", "abandoned", "betrayed", "forgot", "doomed", "deserted", "ditched", "dissed", "discarded", "forgot" };
		List<string> list3 = new List<string>
		{
			"me", "lil old me", "this lil robot", "my life", "my only hope", "my chance", "this poor robot", "my feelings", "our friendship", "my heart",
			"my trust"
		};
		List<string> list4 = new List<string> { "behind", "alone", "in the dark", "without a word", "without warning", "in silence", "without looking back", "with no remorse" };
		List<string> list5 = new List<string> { "I feel lost.", "Why didn't they wait?", "What did I do wrong?", "I can't believe it.", "How could they?", "This can't be happening.", "I'm on my own now.", "They were my only hope.", "I should have known.", "It's so unfair." };
		List<string> list6 = new List<string> { "{subject} {verb} {object}.", "{additional_phrase} {subject} {verb} {object}...", "{subject} {verb} lil me {adverb}.", "{additional_phrase}", "They {verb} me {adverb}...", "Now, {subject} {verb} {object}.", "In the end, {subject} {verb} {object}.", "I can't believe {subject} {verb} {object}...", "{subject} {verb} {object}. {additional_phrase}", "{additional_phrase} {subject} {verb} {object}." };
		int index = UnityEngine.Random.Range(0, list6.Count);
		return list6[index].Replace("{subject}", list[UnityEngine.Random.Range(0, list.Count)]).Replace("{verb}", list2[UnityEngine.Random.Range(0, list2.Count)]).Replace("{object}", list3[UnityEngine.Random.Range(0, list3.Count)])
			.Replace("{adverb}", list4[UnityEngine.Random.Range(0, list4.Count)])
			.Replace("{additional_phrase}", list5[UnityEngine.Random.Range(0, list5.Count)]);
	}

	public static bool MainMenuIsSingleplayer()
	{
		if (MainMenuOpen.instance.MainMenuGetState() == MainMenuOpen.MainMenuGameModeState.SinglePlayer)
		{
			return true;
		}
		return false;
	}

	public static void MenuActionSingleplayerGame(string saveFileName = null, List<string> saveFileBackups = null)
	{
		RunManager.instance.ResetProgress();
		GameManager.instance.SetConnectRandom(_connectRandom: false);
		if (saveFileName != null)
		{
			Debug.Log("Loading save");
			SaveFileLoad(saveFileName, saveFileBackups);
		}
		else
		{
			SaveFileCreate();
		}
		DataDirector.instance.RunsPlayedAdd();
		if (RunManager.instance.loadLevel == 0)
		{
			RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.RunLevel);
		}
		else
		{
			RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.Shop);
		}
	}

	public static void MenuActionHostGame(string saveFileName = null, List<string> saveFileBackups = null)
	{
		RunManager.instance.ResetProgress();
		GameManager.instance.SetConnectRandom(_connectRandom: false);
		if (saveFileName != null)
		{
			SaveFileLoad(saveFileName, saveFileBackups);
		}
		else
		{
			SaveFileCreate();
		}
		GameManager.instance.localTest = false;
		RunManager.instance.waitToChangeScene = true;
		RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false, RunManager.ChangeLevelType.LobbyMenu);
		MainMenuOpen.instance.NetworkConnect();
	}

	public static void SaveFileLoad(string saveFileName, List<string> saveFileBackups)
	{
		StatsManager.instance.LoadGame(saveFileName, saveFileBackups);
	}

	public static void SaveFileDelete(string saveFileName)
	{
		if (!string.IsNullOrEmpty(saveFileName))
		{
			StatsManager.instance.SaveFileDelete(saveFileName);
		}
	}

	public static void SaveFileCreate()
	{
		StatsManager.instance.SaveFileCreate();
	}

	public static void SaveFileSave()
	{
		if (!GameManager.instance.connectRandom)
		{
			StatsManager.instance.SaveFileSave();
		}
	}

	public static bool MainMenuIsMultiplayer()
	{
		if (MainMenuOpen.instance.MainMenuGetState() == MainMenuOpen.MainMenuGameModeState.MultiPlayer)
		{
			return true;
		}
		return false;
	}

	public static void MainMenuSetSingleplayer()
	{
		MainMenuOpen.instance.MainMenuSetState(0);
	}

	public static void MainMenuSetMultiplayer()
	{
		MainMenuOpen.instance.MainMenuSetState(1);
	}

	public static List<PhysGrabObject> PhysGrabObjectAllValuablesWithinRange(float range, Vector3 position, bool doRaycastCheck = false, LayerMask layerMask = default(LayerMask))
	{
		List<PhysGrabObject> list = new List<PhysGrabObject>();
		Collider[] array = Physics.OverlapSphere(position, range, LayerMask.GetMask("PhysGrabObject"));
		for (int i = 0; i < array.Length; i++)
		{
			PhysGrabObject componentInParent = array[i].GetComponentInParent<PhysGrabObject>();
			if (!componentInParent || !componentInParent.isValuable)
			{
				continue;
			}
			Vector3 centerPoint = componentInParent.centerPoint;
			float num = Vector3.Distance(position, centerPoint);
			if (num > range)
			{
				continue;
			}
			Vector3 direction = centerPoint - position;
			bool flag = false;
			if (doRaycastCheck)
			{
				RaycastHit[] array2 = Physics.RaycastAll(position, direction, num, layerMask, QueryTriggerInteraction.Ignore);
				foreach (RaycastHit raycastHit in array2)
				{
					if (raycastHit.collider.transform.CompareTag("Wall"))
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				list.Add(componentInParent);
			}
		}
		return list;
	}

	public static float PhysGrabObjectValuableGrabbedLineDistanceToNearest(Vector3 lineStart, Vector3 lineEnd, float _maxDistance, PhysGrabObject _skipThisPhysGrabObject = null)
	{
		LayerMask layerMask = LayerMaskGetPhysGrabObject();
		RaycastHit[] array = Physics.SphereCastAll(lineStart, _maxDistance, (lineEnd - lineStart).normalized, Vector3.Distance(lineStart, lineEnd), layerMask);
		float num = _maxDistance;
		float num2 = 9f;
		RaycastHit[] array2 = array;
		foreach (RaycastHit raycastHit in array2)
		{
			PhysGrabObject componentInParent = raycastHit.transform.GetComponentInParent<PhysGrabObject>();
			if ((bool)componentInParent && componentInParent.isValuable && componentInParent != _skipThisPhysGrabObject && componentInParent.playerGrabbing.Count > 0)
			{
				float num3 = DistanceToLine(lineStart, lineEnd, componentInParent.centerPoint);
				if (num3 < num)
				{
					num = num3;
					num2 = num;
				}
			}
		}
		if (num2 == 9f)
		{
			return _maxDistance;
		}
		return num2;
	}

	public static List<PlayerAvatar> PlayerGetAllPlayerAvatarWithinRange(float range, Vector3 position, bool doRaycastCheck = false, LayerMask layerMask = default(LayerMask))
	{
		List<PlayerAvatar> list = new List<PlayerAvatar>();
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (!player || player.isDisabled)
			{
				continue;
			}
			Vector3 position2 = player.PlayerVisionTarget.VisionTransform.position;
			float num = Vector3.Distance(position, position2);
			if (num > range)
			{
				continue;
			}
			Vector3 direction = position2 - position;
			bool flag = false;
			if (doRaycastCheck)
			{
				RaycastHit[] array = Physics.RaycastAll(position, direction, num, layerMask, QueryTriggerInteraction.Ignore);
				foreach (RaycastHit raycastHit in array)
				{
					if (raycastHit.collider.transform.CompareTag("Wall"))
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				list.Add(player);
			}
		}
		return list;
	}

	public static List<PlayerAvatar> PlayerGetAllPlayerAvatarWithinRangeAndVision(float range, Vector3 position, PhysGrabObject _thisPhysGrabObject = null)
	{
		LayerMask layerMask = LayerMaskGetVisionObstruct();
		List<PlayerAvatar> list = new List<PlayerAvatar>();
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player.isDisabled)
			{
				continue;
			}
			Vector3 position2 = player.PlayerVisionTarget.VisionTransform.position;
			float num = Vector3.Distance(position, position2);
			if (num > range)
			{
				continue;
			}
			Vector3 direction = position2 - position;
			bool flag = false;
			RaycastHit[] array = Physics.RaycastAll(position, direction, num, layerMask, QueryTriggerInteraction.Ignore);
			for (int i = 0; i < array.Length; i++)
			{
				RaycastHit raycastHit = array[i];
				if (!(raycastHit.transform.GetComponentInParent<PhysGrabObject>() == _thisPhysGrabObject) && !raycastHit.transform.GetComponentInParent<PlayerController>())
				{
					flag = true;
				}
			}
			if (!flag)
			{
				list.Add(player);
			}
		}
		return list;
	}

	public static PlayerAvatar PlayerGetNearestPlayerAvatarWithinRange(float range, Vector3 position, bool doRaycastCheck = false, LayerMask layerMask = default(LayerMask))
	{
		float num = range;
		PlayerAvatar result = null;
		List<PlayerAvatar> list = PlayerGetAllPlayerAvatarWithinRange(range, position, doRaycastCheck, layerMask);
		if (list.Count > 0)
		{
			foreach (PlayerAvatar item in list)
			{
				Vector3 position2 = item.PlayerVisionTarget.VisionTransform.position;
				float num2 = Vector3.Distance(position, position2);
				if (num2 < num)
				{
					num = num2;
					result = item;
				}
			}
		}
		return result;
	}

	public static float PlayerLocalDistance(Vector3 position)
	{
		Vector3 position2 = PlayerController.instance.transform.position;
		return Vector3.Distance(position, position2);
	}

	public static float PlayerNearestDistance(Vector3 position)
	{
		float num = 999f;
		float result = 9f;
		if (GameDirector.instance.PlayerList.Count > 0)
		{
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				if ((bool)player && !player.isDisabled)
				{
					Vector3 position2 = player.PlayerVisionTarget.VisionTransform.position;
					float num2 = Vector3.Distance(position, position2);
					if (num2 < num)
					{
						num = num2;
						result = num;
					}
				}
			}
		}
		return result;
	}

	public static float PlayerNearestLineDistance(Vector3 lineStart, Vector3 lineEnd)
	{
		float num = 999f;
		float result = 9f;
		List<PlayerAvatar> playerList = GameDirector.instance.PlayerList;
		if (playerList.Count > 0)
		{
			foreach (PlayerAvatar item in playerList)
			{
				Vector3 position = item.PlayerVisionTarget.VisionTransform.position;
				float num2 = DistanceToLine(lineStart, lineEnd, position);
				if (num2 < num)
				{
					num = num2;
					result = num;
				}
			}
		}
		return result;
	}

	public static PlayerAvatar PlayerGetNearestLineDistance(Vector3 lineStart, Vector3 lineEnd)
	{
		float num = 999f;
		PlayerAvatar result = null;
		List<PlayerAvatar> playerList = GameDirector.instance.PlayerList;
		if (playerList.Count > 0)
		{
			foreach (PlayerAvatar item in playerList)
			{
				if (!item.isDisabled)
				{
					Vector3 position = item.PlayerVisionTarget.VisionTransform.position;
					float num2 = DistanceToLine(lineStart, lineEnd, position);
					if (num2 < num)
					{
						num = num2;
						result = item;
					}
				}
			}
		}
		return result;
	}

	public static float DistanceToLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
	{
		Vector3 normalized = (lineEnd - lineStart).normalized;
		Vector3 normalized2 = (point - lineStart).normalized;
		float num = Vector3.Angle(normalized, normalized2);
		return Vector3.Distance(lineStart, point) * Mathf.Sin(num * (MathF.PI / 180f));
	}

	public static bool PlayerVisionCheck(Vector3 _position, float _range, PlayerAvatar _player, bool _previouslySeen)
	{
		Vector3 endPosition = _player.PlayerVisionTarget.VisionTransform.position;
		if (_player.isTumbling)
		{
			endPosition = _player.tumble.physGrabObject.centerPoint;
		}
		return PlayerVisionCheckPosition(_position, endPosition, _range, _player, _previouslySeen);
	}

	public static bool PlayerVisionCheckPosition(Vector3 _startPosition, Vector3 _endPosition, float _range, PlayerAvatar _player, bool _previouslySeen)
	{
		if (_player.enemyVisionFreezeTimer > 0f)
		{
			return _previouslySeen;
		}
		LayerMask layerMask = LayerMaskGetVisionObstruct();
		Vector3 direction = _endPosition - _startPosition;
		if (direction.magnitude > _range)
		{
			return false;
		}
		if (direction.magnitude < _range)
		{
			_range = direction.magnitude;
		}
		RaycastHit[] array = Physics.RaycastAll(_startPosition, direction, _range, layerMask, QueryTriggerInteraction.Ignore);
		PlayerAvatar playerAvatar = null;
		Transform transform = null;
		Transform transform2 = null;
		float num = 1000f;
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			float num2 = Vector3.Distance(_startPosition, raycastHit.point);
			if (!(num2 < num))
			{
				continue;
			}
			num = num2;
			transform2 = raycastHit.transform;
			PlayerAvatar playerAvatar2 = null;
			if (raycastHit.transform.CompareTag("Player"))
			{
				playerAvatar2 = raycastHit.transform.GetComponentInParent<PlayerAvatar>();
				if (!playerAvatar2)
				{
					PlayerController componentInParent = raycastHit.transform.GetComponentInParent<PlayerController>();
					if ((bool)componentInParent)
					{
						playerAvatar2 = componentInParent.playerAvatarScript;
					}
				}
			}
			else
			{
				PlayerTumble componentInParent2 = raycastHit.transform.GetComponentInParent<PlayerTumble>();
				if ((bool)componentInParent2)
				{
					playerAvatar2 = componentInParent2.playerAvatar;
				}
			}
			if ((bool)playerAvatar2 && playerAvatar2 == _player)
			{
				playerAvatar = playerAvatar2;
				transform = raycastHit.transform;
			}
		}
		if ((bool)playerAvatar && transform == transform2)
		{
			return true;
		}
		return false;
	}

	public static void PlayerEyesOverride(PlayerAvatar _player, Vector3 _position, float _time, GameObject _obj)
	{
		if (_player.isDisabled && (bool)_player.playerDeathHead && _player.playerDeathHead.spectated)
		{
			_player.playerDeathHead.playerEyes.Override(_position, _time, _obj);
		}
		else
		{
			_player.playerAvatarVisuals.playerEyes.Override(_position, _time, _obj);
		}
	}

	public static void PlayerEyesOverrideSoft(Vector3 _position, float _time, GameObject _obj, float _radius)
	{
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			Vector3 vector = player.PlayerVisionTarget.VisionTransform.position;
			bool flag = false;
			if (player.isDisabled && (bool)player.playerDeathHead && player.playerDeathHead.spectated)
			{
				vector = player.playerDeathHead.physGrabObject.centerPoint;
				flag = true;
			}
			if (Vector3.Distance(_position, vector) < _radius && Vector3.Distance(_position, vector) > 0.5f)
			{
				if (flag)
				{
					player.playerDeathHead.playerEyes.OverrideSoft(_position, _time, _obj);
				}
				else
				{
					player.playerAvatarVisuals.playerEyes.OverrideSoft(_position, _time, _obj);
				}
			}
		}
	}

	public static Transform PlayerGetNearestTransformWithinRange(float range, Vector3 position, bool doRaycastCheck = false, LayerMask layerMask = default(LayerMask))
	{
		float num = range;
		Transform result = null;
		List<PlayerAvatar> list = PlayerGetAllPlayerAvatarWithinRange(range, position, doRaycastCheck, layerMask);
		if (list.Count > 0)
		{
			foreach (PlayerAvatar item in list)
			{
				Vector3 position2 = item.PlayerVisionTarget.VisionTransform.position;
				float num2 = Vector3.Distance(position, position2);
				if (num2 < num)
				{
					num = num2;
					result = item.PlayerVisionTarget.VisionTransform;
				}
			}
		}
		return result;
	}

	public static List<PlayerAvatar> PlayerGetAll()
	{
		return GameDirector.instance.PlayerList;
	}

	public static bool PlayersAllInTruck()
	{
		foreach (PlayerAvatar item in PlayerGetList())
		{
			if (!item.isDisabled && !item.RoomVolumeCheck.inTruck)
			{
				return false;
			}
		}
		return true;
	}

	public static void CursorUnlock(float _time)
	{
		CursorManager.instance.Unlock(_time);
	}

	public static bool OnValidateCheck()
	{
		return false;
	}

	public static void LightAdd(PropLight propLight)
	{
		if (!LightManager.instance.propLights.Contains(propLight))
		{
			LightManager.instance.propLights.Add(propLight);
		}
	}

	public static void LightRemove(PropLight propLight)
	{
		if (LightManager.instance.propLights.Contains(propLight))
		{
			LightManager.instance.propLights.Remove(propLight);
		}
	}

	public static Vector3 EnemyRoamFindPoint(Vector3 _position)
	{
		Vector3 result = Vector3.zero;
		LevelPoint levelPoint = LevelPointGet(_position, 10f, 25f);
		if (!levelPoint)
		{
			levelPoint = LevelPointGet(_position, 0f, 999f);
		}
		if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + UnityEngine.Random.insideUnitSphere * 3f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
		{
			result = hit.position;
		}
		return result;
	}

	public static List<LevelPoint> LevelPointGetWithinDistance(Vector3 pos, float minDist, float maxDist)
	{
		List<LevelPoint> list = new List<LevelPoint>();
		foreach (LevelPoint levelPathPoint in LevelGenerator.Instance.LevelPathPoints)
		{
			float num = Vector3.Distance(levelPathPoint.transform.position, pos);
			if (num >= minDist && num <= maxDist)
			{
				list.Add(levelPathPoint);
			}
		}
		if (list.Count > 0)
		{
			return list;
		}
		return null;
	}

	public static List<LevelPoint> LevelPointsGetAll()
	{
		return LevelGenerator.Instance.LevelPathPoints;
	}

	public static List<LevelPoint> LevelPointListPurgeObstructed(List<LevelPoint> _levelPoints, CapsuleCollider _capsuleCollider)
	{
		List<LevelPoint> list = new List<LevelPoint>();
		Vector3 lossyScale = _capsuleCollider.transform.lossyScale;
		float radius = _capsuleCollider.radius * Mathf.Max(lossyScale.x, lossyScale.z) + 0.1f;
		float num = Mathf.Max(_capsuleCollider.height * lossyScale.y * 0.5f - _capsuleCollider.radius, 0f);
		foreach (LevelPoint _levelPoint in _levelPoints)
		{
			Vector3 vector = _levelPoint.transform.position + _capsuleCollider.transform.TransformVector(_capsuleCollider.center);
			Vector3 start = vector + Vector3.up * num;
			Vector3 end = vector - Vector3.up * num;
			if (!Physics.CheckCapsule(start, end, radius, LayerMaskGetPlayersAndPhysObjects(), QueryTriggerInteraction.Ignore))
			{
				list.Add(_levelPoint);
			}
		}
		return list;
	}

	public static LevelPoint LevelPointsGetClosestToPlayer()
	{
		List<PlayerAvatar> list = PlayerGetList();
		List<LevelPoint> list2 = LevelPointsGetAll();
		float num = 999f;
		LevelPoint result = null;
		foreach (PlayerAvatar item in list)
		{
			if (item.isDisabled)
			{
				continue;
			}
			Vector3 position = item.transform.position;
			foreach (LevelPoint item2 in list2)
			{
				float num2 = Vector3.Distance(position, item2.transform.position);
				if (num2 < num)
				{
					num = num2;
					result = item2;
				}
			}
		}
		return result;
	}

	public static LevelPoint LevelPointsGetClosestToLocalPlayer()
	{
		PlayerAvatar playerAvatar = ((!IsSpectating() || !SpectateCamera.instance.player) ? PlayerAvatar.instance : SpectateCamera.instance.player);
		if ((bool)playerAvatar)
		{
			List<LevelPoint> list = LevelPointsGetAll();
			float num = 999f;
			LevelPoint result = null;
			Vector3 position = playerAvatar.transform.position;
			{
				foreach (LevelPoint item in list)
				{
					float num2 = Vector3.Distance(position, item.transform.position);
					if (num2 < num)
					{
						num = num2;
						result = item;
					}
				}
				return result;
			}
		}
		return LevelPointsGetClosestToPlayer();
	}

	public static List<LevelPoint> LevelPointsGetVisiblePointsBehindPlayers(float _eyeHeight, float _maxDistance = 10f, float _behindDotThreshold = 0.5f, PlayerAvatar _specificPlayer = null)
	{
		List<LevelPoint> list = new List<LevelPoint>();
		float num = _maxDistance * _maxDistance;
		List<PlayerAvatar> list2 = PlayerGetList();
		List<LevelPoint> list3 = LevelPointsGetAll();
		foreach (PlayerAvatar item in list2)
		{
			if (item.isDisabled || ((bool)_specificPlayer && item != _specificPlayer))
			{
				continue;
			}
			Vector3 vector = item.localCamera.transform.position + Vector3.up * _eyeHeight;
			Vector3 forward = item.localCamera.transform.forward;
			foreach (LevelPoint item2 in list3)
			{
				Vector3 vector2 = item2.transform.position - vector;
				float sqrMagnitude = vector2.sqrMagnitude;
				if (!(sqrMagnitude > num) && !(Vector3.Dot(forward, vector2.normalized) > _behindDotThreshold) && !Physics.Raycast(vector, vector2.normalized, Mathf.Sqrt(sqrMagnitude), LayerMaskGetVisionObstruct()))
				{
					list.Add(item2);
				}
			}
		}
		return list;
	}

	public static List<LevelPoint> LevelPointsGetAllCloseToPlayers()
	{
		List<PlayerAvatar> list = PlayerGetList();
		List<LevelPoint> list2 = LevelPointsGetAll();
		List<LevelPoint> list3 = new List<LevelPoint>();
		foreach (PlayerAvatar item2 in list)
		{
			float num = 999f;
			LevelPoint item = null;
			if (item2.isDisabled)
			{
				continue;
			}
			Vector3 position = item2.transform.position;
			foreach (LevelPoint item3 in list2)
			{
				float num2 = Vector3.Distance(position, item3.transform.position);
				if (num2 < num)
				{
					num = num2;
					item = item3;
				}
			}
			list3.Add(item);
		}
		return list3;
	}

	public static List<LevelPoint> LevelPointsGetInPlayerRooms()
	{
		List<PlayerAvatar> list = PlayerGetList();
		List<LevelPoint> list2 = LevelPointsGetAll();
		List<LevelPoint> list3 = new List<LevelPoint>();
		foreach (PlayerAvatar item in list)
		{
			if (item.isDisabled)
			{
				continue;
			}
			foreach (RoomVolume currentRoom in item.RoomVolumeCheck.CurrentRooms)
			{
				foreach (LevelPoint item2 in list2)
				{
					if (item2.Room == currentRoom)
					{
						list3.Add(item2);
					}
				}
			}
		}
		return list3;
	}

	public static List<LevelPoint> LevelPointsGetInStartRoom()
	{
		List<LevelPoint> list = LevelPointsGetAll();
		List<LevelPoint> list2 = new List<LevelPoint>();
		foreach (LevelPoint item in list)
		{
			if (item.inStartRoom)
			{
				list2.Add(item);
			}
		}
		return list2;
	}

	public static List<LevelPoint> LevelPointsGetPlayerDistance(Vector3 _position, float _minDistance, float _maxDistance, bool _startRoomOnly = false)
	{
		List<LevelPoint> list = new List<LevelPoint>();
		foreach (LevelPoint levelPathPoint in LevelGenerator.Instance.LevelPathPoints)
		{
			if ((_startRoomOnly && !levelPathPoint.inStartRoom) || ((bool)levelPathPoint.Room && levelPathPoint.Room.Truck))
			{
				continue;
			}
			float num = 999f;
			bool flag = false;
			Vector3 position = levelPathPoint.transform.position;
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				if (!player.isDisabled)
				{
					float num2 = Vector3.Distance(position, player.transform.position);
					if (num2 < num)
					{
						num = num2;
					}
					if (num2 < _maxDistance)
					{
						flag = true;
					}
				}
			}
			if (num > _minDistance && flag)
			{
				list.Add(levelPathPoint);
			}
		}
		return list;
	}

	public static LevelPoint LevelPointGetPlayerDistance(Vector3 _position, float _minDistance, float _maxDistance, bool _startRoomOnly = false)
	{
		List<LevelPoint> list = LevelPointsGetPlayerDistance(_position, _minDistance, _maxDistance, _startRoomOnly);
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		return null;
	}

	public static LevelPoint LevelPointGetFurthestFromPlayer(Vector3 _position, float _minDistance)
	{
		float num = 0f;
		LevelPoint result = null;
		foreach (LevelPoint levelPathPoint in LevelGenerator.Instance.LevelPathPoints)
		{
			if ((bool)levelPathPoint.Room && levelPathPoint.Room.Truck)
			{
				continue;
			}
			float num2 = 999f;
			float num3 = 0f;
			Vector3 position = levelPathPoint.transform.position;
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				if (!player.isDisabled)
				{
					float num4 = Vector3.Distance(position, player.transform.position);
					if (num4 < num2)
					{
						num2 = num4;
					}
					if (num4 > num3)
					{
						num3 = num4;
					}
				}
			}
			if (num2 > _minDistance && num3 > num)
			{
				num = num3;
				result = levelPathPoint;
			}
		}
		return result;
	}

	public static void PhysLookAtPositionWithForce(Rigidbody rb, Transform transform, Vector3 position, float forceMultiplier)
	{
		Vector3 normalized = (position - transform.position).normalized;
		Vector3 vector = Vector3.Cross(transform.forward, normalized);
		float magnitude = vector.magnitude;
		vector.Normalize();
		rb.AddTorque(vector * magnitude * forceMultiplier);
	}

	public static bool IsNotMasterClient()
	{
		if (GameManager.Multiplayer())
		{
			return !PhotonNetwork.IsMasterClient;
		}
		return false;
	}

	public static bool IsMasterClientOrSingleplayer()
	{
		if (!GameManager.Multiplayer() || !PhotonNetwork.IsMasterClient)
		{
			return !GameManager.Multiplayer();
		}
		return true;
	}

	public static bool IsMasterClient()
	{
		if (GameManager.Multiplayer())
		{
			return PhotonNetwork.IsMasterClient;
		}
		return false;
	}

	public static bool IsMainMenu()
	{
		return IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelMainMenu);
	}

	public static bool IsMultiplayer()
	{
		return GameManager.instance.gameMode == 1;
	}

	public static bool GameEventIsHalloween()
	{
		return GameManager.instance.currentGameEvent == GameManager.GameEvents.Halloween;
	}

	public static bool MenuLevel()
	{
		if (!IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelMainMenu) && !IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelLobbyMenu))
		{
			return IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelSplashScreen);
		}
		return true;
	}

	public static bool IsSplashScreen()
	{
		return IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelSplashScreen);
	}

	public static bool RunIsArena()
	{
		return IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelArena);
	}

	public static void CameraShake(float strength, float duration)
	{
		GameDirector.instance.CameraShake.Shake(strength, duration);
	}

	public static void CameraShakeDistance(Vector3 position, float strength, float duration, float distanceMin, float distanceMax)
	{
		GameDirector.instance.CameraShake.ShakeDistance(strength, distanceMin, distanceMax, position, duration);
	}

	public static void CameraShakeImpact(float strength, float duration)
	{
		GameDirector.instance.CameraImpact.Shake(strength, duration);
	}

	public static void CameraShakeImpactDistance(Vector3 position, float strength, float duration, float distanceMin, float distanceMax)
	{
		GameDirector.instance.CameraImpact.ShakeDistance(strength, distanceMin, distanceMax, position, duration);
	}

	public static Color ColorDifficultyGet(float minValue, float maxValue, float _currentValue)
	{
		Color[] array = new Color[4]
		{
			new Color(0f, 1f, 0f),
			new Color(1f, 1f, 0f),
			new Color(1f, 0.5f, 0f),
			new Color(1f, 0f, 0f)
		};
		int num = Mathf.FloorToInt(Mathf.Lerp(0f, array.Length - 1, Mathf.InverseLerp(minValue, maxValue, _currentValue)));
		float t = Mathf.InverseLerp(minValue, maxValue, _currentValue) * (float)(array.Length - 1) - (float)num;
		Color color = array[Mathf.Clamp(num, 0, array.Length - 1)];
		Color color2 = array[Mathf.Clamp(num + 1, 0, array.Length - 1)];
		return Color.Lerp(color, color2, t);
	}

	public static string TimeToString(float time, bool fancy = false, Color numberColor = default(Color), Color unitColor = default(Color))
	{
		int num = (int)(time / 3600f);
		int num2 = (int)(time % 3600f / 60f);
		int num3 = (int)(time % 60f);
		string text = "h ";
		string text2 = "m ";
		string text3 = "s";
		if (fancy)
		{
			text = "</b></color><color=#" + ColorUtility.ToHtmlStringRGBA(unitColor) + ">h</color> ";
			text2 = "</b></color><color=#" + ColorUtility.ToHtmlStringRGBA(unitColor) + ">m</color> ";
			text3 = "</b></color><color=#" + ColorUtility.ToHtmlStringRGBA(unitColor) + ">s</color>";
		}
		string text4 = "";
		if (num > 0)
		{
			if (fancy)
			{
				text4 = text4 + "<color=#" + ColorUtility.ToHtmlStringRGBA(numberColor) + "><b>";
			}
			text4 = text4 + num + text;
		}
		if (num2 > 0 || num > 0)
		{
			if (fancy)
			{
				text4 = text4 + "<color=#" + ColorUtility.ToHtmlStringRGBA(numberColor) + "><b>";
			}
			text4 = text4 + num2 + text2;
		}
		if ((num == 0 && num2 == 0) || fancy)
		{
			if (fancy)
			{
				text4 = text4 + "<color=#" + ColorUtility.ToHtmlStringRGBA(numberColor) + "><b>";
			}
			text4 = text4 + num3 + text3;
		}
		return text4;
	}

	public static List<PhysGrabObject> PhysGrabObjectGetAllWithinRange(float range, Vector3 position, bool doRaycastCheck = false, LayerMask layerMask = default(LayerMask), PhysGrabObject _thisPhysGrabObject = null)
	{
		List<PhysGrabObject> list = new List<PhysGrabObject>();
		Collider[] array = Physics.OverlapSphere(position, range, LayerMask.GetMask("PhysGrabObject"));
		for (int i = 0; i < array.Length; i++)
		{
			PhysGrabObject componentInParent = array[i].GetComponentInParent<PhysGrabObject>();
			if (!(componentInParent != null))
			{
				continue;
			}
			bool flag = false;
			if (doRaycastCheck)
			{
				Vector3 normalized = (componentInParent.midPoint - position).normalized;
				RaycastHit[] array2 = Physics.RaycastAll(position, normalized, range, layerMask);
				foreach (RaycastHit raycastHit in array2)
				{
					PhysGrabObject componentInParent2 = raycastHit.collider.GetComponentInParent<PhysGrabObject>();
					if (!(componentInParent2 == _thisPhysGrabObject) && !(componentInParent2 == componentInParent) && (componentInParent2 == null || (componentInParent2 != null && componentInParent2 != componentInParent)))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				list.Add(componentInParent);
			}
		}
		return list;
	}

	public static bool LocalPlayerOverlapCheck(float range, Vector3 position, bool doRaycastCheck = false)
	{
		Collider[] array = Physics.OverlapSphere(position, range, LayerMaskGetVisionObstruct());
		foreach (Collider collider in array)
		{
			PlayerController playerController = null;
			if (collider.transform.CompareTag("Player"))
			{
				playerController = collider.GetComponentInParent<PlayerController>();
			}
			else
			{
				PlayerTumble componentInParent = collider.GetComponentInParent<PlayerTumble>();
				if ((bool)componentInParent && componentInParent.playerAvatar.isLocal)
				{
					playerController = PlayerController.instance;
				}
			}
			if (!playerController)
			{
				continue;
			}
			bool flag = false;
			if (doRaycastCheck)
			{
				Vector3 normalized = (collider.transform.position - position).normalized;
				RaycastHit[] array2 = Physics.RaycastAll(position, normalized, range, LayerMask.GetMask("Default"));
				foreach (RaycastHit raycastHit in array2)
				{
					if (raycastHit.transform.CompareTag("Wall"))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return true;
			}
		}
		return false;
	}

	public static Vector3 PhysFollowPosition(Vector3 _currentPosition, Vector3 _targetPosition, Vector3 _currentVelocity, float _maxSpeed)
	{
		return Vector3.ClampMagnitude((_targetPosition - _currentPosition) / Time.fixedDeltaTime, _maxSpeed) - _currentVelocity;
	}

	public static Vector3 PhysFollowYPosition(Vector3 _currentPosition, Vector3 _targetPosition, Vector3 _currentVelocity, float _maxSpeed)
	{
		float value = (_targetPosition.y - _currentPosition.y) / Time.fixedDeltaTime;
		value = Mathf.Clamp(value, 0f - _maxSpeed, _maxSpeed);
		return new Vector3(0f, value - _currentVelocity.y, 0f);
	}

	public static Vector3 PhysFollowPositionWithDamping(Vector3 _currentPosition, Vector3 _targetPosition, Vector3 _currentVelocity, float _maxSpeed, float _damping)
	{
		return Vector3.ClampMagnitude((_targetPosition - _currentPosition) / Time.fixedDeltaTime, _maxSpeed) - _currentVelocity * (1f + _damping);
	}

	public static Vector3 PhysFollowRotation(Transform _transform, Quaternion _targetRotation, Rigidbody _rigidbody, float _maxSpeed)
	{
		_transform.rotation = Quaternion.RotateTowards(_targetRotation, _transform.rotation, 360f);
		(_targetRotation * Quaternion.Inverse(_transform.rotation)).ToAngleAxis(out var angle, out var axis);
		axis.Normalize();
		Vector3 direction = axis * angle * (MathF.PI / 180f) / Time.fixedDeltaTime;
		direction -= _rigidbody.angularVelocity;
		Vector3 vector = _transform.InverseTransformDirection(direction);
		vector = _rigidbody.inertiaTensorRotation * vector;
		vector.Scale(_rigidbody.inertiaTensor);
		Vector3 direction2 = Quaternion.Inverse(_rigidbody.inertiaTensorRotation) * vector;
		return Vector3.ClampMagnitude(_transform.TransformDirection(direction2), _maxSpeed);
	}

	public static Vector3 PhysFollowDirection(Transform _transform, Vector3 _targetDirection, Rigidbody _rigidbody, float _maxSpeed)
	{
		Vector3 normalized = Vector3.Cross(Vector3.up, _targetDirection).normalized;
		Quaternion rotation = _transform.rotation;
		_transform.Rotate(normalized * 100f, Space.World);
		Quaternion rotation2 = _transform.rotation;
		_transform.rotation = rotation;
		return PhysFollowRotation(_transform.transform, rotation2, _rigidbody, _maxSpeed);
	}

	public static LevelPoint LevelPointGet(Vector3 _position, float _minDistance, float _maxDistance)
	{
		LevelPoint result = null;
		List<LevelPoint> list = new List<LevelPoint>();
		foreach (LevelPoint levelPathPoint in LevelGenerator.Instance.LevelPathPoints)
		{
			if (!levelPathPoint.Room || !levelPathPoint.Room.Truck)
			{
				float num = Vector3.Distance(levelPathPoint.transform.position, _position);
				if (num >= _minDistance && num <= _maxDistance)
				{
					list.Add(levelPathPoint);
				}
			}
		}
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		return result;
	}

	public static LevelPoint LevelPointInTargetRoomGet(RoomVolumeCheck _target, float _minDistance, float _maxDistance, LevelPoint ignorePoint = null)
	{
		LevelPoint result = null;
		List<LevelPoint> list = new List<LevelPoint>();
		foreach (LevelPoint levelPathPoint in LevelGenerator.Instance.LevelPathPoints)
		{
			foreach (RoomVolume currentRoom in _target.CurrentRooms)
			{
				if (!(levelPathPoint == ignorePoint) && levelPathPoint.Room == currentRoom)
				{
					float num = Vector3.Distance(levelPathPoint.transform.position, _target.CheckPosition);
					if (num >= _minDistance && num <= _maxDistance)
					{
						list.Add(levelPathPoint);
					}
				}
			}
		}
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		return result;
	}

	public static bool OnScreen(Vector3 position, float paddWidth, float paddHeight)
	{
		paddWidth = (float)Screen.width * paddWidth;
		paddHeight = (float)Screen.height * paddHeight;
		Vector3 vector = CameraUtils.Instance.MainCamera.WorldToScreenPoint(position);
		vector.x *= (float)Screen.width / RenderTextureMain.instance.textureWidth;
		vector.y *= (float)Screen.height / RenderTextureMain.instance.textureHeight;
		if (vector.z > 0f && vector.x > 0f - paddWidth && vector.x < (float)Screen.width + paddWidth && vector.y > 0f - paddHeight && vector.y < (float)Screen.height + paddHeight)
		{
			return true;
		}
		return false;
	}

	public static Quaternion ClampRotation(Quaternion _quaternion, Vector3 _bounds)
	{
		_quaternion.x /= _quaternion.w;
		_quaternion.y /= _quaternion.w;
		_quaternion.z /= _quaternion.w;
		_quaternion.w = 1f;
		float value = 114.59156f * Mathf.Atan(_quaternion.x);
		value = Mathf.Clamp(value, 0f - _bounds.x, _bounds.x);
		_quaternion.x = Mathf.Tan(MathF.PI / 360f * value);
		float value2 = 114.59156f * Mathf.Atan(_quaternion.y);
		value2 = Mathf.Clamp(value2, 0f - _bounds.y, _bounds.y);
		_quaternion.y = Mathf.Tan(MathF.PI / 360f * value2);
		float value3 = 114.59156f * Mathf.Atan(_quaternion.z);
		value3 = Mathf.Clamp(value3, 0f - _bounds.z, _bounds.z);
		_quaternion.z = Mathf.Tan(MathF.PI / 360f * value3);
		return _quaternion.normalized;
	}

	public static Vector3 ClampDirection(Vector3 _direction, Vector3 _forward, float _maxAngle)
	{
		Vector3 result = _direction;
		if (Vector3.Angle(_direction, _forward) > _maxAngle)
		{
			Vector3 axis = Vector3.Cross(_forward, _direction);
			result = Quaternion.AngleAxis(_maxAngle, axis) * _forward;
		}
		return result;
	}

	public static List<PlayerAvatar> PlayerGetList()
	{
		return GameDirector.instance.PlayerList;
	}

	public static int PhotonViewIDPlayerAvatarLocal()
	{
		return PlayerAvatar.instance.GetComponent<PhotonView>().ViewID;
	}

	public static string EmojiText(string inputText)
	{
		inputText = inputText.Replace("{", "<sprite name=");
		inputText = inputText.Replace("}", ">");
		return inputText;
	}

	public static string DollarGetString(int value)
	{
		return value.ToString("#,0", new CultureInfo("de-DE"));
	}

	public static PhysicMaterial PhysicMaterialSticky()
	{
		return AssetManager.instance.physicMaterialStickyExtreme;
	}

	public static PhysicMaterial PhysicMaterialSlippery()
	{
		return AssetManager.instance.physicMaterialSlipperyExtreme;
	}

	public static PhysicMaterial PhysicMaterialSlipperyPlus()
	{
		return AssetManager.instance.physicMaterialSlipperyPlus;
	}

	public static PhysicMaterial PhysicMaterialDefault()
	{
		return AssetManager.instance.physicMaterialDefault;
	}

	public static PhysicMaterial PhysicMaterialPhysGrabObject()
	{
		return AssetManager.instance.physicMaterialPhysGrabObject;
	}

	public static int RunGetLevelsCompleted()
	{
		return RunManager.instance.levelsCompleted;
	}

	public static int MoonLevel()
	{
		return RunManager.instance.moonLevel;
	}

	public static float RunGetDifficultyMultiplier1()
	{
		return Mathf.Clamp01((float)RunManager.instance.levelsCompleted / 9f);
	}

	public static float RunGetDifficultyMultiplier2()
	{
		return Mathf.Clamp01((float)(RunManager.instance.levelsCompleted - 9) / 10f);
	}

	public static float RunGetDifficultyMultiplier3()
	{
		return Mathf.Clamp01((float)(RunManager.instance.levelsCompleted - 19) / 10f);
	}

	public static bool PhysGrabObjectIsGrabbed(PhysGrabObject physGrabObject)
	{
		return physGrabObject.grabbed;
	}

	public static List<PhysGrabber> PhysGrabObjectGetPhysGrabbersGrabbing(PhysGrabObject physGrabObject)
	{
		return physGrabObject.playerGrabbing;
	}

	public static List<PlayerAvatar> PhysGrabObjectGetPlayerAvatarsGrabbing(PhysGrabObject physGrabObject)
	{
		List<PlayerAvatar> list = new List<PlayerAvatar>();
		foreach (PhysGrabber item in physGrabObject.playerGrabbing)
		{
			list.Add(item.playerAvatar);
		}
		return list;
	}

	public static bool PhysGrabberLocalIsGrabbing()
	{
		return PhysGrabber.instance.grabbed;
	}

	public static void PhysGrabberLocalForceGrab(PhysGrabObject physGrabObject)
	{
		PhysGrabber.instance.OverrideGrab(physGrabObject);
	}

	public static PhysGrabObject PhysGrabberLocalGetGrabbedPhysGrabObject()
	{
		if (!PhysGrabber.instance.grabbed)
		{
			return null;
		}
		return PhysGrabber.instance.grabbedObject.GetComponent<PhysGrabObject>();
	}

	public static bool PhysGrabberIsGrabbing(PhysGrabber physGrabber)
	{
		return physGrabber.grabbed;
	}

	public static PhysGrabObject PhysGrabberGetGrabbedPhysGrabObject(PhysGrabber physGrabber)
	{
		if (!physGrabber.grabbed)
		{
			return null;
		}
		return physGrabber.grabbedObject.GetComponent<PhysGrabObject>();
	}

	public static void PhysGrabberForceGrab(PhysGrabber physGrabber, PhysGrabObject physGrabObject)
	{
		physGrabber.OverrideGrab(physGrabObject);
	}

	public static void PhysGrabberLocalChangeAlpha(float alpha)
	{
		PhysGrabber.instance.ChangeBeamAlpha(alpha);
	}

	public static void AwakeRigidbodySphere(Vector3 _position, float _radius)
	{
		Collider[] array = Physics.OverlapSphere(_position, _radius, LayerMaskGetPhysGrabObject());
		for (int i = 0; i < array.Length; i++)
		{
			PhysGrabObject componentInParent = array[i].GetComponentInParent<PhysGrabObject>();
			if ((bool)componentInParent)
			{
				componentInParent.rb.WakeUp();
			}
		}
	}

	public static void AwakeRigidbodyBox(Vector3 _position, Quaternion _rotation, Vector3 _size)
	{
		Collider[] array = Physics.OverlapBox(_position, _size * 0.5f, _rotation, LayerMaskGetPhysGrabObject());
		for (int i = 0; i < array.Length; i++)
		{
			PhysGrabObject componentInParent = array[i].GetComponentInParent<PhysGrabObject>();
			if ((bool)componentInParent)
			{
				componentInParent.rb.WakeUp();
			}
		}
	}

	public static void LightManagerSetCullTargetTransform(Transform target)
	{
		LightManager.instance.lightCullTarget = target;
		LightManager.instance.UpdateInstant();
	}

	public static string MenuGetSelectableID(GameObject gameObject)
	{
		return "" + gameObject.GetInstanceID();
	}

	public static void MenuSelectionBoxTargetSet(MenuPage parentPage, RectTransform rectTransform, Vector2 customOffset = default(Vector2), Vector2 customScale = default(Vector2))
	{
		Vector2 vector = UIGetRectTransformPositionOnScreen(rectTransform, withScreenMultiplier: false);
		Vector2 vector2 = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
		vector += new Vector2(vector2.x / 2f, vector2.y / 2f) + customOffset;
		MenuSelectableElement component = rectTransform.GetComponent<MenuSelectableElement>();
		MenuSelectionBox menuSelectionBox = parentPage.selectionBox;
		Vector2 vector3 = new Vector2(0f, 0f);
		bool isInScrollBox = false;
		if ((bool)component && component.isInScrollBox)
		{
			isInScrollBox = true;
			menuSelectionBox = component.menuScrollBox.menuSelectionBox;
			Transform parent = rectTransform.parent;
			int num = 30;
			while ((bool)parent && !parent.GetComponent<MenuPage>())
			{
				RectTransform component2 = parent.GetComponent<RectTransform>();
				if ((bool)component2 && !component2.GetComponent<MenuSelectableElement>())
				{
					vector3 -= new Vector2(component2.localPosition.x, component2.localPosition.y);
				}
				parent = parent.parent;
				num--;
				if (num <= 0)
				{
					Debug.LogError(rectTransform.name + " - Hover FAIL! Could not find a parent page ");
					break;
				}
			}
		}
		vector += vector3;
		MenuElementAnimations componentInParent = rectTransform.GetComponentInParent<MenuElementAnimations>();
		if ((bool)componentInParent)
		{
			_ = (float)Screen.width / (float)MenuManager.instance.screenUIWidth;
			_ = (float)Screen.height / (float)MenuManager.instance.screenUIHeight;
			componentInParent.GetComponent<RectTransform>();
		}
		menuSelectionBox.MenuSelectionBoxSetTarget(vector, vector2, component.parentPage, isInScrollBox, component.menuScrollBox, customScale);
	}

	public static float MenuGetPitchFromYPos(RectTransform rectTransform)
	{
		return Mathf.Lerp(0.5f, 2f, rectTransform.localPosition.y / (float)Screen.height);
	}

	public static Vector2 UIPositionToUIPosition(Vector3 position)
	{
		Vector3 vector = CameraOverlay.instance.overlayCamera.ScreenToViewportPoint(position) * UIMulti();
		vector.x *= 1.015f;
		vector.y *= 1.015f;
		vector.x /= Screen.width;
		vector.y /= Screen.height;
		float num = HUDCanvas.instance.rect.sizeDelta.x / HUDCanvas.instance.rect.sizeDelta.y;
		float num2 = HUDCanvas.instance.rect.sizeDelta.x * num / HUDCanvas.instance.rect.sizeDelta.y;
		vector.x *= HUDCanvas.instance.rect.sizeDelta.x * num2;
		vector.y *= HUDCanvas.instance.rect.sizeDelta.y * num;
		vector.x -= 18f;
		vector.y -= 15f;
		return new Vector2(vector.x, vector.y);
	}

	public static Vector2 UIMousePosToUIPos()
	{
		Vector3 vector = CameraOverlay.instance.overlayCamera.ScreenToViewportPoint(Input.mousePosition) * UIMulti();
		vector.x *= 1.015f;
		vector.y *= 1.015f;
		vector.x /= Screen.width;
		vector.y /= Screen.height;
		float num = HUDCanvas.instance.rect.sizeDelta.x / HUDCanvas.instance.rect.sizeDelta.y;
		float num2 = HUDCanvas.instance.rect.sizeDelta.x * num / HUDCanvas.instance.rect.sizeDelta.y;
		vector.x *= HUDCanvas.instance.rect.sizeDelta.x * num2;
		vector.y *= HUDCanvas.instance.rect.sizeDelta.y * num;
		return new Vector2(vector.x, vector.y);
	}

	public static Vector2 UIGetRectTransformPositionOnScreen(RectTransform rectTransform, bool withScreenMultiplier = true)
	{
		int num = 1;
		int num2 = 1;
		Vector3 position = rectTransform.position;
		Vector3 position2 = rectTransform.GetComponentInParent<MenuPage>().GetComponent<RectTransform>().position;
		Vector3 vector = position - position2;
		vector -= new Vector3(rectTransform.rect.width * rectTransform.pivot.x, rectTransform.rect.height * rectTransform.pivot.y, 0f);
		if (withScreenMultiplier)
		{
			vector = new Vector2(vector.x * (float)num, vector.y * (float)num2);
		}
		return vector;
	}

	public static Vector2 UIMouseGetLocalPositionWithinRectTransform(RectTransform rectTransform)
	{
		Vector2 vector = UIMousePosToUIPos();
		Vector2 vector2 = UIGetRectTransformPositionOnScreen(rectTransform, withScreenMultiplier: false);
		Vector2 vector3 = new Vector2(vector.x - vector2.x, vector.y - vector2.y);
		float num = rectTransform.rect.width * rectTransform.pivot.x;
		float num2 = rectTransform.rect.height * rectTransform.pivot.y;
		Vector3 lossyScale = rectTransform.lossyScale;
		float num3 = 1f;
		if (lossyScale.y < 1f)
		{
			num3 = 1f + (1f - lossyScale.y);
		}
		if (lossyScale.y > 1f)
		{
			num3 = 1f + (lossyScale.y - 1f);
		}
		return new Vector2((vector3.x + num) * num3, (vector3.y + num2) * num3);
	}

	public static bool UIMouseHover(MenuPage parentPage, RectTransform rectTransform, string menuID, float xPadding = 0f, float yPadding = 0f)
	{
		if ((bool)parentPage.parentPage && !parentPage.parentPage.pageActive)
		{
			return false;
		}
		Vector2 vector = UIMousePosToUIPos();
		if (MenuManager.instance.mouseHoldPosition != Vector2.zero)
		{
			vector = MenuManager.instance.mouseHoldPosition;
		}
		int num = 1;
		int num2 = 1;
		MenuScrollBox componentInParent = rectTransform.GetComponentInParent<MenuScrollBox>();
		if ((bool)componentInParent)
		{
			float num3 = (componentInParent.transform.position.y - 10f) * (float)num2;
			float num4 = (componentInParent.scrollerEndPosition + 32f) * (float)num2;
			if (vector.y > num4 || vector.y < num3)
			{
				return false;
			}
		}
		Vector2 vector2 = UIGetRectTransformPositionOnScreen(rectTransform, withScreenMultiplier: false);
		float num5 = (vector2.x + (rectTransform.rect.xMin - xPadding)) * (float)num;
		float num6 = (vector2.x + (rectTransform.rect.xMax + xPadding)) * (float)num;
		float num7 = (vector2.y + (rectTransform.rect.yMin - yPadding)) * (float)num2;
		float num8 = (vector2.y + (rectTransform.rect.yMax + yPadding)) * (float)num2;
		if (rectTransform.name == "Button Arrow")
		{
			SemiLogger.LogAxel(num5 + " | " + num6 + " | " + num7 + " | " + num8);
		}
		if (rectTransform.name == "Button Arrow")
		{
			SemiLogger.LogAxel(vector.x + " | " + vector.y);
		}
		bool result;
		if (vector.x >= num5 && vector.x <= num6 && vector.y >= num7 && vector.y <= num8)
		{
			result = true;
			if (menuID != "-1")
			{
				if (MenuManager.instance.currentMenuID == menuID)
				{
					MenuManager.instance.MenuHover();
				}
				if (MenuManager.instance.currentMenuID == "")
				{
					MenuManager.instance.currentMenuID = menuID;
				}
			}
		}
		else
		{
			result = false;
			if (menuID != "-1" && MenuManager.instance.currentMenuID == menuID)
			{
				MenuManager.instance.currentMenuID = "";
			}
		}
		if (menuID != "-1")
		{
			if (menuID == MenuManager.instance.currentMenuID)
			{
				return true;
			}
			return false;
		}
		return result;
	}

	public static void UIHideAim()
	{
		Aim.instance.SetState(Aim.State.Hidden);
	}

	public static void UIHideTumble()
	{
		TumbleUI.instance.Hide();
	}

	public static void UIHideWorldSpace()
	{
		WorldSpaceUIParent.instance.Hide();
	}

	public static void UIHideValuableDiscover()
	{
		ValuableDiscover.instance.Hide();
	}

	public static void UIShowArrow(Vector3 startPosition, Vector3 endPosition, float rotation)
	{
		ArrowUI.instance.ArrowShow(startPosition, endPosition, rotation);
	}

	public static void UIShowArrowWorldPosition(Vector3 startPosition, Vector3 endPosition, float rotation)
	{
		ArrowUI.instance.ArrowShowWorldPos(startPosition, endPosition, rotation);
	}

	public static void UIBigMessage(string message, string emoji, float size, Color colorMain, Color colorFlash)
	{
		BigMessageUI.instance.BigMessage(message, emoji, size, colorMain, colorFlash);
	}

	public static void UIFocusText(string message, Color colorMain, Color colorFlash, float time = 3f)
	{
		MissionUI.instance.MissionText(message, colorMain, colorFlash, time);
	}

	public static void UIItemInfoText(ItemAttributes itemAttributes, string message)
	{
		ItemInfoUI.instance.ItemInfoText(itemAttributes, message);
	}

	public static void UIHideHealth()
	{
		HealthUI.instance.Hide();
	}

	public static void UIHideOvercharge()
	{
	}

	public static void UIHideEnergy()
	{
		EnergyUI.instance.Hide();
	}

	public static void UIHideInventory()
	{
		InventoryUI.instance.Hide();
	}

	public static void UIHideHaul()
	{
		HaulUI.instance.Hide();
	}

	public static void UIHideGoal()
	{
		GoalUI.instance.Hide();
	}

	public static void UIHideCurrency()
	{
		CurrencyUI.instance.Hide();
	}

	public static void UIHideShopCost()
	{
		ShopCostUI.instance.Hide();
	}

	public static void UIShowSpectate()
	{
		if (IsMultiplayer() && (bool)SpectateCamera.instance && SpectateCamera.instance.CheckState(SpectateCamera.State.Normal) && SpectateNameUI.instance.Text.text != "" && (!Arena.instance || Arena.instance.currentState != Arena.States.GameOver))
		{
			SpectateNameUI.instance.Show();
		}
	}

	public static Vector3 UIWorldToCanvasPosition(Vector3 _worldPosition)
	{
		RectTransform rect = HUDCanvas.instance.rect;
		if (OnScreen(_worldPosition, 0.5f, 0.5f))
		{
			Vector3 vector = AssetManager.instance.mainCamera.WorldToViewportPoint(_worldPosition);
			return new Vector3(vector.x * rect.sizeDelta.x - rect.sizeDelta.x * 0.5f, vector.y * rect.sizeDelta.y - rect.sizeDelta.y * 0.5f, vector.z);
		}
		return new Vector3((0f - rect.sizeDelta.x) * 2f, (0f - rect.sizeDelta.y) * 2f, 0f);
	}

	public static bool FPSImpulse1()
	{
		return GameDirector.instance.fpsImpulse1;
	}

	public static bool FPSImpulse5()
	{
		return GameDirector.instance.fpsImpulse5;
	}

	public static bool FPSImpulse15()
	{
		return GameDirector.instance.fpsImpulse15;
	}

	public static bool FPSImpulse20()
	{
		return GameDirector.instance.fpsImpulse20;
	}

	public static bool FPSImpulse30()
	{
		return GameDirector.instance.fpsImpulse30;
	}

	public static bool FPSImpulse60()
	{
		return GameDirector.instance.fpsImpulse60;
	}

	public static bool MasterOnlyRPC(PhotonMessageInfo _info)
	{
		if (!IsMultiplayer() || _info.Sender == PhotonNetwork.MasterClient)
		{
			return true;
		}
		return false;
	}

	public static bool OwnerOnlyRPC(PhotonMessageInfo _info, PhotonView _photonView)
	{
		if (!IsMultiplayer() || _info.Sender == _photonView.Owner)
		{
			return true;
		}
		return false;
	}

	public static bool MasterAndOwnerOnlyRPC(PhotonMessageInfo _info, PhotonView _photonView)
	{
		if (!IsMultiplayer() || _info.Sender == PhotonNetwork.MasterClient || _info.Sender == _photonView.Owner)
		{
			return true;
		}
		return false;
	}

	public static void LocalPlayerOverrideEnergyUnlimited()
	{
		PlayerController.instance.EnergyCurrent = 100f;
	}

	public static void HUDSpectateSetName(string name)
	{
		SpectateNameUI.instance.SetName(name);
	}

	public static int ValuableGetTotalNumber()
	{
		return ValuableDirector.instance.valuableSpawnAmount;
	}

	public static bool ValuableTrapActivatedDiceRoll(int rarityLevel)
	{
		if (rarityLevel == 1)
		{
			return UnityEngine.Random.Range(1, 3) == 1;
		}
		if (rarityLevel == 2)
		{
			return UnityEngine.Random.Range(1, 5) == 1;
		}
		if (rarityLevel > 2)
		{
			return UnityEngine.Random.Range(1, 10) == 1;
		}
		return false;
	}

	public static LayerMask LayerMaskGetVisionObstruct()
	{
		return LayerMask.GetMask("Default", "Player", "PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "StaticGrabObject");
	}

	public static LayerMask LayerMaskGetShouldHits()
	{
		return LayerMask.GetMask("Default", "Player", "PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "StaticGrabObject", "Enemy");
	}

	public static LayerMask LayerMaskGetPlayersAndPhysObjects()
	{
		return LayerMask.GetMask("Player", "PhysGrabObject", "PhysGrabObjectHinge", "PhysGrabObjectCart");
	}

	public static LayerMask LayerMaskGetPhysGrabObject()
	{
		return LayerMask.GetMask("PhysGrabObject", "PhysGrabObjectCart", "PhysGrabObjectHinge", "StaticGrabObject");
	}

	public static float BatteryGetChargeRate(int chargeLevel)
	{
		if (chargeLevel == 1)
		{
			return 1f;
		}
		if (chargeLevel == 2)
		{
			return 2f;
		}
		if (chargeLevel >= 3)
		{
			return 5f;
		}
		return 0f;
	}

	public static bool BatteryChargeCondition(ItemBattery battery)
	{
		if ((bool)battery && ((battery.batteryLife < 100f && battery.batteryActive) || (battery.batteryLife < 99f && !battery.batteryActive)))
		{
			return !battery.isUnchargable;
		}
		return false;
	}

	public static bool InventoryAnyEquipButton()
	{
		if (!InputHold(InputKey.Inventory1) && !InputHold(InputKey.Inventory2))
		{
			return InputHold(InputKey.Inventory3);
		}
		return true;
	}

	public static bool InventoryAnyEquipButtonUp()
	{
		if (!InputUp(InputKey.Inventory1) && !InputUp(InputKey.Inventory2))
		{
			return InputUp(InputKey.Inventory3);
		}
		return true;
	}

	public static bool InventoryAnyEquipButtonDown()
	{
		if (!InputDown(InputKey.Inventory1) && !InputDown(InputKey.Inventory2))
		{
			return InputDown(InputKey.Inventory3);
		}
		return true;
	}

	public static bool LevelGenDone()
	{
		return LevelGenerator.Instance.Generated;
	}

	public static bool RunIsLobbyMenu()
	{
		return IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelLobbyMenu);
	}

	public static bool RunIsShop()
	{
		return IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelShop);
	}

	public static bool RunIsLobby()
	{
		return IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelLobby);
	}

	public static bool RunIsTutorial()
	{
		return IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelTutorial);
	}

	public static bool RunIsRecording()
	{
		return IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelRecording);
	}

	public static bool RunIsLevel()
	{
		if (!IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelShop) && !IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelLobby) && !IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelLobbyMenu) && !IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelMainMenu) && !IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelSplashScreen) && !IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelTutorial))
		{
			return !IsCurrentLevel(RunManager.instance.levelCurrent, RunManager.instance.levelArena);
		}
		return false;
	}

	public static bool Photosensitivity()
	{
		return GameplayManager.instance.photosensitivity;
	}

	public static bool Arachnophobia()
	{
		return GameplayManager.instance.arachnophobia;
	}

	public static bool IsSpectating()
	{
		return SpectateCamera.instance;
	}

	public static T Singleton<T>(ref T instance, GameObject gameObject) where T : Component
	{
		Debug.Log("Singleton called for type " + typeof(T).Name + " on GameObject " + gameObject.name);
		if (instance == null)
		{
			Debug.Log("No existing instance found, setting up new instance of " + typeof(T).Name);
			instance = gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
			Debug.Log("DontDestroyOnLoad called for " + gameObject.name);
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
		else if (instance.gameObject != gameObject)
		{
			Debug.Log("Instance already exists for type " + typeof(T).Name + ", destroying game object " + gameObject.name);
			UnityEngine.Object.Destroy(gameObject);
		}
		else
		{
			Debug.Log("Instance matches the current gameObject " + gameObject.name + ", no action needed");
		}
		Debug.Log("Singleton setup completed for type " + typeof(T).Name + " on GameObject " + gameObject.name);
		return instance;
	}

	public static void StatSetBattery(string itemName, int value)
	{
		StatsManager.instance.ItemUpdateStatBattery(itemName, value);
	}

	public static int StatSetRunLives(int value)
	{
		return PunManager.instance.SetRunStatSet("lives", value);
	}

	public static int StatSetRunCurrency(int value)
	{
		return PunManager.instance.SetRunStatSet("currency", value);
	}

	public static int StatSetRunTotalHaul(int value)
	{
		return PunManager.instance.SetRunStatSet("totalHaul", value);
	}

	public static int StatSetRunLevel(int value)
	{
		return PunManager.instance.SetRunStatSet("level", value);
	}

	public static int StatSetSaveLevel(int value)
	{
		return PunManager.instance.SetRunStatSet("save level", value);
	}

	public static int StatSetRunFailures(int value)
	{
		return PunManager.instance.SetRunStatSet("failures", value);
	}

	public static int StatGetItemBattery(string itemName)
	{
		return StatsManager.instance.itemStatBattery[itemName];
	}

	public static int StatGetItemsPurchased(string itemName)
	{
		return StatsManager.instance.itemsPurchased[itemName];
	}

	public static int StatGetRunCurrency()
	{
		return StatsManager.instance.GetRunStatCurrency();
	}

	public static int StatGetRunTotalHaul()
	{
		return StatsManager.instance.GetRunStatTotalHaul();
	}

	public static int StatUpgradeItemBattery(string itemName)
	{
		return PunManager.instance.UpgradeItemBattery(itemName);
	}

	public static void StatSyncAll()
	{
		StatsManager.instance.statsSynced = false;
		PunManager.instance.SyncAllDictionaries();
	}

	public static bool StatsSynced()
	{
		return StatsManager.instance.statsSynced;
	}

	public static void ShopPopulateItemVolumes()
	{
		PunManager.instance.ShopPopulateItemVolumes();
	}

	public static int ShopGetTotalCost()
	{
		return ShopManager.instance.totalCost;
	}

	public static void ShopUpdateCost()
	{
		PunManager.instance.ShopUpdateCost();
	}

	public static void OnLevelGenDone()
	{
		ItemManager.instance.TurnOffIconLightsAgain();
		if (RunIsLobby())
		{
			TutorialDirector.instance.TipsShow();
		}
	}

	public static void OnSceneSwitch(bool _gameOver, bool _leaveGame)
	{
		ItemManager.instance.itemIconLights.SetActive(value: true);
		if (IsMultiplayer())
		{
			ChatManager.instance.ClearAllChatBatches();
		}
		if (RunIsLobby())
		{
			TutorialDirector instance = TutorialDirector.instance;
			if ((bool)instance)
			{
				instance.UpdateRoundEnd();
				instance.TipsStore();
			}
		}
		StatsManager.instance.StuffNeedingResetAtTheEndOfAScene();
		TutorialDirector.instance.TipCancel();
		ItemManager.instance.FetchLocalPlayersInventory();
		ItemManager.instance.powerCrystals.Clear();
		if ((bool)ChargingStation.instance && !_gameOver && !RunManager.instance.levelIsShop)
		{
			PunManager.instance.SetRunStatSet("chargingStationChargeTotal", ChargingStation.instance.chargeTotal);
		}
		if (IsMasterClientOrSingleplayer() && !_leaveGame && !_gameOver && RunManager.instance.levelPrevious != RunManager.instance.levelMainMenu && RunManager.instance.levelPrevious != RunManager.instance.levelLobbyMenu && RunManager.instance.levelPrevious != RunManager.instance.levelRecording)
		{
			SaveFileSave();
		}
		DataDirector.instance.SaveDeleteCheck(_leaveGame);
		if (!_leaveGame)
		{
			StatSyncAll();
		}
		if (_leaveGame)
		{
			SessionManager.instance.Reset();
		}
		if (!_leaveGame && !_gameOver)
		{
			RunManager.instance.UpdateMoonLevel();
		}
	}

	public static PlayerAvatar PlayerAvatarGetFromPhotonID(int photonID)
	{
		PlayerAvatar result = null;
		foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
		{
			if (player.photonView.ViewID == photonID)
			{
				result = player;
				break;
			}
		}
		return result;
	}

	public static PlayerAvatar PlayerAvatarGetFromSteamID(string _steamID)
	{
		PlayerAvatar result = null;
		if (IsMultiplayer())
		{
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				if (player.steamID == _steamID)
				{
					result = player;
				}
			}
		}
		if (!IsMultiplayer())
		{
			result = PlayerAvatar.instance;
		}
		return result;
	}

	public static PlayerAvatar PlayerAvatarGetFromSteamIDshort(int _steamIDshort)
	{
		PlayerAvatar result = null;
		if (IsMultiplayer())
		{
			foreach (PlayerAvatar player in GameDirector.instance.PlayerList)
			{
				if (player.steamIDshort == _steamIDshort)
				{
					result = player;
				}
			}
		}
		if (!IsMultiplayer())
		{
			result = PlayerAvatar.instance;
		}
		return result;
	}

	public static PlayerAvatar PlayerAvatarLocal()
	{
		return PlayerAvatar.instance;
	}

	public static string PlayerGetName(PlayerAvatar player)
	{
		if (IsMultiplayer())
		{
			return player.photonView.Owner.NickName;
		}
		return SteamClient.Name;
	}

	public static string PlayerGetSteamID(PlayerAvatar player)
	{
		return player.steamID;
	}

	public static void TruckPopulateItemVolumes()
	{
		PunManager.instance.TruckPopulateItemVolumes();
	}

	public static void LevelSuccessful()
	{
	}

	public static Quaternion SpringQuaternionGet(SpringQuaternion _attributes, Quaternion _targetRotation, float _deltaTime = -1f)
	{
		if (_deltaTime == -1f)
		{
			_deltaTime = Time.deltaTime;
		}
		if (!_attributes.setup)
		{
			_attributes.lastRotation = _targetRotation;
			_attributes.setup = true;
		}
		if (float.IsNaN(_attributes.springVelocity.x))
		{
			_attributes.springVelocity = Vector3.zero;
			_attributes.lastRotation = _targetRotation;
		}
		_targetRotation = Quaternion.RotateTowards(_attributes.lastRotation, _targetRotation, 360f);
		Quaternion quaternion = _targetRotation;
		Quaternion currentX = _attributes.lastRotation * Conjugate(quaternion);
		Vector3 zero = Vector3.zero;
		DampedSpringGeneralSolution(out var _newX, out var _newV, currentX, _attributes.springVelocity - zero, _deltaTime, _attributes.damping, _attributes.speed);
		float magnitude = _newV.magnitude;
		if (magnitude * Time.deltaTime > MathF.PI)
		{
			_newV *= MathF.PI / magnitude;
		}
		_attributes.springVelocity = _newV + zero;
		_attributes.lastRotation = _newX * quaternion;
		if (_attributes.clamp && Quaternion.Angle(_attributes.lastRotation, _targetRotation) > _attributes.maxAngle)
		{
			_attributes.lastRotation = Quaternion.RotateTowards(_targetRotation, _attributes.lastRotation, _attributes.maxAngle);
		}
		return _attributes.lastRotation;
	}

	public static float SpringFloatGet(SpringFloat _attributes, float _targetFloat, float _deltaTime = -1f)
	{
		if (_deltaTime == -1f)
		{
			_deltaTime = Time.deltaTime;
		}
		float currentX = _attributes.lastPosition - _targetFloat;
		DampedSpringGeneralSolution(out var _newX, out var _newV, currentX, _attributes.springVelocity, _deltaTime, _attributes.damping, _attributes.speed);
		float num = _newX;
		_attributes.springVelocity = _newV;
		_attributes.lastPosition = _targetFloat + num;
		if (_attributes.clamp)
		{
			float lastPosition = _attributes.lastPosition;
			_attributes.lastPosition = Mathf.Clamp(_attributes.lastPosition, _attributes.min, _attributes.max);
			if (lastPosition != _attributes.lastPosition)
			{
				_attributes.springVelocity *= -1f;
			}
		}
		return _attributes.lastPosition;
	}

	public static Vector3 SpringVector3Get(SpringVector3 _attributes, Vector3 _targetPosition, float _deltaTime = -1f)
	{
		if (_deltaTime == -1f)
		{
			_deltaTime = Time.deltaTime;
		}
		Vector3 vector = _attributes.lastPosition - _targetPosition;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < 3; i++)
		{
			DampedSpringGeneralSolution(out var _newX, out var _newV, vector[i], _attributes.springVelocity[i], _deltaTime, _attributes.damping, _attributes.speed);
			zero[i] = _newX;
			_attributes.springVelocity[i] = _newV;
		}
		_attributes.lastPosition = _targetPosition + zero;
		if (_attributes.clamp && Vector3.Distance(_attributes.lastPosition, _targetPosition) > _attributes.maxDistance)
		{
			_attributes.lastPosition = _targetPosition + (_attributes.lastPosition - _targetPosition).normalized * _attributes.maxDistance;
		}
		return _targetPosition + zero;
	}

	public static void DampedSpringGeneralSolution(out float _newX, out float _newV, float _currentX, float _currentV, float _time, float _criticality, float _naturalFrequency)
	{
		if (_criticality < 0f)
		{
			_criticality = 0f;
		}
		if (_naturalFrequency <= 0f)
		{
			_naturalFrequency = 1f;
		}
		if (_criticality == 1f)
		{
			float num = _naturalFrequency * _time;
			float num2 = Mathf.Exp(0f - num);
			float num3 = _currentV + _naturalFrequency * _currentX;
			_newX = num2 * (_currentX + num3 * _time);
			_newV = num2 * (num3 * (1f - num) - _naturalFrequency * _currentX);
		}
		else if (_criticality < 1f)
		{
			float num4 = _naturalFrequency * Mathf.Sqrt(1f - _criticality * _criticality);
			float num5 = _criticality * _naturalFrequency;
			float num6 = 1f / num4 * (num5 * _currentX + _currentV);
			float num7 = Mathf.Exp((0f - num5) * _time);
			float f = num4 * _time;
			float num8 = Mathf.Cos(f);
			float num9 = Mathf.Sin(f);
			_newX = num7 * (_currentX * num8 + num6 * num9);
			_newV = num7 * (num8 * (num6 * num4 - num5 * _currentX) - num9 * (_currentX * num4 + num6 * num5));
		}
		else
		{
			float num10 = Mathf.Sqrt(_criticality * _criticality - 1f);
			float num11 = _naturalFrequency * (num10 - _criticality);
			float num12 = (0f - _naturalFrequency) * (num10 + _criticality);
			float num13 = (num11 * _currentX - _currentV) / (num11 - num12);
			float num14 = _currentX - num13;
			float num15 = Mathf.Exp(num11 * _time);
			float num16 = Mathf.Exp(num12 * _time);
			float num17 = num14 * num15;
			float num18 = num13 * num16;
			_newX = num17 + num18;
			_newV = num11 * num17 + num12 * num18;
		}
	}

	public static void DampedSpringGeneralSolution(out Quaternion _newX, out Vector3 _newV, Quaternion _currentX, Vector3 _currentV, float _time, float _criticality, float _naturalFrequency)
	{
		if (_criticality < 0f)
		{
			_criticality = 0f;
		}
		if (_naturalFrequency <= 0f)
		{
			_naturalFrequency = 1f;
		}
		if (_criticality == 1f)
		{
			float num = _naturalFrequency * _time;
			float num2 = Mathf.Exp(0f - num);
			Vector3 vector = _currentV + ToAngularVelocity(_currentX, 1f / _naturalFrequency);
			_newX = QuaternionScale(ToQuaternionFromAngularVelocityAndTime(vector, _time) * _currentX, num2);
			_newV = num2 * (vector * (1f - num) - ToAngularVelocity(_currentX, 1f / _naturalFrequency));
		}
		else if (_criticality < 1f)
		{
			float num3 = _naturalFrequency * Mathf.Sqrt(1f - _criticality * _criticality);
			float num4 = _criticality * _naturalFrequency;
			Vector3 vector2 = 1f / num3 * (ToAngularVelocity(_currentX, 1f / num4) + _currentV);
			float num5 = Mathf.Exp((0f - num4) * _time);
			float f = num3 * _time;
			float num6 = Mathf.Cos(f);
			float num7 = Mathf.Sin(f);
			_newX = QuaternionScale(ToQuaternionFromAngularVelocityAndTime(vector2, num7) * QuaternionScale(_currentX, num6), num5);
			_newV = num5 * (num6 * (vector2 * num3 - ToAngularVelocity(_currentX, 1f / num4)) - num7 * (ToAngularVelocity(_currentX, 1f / num3) + vector2 * num4));
		}
		else
		{
			float num8 = Mathf.Sqrt(_criticality * _criticality - 1f);
			float num9 = _naturalFrequency * (num8 - _criticality);
			float num10 = (0f - _naturalFrequency) * (num8 + _criticality);
			Vector3 vector3 = (ToAngularVelocity(_currentX, 1f / num9) - _currentV) / (num9 - num10);
			Quaternion quaternion = _currentX * Conjugate(ToQuaternionFromAngularVelocityAndTime(vector3, 1f));
			float num11 = Mathf.Exp(num9 * _time);
			float num12 = Mathf.Exp(num10 * _time);
			Quaternion quaternion2 = QuaternionScale(quaternion, num11);
			Vector3 vector4 = ToAngularVelocity(quaternion, 1f / num11);
			Vector3 vector5 = vector3 * num12;
			Quaternion quaternion3 = ToQuaternionFromAngularVelocityAndTime(vector3, num12);
			_newX = quaternion3 * quaternion2;
			_newV = num9 * vector4 + num10 * vector5;
		}
	}

	public static Vector3 ToAngularVelocity(Quaternion _dQ, float _dT)
	{
		ToAngleAndAxis(out var _angleRadians, out var _axis, _dQ);
		return _angleRadians / _dT * _axis;
	}

	public static void ToAngleAndAxis(out float _angleRadians, out Vector3 _axis, Quaternion _Q)
	{
		float num = Mathf.Sqrt(Quaternion.Dot(_Q, _Q));
		_Q.x /= num;
		_Q.y /= num;
		_Q.z /= num;
		_Q.w /= num;
		_axis = new Vector3(_Q.x, _Q.y, _Q.z);
		float magnitude = _axis.magnitude;
		if (Mathf.Abs(_Q.w) > 0.99f)
		{
			_angleRadians = 2f * Mathf.Asin(magnitude);
			if (magnitude == 0f)
			{
				_axis = new Vector3(1f, 0f, 0f);
			}
			else
			{
				_axis /= magnitude;
			}
		}
		else
		{
			_angleRadians = 2f * Mathf.Acos(_Q.w);
			_axis /= magnitude;
		}
	}

	public static Quaternion Conjugate(Quaternion q)
	{
		return new Quaternion(0f - q.x, 0f - q.y, 0f - q.z, q.w);
	}

	public static Quaternion QuaternionScale(Quaternion _Q, float _power)
	{
		ToAngleAndAxis(out var _angleRadians, out var _axis, _Q);
		return ToQuaternion(_angleRadians * _power, _axis);
	}

	public static Quaternion ToQuaternion(float _angleRadians, Vector3 _axis)
	{
		Vector3 normalized = _axis.normalized;
		float num = Mathf.Sin(_angleRadians * 0.5f);
		return new Quaternion(normalized.x * num, normalized.y * num, normalized.z * num, Mathf.Cos(_angleRadians * 0.5f));
	}

	public static Quaternion ToQuaternionFromAngularVelocityAndTime(Vector3 _omega, float _time)
	{
		float num = _omega.magnitude * _time;
		if (Mathf.Abs(num) > 1E-15f)
		{
			Vector3 normalized = _omega.normalized;
			float num2 = Mathf.Sin(num * 0.5f);
			return new Quaternion(normalized.x * num2, normalized.y * num2, normalized.z * num2, Mathf.Cos(num * 0.5f));
		}
		return Quaternion.identity;
	}

	public static void Log(object message, GameObject gameObject, Color? color = null)
	{
	}

	public static void DoNotLookEffect(GameObject _gameObject, bool _vignette = true, bool _zoom = true, bool _saturation = true, bool _contrast = true, bool _shake = true, bool _glitch = true)
	{
		float speedIn = 3f;
		float speedOut = 1f;
		if (_vignette)
		{
			PostProcessing.Instance.VignetteOverride(new Color(0.16f, 0.2f, 0.26f), 0.5f, 1f, speedIn, speedOut, 0.1f, _gameObject);
		}
		if (_zoom)
		{
			CameraZoom.Instance.OverrideZoomSet(65f, 0.1f, speedIn, speedOut, _gameObject, 150);
		}
		if (_saturation)
		{
			PostProcessing.Instance.SaturationOverride(-25f, speedIn, speedOut, 0.1f, _gameObject);
		}
		if (_contrast)
		{
			PostProcessing.Instance.ContrastOverride(10f, speedIn, speedOut, 0.1f, _gameObject);
		}
		if (_shake)
		{
			GameDirector.instance.CameraImpact.Shake(15f * Time.deltaTime, 0.1f);
			GameDirector.instance.CameraShake.Shake(15f * Time.deltaTime, 1f);
		}
		if (_glitch)
		{
			CameraGlitch.Instance.DoNotLookEffectSet();
		}
	}

	public static void CameraOverrideStopAim()
	{
		CameraAim.Instance.OverrideAimStop();
	}

	public static ExtractionPoint ExtractionPointGetNearest(Vector3 position)
	{
		ExtractionPoint result = null;
		float num = float.PositiveInfinity;
		foreach (GameObject extractionPoint in RoundDirector.instance.extractionPointList)
		{
			float num2 = Vector3.Distance(position, extractionPoint.transform.position);
			if (num2 < num)
			{
				num = num2;
				result = extractionPoint.GetComponent<ExtractionPoint>();
			}
		}
		return result;
	}

	public static ExtractionPoint ExtractionPointGetNearestNotActivated(Vector3 position)
	{
		ExtractionPoint result = null;
		float num = float.PositiveInfinity;
		foreach (GameObject extractionPoint in RoundDirector.instance.extractionPointList)
		{
			if (extractionPoint.GetComponent<ExtractionPoint>().currentState == ExtractionPoint.State.Idle)
			{
				float num2 = Vector3.Distance(position, extractionPoint.transform.position);
				if (num2 < num)
				{
					num = num2;
					result = extractionPoint.GetComponent<ExtractionPoint>();
				}
			}
		}
		return result;
	}

	public static float Remap(float origFrom, float origTo, float targetFrom, float targetTo, float value)
	{
		float t = Mathf.InverseLerp(origFrom, origTo, value);
		return Mathf.Lerp(targetFrom, targetTo, t);
	}

	public static bool InputDown(InputKey key)
	{
		if (Application.isEditor && (key == InputKey.Back || key == InputKey.Menu))
		{
			key = InputKey.BackEditor;
		}
		return InputManager.instance.KeyDown(key);
	}

	public static bool InputUp(InputKey key)
	{
		return InputManager.instance.KeyUp(key);
	}

	public static bool InputHold(InputKey key)
	{
		return InputManager.instance.KeyHold(key);
	}

	public static Vector2 InputMousePosition()
	{
		return InputManager.instance.GetMousePosition();
	}

	public static Vector2 InputMovement()
	{
		return InputManager.instance.GetMovement();
	}

	public static float InputMovementX()
	{
		return InputManager.instance.GetMovementX();
	}

	public static float InputMovementY()
	{
		return InputManager.instance.GetMovementY();
	}

	public static float InputScrollY()
	{
		return InputManager.instance.GetScrollY();
	}

	public static float InputMouseX()
	{
		return InputManager.instance.GetMouseX();
	}

	public static float InputMouseY()
	{
		return InputManager.instance.GetMouseY();
	}

	public static void InputDisableMovement()
	{
		InputManager.instance.DisableMovement();
	}

	public static void InputDisableAiming()
	{
		InputManager.instance.DisableAiming();
	}

	public static bool NoTextInputsActive()
	{
		if ((!ChatManager.instance || !ChatManager.instance.StateIsActive()) && (!MenuManager.instance || !MenuManager.instance.textInputActive))
		{
			if ((bool)DebugConsoleUI.instance)
			{
				return !DebugConsoleUI.instance.chatActive;
			}
			return true;
		}
		return false;
	}

	public static void OpenFile(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return;
		}
		path = Path.GetFullPath(path);
		if (!Directory.Exists(path) && !File.Exists(path))
		{
			return;
		}
		try
		{
			Application.OpenURL("file:///" + path.Replace("\\", "/"));
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to open file: " + ex);
		}
	}

	public static bool GetRoomVolumeAtPosition(Vector3 worldPosition, out RoomVolume room, out Vector3 localPosition)
	{
		room = null;
		localPosition = default(Vector3);
		RoomVolume roomVolume = null;
		Collider[] array = Physics.OverlapSphere(worldPosition, 0.1f, LayerMask.GetMask("RoomVolume"), QueryTriggerInteraction.Collide);
		if (array != null && array.Length != 0)
		{
			Collider[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RoomVolume componentInParent = array2[i].GetComponentInParent<RoomVolume>();
				if ((bool)componentInParent)
				{
					roomVolume = componentInParent;
					break;
				}
			}
		}
		if (!roomVolume)
		{
			return false;
		}
		room = roomVolume;
		Transform transform = (room.Module ? room.Module.transform : room.transform);
		localPosition = transform.InverseTransformPoint(worldPosition);
		return true;
	}

	public static bool GetModuleAtPosition(Vector3 worldPosition, out Module module, out Vector3 localPosition)
	{
		module = null;
		localPosition = default(Vector3);
		Module module2 = null;
		Collider[] array = Physics.OverlapSphere(worldPosition, 0.5f, LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide);
		if (array != null && array.Length != 0)
		{
			Collider[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Module componentInParent = array2[i].GetComponentInParent<Module>();
				if ((bool)componentInParent)
				{
					module2 = componentInParent;
					break;
				}
			}
		}
		if (!module2)
		{
			return false;
		}
		module = module2;
		localPosition = module.transform.InverseTransformPoint(worldPosition);
		return true;
	}

	public static void PhysFollowPoint(Rigidbody rb, Vector3 targetPos, float spring, float damping, float maxSpeed = float.PositiveInfinity, float maxAccel = float.PositiveInfinity, float stopDistance = 0f)
	{
		if (!rb || rb.isKinematic)
		{
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		if (fixedDeltaTime <= 0f || rb == null)
		{
			return;
		}
		Vector3 vector = targetPos - rb.position;
		if (stopDistance > 0f && vector.sqrMagnitude <= stopDistance * stopDistance)
		{
			rb.velocity *= Mathf.Clamp01(1f - damping * fixedDeltaTime);
			return;
		}
		Vector3 vector2 = vector * spring - rb.velocity * damping;
		if (maxAccel < float.PositiveInfinity)
		{
			vector2 = Vector3.ClampMagnitude(vector2, maxAccel);
		}
		Vector3 vector3 = rb.velocity + vector2 * fixedDeltaTime;
		if (maxSpeed < float.PositiveInfinity)
		{
			vector3 = Vector3.ClampMagnitude(vector3, maxSpeed);
		}
		rb.velocity = vector3;
	}

	public static void PhysFollowRotationTorque(Rigidbody rb, Quaternion targetRotation, float torqueSpring, float torqueDamping, float maxAngularAccel = float.PositiveInfinity, float maxAngularSpeed = 20f, float stopAngleDeg = 0f)
	{
		if (!rb || rb.isKinematic)
		{
			return;
		}
		if (maxAngularSpeed > 0f)
		{
			rb.maxAngularVelocity = Mathf.Max(rb.maxAngularVelocity, maxAngularSpeed);
		}
		Quaternion quaternion = targetRotation * Quaternion.Inverse(rb.rotation);
		quaternion.Normalize();
		if (quaternion.w < 0f)
		{
			quaternion.x = 0f - quaternion.x;
			quaternion.y = 0f - quaternion.y;
			quaternion.z = 0f - quaternion.z;
			quaternion.w = 0f - quaternion.w;
		}
		quaternion.ToAngleAxis(out var angle, out var axis);
		if (axis.sqrMagnitude < 1E-12f)
		{
			return;
		}
		if (angle > 180f)
		{
			angle -= 360f;
		}
		if (stopAngleDeg > 0f && Mathf.Abs(angle) <= stopAngleDeg)
		{
			float fixedDeltaTime = Time.fixedDeltaTime;
			rb.angularVelocity *= Mathf.Clamp01(1f - torqueDamping * fixedDeltaTime);
			return;
		}
		float num = angle * (MathF.PI / 180f);
		Vector3 vector = axis.normalized * num * torqueSpring - rb.angularVelocity * torqueDamping;
		if (maxAngularAccel < float.PositiveInfinity)
		{
			vector = Vector3.ClampMagnitude(vector, maxAngularAccel);
		}
		rb.AddTorque(vector, ForceMode.Acceleration);
	}

	public static bool IsCurrentLevel(Level level, Level level2)
	{
		if (!level)
		{
			level = RunManager.instance?.levelCurrent;
		}
		if ((bool)level)
		{
			return level == level2;
		}
		return false;
	}
}
