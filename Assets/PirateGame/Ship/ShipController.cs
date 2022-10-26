
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.CustomUtils;
using PirateGame.Cameras;

namespace PirateGame.Ships
{
	[RequireComponent(typeof(PlayerInput))]
	public class ShipController : MonoBehaviour
	{
		public Vector3 Movement => m_Movement;
		public Ship Ship => this.GetComponentInParent<Ship>();
		public Rigidbody Rigidbody => Ship.Rigidbody;
		public PlayerInput PlayerInput => this.GetComponentInParent<PlayerInput>();


		private ShipInternal ShipInternal => Ship.Internal;


        [SerializeField] public Camera m_Camera;
		[SerializeField] public VirtualCamera VirtualCamera;

		[SerializeField] private float m_Speed = 10;
	
		[SerializeField] private float spinSpeed = 10f;
	
		[SerializeField] private float spinAcceleration = 10f;
		[SerializeField] private float m_Acceleration = 50;

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

			if (m_Camera == null)
			{
				Debug.LogWarning($"Camera not assigned to {this.GetType().Name}", this);
			}else {
				m_Camera.GetComponent<CameraController>()?.SetVCam(VirtualCamera);
			}

			PlayerInput.enabled = true;
		}

		void OnDisable()
		{
			if (m_Camera == null)
			{
				Debug.LogWarning($"Camera not assigned to PlayerController", this);
			}

			PlayerInput.enabled = false;
		}

		void OnSteer(InputValue input){
			ShipInternal.Steering = input.Get<float>();
        }

		void OnThrottle(InputValue input){
			//m_RelativeMovement = Input.Get<Vector2>();
		
			m_Throttle = input.Get<float>();
		}

		void FixedUpdate()
		{
			AddSteerForce();
			AddThrottleForce();
		}

		void AddThrottleForce()
		{
			Vector3 CurrentVelocity = Rigidbody.velocity;
			Vector3 TargetVel = transform.forward *((m_Throttle >0) ? m_Throttle : m_Throttle/10)  *  m_Speed * Time.deltaTime;
			Vector3 Difference = TargetVel- CurrentVelocity;

			Vector3 AddVel = Difference.normalized * m_Acceleration * Time.fixedDeltaTime;
			if(AddVel.magnitude > Difference.magnitude){
				AddVel = Difference;
			}

			Rigidbody.AddForce(AddVel, ForceMode.VelocityChange);
		
		}

		void AddSteerForce()
		{
			Vector3 CurrentSpin = Rigidbody.angularVelocity;

			Vector3 TargetSpin = new Vector3(0f, ShipInternal.Steering, 0f).normalized * spinSpeed;

			Vector3 DeltaSpin = TargetSpin - CurrentSpin;

			Vector3 AddSpin = DeltaSpin.normalized * spinAcceleration * Time.fixedDeltaTime;

			if(AddSpin.magnitude > DeltaSpin.magnitude){
					AddSpin = DeltaSpin;
			}
			Rigidbody.AddTorque(AddSpin,ForceMode.VelocityChange);
		}




		ActionContext Ac;
		public void ContextEnter(ActionContext A , PlayerController P){
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