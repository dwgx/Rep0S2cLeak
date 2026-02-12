using UnityEngine;

public static class SpringUtils
{
	public class tDampedSpringMotionParams
	{
		public float m_posPosCoef;

		public float m_posVelCoef;

		public float m_velPosCoef;

		public float m_velVelCoef;
	}

	public static void CalcDampedSpringMotionParams(ref tDampedSpringMotionParams pOutParams, float deltaTime, float angularFrequency, float dampingRatio)
	{
		if (dampingRatio < 0f)
		{
			dampingRatio = 0f;
		}
		if (angularFrequency < 0f)
		{
			angularFrequency = 0f;
		}
		if (angularFrequency < 0.0001f)
		{
			pOutParams.m_posPosCoef = 1f;
			pOutParams.m_posVelCoef = 0f;
			pOutParams.m_velPosCoef = 0f;
			pOutParams.m_velVelCoef = 1f;
		}
		else if (dampingRatio > 1.0001f)
		{
			float num = (0f - angularFrequency) * dampingRatio;
			float num2 = angularFrequency * Mathf.Sqrt(dampingRatio * dampingRatio - 1f);
			float num3 = num - num2;
			float num4 = num + num2;
			float num5 = Mathf.Exp(num3 * deltaTime);
			float num6 = Mathf.Exp(num4 * deltaTime);
			float num7 = 1f / (2f * num2);
			float num8 = num5 * num7;
			float num9 = num6 * num7;
			float num10 = num3 * num8;
			float num11 = num4 * num9;
			pOutParams.m_posPosCoef = num8 * num4 - num11 + num6;
			pOutParams.m_posVelCoef = 0f - num8 + num9;
			pOutParams.m_velPosCoef = (num10 - num11 + num6) * num4;
			pOutParams.m_velVelCoef = 0f - num10 + num11;
		}
		else if (dampingRatio < 0.9999f)
		{
			float num12 = angularFrequency * dampingRatio;
			float num13 = angularFrequency * Mathf.Sqrt(1f - dampingRatio * dampingRatio);
			float num14 = Mathf.Exp((0f - num12) * deltaTime);
			float num15 = Mathf.Cos(num13 * deltaTime);
			float num16 = Mathf.Sin(num13 * deltaTime);
			float num17 = 1f / num13;
			float num18 = num14 * num16;
			float num19 = num14 * num15;
			float num20 = num14 * num12 * num16 * num17;
			pOutParams.m_posPosCoef = num19 + num20;
			pOutParams.m_posVelCoef = num18 * num17;
			pOutParams.m_velPosCoef = (0f - num18) * num13 - num12 * num20;
			pOutParams.m_velVelCoef = num19 - num20;
		}
		else
		{
			float num21 = Mathf.Exp((0f - angularFrequency) * deltaTime);
			float num22 = deltaTime * num21;
			float num23 = num22 * angularFrequency;
			pOutParams.m_posPosCoef = num23 + num21;
			pOutParams.m_posVelCoef = num22;
			pOutParams.m_velPosCoef = (0f - angularFrequency) * num23;
			pOutParams.m_velVelCoef = 0f - num23 + num21;
		}
	}

	public static void UpdateDampedSpringMotion(ref float pPos, ref float pVel, float equilibriumPos, in tDampedSpringMotionParams springParams)
	{
		float num = pPos - equilibriumPos;
		float num2 = pVel;
		pPos = num * springParams.m_posPosCoef + num2 * springParams.m_posVelCoef + equilibriumPos;
		pVel = num * springParams.m_velPosCoef + num2 * springParams.m_velVelCoef;
	}
}
