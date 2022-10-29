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

		public CannonGroup BroadsideCannons { get => _BroadsideCannons; private set => _BroadsideCannons = value; }
		[SerializeField] private CannonGroup _BroadsideCannons;




		[ReadOnly] public Ship Target;

		[ReadOnly] public List<Ship> NearbyShips = new List<Ship>();



		void FixedUpdate()
		{
			TargetNearestShip();
			FireBroadsideCannons();
			CheckCanRaid();
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

		#region Track Radiable Ships
		[System.Serializable]
		public class ShipCollidersDictionary : UnityDictionary<Ship, List<Collider>> { }

		[SerializeField, UnityDictionary("Ship", "Colliders")]
		private ShipCollidersDictionary m_RaidableShips = new ShipCollidersDictionary();

		void OnTriggerEnter(Collider other)
		{
			if (other.isTrigger || other.attachedRigidbody == null) return;
			if (!other.attachedRigidbody.TryGetComponent(out Ship otherShip)) return;
			if (otherShip == Ship) return;

			if (!m_RaidableShips.TryGetValue(otherShip, out List<Collider> colliderList))
			{
				colliderList = new List<Collider>();
				m_RaidableShips[otherShip] = colliderList;
			}
			colliderList.Add(other);
		}
		void OnTriggerExit(Collider other)
		{
			if (other.isTrigger || other.attachedRigidbody == null) return;
			if (!other.attachedRigidbody.TryGetComponent(out Ship otherShip)) return;
			if (otherShip == Ship) return;

			var list = m_RaidableShips[otherShip];
			list.Remove(other);
			if (list.Count <= 0)
			{
				m_RaidableShips.Remove(otherShip);
			}
		}
		#endregion // Track Raidable Ships

		[SerializeField, ReadOnly] private bool m_CanRaid = false;
		public bool CheckCanRaid()
		{
			m_CanRaid = m_RaidableShips.ContainsKey(Target);
			return m_CanRaid;
		}

		void OnDrawGizmosSelected()
		{
			if (Target == null) return;
			
			Gizmos.DrawRay(Ship.Rigidbody.position, Target.Rigidbody.position);
		}
	}
}

