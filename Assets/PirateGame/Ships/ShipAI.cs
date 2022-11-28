using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CustomUtils;
using PirateGame.Crew;

namespace PirateGame.Ships
{
	public class ShipAI : Ship.ShipBehaviour
	{
		[Header("Config")]
		[SerializeField]
		private int m_CrewCount = 5;

		[SerializeField]
		private CrewDirector m_Crew;

		[SerializeField, Min(0)]
		private float m_AggroRadius = 50;

		[SerializeField, Min(0)]
		private float m_AttackRadius = 20;

		[Header("Rewards")]
		[SerializeField, SpanRange(0, 50), SpanInt]
		private Span m_SinkGoldReward = new Span(0, 3);

		[SerializeField, Range(0, 20)]
		private int m_PlunderGoldBonus = 3;

		[SerializeField, SpanRange(0, 10), SpanInt]
		private Span m_PlunderCrewReward = new Span(0, 1);


		[Header("Info")]
		[SerializeField, ReadOnly]
		private bool m_RaidCrewSpawned = false;


		// Start is called before the first frame update
		void Start()
		{
		}

		private CrewDirector CreateCrew()
		{
			if (m_Crew == null)
			{
				m_Crew = new GameObject("EnemyCrew", typeof(CrewDirector)).GetComponent<CrewDirector>();
				m_Crew.transform.parent = this.transform;
			}
			m_Crew.Count = m_CrewCount;
			this.Ship.AssignCrew(m_Crew);
			m_Crew.Board(this.Ship);
			return m_Crew;
		}

		// Update is called once per frame
		void Update()
		{
			
		}

		void FixedUpdate()
		{
			if (Ship.IsRaided || Ship.IsPlundered || Ship.Health <= 0) return;

			if (!Aggro())
			{
				Patrol();
			}

			if (Ship.Internal.Combat.Target != null && !Ship.IsRaided)
			{
				Ship.Internal.Combat.FireBroadsideCannons();
				Ship.Internal.Combat.FireDeckCannons();
			}
		}

		protected override void OnRaid()
		{
			base.OnRaid();
			if (!m_RaidCrewSpawned)
			{
				CreateCrew();
				m_RaidCrewSpawned = true;
			}
			Ship.Internal.Physics.Stop();
		}

		protected override void OnSink()
		{
			if (m_Crew != null)
			{
				Object.Destroy(m_Crew.gameObject);
			}

			Player player = FindNearestPlayer();
			player.Gold += Mathf.RoundToInt(m_SinkGoldReward.RandomInRange());
		}

		protected override void OnPlunder()
		{
			base.OnPlunder();

			Player player = FindNearestPlayer();
			player.Gold += m_PlunderGoldBonus;
			player.CrewCount += Mathf.RoundToInt(m_PlunderCrewReward.RandomInRange());
		}

		/// <summary>
		/// Chase the nearest valid target
		/// </summary>
		/// <returns>true if found a target</returns>
		private bool Aggro()
		{
			Ship target = FindNearestPlayerShip();
			if (target == null) return false;

			Ship.Internal.Combat.Target = target;

			float distance = Vector3.Distance(target.Rigidbody.position, Ship.Rigidbody.position);
			if (distance > m_AggroRadius) return false;

			if (distance <= m_AttackRadius)
			{
				Attack(target);
			}
			else
			{
				Chase(target);
			}

			return true;
		}

		private void Attack(Ship target)
		{
			ShipPhysics physics = Ship.Internal.Physics;

			physics.Throttle *= 0.9f;

			// Get directions
			Vector3 currentDirection = physics.GetCurrentDirection();
			Vector3 enemyDirection = physics.ProjectOnWater(physics.Rigidbody.position - target.Rigidbody.position);
			Vector3 perpendicular = Vector3.Cross(enemyDirection, physics.GetWaterNormal());

			// Find nearest perpendicular direction
			Vector3 targetDirection;
			if (Vector3.Distance(currentDirection, perpendicular) < Vector3.Distance(currentDirection, -perpendicular))
			{
				targetDirection = perpendicular;
			}
			else
			{
				targetDirection = -perpendicular;
			}

			// Turn a side towards the ship
			physics.RotateTowards(targetDirection);
		}

		private void Chase(Ship target)
		{
			// Go straight for the ship
			Ship.Internal.Physics.MoveTowards(target.Rigidbody.position);
		}

		private void Patrol()
		{
			Ship.Internal.Physics.MoveTowards(Ship.Rigidbody.position);
		}

		private Ship FindNearestPlayerShip()
		{
			Ship nearestShip = null;

			var shipControllers = Object.FindObjectsOfType<ShipController>(false);
			float minDistance = Mathf.Infinity;
			foreach (var shipController in shipControllers)
			{
				float distance = Vector3.Distance(this.Ship.Rigidbody.position, shipController.Rigidbody.position);
				if (distance < minDistance)
				{
					minDistance = distance;
					nearestShip = shipController.GetComponentInParent<Ship>();
				}
			}

			return nearestShip;
		}

		private Player FindNearestPlayer()
		{
			// XXX For now only gets first player

			Player nearestPlayer = null;
			foreach (var player in Object.FindObjectsOfType<Player>())
			{
				if (nearestPlayer == null)
				{
					nearestPlayer = player;
				}
				else
				{
					Debug.LogError("Found multiple players!", this);
				}
			}

			return nearestPlayer;
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(Ship.Rigidbody.position, m_AggroRadius);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(Ship.Rigidbody.position, m_AttackRadius);
		}
	}
}

