using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.CustomUtils;
using UnityEngine.Rendering;
using StackTrace = System.Diagnostics.StackTrace;

namespace PirateGame.Ships
{
	public class ShipCombat : Ship.ShipBehaviour
	{
		public float DamagePerCannon = 10;
		public float TargetRange = 100;
		public float ReloadDelay = 2;

		private float TargetRangeSqr => TargetRange * TargetRange;

		public CannonGroup BroadsideCannons { get => _BroadsideCannons; private set => _BroadsideCannons = value; }
		[SerializeField] private CannonGroup _BroadsideCannons;

		public CannonGroup DeckCannons { get => _deckCannons; private set => _deckCannons = value; }
		[SerializeField] private CannonGroup _deckCannons;

		[SerializeField] private Transform m_TargetPointsParent;
		public IEnumerable<Transform> TargetPoints
		{ 
			get 
			{
				for (int i = 0; i < m_TargetPointsParent.childCount; i++)
				{
					yield return m_TargetPointsParent.GetChild(i).transform;
				}
			} 
		} 


		[ReadOnly] public Ship Target;

		[ReadOnly] public List<Ship> NearbyShips = new List<Ship>();


		void FixedUpdate()
		{
			//TargetNearestShip();
			//CheckCanRaid();
		}


		public Ship TargetNearestShip()
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
			return Target;
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

		private void FireCannons(CannonGroup cannonGroup)
		{
			if (!CheckHasValidTarget()) return;

			if (cannonGroup.IsReloading) return; // Can't fire because not done reloading

			foreach (var cannon in cannonGroup.GetAllInRange(Target.Internal.Combat.TargetPoints))
			{
				cannon.Fire(Target.Internal.Combat.TargetPoints);
				// Deal damage based on how many cannons fired
				Target.TakeDamage(DamagePerCannon);
			}

			cannonGroup.Reload(ReloadDelay);
		}

		/// <summary>
		/// Fire the boradside cannons at the current target.
		/// </summary>
		public void FireBroadsideCannons()
		{
			FireCannons(BroadsideCannons);
		}

		/// <summary>
		/// Fire the deck cannons at the current target.
		/// </summary>
		public void FireDeckCannons()
		{
			FireCannons(DeckCannons);
		}


		/// <summary>
		/// Find which deck cannons are in range of the target.
		/// Useful for making crewmates man the cannons.
		/// </summary>
		public IEnumerable<Cannon> GetDeckCannonsInRange()
		{
			return DeckCannons.GetAllInRange(Target.Internal.Combat.TargetPoints);
		}

		public bool HasCannonsInRange()
		{
			foreach (var _ in DeckCannons.GetAllInRange(Target.Internal.Combat.TargetPoints))
			{
				// Return true for the first cannon found
				return true;
			}

			foreach (var _ in BroadsideCannons.GetAllInRange(Target.Internal.Combat.TargetPoints))
			{
				// Return true for the first cannon found
				return true;
			}

			return false;
		}

		#region Track Radiable Ships
		[System.Serializable]
		public class ShipCollidersDictionary : UnityDictionary<Ship, List<Collider>> { }

		[SerializeField, UnityDictionary("Ship", "Colliders"), ReadOnly]
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
			if (!CheckHasValidTarget()) return false;
			m_CanRaid = m_RaidableShips.ContainsKey(Target);
			return m_CanRaid;
		}

		protected override void OnShipCollisionEnter(Collision collision)
		{
			//Debug.Log("RAM!", this);
		}

		private bool CheckHasValidTarget()
		{
			StackTrace stackTrace = new StackTrace();
			string caller = stackTrace.GetFrame(1).GetMethod().Name;

			if (Target == null)
			{
				Debug.LogWarning($"{caller}: Target is null\n{stackTrace}", this);
				return false;
			}

			return true;
		}

		void OnDrawGizmosSelected()
		{
			if (Target == null) return;
			
			Gizmos.DrawRay(Ship.Rigidbody.position, Target.Rigidbody.position);

			Gizmos.color = Color.red;
			foreach (var cannon in DeckCannons.GetAllInRange(Target.Rigidbody.position))
			{
				Gizmos.DrawRay(cannon.transform.position, Target.Rigidbody.position - cannon.transform.position);
			}
		}
	}
}

