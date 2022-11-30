using PirateGame.Crew;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace PirateGame.Humanoids
{
	[System.Serializable]
	public enum PathfindResult
	{
		Invalid = -3,
		RequestFail = -2,
		Fail = -1,
		Pending = 0,
		Success = 1,
	}

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

		[SerializeField, ReadOnly] private NavMeshPathStatus m_PathStatus;
		[SerializeField, ReadOnly] private PathfindResult m_PathfindResult;
		[SerializeField, ReadOnly] private Vector3 m_WarpPoint;
		[SerializeField, ReadOnly] private bool m_DidWarp;
		[SerializeField, ReadOnly] private bool m_HasPath;
		[SerializeField, ReadOnly] private bool m_PathPending;
		[SerializeField, ReadOnly] private bool m_IsPathStale;
		[SerializeField, ReadOnly] private NavMeshPath m_Path;


		[SerializeField, ReadOnly] private bool m_WillTeleport = false;
		[SerializeField, ReadOnly] private Vector3 m_TeleportTo;

		void Start()
		{
			Agent.updateUpAxis = true;
		}

		void Update()
		{
			AntiSlide(Time.deltaTime);
		}

        public bool GetCombat()
        {
            return m_IsInCombat;
        }

		// Update is called once per frame
		void FixedUpdate()
		{
			if (m_WillTeleport)
			{
				m_WillTeleport = false;
				Agent.Warp(m_TeleportTo);
				Agent.nextPosition = m_TeleportTo;
				Rigidbody.MovePosition(m_TeleportTo);
				this.transform.position = m_TeleportTo;
				m_WarpPoint = m_TeleportTo;
			}

			if (Goal == null) return;
			if (!Goal.gameObject.activeInHierarchy) return;

			if (!m_IsStationary)
			{
				m_PathfindResult = UpdateDestination(); 
				if ((int)m_PathfindResult < 0)
				{
					m_WarpPoint = Vector3.Lerp(m_WarpPoint, Goal.transform.position, 0.2f);
					bool _ = TryGetNearestPointOnNavMesh(m_WarpPoint, out Vector3 point);
					Agent.Warp(point);
					m_DidWarp = true;
				}
				else if (m_PathfindResult == PathfindResult.Success)
				{
					m_DidWarp = false;
					m_WarpPoint = this.transform.position;
				}
			}
			else
			{
				m_DidWarp = false;
				m_WarpPoint = this.transform.position;
			}

			Agent.enabled = true;

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

		public bool TryGetNearestPointOnNavMesh(in Vector3 target, out Vector3 point)
		{
			point = target;
			NavMeshQueryFilter filter = new NavMeshQueryFilter()
			{
				agentTypeID = Agent.agentTypeID,
				areaMask = Agent.areaMask,
			};

			float maxMultiplier = 10;

			for (float multiplier = 2; multiplier <= maxMultiplier; multiplier += 2)
			{
				if (NavMesh.SamplePosition(target, out NavMeshHit hit, Agent.height * multiplier, filter))
				{
					point = hit.position;
					return true;
				}
			}
			

			return false;
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

		public void Teleport(Vector3 position)
		{
			m_WillTeleport = true;
			m_TeleportTo = position;
			m_WarpPoint = position;
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

		/// <summary>
		/// Update the agent's destination to navigate to the goal.
		/// </summary>
		/// <returns><c>false</c> if, and only if, the agent cannot navigate to the destination</returns>
		PathfindResult UpdateDestination()
		{
			if (Agent.isOnOffMeshLink)
			{
				m_PathfindResult = PathfindResult.Success;
				goto returnResult;
			} 

			if (!Agent.isOnNavMesh && !Agent.isOnOffMeshLink)
			{
				m_PathfindResult = PathfindResult.RequestFail;
				goto returnResult;
			}

			if (false && !m_DidWarp && m_Path != null && m_Path.corners.Length > 0 && Vector3.Distance(Goal.transform.position, m_Path.corners.Last()) < Agent.stoppingDistance)
			{
				// No need to update the path
			}
			//else if (!Agent.SetDestination(Goal.transform.position))
			else 
			{
				NavMeshPath newPath = new NavMeshPath();
				if (!Agent.CalculatePath(Goal.transform.position, newPath))
				//if (!Agent.SetDestination(Goal.transform.position))
				{
					m_PathfindResult = PathfindResult.RequestFail;
					goto returnResult;
				}
				m_Path = newPath;
				Agent.path = m_Path;
				m_Path = Agent.path;
			}

			if (m_Path.status == NavMeshPathStatus.PathComplete)
				m_PathfindResult = PathfindResult.Success;
			else if (m_Path.status == NavMeshPathStatus.PathPartial)
				m_PathfindResult = PathfindResult.Fail;
			else if (m_Path.status == NavMeshPathStatus.PathInvalid)
				m_PathfindResult = PathfindResult.Invalid;

			if (m_Path.status != NavMeshPathStatus.PathInvalid)
			{
				Agent.path = m_Path;
			}

			returnResult:

			if (m_Path != null)
			{
				m_PathStatus = m_Path.status;
			}
			m_HasPath = Agent.hasPath;
			m_PathPending = Agent.pathPending;
			m_IsPathStale = Agent.isPathStale;

			return m_PathfindResult;
		}


		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(Goal.transform.position, 1);
			Gizmos.DrawLine(Goal.transform.position, this.transform.position);

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(Agent.pathEndPosition, 1.2f);
			Vector3 lastPoint = this.transform.position;
			foreach (var point in Agent.path.corners)
			{
				Gizmos.DrawLine(lastPoint, point);
				lastPoint = point;
			}

			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(m_WarpPoint, 1);
			Gizmos.DrawLine(m_WarpPoint, this.transform.position);
		}
	}
}


