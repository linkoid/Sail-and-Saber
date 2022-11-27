using System.Collections.Generic;
using UnityEngine;
using PirateGame.Ships;
using PirateGame.Humanoids;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.Experimental.AI;
using UnityEditor;

namespace PirateGame.Crew
{
	/// <summary>
	/// Handles automated tasks, pathfinding, and animations for a crewmate.
	/// </summary>
	[RequireComponent(typeof(AIHumanoid))]
	public class Crewmate : MonoBehaviour
	{
		public int Health = 10;
		public int Strength = 1;
		
		[SerializeField] private SoundEffect m_YarrSound;
		[SerializeField] private SoundEffect m_ArrgSound;

		private Transform PathfindGoal => (_pathfindGoal != null) ? _pathfindGoal : CreatePathfindGoal();
		[SerializeField, ReadOnly] private Transform _pathfindGoal;

		public AIHumanoid Humanoid => this.GetComponent<AIHumanoid>();

		void Start()
		{
			_pathfindGoal = CreatePathfindGoal();
		}

        public void TakeDamage(int damage)
        {
            Health -= damage;
            //Debug.Log("Took damage! This one has " + Health + " health left");
			if (Health <= 0)
			{
				Humanoid.SendMessage("OnDie", true);
			}
        }

		/// <summary>
		/// Raid the specified ship, attacking the specified enemy
		/// </summary>
		public void Raid(Ship ship, Crewmate enemy)
		{
			Humanoid.Standby();
			Humanoid.Attack(enemy.Humanoid);
			TryPlaySound(m_ArrgSound, nameof(m_ArrgSound));
		}

		/// <summary>
		/// Support the specified ship, at the specified supportObject.
		/// </summary>
		public void Support(Ship ship, Component supportObject)
		{
			Humanoid.Standby();
			Humanoid.Goal = supportObject;
			TryPlaySound(m_YarrSound, nameof(m_YarrSound));
		}

		/// <summary>
		/// Defend the specified ship, by attacking the specified enemy
		/// </summary>
		public void Defend(Ship ship, Crewmate enemy)
		{
			Humanoid.Standby();
			Humanoid.Defend(enemy.Humanoid);
			TryPlaySound(m_ArrgSound, nameof(m_ArrgSound));
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
			if (ship is Fortress fort)
			{
				bounds.size *= 0.5f;
			}
			randomPos = Vector3.Scale(randomPos, bounds.size);
			randomPos = ship.transform.TransformPoint(randomPos);

			if (Humanoid.TryGetNearestPointOnNavMesh(randomPos, out Vector3 point))
			{
				return point;
			}
			else
			{
				return ship.transform.position;
			}
		}


		void OnDrawGizmos()
		{
			Handles.Label(this.transform.position, $"{Health}", new GUIStyle()
			{
				alignment = TextAnchor.MiddleCenter,
				fontStyle = FontStyle.Bold,
				fontSize = 20,
				normal = new GUIStyleState()
				{
					textColor = Color.red,
				},
			});
		}
		
		void TryPlaySound(SoundEffect sound, string name)
		{
			if (sound != null)
			{
				sound.Play();
			}
			else
			{
				Debug.LogWarning($"Crewmate: {name} has not been assigned", this);
			}
		}
	}
}
