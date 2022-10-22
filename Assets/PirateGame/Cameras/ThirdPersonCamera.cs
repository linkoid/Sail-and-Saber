using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Cameras
{
	[ExecuteAlways]
	[RequireComponent(typeof(VirtualCamera))]
	public class ThirdPersonCamera : MonoBehaviour
	{
		public Transform Target;
		public float Distance = 5;
		public float Height = 3;
		[Range(0,1)] public float TranslationPower = 0.9f;
		[Range(0,1)] public float RotationPower = 0.9f;

		[Range(0,1)] public float OrbitPower = 0.9f;
		[SerializeField] Vector3 OrbitDirection;
		void LateUpdate()
		{
			Vector3 targetPosition = Target.position;
			if (Target.TryGetComponent(out Rigidbody targetRigidbody))
			{
				// These values are better interpolated
				targetPosition = targetRigidbody.position;
			}
		
			// Find the direction from the camera to the target
			Vector3 direction = (Target.position - this.transform.position).normalized;
			Vector3 up = -Physics.gravity.normalized;

			// flatten the direction to a plane defined by the up vector
			Vector3 flatDirection = Vector3.ProjectOnPlane(direction, up).normalized;
	
			Vector3 toWorld = Target.TransformDirection(OrbitDirection);
			flatDirection = Vector3.Slerp( flatDirection,  Vector3.ProjectOnPlane(toWorld,up).normalized,OrbitPower);
			Vector3 goalPosition = Target.position - flatDirection * Distance + up * Height;
			this.transform.position = Vector3.Lerp(this.transform.position, goalPosition, TranslationPower);

			Quaternion goalRotation = Quaternion.LookRotation(direction, up);
			this.transform.rotation = Quaternion.Slerp(this.transform.rotation, goalRotation, RotationPower);
		}
	}

}
