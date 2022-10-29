using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace PirateGame.Humanoids
{
	[RequireComponent(typeof(NavMeshAgent))]
	public class SimplePathfind : MonoBehaviour
	{
		public Transform Goal;
		private NavMeshLinkInstance NavMeshLinkInstance;
		private NavMeshAgent Agent => this.GetComponent<NavMeshAgent>();

		// Update is called once per frame
		void FixedUpdate()
		{
			if (Goal == null) return;
			if (!Goal.gameObject.activeInHierarchy) return;

			Agent.SetDestination(Goal.position);
		}
	}
}


