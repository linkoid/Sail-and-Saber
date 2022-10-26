using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Ships
{
	public class ShipPhysics : Ship.ShipBehaviour
	{
		public Rigidbody Rigidbody => Ship.Rigidbody;

		[Header("Config")]
		[SerializeField] private float m_Speed = 200f;
		[SerializeField] private float m_Acceleration = 5f;
		[SerializeField] private float m_SpinSpeed = 0.5f;
		[SerializeField] private float m_SpinAcceleration = 0.2f;
		
		[Header("Values")]
		[ReadOnly] public float Steering;
		[ReadOnly] public float Throttle;

		void FixedUpdate()
		{
			AddSteerForce();
			AddThrottleForce();
		}

		void AddSteerForce()
		{
			Vector3 CurrentSpin = Rigidbody.angularVelocity;

			Vector3 TargetSpin = new Vector3(0f, Steering, 0f).normalized * m_SpinSpeed;

			Vector3 DeltaSpin = TargetSpin - CurrentSpin;

			Vector3 AddSpin = DeltaSpin.normalized * m_SpinAcceleration * Time.fixedDeltaTime;

			if (AddSpin.magnitude > DeltaSpin.magnitude)
			{
				AddSpin = DeltaSpin;
			}
			Rigidbody.AddTorque(AddSpin, ForceMode.VelocityChange);
		}

		void AddThrottleForce()
		{
			Vector3 CurrentVelocity = Rigidbody.velocity;
			Vector3 TargetVel = transform.forward * ((Throttle > 0) ? Throttle : Throttle / 10) * m_Speed * Time.fixedDeltaTime;
			Vector3 Difference = TargetVel - CurrentVelocity;

			Vector3 AddVel = Difference.normalized * m_Acceleration * Time.fixedDeltaTime;
			if (AddVel.magnitude > Difference.magnitude)
			{
				AddVel = Difference;
			}

			Rigidbody.AddForce(AddVel, ForceMode.VelocityChange);
		}
	}
}

