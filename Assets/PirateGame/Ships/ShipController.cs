
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
        public float BaseReloadDelay = 2.5f;


        [SerializeField] public Camera m_Camera;
		[SerializeField] public CinemachineVirtualCameraBase VirtualCamera;

		[SerializeField, ReadOnly] private Vector3 m_Movement = Vector3.zero;

		[SerializeField, ReadOnly] private float m_Throttle;


		[SerializeField, ReadOnly] private bool m_WasInCombat;
		[SerializeField, ReadOnly] private float m_OutOfCombatSpeedBonus = 0.5f;


		void Awake()
		{
			PlayerInput.enabled = this.enabled;
		}

		void Start()
		{
			m_WasInCombat = true;
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


		void FixedUpdate()
		{
            //Set reload delay based on crewmate count
            Ship.Internal.Combat.ReloadDelay = BaseReloadDelay - (Ship.Crew.Count * 0.03f);
            if (Ship.Internal.Combat.ReloadDelay <= 1)
                Ship.Internal.Combat.ReloadDelay = 1;

            bool isInCombat = Ship.Internal.Combat.NearbyShips.Count > 0;
			if (isInCombat == m_WasInCombat)
			{
				// Do nothing
			}
			else if (isInCombat)
			{
				Ship.IncreaseSpeedModifier(-m_OutOfCombatSpeedBonus);
			}
			else
			{
				Ship.IncreaseSpeedModifier(+m_OutOfCombatSpeedBonus);
			}
			m_WasInCombat = isInCombat;
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