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

		public AIHumanoid Humanoid => this.GetComponent<AIHumanoid>();

        public int health = 10;
        public int strength = 1;

		void Start()
		{
			_pathfindGoal = CreatePathfindGoal();
		}

        public void TakeDamage(int damage)
        {
            health -= damage;
            Debug.Log("Took damage! This one has " + health + " health left");
        }

		/// <summary>
		/// Raid the specified ship, attacking the specified enemy
		/// </summary>
		public void Raid(Ship ship, Crewmate enemy)
		{
			Humanoid.Standby();
			Humanoid.Attack(enemy.Humanoid);
		}

		/// <summary>
		/// Support the specified ship, at the specified supportObject.
		/// </summary>
		public void Support(Ship ship, Component supportObject)
		{
			Humanoid.Standby();
			Humanoid.Goal = supportObject;
		}

		/// <summary>
		/// Defend the specified ship, by attacking the specified enemy
		/// </summary>
		public void Defend(Ship ship, Crewmate enemy)
		{
			Humanoid.Standby();
			Humanoid.Defend(enemy.Humanoid);
		}

		/// <summary>
		/// Board the specified ship, waiting at a random location.
		/// </summary>
		public void Board(Ship ship)
		{
			PathfindGoal.parent = ship.transform;
			PathfindGoal.position = GetRandomNavPointOnShip(ship);
			Board(ship, PathfindGoal);
		}

		/// <summary>
		/// Board the specified ship, and wait at the specified standbyObject
		/// </summary>
		public void Board(Ship ship, Component standbyObject)
		{
			Humanoid.Standby();
			//PathfindGoal.SetParent(ship.transform, true);
			Humanoid.Goal = standbyObject;
			//Humanoid.Agent.Warp(PathfindGoal.position);
		}

		private Transform CreatePathfindGoal()
		{
			_pathfindGoal = new GameObject("PathfindGoal").transform;
			return _pathfindGoal;
		}

		private Vector3 GetRandomNavPointOnShip(Ship ship)
		{
			Vector3 randomPos = new Vector3(Random.value, 0.5f, Random.value);
			randomPos -= Vector3.one * 0.5f;
			Bounds bounds = ship.NavMeshSurface.navMeshData.sourceBounds;
			randomPos = Vector3.Scale(randomPos, bounds.size);
			randomPos = ship.transform.TransformPoint(randomPos);

			NavMeshQueryFilter filter = new NavMeshQueryFilter()
			{
				agentTypeID = Humanoid.Agent.agentTypeID,
				areaMask = Humanoid.Agent.areaMask,
			};

			if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, Humanoid.Agent.height * 2, filter))
			{
				return hit.position;
			}
			else
			{
				return ship.transform.position;
			}
		}
	}
}
