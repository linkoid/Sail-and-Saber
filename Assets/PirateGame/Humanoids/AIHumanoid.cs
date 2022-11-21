using PirateGame.Crew;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace PirateGame.Humanoids
{
	[SelectionBase]
	[RequireComponent(typeof(NavMeshAgent))]
	public class AIHumanoid : MonoBehaviour, IHumanoid
	{
		public Component Goal;

		public bool IsJumping => CheckIsJumping();
		public bool IsGrounded => CheckIsGrounded();
		public Vector3 Movement => Agent.desiredVelocity;
		public Vector3 Velocity => Agent.velocity;
		public Rigidbody Rigidbody => this.GetComponent<Rigidbody>();

		public NavMeshAgent Agent => this.GetComponent<NavMeshAgent>();


		[SerializeField] private bool m_IsInCombat = false;
		[SerializeField] private bool m_IsStationary = false;

		void Start()
		{
			Agent.updateUpAxis = true;
		}

		void Update()
		{
			AntiSlide(Time.deltaTime);
		}

		// Update is called once per frame
		void FixedUpdate()
		{
			if (Goal == null) return;
			if (!Goal.gameObject.activeInHierarchy) return;

			if (!m_IsStationary)
				Agent.SetDestination(Goal.transform.position);

			if (m_IsInCombat && TryAttack())
			{
				Agent.updateRotation = false;
				Vector3 dir = (Goal.transform.position - Rigidbody.position).normalized;
				Vector3 up = (Agent.navMeshOwner != null) ? (Agent.navMeshOwner as Component).transform.up : -Physics.gravity.normalized;
				dir = Vector3.ProjectOnPlane(dir, up).normalized;
				Vector3 finalDir = Vector3.Slerp(dir, this.transform.forward, 0.1f);
				Rigidbody.MoveRotation(Quaternion.LookRotation(finalDir, up));
			}
			else
			{
				Agent.updateRotation = true;
			}


			//AntiSlide(Time.fixedDeltaTime);
		}

		/// <summary>
		/// Attack the specified enemy
		/// </summary>
		public void Attack(IHumanoid enemy)
		{
			m_IsInCombat = true;
			m_IsStationary = false;
			Goal = enemy as Behaviour;
		}

		/// <summary>
		/// Attack the specified enemy
		/// </summary>
		public void Defend(IHumanoid enemy)
		{
			m_IsInCombat = true;
			m_IsStationary = true;
			Goal = enemy as Behaviour;
		}


		/// <summary>
		/// Stop all actions
		/// </summary>
		public void Standby()
		{
			m_IsInCombat = false;
			m_IsStationary = false;
			TryAttack();
			Goal = null;
		}

		private bool CheckIsJumping()
		{
			if (Agent.isOnOffMeshLink)
			{
				OffMeshLinkData linkData = Agent.currentOffMeshLinkData;
				switch (linkData.linkType)
				{
					case OffMeshLinkType.LinkTypeDropDown:
					case OffMeshLinkType.LinkTypeJumpAcross:
						return true;
					default:
						return false;
					case OffMeshLinkType.LinkTypeManual:
						// continue to below
						break;
				}

				int jumpArea = NavMesh.GetAreaFromName("Jump");
				if (linkData.offMeshLink != null)
				{
					return linkData.offMeshLink.area == jumpArea;
				}
				else if (Agent.navMeshOwner is NavMeshLink navMeshLink)
				{
					return navMeshLink.area == jumpArea;
				}
				else if (Agent.navMeshOwner == null)
				{
					//Debug.LogWarning($"Agent owner is null", this);
					return true;
				}
				else
				{
					Debug.LogWarning($"Unknown OffMeshLink-like owner {Agent.navMeshOwner}", this);
				}
			}

			return false;
		}

		private bool CheckIsGrounded()
		{
			if (Agent.isOnOffMeshLink)
			{
				switch (Agent.currentOffMeshLinkData.linkType)
				{
					case OffMeshLinkType.LinkTypeJumpAcross:
					case OffMeshLinkType.LinkTypeDropDown:
						return false;

					case OffMeshLinkType.LinkTypeManual:
					default:
						break;
				}
			}

			if (!Agent.isOnNavMesh)
			{
				return false;
			}

			return true;
		}

		private bool TryAttack()
		{
			bool attack = false;

			float distance;
			if (Goal == null)
				distance = Mathf.Infinity;
			else 
				distance = Vector3.Distance(Rigidbody.position, Goal.transform.position);

			if (!m_IsInCombat)
			{
				attack = false;
			}
			else if (distance <= Agent.stoppingDistance)
			{
				attack = true;
			}
			else if (distance <= Agent.radius * 2.1)
			{
				attack = true;
			}

			this.SendMessage("OnAttack", attack);

			return attack;
		}

		[SerializeField, ReadOnly] private Vector3 m_AntiSlide;
		private void AntiSlide(float deltaTime)
		{
			if (!Agent.isOnNavMesh) return;
			if (!TryGetNavMeshOwner(out Component component)) return;
			if (!component.TryGetComponent(out Rigidbody ground)) return;

			Vector3 currentPosition = Agent.nextPosition;
			Vector3 groundVelocity = ground.GetPointVelocity(currentPosition);

			m_AntiSlide = groundVelocity * deltaTime;
			Vector3 newPosition = currentPosition + m_AntiSlide;

			Agent.nextPosition = newPosition;
		}

		private bool TryGetNavMeshOwner(out Component owner)
		{
			owner = null;
			if (Agent.navMeshOwner is GameObject gameObject)
			{
				owner = gameObject.transform;
				return true;
			}
			
			if (Agent.navMeshOwner is Component component)
			{
				owner = component;
				return true;
			}

			return false;
		}
	}
}


