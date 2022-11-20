
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.CustomUtils;
using Cinemachine;
using PirateGame.Cameras;

namespace PirateGame.Ships
{
	[RequireComponent(typeof(PlayerInput))]
	public class ShipController : Ship.ShipBehaviour
	{
		public Vector3 Movement => m_Movement;
		public Rigidbody Rigidbody => Ship.Rigidbody;
		public PlayerInput PlayerInput => this.GetComponent<PlayerInput>();


        [SerializeField] public Camera m_Camera;
		[SerializeField] public CinemachineVirtualCameraBase VirtualCamera;

		[SerializeField, ReadOnly] private Vector3 m_Movement = Vector3.zero;

		[SerializeField, ReadOnly] private float m_Throttle;


        void Awake()
		{
			PlayerInput.enabled = this.enabled;
		}

		void OnEnable()
		{
			if (Ship == null)
			{
				Debug.LogWarning($"Could not find any Ship component in parents", this);
			}

			if (VirtualCamera == null)
			{
				Debug.LogWarning($"VirtualCamera not assigned to {this.GetType().Name}", this);
			} 
			else 
			{
				//m_Camera.GetComponent<CameraController>()?.SetVCam(VirtualCamera);
				VirtualCamera.enabled = true;
			}

			PlayerInput.enabled = true;
		}

		void OnDisable()
		{
			PlayerInput.enabled = false;
		}

		void OnSteer(InputValue input){
			Ship.Internal.Physics.Steering = input.Get<float>();
        }

		void OnThrottle(InputValue input){
			Ship.Internal.Physics.Throttle = input.Get<float>();
		}




		ActionContext Ac;
		public void ContextEnter(ActionContext A , HumanoidController P){
			P.enabled = false;
			m_Camera =  P.m_Camera;
			this.enabled = true;
			Ac = A;
		}

		public void OnExitContext(InputValue input)
		{
			if (input.isPressed)
			{
				Debug.Log("E");
				
				Ac.ActivePlayer.enabled = true;
				this.enabled = false;
				Ac.Exit(Ac.ActivePlayer);
			}
		}
	}
}