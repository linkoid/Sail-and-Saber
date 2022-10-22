using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


namespace PirateGame.Cameras
{
	public class VirtualCamera : MonoBehaviour
	{
		[Range(1e-05f, 179f)]
		public float FieldOfView = 60;

		[SerializeField, HideInInspector]
		private Rigidbody m_Rigidbody;

		[SerializeField, ReadOnly] private Vector3 m_Position;
		[SerializeField, ReadOnly] private Quaternion m_Rotation;

		void Awake()
		{
			if (!this.TryGetComponent(out m_Rigidbody))
			{
				m_Rigidbody = this.AddComponent<Rigidbody>();
			}
			m_Rigidbody.isKinematic = true;
			m_Rigidbody.useGravity = false;
			m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		}

		void Update()
		{
			transform.position = m_Position;
			transform.rotation = m_Rotation;
		}

		void LateUpdate()
		{
			m_Position = transform.position;
			m_Rotation = transform.rotation;
		}

		public CameraState State => new CameraState()
		{
			Position = transform.position,
			Rotation = transform.rotation,
			FieldOfView = FieldOfView,
		};

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.matrix = this.transform.localToWorldMatrix;
			Gizmos.DrawFrustum(Vector3.zero, this.FieldOfView, 10, 0, 16/9.0f);
		}
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

