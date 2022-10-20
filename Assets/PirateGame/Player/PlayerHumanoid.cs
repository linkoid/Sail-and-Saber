using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.CustomUtils;

namespace PirateGame
{
	[RequireComponent(typeof(Rigidbody))]
public class PlayerHumanoid : MonoBehaviour,IHumanoid
{
	    public bool IsJumping => m_JumpTime > 0;
		public bool IsGrounded => m_IsGrounded;


		public Vector3 m_Movement = Vector3.zero;

		
		public Vector3 Movement {get => m_Movement ; set => m_Movement = value ;}
		public Rigidbody Rigidbody => GetComponent<Rigidbody>();

		[SerializeField, ReadOnly] private float m_JumpTime = 0;
		[SerializeField, ReadOnly] private float m_JumpCancelTime = 0;

		[SerializeField, ReadOnly] private Vector3 m_GroundVelocity = Vector3.zero;
		[SerializeField, ReadOnly] private Vector3 m_PrevGroundVelocity = Vector3.zero;
		[SerializeField, ReadOnly] private bool m_IsGrounded = false;
		[SerializeField] private float m_Speed = 10;
		[SerializeField] private float m_Acceleration = 50;
        
		[SerializeField] private float m_JumpSpeed = 10;
		[SerializeField] private float m_JumpDuration = 0.5f;
		[SerializeField, ReadOnly] private List<Collider> m_GroundColliders = new List<Collider>();


		public void Jump(){
			if (m_IsGrounded && m_JumpTime <= 0)
				{
					m_JumpTime = m_JumpDuration;
					Rigidbody.AddForce(-Physics.gravity.normalized * m_JumpSpeed, ForceMode.VelocityChange);
				}
		}

		public void ReleaseJump(){
					
				m_JumpCancelTime = Mathf.Max(0, m_JumpTime);
				m_JumpTime = 0;
		}

        void Awake()
		{
			m_GroundVelocity = Vector3.zero;
			m_PrevGroundVelocity = Vector3.zero;
			m_IsGrounded = false;
		}
    // Start is called before the first frame updat
    

    

		void FixedUpdate()
		{
			AddMovementForce();
			AddJumpForce();

			m_PrevGroundVelocity = m_GroundVelocity;
		}

        
		void AddMovementForce()
		{

			Vector3 groundDeltaV = m_GroundVelocity - m_PrevGroundVelocity;
			Rigidbody.AddForce(groundDeltaV, ForceMode.VelocityChange);

			Vector3 referenceVelocity = Rigidbody.velocity + groundDeltaV;
			Vector3 targetVelocity = m_Movement * m_Speed + m_GroundVelocity;
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

		void OnCollisionStay(Collision collision)
		{
			OnGroundCollision(collision);

			// Hitting something while in the air resets ground velocity.
			if (!m_IsGrounded)
			{
				m_GroundVelocity = Vector3.zero;
			}
		}

		void OnCollisionExit(Collision collision)
		{
			OnGroundCollision(collision);
		}

		void OnGroundCollision(Collision collision)
		{
			var hitRigidbody = collision.rigidbody;
			if (hitRigidbody)
			{
				var constraints = hitRigidbody.constraints;
				// Ignore rigidbodies that can fall (move on Y axis)
				if (!constraints.HasFlag(RigidbodyConstraints.FreezePositionY)) return;
			}

			// Is the ground collider still acting as ground?
			if (CheckGrounding(collision))
			{
				if (m_GroundColliders.Contains(collision.collider))
				{
					OnGroundCollisionStay(collision);
				}
				else
				{
					m_GroundColliders.Add(collision.collider);
					OnGroundCollisionEnter(collision);
				}
			}
			else
			{
				if (m_GroundColliders.Contains(collision.collider))
				{
					// XXX If the player has more than one collider, this doesn't work.
					// because OnCollisionExit is called for any local collider.
					m_GroundColliders.Remove(collision.collider);
					OnGroundCollisionExit(collision);
				}
			}
		}

		void OnGroundCollisionEnter(Collision collision)
		{
			m_IsGrounded = true;
		}

		void OnGroundCollisionStay(Collision collision)
		{
			var hitRigidbody = collision.rigidbody;
			if (hitRigidbody)
			{
				m_GroundVelocity = hitRigidbody.velocity;
			}
			else
			{
				m_GroundVelocity = Vector3.zero;
			}

			m_IsGrounded = true;
		}

		void OnGroundCollisionExit(Collision collision)
		{
			if (m_GroundColliders.Count <= 0)
			{
				m_IsGrounded = false;
			}
		}

		bool CheckGrounding(Collision collision)
		{
			Vector3 expectedImpulse = Rigidbody.mass * -Physics.gravity * Time.fixedDeltaTime;
			float alignment = Vector3.Dot(expectedImpulse, collision.GetImpulse()) / expectedImpulse.sqrMagnitude;
			if (alignment >= 0.5f)
			{
				return true;
			}

			return false;
		}
}
}
