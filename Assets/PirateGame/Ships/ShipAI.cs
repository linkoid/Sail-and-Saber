using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CustomUtils;
using PirateGame.Crew;

namespace PirateGame.Ships
{
	public abstract class ShipAIBase : Ship.ShipBehaviour
	{
		protected virtual void Start()
		{
			CreateCrew();
		}

		[Header("Config")]
		[SerializeField] protected int m_CrewCount = 5;
		[SerializeField] protected int m_RaidCrewCount = 10;
		[SerializeField] protected CrewDirector m_Crew;
		[SerializeField, Min(0)] protected float m_AggroRadius = 50;
		[SerializeField, Min(0)] protected float m_AttackRadius = 20;


		[Header("Rewards")]
		[SerializeField, SpanRange(0, 50), SpanInt]
		protected Span m_SinkGoldReward = new Span(0, 3);

		[SerializeField, Range(0, 20)]
		protected int m_PlunderGoldBonus = 3;

		[SerializeField, SpanRange(0, 10), SpanInt]
		protected Span m_PlunderCrewReward = new Span(0, 1);

		[Header("Info")]
		[SerializeField, ReadOnly]
		protected bool m_RaidCrewSpawned = false;

		protected CrewDirector CreateCrew()
		{
			if (m_Crew == null)
			{
				m_Crew = new GameObject("EnemyCrew", typeof(CrewDirector)).GetComponent<CrewDirector>();
				m_Crew.transform.parent = this.transform;
			}
			this.Ship.AssignCrew(m_Crew);
			m_Crew.Board(this.Ship);
			m_Crew.Count = m_CrewCount;
			return m_Crew;
		}

		protected override void OnRaid()
		{
			base.OnRaid();
			if (!m_RaidCrewSpawned)
			{
				m_Crew.Count = m_RaidCrewCount;
				m_RaidCrewSpawned = true;
			}
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

		protected Player FindNearestPlayer()
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

		protected virtual void FireAllCannons()
		{

			if (Ship.Internal.Combat.Target != null && !Ship.IsRaided)
			{
				var cannons = Ship.Internal.Combat.GetDeckCannonsInRange();

				m_Crew.ManCannons(cannons);

				Ship.Internal.Combat.FireBroadsideCannons();
				Ship.Internal.Combat.FireDeckCannons();
			}
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(Ship.Rigidbody.position, m_AggroRadius);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(Ship.Rigidbody.position, m_AttackRadius);
		}
	}


	public class ShipAI : ShipAIBase
	{

		protected void FixedUpdate()
		{
			Ship.Internal.Combat.TargetRange = m_AggroRadius;

			if (Ship.IsRaided || Ship.IsPlundered || Ship.Health <= 0) return;

			if (!Aggro())
			{
				Patrol();
			}

			FireAllCannons();
		}

		protected override void OnRaid()
		{
			base.OnRaid();
			Ship.Internal.Physics.Stop();
		}

		/// <summary>
		/// Chase the nearest valid target
		/// </summary>
		/// <returns>true if found a target</returns>
		protected bool Aggro()
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

		protected void Attack(Ship target)
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

		protected void Chase(Ship target)
		{
			// Go straight for the ship
			Ship.Internal.Physics.MoveTowards(target.Rigidbody.position);
		}

		protected void Patrol()
		{
			Ship.Internal.Physics.MoveTowards(Ship.Rigidbody.position);
		}

		protected Ship FindNearestPlayerShip()
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
	}
}

