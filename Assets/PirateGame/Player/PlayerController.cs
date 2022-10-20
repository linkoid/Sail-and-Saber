using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.CustomUtils;
using PirateGame;
using PirateGame.Cameras;

namespace PirateGame
{
	[RequireComponent(typeof(PlayerInput), typeof(PlayerHumanoid))]
	public class PlayerController : MonoBehaviour
	{
		public PlayerHumanoid Humanoid => gameObject.GetComponent<PlayerHumanoid>();
		public CameraController CameraController => m_Camera.GetComponent<CameraController>();

		[SerializeField] public Camera m_Camera;
		[SerializeField] private VirtualCamera m_ThirdPersonCamera;
		[SerializeField, ReadOnly] private Vector2 m_RelativeMovement = Vector2.zero;
		[SerializeField, ReadOnly] private bool m_IsMoveTo = false;
		[SerializeField, ReadOnly] private Vector3 m_MoveTo = Vector3.zero;


		private void FixedUpdate()
		{
			// Every fixed update, set the humanoid's movement using m_RelativeMovement
			Humanoid.Movement = GetMovement();
		}

		void OnEnable()
		{
			if (m_Camera == null)
			{
				Debug.LogWarning($"Camera not assigned to PlayerController", this);
			}
			else
			{
				CameraController.SetVCam(m_ThirdPersonCamera);
			}
		}

		void OnDisable()
		{
			Humanoid.Movement = Vector3.zero;
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
				Humanoid.Jump();
			}
			else // Released
			{
				Humanoid.ReleaseJump();
			}
		}


		Vector3 GetMovement()
		{
			Vector3 up = -Physics.gravity.normalized;
			if (m_IsMoveTo)
			{
				Vector3 goal = Vector3.ProjectOnPlane(m_MoveTo, up) + Vector3.Scale(Humanoid.Rigidbody.position, up);
				Vector3 offset = goal - Humanoid.Rigidbody.position;
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
			// Keep track of all nearby action contexts
			if (collider.TryGetComponent(out ActionContext context))
			{
				if (m_ActionContexts.Contains(context)) return;
				m_ActionContexts.Add(context);
			}
		}

		void OnTriggerExit(Collider collider)
		{
			// Stop keeping track of the action context after leaving it's area
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
				// TODO Find the best action context if there is more than one nearby
				m_ActionContexts[0].Enter(this);
			}
		}

		


		#endregion
	}
}


