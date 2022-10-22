using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CustomUtils;

namespace PirateGame
{
	[RequireComponent(typeof(Rigidbody))]
	public class PlayerHumanoid : MonoBehaviour, IHumanoid
	{
		public bool IsJumping => m_JumpTime > 0;
		public bool IsGrounded => m_HumanoidCollider.IsGrounded;
		public Vector3 Movement {get => m_Movement ; set => m_Movement = value ;}
		public Rigidbody Rigidbody => GetComponent<Rigidbody>();


		[SerializeField] private HumanoidCollider m_HumanoidCollider;

		[SerializeField] private Vector3 m_Movement = Vector3.zero;

		[SerializeField] private float m_Speed = 10;
		[SerializeField] private float m_Acceleration = 50;
        
		[SerializeField] private float m_JumpSpeed = 10;
		[SerializeField] private float m_JumpDuration = 0.5f;

		[SerializeField, ReadOnly] private float m_JumpTime = 0;
		[SerializeField, ReadOnly] private float m_JumpCancelTime = 0;

		void Awake()
		{
		}
		
		void FixedUpdate()
		{
			AddMovementForce();
			AddJumpForce();
		}

		public void Jump()
		{
			if (IsGrounded && m_JumpTime <= 0)
			{
				m_JumpTime = m_JumpDuration;
				Rigidbody.AddForce(-Physics.gravity.normalized * m_JumpSpeed, ForceMode.VelocityChange);
			}
		}

		public void ReleaseJump()
		{

			m_JumpCancelTime = Mathf.Max(0, m_JumpTime);
			m_JumpTime = 0;
		}

		void AddMovementForce()
		{
			m_HumanoidCollider.TargetVelocity = m_Movement * m_Speed;

			if (IsGrounded) return; // Let the Humanoid Collider handle the movement
			
			// else
			
			Vector3 groundDeltaV = m_HumanoidCollider.GroundVelocity;
			Vector3 referenceVelocity = Rigidbody.velocity + groundDeltaV;
			Vector3 targetVelocity = m_Movement * m_Speed + m_HumanoidCollider.GroundVelocity;
			Vector3 deltaV = Vector3.Scale(targetVelocity - referenceVelocity, new Vector3(1, 0, 1));
			float maxDeltaV = (m_Acceleration * Time.fixedDeltaTime);
			if (deltaV.magnitude > maxDeltaV)
			{
				deltaV = deltaV.normalized * maxDeltaV;
			}
			Rigidbody.AddForce(deltaV, ForceMode.VelocityChange);
		}

		void AddJumpForce()
		{
			bool wasJumping = (m_JumpTime > 0);
			m_JumpTime -= Time.fixedDeltaTime;
			if (m_JumpTime > 0)
			{
				// Slight anti-gravity for the duration of the jump
				Rigidbody.AddForce(-Physics.gravity * 0.25f, ForceMode.Acceleration);
			}
			else if (m_JumpCancelTime > 0)
			{
				float timeLeftFactor = Mathf.Max(0, (m_JumpCancelTime / m_JumpDuration));
				Rigidbody.AddForce(Physics.gravity.normalized * m_JumpSpeed * timeLeftFactor * .5f, ForceMode.VelocityChange);
				m_JumpCancelTime = 0;
			}
			else if (wasJumping)
			{
				Rigidbody.AddForce(Physics.gravity.normalized * m_JumpSpeed * 0.25f, ForceMode.VelocityChange);
			}
		}

		void OnCollisionEnter(Collision collision)
		{
			m_HumanoidCollider.OnCollisionEnter(collision);
		}

		void OnCollisionStay(Collision collision)
		{
			m_HumanoidCollider.OnCollisionStay(collision);
		}

		void OnCollisionExit(Collision collision)
		{
			m_HumanoidCollider.OnCollisionExit(collision);
		}
	}
}
