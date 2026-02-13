//bear witness
namespace RBitUtils
{
	using System;
    using UnityEngine;
    using System.Collections.Generic;
    public static class Misc
    {
		public static void CheckChange<T>(this T self, ref T other, Action callback)
		{
			if (!self.Equals(other))
			{
				other = self;
				callback();
			}
		}

        public static bool Contains(this LayerMask mask, GameObject gameObject)
        {
            return (mask & (1 << gameObject.layer)) != 0;
        }

        public static bool IsEmpty<T>(this T[] self)
        {
            if (self.Length == 0) return true;
            foreach (T item in self) if (item != null) return false;
            return true;
        }
    }

	public static class LerpPlus
	{
        public static float LerpDFactor(float k, float t)
        {
            return Mathf.Pow(1 - k, 1 / t);
        }

        #region float
        public static float LerpD(float a, float b, float k, float t, float d)
        {
            return Mathf.Lerp(
                a, b,
                1 - Mathf.Pow(
                    1 - k,
                    d / t));
        }
        public static float LerpD(float a, float b, float f, float d)
        {
            return Mathf.Lerp(
                a, b,
                1 - Mathf.Pow(f, d));
        }

        public static float LerpAngleD(float a, float b, float k, float t, float d)
        {
            return Mathf.LerpAngle(
                a, b,
                1 - Mathf.Pow(
                    1 - k,
                    d / t));
        }

        public static float LerpAngleD(float a, float b, float f, float d)
        {
            return Mathf.LerpAngle(
                a, b,
                1 - Mathf.Pow(f, d));
        }
        #endregion

        #region vector
        public static Vector2 LerpD(Vector2 a, Vector2 b, float f, float d)
        {
            return Vector2.Lerp(
                a, b,
                1 - Mathf.Pow(f, d));
        }
        public static Vector3 LerpD(Vector3 a, Vector3 b, float f, float d)
        {
            return Vector3.Lerp(
                a, b,
                1 - Mathf.Pow(f, d));
        }

        public static Vector3 SlerpD(Vector3 a, Vector3 b, float f, float d)
        {
            return Vector3.SlerpUnclamped(
                a, b,
                1 - Mathf.Pow(f, d));
        }

        public static Vector2 LerpD(Vector2 a, Vector2 b, float k, float t, float d)
        {
            return Vector2.Lerp(
                a, b,
                1 - Mathf.Pow(
                    1 - k,
                    d / t));
        }
        public static Vector3 LerpD(Vector3 a, Vector3 b, float k, float t, float d)
        {
            return Vector3.Lerp(
                a, b,
                1 - Mathf.Pow(
                    1 - k,
                    d / t));
        }
        public static Vector3 SlerpD(Vector3 a, Vector3 b, float k, float t, float d)
        {
            return Vector3.SlerpUnclamped(
                a, b,
                1 - Mathf.Pow(
                    1 - k,
                    d / t));
        }
        #endregion

        #region gradient
        /// <summary>
        /// https://discussions.unity.com/t/lerp-from-one-gradient-to-another/590382/3
        /// </summary>
        public static UnityEngine.Gradient Lerp(
            UnityEngine.Gradient a, UnityEngine.Gradient b, float t, bool noAlpha = false, bool noColor = false)
        {
            //list of all the unique key timesS
            var keysTimes = new List<float>();

            if (!noColor)
            {
                for (int i = 0; i < a.colorKeys.Length; i++)
                {
                    float k = a.colorKeys[i].time;
                    if (!keysTimes.Contains(k))
                        keysTimes.Add(k);
                }

                for (int i = 0; i < b.colorKeys.Length; i++)
                {
                    float k = b.colorKeys[i].time;
                    if (!keysTimes.Contains(k))
                        keysTimes.Add(k);
                }
            }

            if (!noAlpha)
            {
                for (int i = 0; i < a.alphaKeys.Length; i++)
                {
                    float k = a.alphaKeys[i].time;
                    if (!keysTimes.Contains(k))
                        keysTimes.Add(k);
                }

                for (int i = 0; i < b.alphaKeys.Length; i++)
                {
                    float k = b.alphaKeys[i].time;
                    if (!keysTimes.Contains(k))
                        keysTimes.Add(k);
                }
            }

            GradientColorKey[] clrs = new GradientColorKey[keysTimes.Count];
            GradientAlphaKey[] alphas = new GradientAlphaKey[keysTimes.Count];

            //Pick colors of both gradients at key times and lerp them
            for (int i = 0; i < keysTimes.Count; i++)
            {
                float key = keysTimes[i];
                var clr = Color.Lerp(a.Evaluate(key), b.Evaluate(key), t);
                clrs[i] = new GradientColorKey(clr, key);
                alphas[i] = new GradientAlphaKey(clr.a, key);
            }

            var g = new UnityEngine.Gradient();
            g.SetKeys(clrs, alphas);

            return g;
        }

        public static Gradient LerpD(
            Gradient a, Gradient b, float k, float t, float d)
        {
            return Lerp(
                a, b,
                1 - Mathf.Pow(
                    1 - k,
                    d / t));
        }
        public static Gradient LerpD(Gradient a, Gradient b, float f, float d)
        {
            return Lerp(
                a, b,
                1 - Mathf.Pow(f, d));
        }
        #endregion

    }

	public static class MathPlus
	{
        public static float SQRT2OVER2 = Mathf.Sqrt(2) / 2;
    }

    public static class VectorPlus
    {
        public static Vector2Int RoundToInt(this Vector2 v) => 
			new(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));

        public static Vector2 xz(this Vector3 v) => new(v.x, v.z);

        public static Vector3 xz(this Vector2 v, float y) => new(v.x, y, v.y);

        public static Vector2 Scaled(
			this Vector2 v, Vector2 other)
        {
            v.Scale(other);
            return v;
        }

        public static Vector3 Scaled(
			this Vector3 v, Vector3 other)
        {
            v.Scale(other);
            return v;
        }

        public static Vector3 DivideBy(
			this Vector3 v, Vector3 other) => new(v.x / other.x, v.y / other.y, v.z / other.z);
        public static Vector2 DivideBy(
			this Vector2 v, Vector2 other) => new(v.x / other.x, v.y / other.y);
    }

    public static class DebugPlus
	{
        public static void DrawCross(
			Vector3 pos, float size = 10, Color? color = null)
        {
            var c = color ?? Color.white;

            Debug.DrawLine(pos + Vector3.left * size / 2, pos + Vector3.right * size / 2, c);
            Debug.DrawLine(pos + Vector3.up * size / 2, pos + Vector3.down * size / 2, c);
        }

        public static void DrawBounds(
			Bounds bounds, Color? color = null)
        {
            if (!color.HasValue) { color = Color.white; }

            Debug.DrawLine(new(bounds.min.x, bounds.min.y), new(bounds.min.x, bounds.max.y), (Color)color);
            Debug.DrawLine(new(bounds.max.x, bounds.min.y), new(bounds.max.x, bounds.max.y), (Color)color);
            Debug.DrawLine(new(bounds.min.x, bounds.min.y), new(bounds.max.x, bounds.min.y), (Color)color);
            Debug.DrawLine(new(bounds.min.x, bounds.max.y), new(bounds.max.x, bounds.max.y), (Color)color);
        }
    }

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