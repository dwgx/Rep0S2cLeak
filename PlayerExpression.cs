using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerExpression : MonoBehaviour
{
	public class OverrideExpression
	{
		public int index;

		public float percent;

		public float timer;
	}

	public class ToggleExpressionInput
	{
		public int index;

		public bool active;

		public InputKey inputKey;
	}

	public PlayerAvatar playerAvatar;

	public bool onlyVisualRepresentation;

	public GameObject eyelidLeft;

	public GameObject eyelidRight;

	public Transform eyelidLeftScale;

	public Transform eyelidRightScale;

	public Transform leftUpperEyelidRotationX;

	public Transform leftUpperEyeLidRotationZ;

	public Transform leftLowerEyelidRotationX;

	public Transform leftLowerEyelidRotationZ;

	public Transform rightUpperEyelidRotationX;

	public Transform rightUpperEyelidRotationZ;

	public Transform rightLowerEyelidRotationX;

	public Transform rightLowerEyelidRotationZ;

	public Transform pupilLeftScale;

	internal float pupilLeftScaleAmount = 1f;

	public Transform pupilLeftRotationOffset;

	public Transform pupilRightScale;

	internal float pupilRightScaleAmount = 1f;

	public Transform pupilRightRotationOffset;

	private bool isExpressing;

	[Header("Expression Settings (set percentages here)")]
	public List<ExpressionSettings> expressions;

	private EyeSettings blendedLeftEye;

	private EyeSettings blendedRightEye;

	private float blendedHeadTilt;

	private SpringFloat leftLidsScale;

	private SpringFloat rightLidsScale;

	private SpringFloat leftUpperLidClosedAngle;

	private SpringFloat leftLowerLidClosedAngle;

	private SpringFloat rightUpperLidClosedAngle;

	private SpringFloat rightLowerLidClosedAngle;

	private SpringFloat leftUpperLidRotationZSpring;

	private SpringFloat rightUpperLidRotationZSpring;

	private SpringFloat leftLowerLidRotationZSpring;

	private SpringFloat rightLowerLidRotationZSpring;

	private SpringFloat leftPupilSizeSpring;

	private SpringFloat rightPupilSizeSpring;

	private PlayerAvatarVisuals playerVisuals;

	private List<int> activeExpressions = new List<int>();

	private bool isLocal;

	private List<OverrideExpression> overrideExpressions = new List<OverrideExpression>();

	private List<ToggleExpressionInput> inputToggleList = new List<ToggleExpressionInput>();

	private List<ToggleExpressionInput> inputToggleListNew = new List<ToggleExpressionInput>();

	private float inputToggleListNewTimer;

	private void Start()
	{
		playerVisuals = GetComponent<PlayerAvatarVisuals>();
		if (!playerAvatar)
		{
			playerAvatar = PlayerAvatar.instance;
		}
		if ((bool)playerAvatar && playerAvatar.isLocal)
		{
			isLocal = true;
		}
		leftUpperLidClosedAngle = new SpringFloat();
		leftUpperLidClosedAngle.damping = 0.5f;
		leftUpperLidClosedAngle.speed = 20f;
		leftLowerLidClosedAngle = new SpringFloat();
		leftLowerLidClosedAngle.damping = 0.5f;
		leftLowerLidClosedAngle.speed = 20f;
		rightUpperLidClosedAngle = new SpringFloat();
		rightUpperLidClosedAngle.damping = 0.5f;
		rightUpperLidClosedAngle.speed = 20f;
		rightLowerLidClosedAngle = new SpringFloat();
		rightLowerLidClosedAngle.damping = 0.5f;
		rightLowerLidClosedAngle.speed = 20f;
		leftUpperLidRotationZSpring = new SpringFloat();
		leftUpperLidRotationZSpring.damping = 0.5f;
		leftUpperLidRotationZSpring.speed = 20f;
		rightUpperLidRotationZSpring = new SpringFloat();
		rightUpperLidRotationZSpring.damping = 0.5f;
		rightUpperLidRotationZSpring.speed = 20f;
		leftLowerLidRotationZSpring = new SpringFloat();
		leftLowerLidRotationZSpring.damping = 0.5f;
		leftLowerLidRotationZSpring.speed = 20f;
		rightLowerLidRotationZSpring = new SpringFloat();
		rightLowerLidRotationZSpring.damping = 0.5f;
		rightLowerLidRotationZSpring.speed = 20f;
		leftLidsScale = new SpringFloat();
		leftLidsScale.damping = 0.5f;
		leftLidsScale.speed = 20f;
		rightLidsScale = new SpringFloat();
		rightLidsScale.damping = 0.5f;
		rightLidsScale.speed = 20f;
		leftPupilSizeSpring = new SpringFloat();
		leftPupilSizeSpring.damping = 0.5f;
		leftPupilSizeSpring.speed = 20f;
		rightPupilSizeSpring = new SpringFloat();
		rightPupilSizeSpring.damping = 0.5f;
		rightPupilSizeSpring.speed = 20f;
		pupilRightScale.localScale = Vector3.one;
		pupilLeftScale.localScale = Vector3.one;
		inputToggleList.Add(new ToggleExpressionInput
		{
			index = 1,
			active = false,
			inputKey = InputKey.Expression1
		});
		inputToggleList.Add(new ToggleExpressionInput
		{
			index = 2,
			active = false,
			inputKey = InputKey.Expression2
		});
		inputToggleList.Add(new ToggleExpressionInput
		{
			index = 3,
			active = false,
			inputKey = InputKey.Expression3
		});
		inputToggleList.Add(new ToggleExpressionInput
		{
			index = 4,
			active = false,
			inputKey = InputKey.Expression4
		});
		inputToggleList.Add(new ToggleExpressionInput
		{
			index = 5,
			active = false,
			inputKey = InputKey.Expression5
		});
		inputToggleList.Add(new ToggleExpressionInput
		{
			index = 6,
			active = false,
			inputKey = InputKey.Expression6
		});
	}

	private void DoExpression(int _index, float _percent, bool _playerInput = false)
	{
		if (_playerInput)
		{
			PlayerExpressionsUI.instance.ShrinkReset();
			TutorialDirector.instance.playerUsedExpression = true;
		}
		activeExpressions.Add(_index);
		expressions[_index].weight = Mathf.Lerp(expressions[_index].weight, _percent, playerVisuals.deltaTime * 5f);
		expressions[_index].timer = 0.2f;
		if (!expressions[_index].isExpressing && !onlyVisualRepresentation)
		{
			playerAvatar.PlayerExpressionSet(_index, _percent);
		}
		expressions[_index].isExpressing = true;
	}

	public void OverrideExpressionSet(int _index, float _percent)
	{
		foreach (OverrideExpression overrideExpression2 in overrideExpressions)
		{
			if (_index == overrideExpression2.index)
			{
				overrideExpression2.percent = _percent;
				overrideExpression2.timer = 0.2f;
				return;
			}
		}
		OverrideExpression overrideExpression = new OverrideExpression();
		overrideExpression.index = _index;
		overrideExpression.percent = _percent;
		overrideExpression.timer = 0.2f;
		overrideExpressions.Add(overrideExpression);
	}

	private void ResetExpressions(int _index)
	{
		if (expressions[_index].isExpressing && !onlyVisualRepresentation)
		{
			playerAvatar.PlayerExpressionSet(_index, 0f);
		}
		expressions[_index].isExpressing = false;
	}

	private void ExpressionTimerTick()
	{
		isExpressing = false;
		foreach (ExpressionSettings expression in expressions)
		{
			if (expression.weight > 0f)
			{
				expression.weight = Mathf.Lerp(expression.weight, 0f, playerVisuals.deltaTime * 5f);
			}
			if (expression.timer > 0f)
			{
				expression.timer -= playerVisuals.deltaTime;
				if (expression.timer <= 0f)
				{
					StopExpression(expressions.IndexOf(expression));
				}
				isExpressing = true;
			}
		}
	}

	private void StopExpression(int _index)
	{
		playerAvatar.PlayerExpressionStop(_index);
		expressions[_index].isExpressing = false;
	}

	private void ToggleExpression(int _index)
	{
		TutorialDirector.instance.playerUsedExpression = true;
		foreach (ToggleExpressionInput inputToggle in inputToggleList)
		{
			if (inputToggle.index == _index)
			{
				inputToggleListNew.Add(inputToggle);
				inputToggleListNewTimer = 0.1f;
			}
		}
	}

	private void Update()
	{
		if (!LevelGenerator.Instance.Generated && !SemiFunc.MenuLevel())
		{
			DoExpression(1, 100f);
		}
		if (!playerAvatar)
		{
			if (playerVisuals.expressionAvatar)
			{
				playerVisuals.animator.Play("Crouch", 0, 0f);
			}
			playerAvatar = PlayerAvatar.instance;
			isLocal = true;
		}
		List<int> list = new List<int>();
		foreach (int activeExpression in activeExpressions)
		{
			list.Add(activeExpression);
		}
		activeExpressions.Clear();
		if (isLocal)
		{
			if (!MenuManager.instance.currentMenuPage)
			{
				foreach (OverrideExpression overrideExpression in overrideExpressions)
				{
					DoExpression(overrideExpression.index, overrideExpression.percent);
				}
				if (!InputManager.instance.InputToggleGet(InputKey.Expression1))
				{
					if (SemiFunc.InputHold(InputKey.Expression1))
					{
						DoExpression(1, 100f, _playerInput: true);
					}
					if (SemiFunc.InputHold(InputKey.Expression2))
					{
						DoExpression(2, 100f, _playerInput: true);
					}
					if (SemiFunc.InputHold(InputKey.Expression3))
					{
						DoExpression(3, 100f, _playerInput: true);
					}
					if (SemiFunc.InputHold(InputKey.Expression4))
					{
						DoExpression(4, 100f, _playerInput: true);
					}
					if (SemiFunc.InputHold(InputKey.Expression5))
					{
						DoExpression(5, 100f, _playerInput: true);
					}
					if (SemiFunc.InputHold(InputKey.Expression6))
					{
						DoExpression(6, 100f, _playerInput: true);
					}
				}
				else
				{
					if (this == playerAvatar.playerExpression)
					{
						inputToggleListNewTimer -= Time.deltaTime;
						if (SemiFunc.InputDown(InputKey.Expression1))
						{
							ToggleExpression(1);
						}
						if (SemiFunc.InputDown(InputKey.Expression2))
						{
							ToggleExpression(2);
						}
						if (SemiFunc.InputDown(InputKey.Expression3))
						{
							ToggleExpression(3);
						}
						if (SemiFunc.InputDown(InputKey.Expression4))
						{
							ToggleExpression(4);
						}
						if (SemiFunc.InputDown(InputKey.Expression5))
						{
							ToggleExpression(5);
						}
						if (SemiFunc.InputDown(InputKey.Expression6))
						{
							ToggleExpression(6);
						}
						if (inputToggleListNewTimer <= 0f && inputToggleListNew.Count > 0)
						{
							PlayerExpressionsUI.instance.ShrinkReset();
							int num = 0;
							foreach (ToggleExpressionInput inputToggle in inputToggleList)
							{
								if (inputToggle.active)
								{
									num++;
								}
							}
							bool flag = true;
							foreach (ToggleExpressionInput inputToggle2 in inputToggleList)
							{
								foreach (ToggleExpressionInput item in inputToggleListNew)
								{
									if (item.index == inputToggle2.index && !inputToggle2.active)
									{
										flag = false;
										break;
									}
								}
								if (!flag)
								{
									break;
								}
							}
							if (flag)
							{
								foreach (ToggleExpressionInput inputToggle3 in inputToggleList)
								{
									if (!inputToggle3.active)
									{
										continue;
									}
									bool flag2 = true;
									foreach (ToggleExpressionInput item2 in inputToggleListNew)
									{
										if (item2.index == inputToggle3.index)
										{
											flag2 = false;
											break;
										}
									}
									if (flag2)
									{
										flag = false;
										break;
									}
								}
							}
							bool flag3 = false;
							foreach (ToggleExpressionInput inputToggle4 in inputToggleList)
							{
								bool flag4 = true;
								foreach (ToggleExpressionInput item3 in inputToggleListNew)
								{
									if (item3.index == inputToggle4.index)
									{
										flag4 = false;
										break;
									}
								}
								if (flag4)
								{
									if (!SemiFunc.InputHold(inputToggle4.inputKey))
									{
										inputToggle4.active = false;
									}
									else
									{
										flag3 = true;
									}
								}
							}
							if (inputToggleListNew.Count == 1)
							{
								foreach (ToggleExpressionInput inputToggle5 in inputToggleList)
								{
									foreach (ToggleExpressionInput item4 in inputToggleListNew)
									{
										if (item4.index == inputToggle5.index)
										{
											if (num > 1 && !flag3)
											{
												inputToggle5.active = true;
											}
											else
											{
												inputToggle5.active = !inputToggle5.active;
											}
										}
									}
								}
							}
							else
							{
								foreach (ToggleExpressionInput inputToggle6 in inputToggleList)
								{
									foreach (ToggleExpressionInput item5 in inputToggleListNew)
									{
										if (item5.index == inputToggle6.index)
										{
											if (flag)
											{
												inputToggle6.active = false;
											}
											else
											{
												inputToggle6.active = true;
											}
										}
									}
								}
							}
							inputToggleListNew.Clear();
						}
					}
					foreach (ToggleExpressionInput inputToggle7 in playerAvatar.playerExpression.inputToggleList)
					{
						if (inputToggle7.active)
						{
							DoExpression(inputToggle7.index, 100f);
						}
					}
				}
			}
		}
		else
		{
			foreach (KeyValuePair<int, float> playerExpression in playerAvatar.playerExpressions)
			{
				expressions[playerExpression.Key].weight = Mathf.Lerp(expressions[playerExpression.Key].weight, playerExpression.Value, playerVisuals.deltaTime * 5f);
				if (!expressions[playerExpression.Key].stopExpressing)
				{
					expressions[playerExpression.Key].isExpressing = true;
					expressions[playerExpression.Key].timer = 0.2f;
					activeExpressions.Add(playerExpression.Key);
				}
			}
		}
		foreach (OverrideExpression item6 in overrideExpressions.ToList())
		{
			if (item6.timer <= 0f)
			{
				overrideExpressions.Remove(item6);
			}
			else
			{
				item6.timer -= playerVisuals.deltaTime;
			}
		}
		bool flag5 = false;
		foreach (int activeExpression2 in activeExpressions)
		{
			if (!list.Contains(activeExpression2))
			{
				flag5 = true;
				break;
			}
		}
		if (!flag5)
		{
			foreach (int item7 in list)
			{
				if (!activeExpressions.Contains(item7))
				{
					flag5 = true;
					break;
				}
			}
		}
		if (flag5)
		{
			playerVisuals.HeadTiltImpulse(50f);
		}
		if (isExpressing)
		{
			if (playerVisuals.expressionAvatar)
			{
				playerVisuals.animator.SetBool("Crouching", value: false);
			}
			expressions[0].weight = Mathf.Lerp(expressions[0].weight, 0f, playerVisuals.deltaTime * 5f);
			blendedLeftEye = BlendEyeSettings(isLeft: true);
			blendedRightEye = BlendEyeSettings(isLeft: false);
			eyelidLeft.SetActive(value: true);
			eyelidRight.SetActive(value: true);
			blendedHeadTilt = 0f;
			float num2 = 0f;
			foreach (ExpressionSettings expression in expressions)
			{
				if (expression.isExpressing)
				{
					blendedHeadTilt += expression.headTiltAmount;
					num2 += 1f;
				}
			}
			if (num2 > 0f)
			{
				blendedHeadTilt /= num2;
			}
			playerVisuals.HeadTiltOverride(blendedHeadTilt);
			float num3 = SemiFunc.SpringFloatGet(leftLidsScale, 1f, playerVisuals.deltaTime);
			eyelidLeftScale.localScale = new Vector3(num3, num3, num3);
			num3 = SemiFunc.SpringFloatGet(rightLidsScale, 1f, playerVisuals.deltaTime);
			eyelidRightScale.localScale = new Vector3(num3, num3, num3);
			float x = SemiFunc.SpringFloatGet(leftUpperLidClosedAngle, blendedLeftEye.upperLidClosedPercent, playerVisuals.deltaTime);
			leftUpperEyelidRotationX.localRotation = Quaternion.Euler(x, 0f, 0f);
			float x2 = SemiFunc.SpringFloatGet(leftLowerLidClosedAngle, blendedLeftEye.lowerLidClosedPercent, playerVisuals.deltaTime);
			leftLowerEyelidRotationX.localRotation = Quaternion.Euler(x2, 0f, 0f);
			x = SemiFunc.SpringFloatGet(rightUpperLidClosedAngle, blendedRightEye.upperLidClosedPercent, playerVisuals.deltaTime);
			rightUpperEyelidRotationX.localRotation = Quaternion.Euler(x, 0f, 0f);
			x2 = SemiFunc.SpringFloatGet(rightLowerLidClosedAngle, blendedRightEye.lowerLidClosedPercent, playerVisuals.deltaTime);
			rightLowerEyelidRotationX.localRotation = Quaternion.Euler(x2, 0f, 0f);
			float z = SemiFunc.SpringFloatGet(leftUpperLidRotationZSpring, blendedLeftEye.upperLidAngle, playerVisuals.deltaTime);
			leftUpperEyeLidRotationZ.localRotation = Quaternion.Euler(0f, 0f, z);
			z = SemiFunc.SpringFloatGet(rightUpperLidRotationZSpring, blendedRightEye.upperLidAngle, playerVisuals.deltaTime);
			rightUpperEyelidRotationZ.localRotation = Quaternion.Euler(0f, 0f, z);
			float z2 = SemiFunc.SpringFloatGet(leftLowerLidRotationZSpring, blendedLeftEye.lowerLidAngle, playerVisuals.deltaTime);
			leftLowerEyelidRotationZ.localRotation = Quaternion.Euler(0f, 0f, z2);
			z2 = SemiFunc.SpringFloatGet(rightLowerLidRotationZSpring, blendedRightEye.lowerLidAngle, playerVisuals.deltaTime);
			rightLowerEyelidRotationZ.localRotation = Quaternion.Euler(0f, 0f, z2);
			pupilLeftScaleAmount = SemiFunc.SpringFloatGet(leftPupilSizeSpring, blendedLeftEye.pupilSize, playerVisuals.deltaTime);
			pupilRightScaleAmount = SemiFunc.SpringFloatGet(rightPupilSizeSpring, blendedRightEye.pupilSize, playerVisuals.deltaTime);
			if (isLocal && !playerAvatar.isDisabled)
			{
				PlayerExpressionsUI.instance.Show();
			}
		}
		else if (eyelidLeft.activeSelf)
		{
			blendedLeftEye = BlendEyeSettings(isLeft: true);
			blendedRightEye = BlendEyeSettings(isLeft: false);
			expressions[0].weight = Mathf.Lerp(expressions[0].weight, 100f, playerVisuals.deltaTime * 20f);
			expressions[1].weight = Mathf.Lerp(expressions[1].weight, 0f, playerVisuals.deltaTime * 20f);
			expressions[2].weight = Mathf.Lerp(expressions[2].weight, 0f, playerVisuals.deltaTime * 20f);
			if (expressions[0].weight > 50f)
			{
				if (playerVisuals.expressionAvatar)
				{
					playerVisuals.animator.SetBool("Crouching", value: true);
				}
				float num4 = SemiFunc.SpringFloatGet(leftLidsScale, 0.8f, playerVisuals.deltaTime);
				eyelidLeftScale.localScale = new Vector3(num4, num4, num4);
				num4 = SemiFunc.SpringFloatGet(rightLidsScale, 0.8f, playerVisuals.deltaTime);
				eyelidRightScale.localScale = new Vector3(num4, num4, num4);
			}
			float x3 = SemiFunc.SpringFloatGet(leftUpperLidClosedAngle, blendedLeftEye.upperLidClosedPercent, playerVisuals.deltaTime);
			leftUpperEyelidRotationX.localRotation = Quaternion.Euler(x3, 0f, 0f);
			float x4 = SemiFunc.SpringFloatGet(leftLowerLidClosedAngle, blendedLeftEye.lowerLidClosedPercent, playerVisuals.deltaTime);
			leftLowerEyelidRotationX.localRotation = Quaternion.Euler(x4, 0f, 0f);
			x3 = SemiFunc.SpringFloatGet(rightUpperLidClosedAngle, blendedRightEye.upperLidClosedPercent, playerVisuals.deltaTime);
			rightUpperEyelidRotationX.localRotation = Quaternion.Euler(x3, 0f, 0f);
			x4 = SemiFunc.SpringFloatGet(rightLowerLidClosedAngle, blendedRightEye.lowerLidClosedPercent, playerVisuals.deltaTime);
			rightLowerEyelidRotationX.localRotation = Quaternion.Euler(x4, 0f, 0f);
			float z3 = SemiFunc.SpringFloatGet(leftUpperLidRotationZSpring, blendedLeftEye.upperLidAngle, playerVisuals.deltaTime);
			leftUpperEyeLidRotationZ.localRotation = Quaternion.Euler(0f, 0f, z3);
			z3 = SemiFunc.SpringFloatGet(rightUpperLidRotationZSpring, blendedRightEye.upperLidAngle, playerVisuals.deltaTime);
			rightUpperEyelidRotationZ.localRotation = Quaternion.Euler(0f, 0f, z3);
			float z4 = SemiFunc.SpringFloatGet(leftLowerLidRotationZSpring, blendedLeftEye.lowerLidAngle, playerVisuals.deltaTime);
			leftLowerEyelidRotationZ.localRotation = Quaternion.Euler(0f, 0f, z4);
			z4 = SemiFunc.SpringFloatGet(rightLowerLidRotationZSpring, blendedRightEye.lowerLidAngle, playerVisuals.deltaTime);
			rightLowerEyelidRotationZ.localRotation = Quaternion.Euler(0f, 0f, z4);
			pupilLeftScaleAmount = SemiFunc.SpringFloatGet(leftPupilSizeSpring, blendedLeftEye.pupilSize, playerVisuals.deltaTime);
			pupilRightScaleAmount = SemiFunc.SpringFloatGet(rightPupilSizeSpring, blendedRightEye.pupilSize, playerVisuals.deltaTime);
			if (expressions[0].weight > 82f)
			{
				pupilRightScaleAmount = 1f;
				pupilRightScale.localScale = Vector3.one;
				pupilLeftScaleAmount = 1f;
				pupilLeftScale.localScale = Vector3.one;
				eyelidLeft.SetActive(value: false);
				eyelidRight.SetActive(value: false);
			}
		}
		ExpressionTimerTick();
	}

	private EyeSettings BlendEyeSettings(bool isLeft)
	{
		EyeSettings eyeSettings = new EyeSettings();
		float num = 0f;
		foreach (ExpressionSettings expression in expressions)
		{
			float weight = expression.weight;
			num += weight;
			EyeSettings eyeSettings2 = (isLeft ? expression.leftEye : expression.rightEye);
			eyeSettings.upperLidAngle += eyeSettings2.upperLidAngle * weight;
			eyeSettings.upperLidClosedPercent += eyeSettings2.upperLidClosedPercent * weight;
			eyeSettings.upperLidClosedPercentJitterAmount += eyeSettings2.upperLidClosedPercentJitterAmount * weight;
			eyeSettings.upperLidClosedPercentJitterSpeed += eyeSettings2.upperLidClosedPercentJitterSpeed * weight;
			eyeSettings.lowerLidAngle += eyeSettings2.lowerLidAngle * weight;
			eyeSettings.lowerLidClosedPercent += eyeSettings2.lowerLidClosedPercent * weight;
			eyeSettings.lowerLidClosedPercentJitterAmount += eyeSettings2.lowerLidClosedPercentJitterAmount * weight;
			eyeSettings.lowerLidClosedPercentJitterSpeed += eyeSettings2.lowerLidClosedPercentJitterSpeed * weight;
			eyeSettings.pupilSize += eyeSettings2.pupilSize * weight;
			eyeSettings.pupilSizeJitterAmount += eyeSettings2.pupilSizeJitterAmount * weight;
			eyeSettings.pupilSizeJitterSpeed += eyeSettings2.pupilSizeJitterSpeed * weight;
			eyeSettings.pupilPositionJitter += eyeSettings2.pupilPositionJitter * weight;
			eyeSettings.pupilPositionJitterAmount += eyeSettings2.pupilPositionJitterAmount * weight;
			eyeSettings.pupilOffsetRotationX += eyeSettings2.pupilOffsetRotationX * weight;
			eyeSettings.pupilOffsetRotationY += eyeSettings2.pupilOffsetRotationY * weight;
		}
		if (num > 0f)
		{
			eyeSettings.upperLidAngle /= num;
			eyeSettings.upperLidClosedPercent /= num;
			eyeSettings.upperLidClosedPercentJitterAmount /= num;
			eyeSettings.upperLidClosedPercentJitterSpeed /= num;
			eyeSettings.lowerLidAngle /= num;
			eyeSettings.lowerLidClosedPercent /= num;
			eyeSettings.lowerLidClosedPercentJitterAmount /= num;
			eyeSettings.lowerLidClosedPercentJitterSpeed /= num;
			eyeSettings.pupilSize /= num;
			eyeSettings.pupilSizeJitterAmount /= num;
			eyeSettings.pupilSizeJitterSpeed /= num;
			eyeSettings.pupilPositionJitter /= num;
			eyeSettings.pupilPositionJitterAmount /= num;
			eyeSettings.pupilOffsetRotationX /= num;
			eyeSettings.pupilOffsetRotationY /= num;
		}
		return eyeSettings;
	}

	public void FetchTransformValues()
	{
		if (expressions == null)
		{
			expressions = new List<ExpressionSettings>();
		}
		ExpressionSettings expressionSettings;
		if (expressions.Count == 0)
		{
			expressionSettings = new ExpressionSettings();
			expressionSettings.expressionName = "Default Expression";
			expressionSettings.weight = 100f;
			expressionSettings.leftEye = new EyeSettings();
			expressionSettings.rightEye = new EyeSettings();
			expressions.Add(expressionSettings);
		}
		else
		{
			int index = expressions.Count - 1;
			expressionSettings = expressions[index];
		}
		if (leftUpperEyeLidRotationZ != null)
		{
			expressionSettings.leftEye.upperLidAngle = leftUpperEyeLidRotationZ.localRotation.eulerAngles.z;
		}
		if (leftUpperEyelidRotationX != null)
		{
			float x = leftUpperEyelidRotationX.localRotation.eulerAngles.x;
			float x2 = leftLowerEyelidRotationX.localRotation.eulerAngles.x;
			expressionSettings.leftEye.upperLidClosedPercent = x;
			expressionSettings.leftEye.lowerLidClosedPercent = x2;
		}
		if (leftLowerEyelidRotationZ != null)
		{
			expressionSettings.leftEye.lowerLidAngle = leftLowerEyelidRotationZ.localRotation.eulerAngles.z;
		}
		if (pupilLeftScale != null)
		{
			expressionSettings.leftEye.pupilSize = pupilLeftScale.localScale.x;
		}
		if (pupilLeftRotationOffset != null)
		{
			expressionSettings.leftEye.pupilOffsetRotationX = pupilLeftRotationOffset.localRotation.eulerAngles.x;
			expressionSettings.leftEye.pupilOffsetRotationY = pupilLeftRotationOffset.localRotation.eulerAngles.y;
		}
		if (rightUpperEyelidRotationZ != null)
		{
			expressionSettings.rightEye.upperLidAngle = rightUpperEyelidRotationZ.localRotation.eulerAngles.z;
		}
		if (rightUpperEyelidRotationX != null)
		{
			float x3 = rightUpperEyelidRotationX.localRotation.eulerAngles.x;
			float x4 = rightLowerEyelidRotationX.localRotation.eulerAngles.x;
			expressionSettings.rightEye.upperLidClosedPercent = x3;
			expressionSettings.rightEye.lowerLidClosedPercent = x4;
		}
		if (rightLowerEyelidRotationZ != null)
		{
			expressionSettings.rightEye.lowerLidAngle = rightLowerEyelidRotationZ.localRotation.eulerAngles.z;
		}
		if (leftUpperEyelidRotationX != null)
		{
			expressionSettings.rightEye.pupilSize = pupilRightScale.localScale.x;
		}
		if (pupilRightRotationOffset != null)
		{
			expressionSettings.rightEye.pupilOffsetRotationX = pupilRightRotationOffset.localRotation.eulerAngles.x;
			expressionSettings.rightEye.pupilOffsetRotationY = pupilRightRotationOffset.localRotation.eulerAngles.y;
		}
		Debug.Log("Transform values have been fetched into the EyeSettings of expression: " + expressionSettings.expressionName);
	}
}
