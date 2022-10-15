using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.CustomUtils;
using PirateGame;

namespace PirateGame
{
	[RequireComponent(typeof(PlayerInput), typeof(Rigidbody))]
	public class PlayerController : MonoBehaviour, IHumanoid
	{
		public bool IsJumping => m_JumpTime > 0;
		public bool IsGrounded => m_IsGrounded;
		public Vector3 Movement => m_Movement;
		public Rigidbody Rigidbody => GetComponent<Rigidbody>();

		[SerializeField] public Camera m_Camera;

		[SerializeField] private float m_Speed = 10;
		[SerializeField] private float m_Acceleration = 50;
		[SerializeField] private float m_JumpSpeed = 10;
		[SerializeField] private float m_JumpDuration = 0.5f;

		[SerializeField, ReadOnly] private Vector3 m_Movement = Vector3.zero;
		[SerializeField, ReadOnly] private Vector2 m_RelativeMovement = Vector2.zero;
		[SerializeField, ReadOnly] private bool m_IsMoveTo = false;
		[SerializeField, ReadOnly] private Vector3 m_MoveTo = Vector3.zero;

		[SerializeField, ReadOnly] private float m_JumpTime = 0;
		[SerializeField, ReadOnly] private float m_JumpCancelTime = 0;

		[SerializeField, ReadOnly] private Vector3 m_GroundVelocity = Vector3.zero;
		[SerializeField, ReadOnly] private Vector3 m_PrevGroundVelocity = Vector3.zero;
		[SerializeField, ReadOnly] private bool m_IsGrounded = false;
		[SerializeField, ReadOnly] private List<Collider> m_GroundColliders = new List<Collider>();

		void Awake()
		{
			m_GroundVelocity = Vector3.zero;
			m_PrevGroundVelocity = Vector3.zero;
			m_IsGrounded = false;
		}

		void OnEnable()
		{
			if (m_Camera == null)
			{
				Debug.LogWarning($"Camera not assigned to PlayerController", this);
			}
		}

		void OnMove(InputValue input)
		{
			m_RelativeMovement = input.Get<Vector2>();
			m_IsMoveTo = false;
		}

		void OnMoveTo(InputValue input)
		{
			var screenPos = input.Get<Vector2>();

			if (screenPos == Vector2.zero) return;

			if (m_Camera != null)
			{
				var ray = m_Camera.ScreenPointToRay(screenPos);
				if (Physics.Raycast(ray, out RaycastHit hitInfo, m_Camera.farClipPlane))
				{
					m_MoveTo = hitInfo.point;
					m_IsMoveTo = true;
				}
			}
		}

		void OnJump(InputValue input)
		{
			if (input.Get<float>() > 0.5f)
			{
				if (m_IsGrounded && m_JumpTime <= 0)
				{
					m_JumpTime = m_JumpDuration;
					Rigidbody.AddForce(-Physics.gravity.normalized * m_JumpSpeed, ForceMode.VelocityChange);
				}
			}
			else // Released
			{
				m_JumpCancelTime = Mathf.Max(0, m_JumpTime);
				m_JumpTime = 0;
			}
		}



		void FixedUpdate()
		{
			AddMovementForce();
			AddJumpForce();

			m_PrevGroundVelocity = m_GroundVelocity;
		}

		void AddMovementForce()
		{
			m_Movement = GetMovement();

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

		Vector3 GetMovement()
		{
			Vector3 up = -Physics.gravity.normalized;
			if (m_IsMoveTo)
			{
				Vector3 goal = Vector3.ProjectOnPlane(m_MoveTo, up) + Vector3.Scale(Rigidbody.position, up);
				Vector3 offset = goal - Rigidbody.position;
				if (offset.sqrMagnitude < 0.1f * 0.1f)
				{
					m_IsMoveTo = false;
					return Vector3.zero;
				}
				// else
				return offset.normalized;
			}
			else
			{
				Vector3 direction = new Vector3(m_RelativeMovement.x, 0, m_RelativeMovement.y);
				if (m_Camera != null)
				{
					direction = m_Camera.transform.TransformDirection(direction);
					direction = Vector3.ProjectOnPlane(direction, up);
				}
				direction.Normalize();

				float magnitude = Mathf.Min(m_RelativeMovement.magnitude, 1);
				var movement = direction * magnitude;
				return movement;
			}
		}


		#region Context Actions

		[SerializeField, ReadOnly] private List<ActionContext> m_ActionContexts = new List<ActionContext>();

		void OnTriggerEnter(Collider collider)
		{
			Debug.Log(collider);
			if (collider.TryGetComponent(out ActionContext context))
			{
				if (m_ActionContexts.Contains(context)) return;
				m_ActionContexts.Add(context);
			}
		}

		void OnTriggerExit(Collider collider)
		{
			if (collider.TryGetComponent(out ActionContext context))
			{
				if (!m_ActionContexts.Contains(context)) return;
				m_ActionContexts.Remove(context);
			}
		}

		void OnContextAction(InputValue input)
		{
			if (input.isPressed && m_ActionContexts.Count > 0)
			{
				m_ActionContexts[0].Enter(this);
			}
		}


		#endregion
	}
}


