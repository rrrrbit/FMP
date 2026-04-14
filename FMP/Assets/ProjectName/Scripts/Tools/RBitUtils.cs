namespace RBitUtils
{
	using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    public static class Misc
    {
        /// <summary>
        /// Check if a variable has changed since the last callback (using a buffer var) and if it has, invoke a callback.
        /// </summary>
        public static void CheckChange<T>(this T self, ref T other, Action callback)
        {
            if (!self.Equals(other))
            {
                callback();
                other = self; 
            }
        }

        /// <summary>
        /// Check if a layermask contains a GameObject's layer
        /// </summary>
        public static bool Contains(this LayerMask mask, GameObject gameObject)
        {
            return (mask & (1 << gameObject.layer)) != 0;
        }

        /// <summary>
        /// Checks if an array has no items or only nulls.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsEmpty<T>(this T[] self)
        {
            if (self.Length == 0) return true;
            foreach (T item in self) if (item != null) return false;
            return true;
        }

        public static int Rows<T>(this T[,] self) => self.GetLength(0);
        public static int Cols<T>(this T[,] self) => self.GetLength(1);
        public static Vector2Int Dimensions<T>(this T[,] self) => new(self.GetLength(0), self.GetLength(1));
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
        public static Vector2 xy(this Vector3 v) => new(v.x, v.y);

        public static Vector3 xz(this Vector2 v, float y) => new(v.x, y, v.y);
        public static Vector3 xy(this Vector2 v, float z = 0) => new(v.x, v.y, z);

        /// <summary>
        /// Multiplies two vectors componentwise. (Some fucking idiot decided Vector.Scale should be in-place)
        /// </summary>
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

        /// <summary>
        /// Divides a vector by another componentwise.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Vector3 DivideBy(
            this Vector3 v, Vector3 other) => new(v.x / other.x, v.y / other.y, v.z / other.z);
        public static Vector2 DivideBy(
            this Vector2 v, Vector2 other) => new(v.x / other.x, v.y / other.y);

        /// <summary>
        /// Apply a float function to the components of a vector.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Vector2 Distribute(this Vector2 vec, Func<float, float> func) => new Vector2(func(vec.x), func(vec.y));
        public static Vector3 Distribute(this Vector3 vec, Func<float, float> func) => new Vector3(func(vec.x), func(vec.y), func(vec.z));

        public static Vector2 ClampLength(this Vector2 v, float maxLength)
        {
            if (v.sqrMagnitude > maxLength * maxLength)
            {
                return v.normalized * maxLength;
            }
            return v;
        }
        public static Vector2 WithMag(this Vector2 v, float mag)
        {
            return v.normalized * mag;
        }
        public static Vector3 ClampLength(this Vector3 v, float maxLength)
		{
			if (v.sqrMagnitude > maxLength * maxLength)
			{
				return v.normalized * maxLength;
			}
			return v;
		}
		public static Vector3 WithMag(this Vector3 v, float mag) {
			return v.normalized * mag;
		}
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
}