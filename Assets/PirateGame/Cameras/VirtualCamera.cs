using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PirateGame.Cameras
{
	public class VirtualCamera : MonoBehaviour
	{
		public CameraState State => new CameraState()
		{
			Position = transform.position,
			Rotation = transform.rotation,
			FieldOfView = FieldOfView,
		};

		[Range(1e-05f, 179f)]
		public float FieldOfView = 60;
	}

	public struct CameraState
	{
		public Vector3 Position;
		public Quaternion Rotation;
		public float FieldOfView;

		/// <summary>
		/// Update the given camera to match the virtual camera.
		/// </summary>
		/// <remarks>Should be called by the camera itself.</remarks>
		public void ApplyTo(Camera camera)
		{
			camera.fieldOfView = this.FieldOfView;
			camera.transform.position = this.Position;
			camera.transform.rotation = this.Rotation;
		}
	}
}

