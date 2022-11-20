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
		[SerializeField] private float m_ReverseFactor = 0.5f;
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
			Vector3 currentSpin = Rigidbody.angularVelocity;
			Vector3 targetSpin = new Vector3(0f, Steering, 0f).normalized * m_SpinSpeed;
			Vector3 deltaSpin = targetSpin - currentSpin;
			deltaSpin = Vector3.Scale(deltaSpin, -Physics.gravity.normalized);

			Vector3 addSpin = deltaSpin.normalized * m_SpinAcceleration * Time.fixedDeltaTime;
			if (addSpin.magnitude > deltaSpin.magnitude)
			{
				addSpin = deltaSpin;
			}
			Rigidbody.AddTorque(addSpin, ForceMode.VelocityChange);
		}

		void AddThrottleForce()
		{
			float speed = m_Speed * Throttle;
			if (Throttle < 0)
			{
				speed *= m_ReverseFactor;
			}
			Vector3 currentVelocity = Rigidbody.velocity;
			Vector3 targetVel = transform.forward * speed * Time.fixedDeltaTime;
			Vector3 deltaVel = targetVel - currentVelocity;
			deltaVel = Vector3.ProjectOnPlane(deltaVel, -Physics.gravity.normalized);

			Vector3 addVel = deltaVel.normalized * m_Acceleration * Time.fixedDeltaTime;
			if (addVel.magnitude > deltaVel.magnitude)
			{
				addVel = deltaVel;
			}

			Rigidbody.AddForce(addVel, ForceMode.VelocityChange);
		}
	}
}

