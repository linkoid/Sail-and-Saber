using System.Collections.Generic;
using UnityEngine;
using PirateGame.Ships;
using PirateGame.Humanoids;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.Experimental.AI;

namespace PirateGame.Crew
{
	/// <summary>
	/// Handles automated tasks, pathfinding, and animations for a crewmate.
	/// </summary>
	[RequireComponent(typeof(AIHumanoid))]
	public class Crewmate : MonoBehaviour
	{
		private Transform PathfindGoal => (_pathfindGoal != null) ? _pathfindGoal : CreatePathfindGoal();
		[SerializeField, ReadOnly] private Transform _pathfindGoal;

		private AIHumanoid Humanoid => this.GetComponent<AIHumanoid>();

		/// <summary>
		/// Raid the specified ship, attacking the specified enemy
		/// </summary>
		public void Raid(Ship ship, Crewmate enemy)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Support the specified ship, at the specified supportObject.
		/// </summary>
		public void Support(Ship ship, GameObject supportObject)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Defend the specified ship, by attacking the specified enemy
		/// </summary>
		public void Defend(Ship ship, Crewmate enemy)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Board the specified ship, waiting at a random location.
		/// </summary>
		public void Board(Ship ship)
		{
			Board(ship, ship.gameObject);
			if (NavMesh.FindClosestEdge(ship.NavMeshSurface.navMeshData.position, out NavMeshHit hit, Humanoid.Agent.areaMask))
			{
				PathfindGoal.position = hit.position;
			}
			else
			{
				PathfindGoal.position = ship.transform.position;
			}
		}

		/// <summary>
		/// Board the specified ship, and wait at the specified standbyObject
		/// </summary>
		public void Board(Ship ship, GameObject standbyObject)
		{
			PathfindGoal.SetParent(ship.transform, true);
			PathfindGoal.position = standbyObject.transform.position;
			Humanoid.Goal = PathfindGoal;
		}

		private Transform CreatePathfindGoal()
		{
			_pathfindGoal = new GameObject("PathfindGoal").transform;
			return _pathfindGoal;
		}
	}
}
