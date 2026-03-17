//bear witness
namespace RBitUtils
{
	using System;
    using UnityEngine;
    using System.Collections.Generic;
    
    /// <summary>
    /// Taken from <a href="https://www.youtube.com/watch?v=KPoeNZZ6H4s">t3ssel8r's video on proc anim</a>
    /// </summary>
    public static class SecondOrderDynamics
	{
		public static void InitConstants(
			float f, float z, float r,
			out float k1, out float k2,
			out float k3)
		{
			k1 = z / (Mathf.PI * f);
			k2 = 1 / (4 * (Mathf.PI * f) * (Mathf.PI * f));
			k3 = r * z / (2 * Mathf.PI * f);
		}

		public static float UpdateFloat(
			float dt, float x, float? xd,
			ref float xp, ref float y, ref float yd,
			float k1, float k2, float k3)
		{
			if (xd == null) // estimate velocity if absent
			{
				xd = (x - xp) / dt;
				xp = x;
			}

			float k2_stable = Mathf.Max(k2, dt * dt / 2 + dt * k1 / 2, dt * k1); // clamp k2 to guarantee stability without jitter
			y += dt * yd; // integrate by vel
			yd += dt * (x + k3 * (float)xd - y - k1 * yd) / k2_stable; // integrate velocity by acceleration

			return y;
		}
		public static Vector2 UpdateVector2(
			float dt, Vector2 x, Vector2? xd,
			ref Vector2 xp, ref Vector2 y, ref Vector2 yd,
			float k1, float k2, float k3)
		{
			if (xd == null)
			{
				xd = (x - xp) / dt;
				xp = x;
			}

			float k2_stable = Mathf.Max(k2, dt * dt / 2 + dt * k1 / 2, dt * k1);
			y += dt * yd;
			yd += dt * (x + k3 * (Vector2)xd - y - k1 * yd) / k2_stable;

			return y;
		}
		public static Vector3 UpdateVector3(
			float dt, Vector3 x, Vector3? xd,
			ref Vector3 xp, ref Vector3 y, ref Vector3 yd,
			float k1, float k2, float k3)
		{
			if (xd == null)
			{
				xd = (x - xp) / dt;
				xp = x;
			}

			float k2_stable = Mathf.Max(k2, dt * dt / 2 + dt * k1 / 2, dt * k1);
			y += dt * yd;
			yd += dt * (x + k3 * (Vector3)xd - y - k1 * yd) / k2_stable;

			return y;
		}

		public class F
		{
			float xp, y, yd;
			float k1, k2, k3;

			public F(float x0, float f, float z, float r)
			{
				InitConstants(
					f, z, r,
					out k1, out k2, out k3);
				xp = y = x0;
				yd = 0;
			}

			public float Update(float dt, float x, float? xd = null) => UpdateFloat(dt, x, xd, ref xp, ref y, ref yd, k1, k2, k3);
		}

		public class V2
		{
			Vector2 xp, y, yd;
			float k1, k2, k3;

			public V2(Vector2 x0, float f, float z, float r)
			{
				InitConstants(
					f, z, r,
					out k1, out k2, out k3);
				xp = y = x0;
				yd = Vector2.zero;
			}

			public Vector2 Update(float dt, Vector2 x, Vector2? xd = null) => UpdateVector2(dt, x, xd, ref xp, ref y, ref yd, k1, k2, k3);
		}

		public class V3
		{
			Vector3 xp, y, yd;
			float k1, k2, k3;

			public V3(Vector3 x0, float f, float z, float r)
			{
				InitConstants(
					f, z, r,
					out k1, out k2, out k3);
				xp = y = x0;
				yd = Vector3.zero;
			}

			public Vector3 Update(float dt, Vector3 x, Vector3? xd = null) => UpdateVector3(dt, x, xd, ref xp, ref y, ref yd, k1, k2, k3);
		}
	}

	
}