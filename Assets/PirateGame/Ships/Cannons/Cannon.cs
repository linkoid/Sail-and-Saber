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

		[SerializeField] private Transform m_RangeOrigin;
		[SerializeField] private Transform m_AimTarget;
		[SerializeField] private Transform m_RestTarget;
		[SerializeField] private ParticleSystem m_Emitter;
		[SerializeField] private GameObject m_CannonBall;

		[SerializeField] private bool m_FireButton = false;

		void Start()
		{
			m_Emitter.Stop(true);
		}

		void OnValidate()
		{
			if (Application.IsPlaying(this) && m_FireButton)
			{
				Fire(m_Emitter.transform.position);
				m_FireButton = false;
			}
			
		}

		public bool CheckInRange(Vector3 target)
		{
			// XXX : Probably broken

			Vector3 xzTarget = Vector3.ProjectOnPlane(target, m_RangeOrigin.up   );
			Vector3 xyTarget = Vector3.ProjectOnPlane(target, m_RangeOrigin.right);
			float xzAngle = Vector3.Angle(xzTarget - m_RangeOrigin.position, m_RangeOrigin.forward);
			float xyAngle = Vector3.Angle(xyTarget - m_RangeOrigin.position, m_RangeOrigin.forward);

			if (xzAngle > Angle.x) return false;
			if (xyAngle > Angle.y) return false;

			float distSqrd = (target - m_RangeOrigin.position).sqrMagnitude;
			if (distSqrd > RangeSqrd) return false;

			return true;
		}

		public void Fire(Vector3 target)
		{
			m_FireGizmoTarget = target;

			// stop any existing fire animations
			this.StopCoroutine("FireAnimation");

			// start fire animation
			StartCoroutine(FireAnimation(target, Random.Range(0.1f, 0.5f)));


		}

		private IEnumerator FireAnimation(Vector3 target, float delay)
		{
			float startTime = Time.time;

			Debug.Log("Firing!");

			// Aim the cannon
			for (float elapsed = 0; elapsed < delay; elapsed = Time.time - startTime)
			{
				Aim(target, elapsed / delay);
				yield return new WaitForSeconds(0);
			}

			Aim(target);

			// Fire the cannon
			m_Emitter.Play(true);
			yield return new WaitForSeconds(m_Emitter.main.duration);
			m_Emitter.Stop(true);
			m_FireGizmoTarget = Vector3.positiveInfinity;

			// TODO : Maybe slowly return the cannon to neutral position?
		}

		private void Aim(Vector3 target, float factor=1f)
		{
			var currentRotation = m_AimTarget.rotation;
			var targetRotation = Quaternion.LookRotation((target - m_AimTarget.position).normalized, this.transform.up);
			m_AimTarget.rotation = Quaternion.Lerp(currentRotation, targetRotation, factor);
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.matrix = m_RangeOrigin.localToWorldMatrix;
			float angle = Mathf.Max(Angle.x, Angle.y);
			float range = angle > 180 ? -Range : Range;
			float ratio = Mathf.Tan(Mathf.Deg2Rad*Angle.x/2) / Mathf.Tan(Mathf.Deg2Rad*Angle.y/2);
			Gizmos.DrawFrustum(Vector3.zero, Angle.y, range, 0, ratio);
			//Gizmos.DrawFrustum(Vector3.zero, Angle.y, Angle.y > 180 ? -Range : Range, 0, 0);
		}

		Vector3 m_FireGizmoTarget = Vector3.positiveInfinity;
		void OnDrawGizmos()
		{
			if (m_FireGizmoTarget == Vector3.positiveInfinity) return;

			Gizmos.color = Color.red;
			Gizmos.DrawLine(m_RangeOrigin.position, m_FireGizmoTarget);

			float dist = Vector3.Distance(m_FireGizmoTarget, m_RangeOrigin.position);
			Gizmos.matrix = Matrix4x4.LookAt(m_RangeOrigin.position, m_FireGizmoTarget, m_RangeOrigin.up);
			Gizmos.DrawFrustum(Vector3.zero, Angle.x / 10, dist, 0, Angle.x / Angle.y);
		}
	}
}

