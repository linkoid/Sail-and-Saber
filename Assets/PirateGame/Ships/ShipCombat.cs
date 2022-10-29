using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PirateGame.Ships
{
	public class ShipCombat : Ship.ShipBehaviour
	{

		public float TargetRange = 100;
		private float TargetRangeSqr => TargetRange * TargetRange;


		public CannonGroup BroadsideCannons;



		[ReadOnly] public Ship Target;

		[ReadOnly] public List<Ship> NearbyShips = new List<Ship>();


		void FixedUpdate()
		{
			TargetNearestShip();
			FireBroadsideCannons();
		}


		public void TargetNearestShip()
		{
			FindNearbyShips();
			Target = null;
			float minDistSqr = Mathf.Infinity;
			foreach (var ship in NearbyShips)
			{
				if (Target == null)
				{
					Target = ship;
					continue;
				}

				float distSqr = (Target.Rigidbody.position - Ship.Rigidbody.position).sqrMagnitude;
				if (minDistSqr < distSqr)
				{
					Target = ship;
					minDistSqr = distSqr;
				}
			}
		}

		private void FindNearbyShips()
		{
			NearbyShips.Clear();
			foreach (var ship in GameObject.FindObjectsOfType<Ship>())
			{
				if (ship == Ship) continue;

				// TODO Also check to make sure the ship isn't a friendly or already sunk

				if ((ship.Rigidbody.position - Ship.Rigidbody.position).sqrMagnitude <= TargetRangeSqr)
				{
					NearbyShips.Add(ship);
				}
			}
		}


		/// <summary>
		/// Figure out which side to use and if the target is close enough
		/// </summary>
		public void FireBroadsideCannons()
		{
			foreach (var cannon in BroadsideCannons.GetAllInRange(Target.Rigidbody.position))
			{
				cannon.Fire(Target.Rigidbody.position);
			}
		}

		void OnDrawGizmosSelected()
		{
			if (Target == null) return;
			
			Gizmos.DrawRay(Ship.Rigidbody.position, Target.Rigidbody.position);
		}
	}
}

