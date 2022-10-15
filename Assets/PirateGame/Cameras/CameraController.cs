using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CustomUtils;

namespace PirateGame.Cameras
{
	[RequireComponent(typeof(Camera))]
	public class CameraController : MonoBehaviour
	{
		public VirtualCamera ActiveVCam { get => _activeVCam; protected set => _activeVCam = value; }
		[SerializeField] VirtualCamera _activeVCam;

		public Camera Camera => GetComponent<Camera>();

		// Update is called once per frame
		void Update()
		{
			ActiveVCam.State.ApplyTo(Camera);
		}

		void SetVCam(VirtualCamera camera)
		{
			ActiveVCam = camera;
		}
	}
}

