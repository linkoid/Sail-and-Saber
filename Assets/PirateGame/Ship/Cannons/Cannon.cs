using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Ships
{
	public class Cannon : MonoBehaviour
	{
		public Vector2 Angle = new Vector2(90, 30);
		public float Range = 50;

		private float RangeSqrd => Range * Range;

		public bool CheckInRange(Vector3 target)
		{
			Vector3 xzTarget = Vector3.ProjectOnPlane(target, this.transform.up   );
			Vector3 xyTarget = Vector3.ProjectOnPlane(target, this.transform.right);
			float xzAngle = Vector3.Angle(xzTarget - this.transform.position, this.transform.forward);
			float xyAngle = Vector3.Angle(xyTarget - this.transform.position, this.transform.forward);

			if (xzAngle > Angle.x) return false;
			if (xyAngle > Angle.y) return false;

			float distSqrd = (target - this.transform.position).sqrMagnitude;
			if (distSqrd > RangeSqrd) return false;

			return true;
		}

		public void Fire(Vector3 target)
		{
			fireGizmoTarget = target;

			// TODO
			// fire animation
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.matrix = this.transform.localToWorldMatrix;
			Gizmos.DrawFrustum(Vector3.zero, Angle.y, Range, 0, Angle.x / Angle.y);
		}

		Vector3 fireGizmoTarget = new Vector3();
		void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(this.transform.position, fireGizmoTarget);

			float dist = Vector3.Distance(fireGizmoTarget, this.transform.position);
			Gizmos.matrix = Matrix4x4.LookAt(this.transform.position, fireGizmoTarget, this.transform.up);
			Gizmos.DrawFrustum(Vector3.zero, Angle.y / 10, dist, 0, Angle.x / Angle.y);

			fireGizmoTarget = this.transform.position;
		}
	}
}

