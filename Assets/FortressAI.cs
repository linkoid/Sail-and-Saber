using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PirateGame.Crew;

namespace PirateGame.Ships
{
	public class FortressAI : ShipAIBase
	{
		void FixedUpdate()
		{
			if (Ship.IsRaided || Ship.IsPlundered || Ship.Health <= 0) return;

			Aggro();

			FireAllCannons();
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
			base.OnSink();
			(Ship as Fortress).Capture();
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

