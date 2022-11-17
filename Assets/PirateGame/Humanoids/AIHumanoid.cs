using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PirateGame.Humanoids
{
	[RequireComponent(typeof(NavMeshAgent))]
	public class AIHumanoid : MonoBehaviour, IHumanoid
	{
		public Transform Goal;
		private NavMeshLinkInstance NavMeshLinkInstance;

		public bool IsJumping => CheckIsJumping();
		public bool IsGrounded => CheckIsGrounded();
		public Vector3 Movement => Agent.desiredVelocity;
		public Vector3 Velocity => Agent.velocity;
		public Rigidbody Rigidbody => this.GetComponent<Rigidbody>();

		public NavMeshAgent Agent => this.GetComponent<NavMeshAgent>();

		// Update is called once per frame
		void FixedUpdate()
		{
			if (Goal == null) return;
			if (!Goal.gameObject.activeInHierarchy) return;

			Agent.SetDestination(Goal.position);
		}

		private bool CheckIsJumping()
		{
			if (Agent.isOnOffMeshLink)
			{
				if (Agent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeJumpAcross)
				{
					return true;
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
	}
}


