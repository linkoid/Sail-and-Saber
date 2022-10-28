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


		[SerializeField] private Vector2 m_BroadsideAngle = new Vector2(90, 30);
		[SerializeField] private float m_BroadsideRange = 50;



		[ReadOnly] public Ship Target;

		[ReadOnly] public List<Ship> NearbyShips = new List<Ship>();


		void FixedUpdate()
		{
			TargetNearestShip();
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





		
		void OnDrawGizmos()
		{
			Matrix4x4 rightMatrix = Matrix4x4.LookAt(Vector3.zero, Vector3.right, Vector3.up);
			Matrix4x4 leftMatrix  = Matrix4x4.LookAt(Vector3.zero, Vector3.left , Vector3.up);

			Gizmos.color = Color.yellow;

			Gizmos.matrix = Ship.transform.localToWorldMatrix * rightMatrix;
			Gizmos.DrawFrustum(Vector3.zero, m_BroadsideAngle.y, m_BroadsideRange, 0, m_BroadsideAngle.x / m_BroadsideAngle.y);
			
			Gizmos.matrix = Ship.transform.localToWorldMatrix * leftMatrix;
			Gizmos.DrawFrustum(Vector3.zero, m_BroadsideAngle.y, m_BroadsideRange, 0, m_BroadsideAngle.x / m_BroadsideAngle.y);

			if (Target != null)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawRay(Ship.Rigidbody.position, Target.Rigidbody.position);
			}
		}
	}
}

