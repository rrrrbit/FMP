namespace RBitUtils
{
	namespace ResponseTypes
	{
		using System;
		using Unity.VisualScripting.FullSerializer;
		using UnityEngine;
		/// <summary>
		/// Update an output value y based on an input value x with respect to timestep.
		/// </summary>
		public abstract class ResponseType
		{
			public float x, y;
			public abstract void Update(float dt);
			public virtual float Update(float dt, float x)
			{
				this.x = x;
				Update(dt);
				return y;
			}
		}
		/// <summary>
		/// Update an output value y based on an input value x and its velocity dx, with respect to timestep.
		/// </summary>
		public abstract class VelResponseType : ResponseType
		{
			public float xp, dx;
			public override float Update(float dt, float x)
			{
				dx = (x - xp) / dt; // estimate vel
				xp = x;
				base.Update(dt, x);
				return y;
			}
			public virtual float Update(float dt, float x, float xd)
			{
				this.dx = xd;
				base.Update(dt, x);
				return y;
			}
		}
		/// <summary>
		/// Response for all components of a vector.
		/// </summary>
		/// <typeparam name="T">Response type to use.</typeparam>
		public class Vec2Response<T> where T : ResponseType
		{
			T rx, ry;

			public Vec2Response(
				Func<float, T> factory,
				Vector2 x0)
			{
				rx = factory(x0.x);
				ry = factory(x0.y);
			}

			public Vector2 Update(float dt, Vector2 x)
				=> new(
					rx.Update(dt, x.x),
					ry.Update(dt, x.y));
		}
		public class Vec3Response<T> where T : ResponseType
		{
			T rx, ry, rz;

			public Vec3Response(
				Func<float, T> factory,
				Vector3 x0)
			{
				rx = factory(x0.x);
				ry = factory(x0.y);
				rz = factory(x0.z);
			}

			public Vector3 Update(float dt, Vector3 x)
				=> new(
					rx.Update(dt, x.x),
					ry.Update(dt, x.y),
					rz.Update(dt, x.z));
		}
		///<summary>
		///Physics based response supporting easing, overshoot and spring motion taken from <a href="https://www.youtube.com/watch?v=KPoeNZZ6H4s">t3ssel8r's video on procedural animation.</a>
		///</summary>
		public class Spring : VelResponseType
		{
			public float yd;
			private float k1, k2, k3;

			public Spring(float x0, float f, float z, float r)
			{
				var pif = Mathf.PI * f;
				k1 = z / pif;
				k2 = 1 / (4 * pif * pif);
				k3 = r * k1 / 2;

				xp = y = x0;
				yd = 0;
			}
			public override void Update(float dt)
			{
				float k2_stable = Mathf.Max(k2, dt * dt / 2 + dt * k1 / 2, dt * k1); // clamp k2 to guarantee stability without jitter
				y += dt * yd; // integrate by vel
				yd += dt * (x + k3 * dx - y - k1 * yd) / k2_stable; // integrate velocity by acceleration
			}
		}
        ///<summary>
        ///Response type based on Lerp.
        ///</summary>
        public class LerpD : ResponseType
        {
            float f;

            public LerpD(float x0, float k, float t)
            {
                x = y = x0;
                f = Mathf.Pow(1 - k, 1 / t);
            }

            public override void Update(float dt)
            {
                y = Mathf.Lerp(
                    y, x,
                    1 - Mathf.Pow(f, dt));
            }
        }
        ///<summary>
        ///Response type that updates at a fixed or velocity-dependent rate.
        ///</summary>
        public class AdaptiveRate : VelResponseType
		{
			private float timer;
			public float rate;
			public Func<float, float> rateMultOverVel;	

			public AdaptiveRate(float x0, float rate, Func<float, float> rateMultOverVel)
			{
				x = y = x0;
				this.rate = rate;
				this.rateMultOverVel = rateMultOverVel;
			}
			public override void Update(float dt)
			{
				timer += dt;
				if (timer >= (1 / (rate * rateMultOverVel(dx))))
				{
					timer = 0;
					y = x;
				}
			}
		}
		

		public class AdaptiveStepV3
		{
			private float timer;
			public float rate;
			public Func<float, float> rateMultOverVel;

			public Vector3 xv, yv, xdv; // actual vec state

			public AdaptiveStepV3(Vector3 x0, float rate,
				Func<float, float> rateMultOverVel)
			{
				xv = yv = x0;
				this.rate = rate;
				this.rateMultOverVel = rateMultOverVel;
			}

			public Vector3 Update(float dt, Vector3 x)
			{
				var vel = (x - xv).magnitude; // scalar vel
				xv = x;
				timer += dt;
				if (timer >= 1f / (rate * rateMultOverVel(vel)))
				{
					timer = 0;
					yv = x; // snap ALL components together
				}

				Debug.Log((rate * rateMultOverVel(vel)));

				return yv;
			}
		}
	}
}