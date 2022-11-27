using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PirateGame.Crew;

namespace PirateGame.Ships
{
	public class FortressAI : Ship.ShipBehaviour
	{
		[SerializeField]
		private int m_CrewCount = 5;

		[SerializeField]
		private int m_RaidCrewCount = 10;

		[SerializeField]
		private CrewDirector m_Crew;

		[SerializeField, Min(0)]
		private float m_AttackRadius = 20;


		// Start is called before the first frame update
		void Start()
		{
			CreateCrew();
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
			if (Ship.IsRaided || Ship.IsPlundered) return;

			Aggro();

			if (Ship.Internal.Combat.Target != null && !Ship.IsRaided)
			{
				Ship.Internal.Combat.FireBroadsideCannons();
				Ship.Internal.Combat.FireDeckCannons();
			}
		}

		protected override void OnRaid()
		{
			base.OnRaid();
			m_Crew.Count = m_RaidCrewCount;
		}

		protected override void OnSink()
		{
		}

		/// <summary>
		/// Chase the nearest valid target
		/// </summary>
		/// <returns>true if found a target</returns>
		private bool Aggro()
		{
			Ship target = FindNearestPlayerShip();
			if (target == null) return false;

			var shipInternal = Ship.Internal;
			var combat = shipInternal.Combat;
			var combatTarget = Ship.Internal;

			Ship.Internal.Combat.Target = target;

			return true;
		}

		private Ship FindNearestPlayerShip()
		{
			Ship nearestShip = null;

			var shipControllers = Object.FindObjectsOfType<ShipController>(false);
			float minDistance = Mathf.Infinity;
			foreach (var shipController in shipControllers)
			{
				float distance = Vector3.Distance(this.transform.position, shipController.Rigidbody.position);
				if (distance < minDistance)
				{
					minDistance = distance;
					nearestShip = shipController.GetComponentInParent<Ship>();
				}
			}

			return nearestShip;
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(this.transform.position, m_AttackRadius);
		}
	}
}

