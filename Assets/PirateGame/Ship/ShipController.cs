
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.CustomUtils;

namespace PirateGame.Ships{

    
[RequireComponent(typeof(Rigidbody))]
public class ShipController : MonoBehaviour
{
	public Vector3 Movement => m_Movement;
	public Rigidbody Rigidbody => GetComponent<Rigidbody>();

	[SerializeField] public Camera m_Camera;

	[SerializeField] private float m_Speed = 10;
	[SerializeField] private float m_Acceleration = 50;

	[SerializeField, ReadOnly] private Vector3 m_Movement = Vector3.zero;
	[SerializeField, ReadOnly] private Vector2 m_RelativeMovement = Vector2.zero;
    
	[SerializeField, ReadOnly] private Vector2 m_RelativeRotation = Vector2.zero;

    [SerializeField, ReadOnly] private float rotationValue,positionValue;

	[SerializeField, ReadOnly] private bool    m_IsMoveTo = false;
	[SerializeField, ReadOnly] private Vector3 m_MoveTo = Vector3.zero;

	[SerializeField, ReadOnly] private Vector3 m_GroundVelocity = Vector3.zero;
	[SerializeField, ReadOnly] private Vector3 m_PrevGroundVelocity = Vector3.zero;
	[SerializeField, ReadOnly] private bool m_IsGrounded = false;
	[SerializeField, ReadOnly] private List<Collider> m_GroundColliders = new List<Collider>();

	void Awake()
	{
		m_GroundVelocity = Vector3.zero;
		m_PrevGroundVelocity = Vector3.zero;
		m_IsGrounded = false;
	}

	void OnEnable()
	{
		if (m_Camera == null)
		{
			Debug.LogWarning($"Camera not assigned to PlayerController", this);
		}
	}

    void OnSteer(InputValue input){
		rotationValue = input.Get<float>();
    }

    
    void OnThrottle(InputValue input){
        //m_RelativeMovement = Input.Get<Vector2>();
        
		positionValue = input.Get<float>();

    }

	void FixedUpdate()
    {
		
        addSteerForce();
		addThrottleForce();
		m_PrevGroundVelocity = m_GroundVelocity;
	}

	void addThrottleForce()
	{
		//Rigidbody.velocity = new Vector3(positionValue ,0f,positionValue);

		Vector3 movement = transform.forward *((positionValue >0) ? positionValue : positionValue/10)  *  m_Speed * Time.deltaTime;
        Rigidbody.velocity = movement;
		
		//Rigidbody.AddForce(deltaV, ForceMode.VelocityChange);
	}

    
	void addSteerForce()
	{
        //get axis of movement  

		// add axis to the rotation of the object slowly
        // might need rotation speed 
        Rigidbody.angularVelocity = new Vector3(0f,rotationValue,0f).normalized;

	}


	void OnCollisionStay(Collision collision)
	{
		OnGroundCollision(collision);

		// Hitting something while in the air resets ground velocity.
		if (!m_IsGrounded)
		{
			m_GroundVelocity = Vector3.zero;
		}
	}

	void OnCollisionExit(Collision collision)
	{
		OnGroundCollision(collision);
	}

	void OnGroundCollision(Collision collision)
	{
		var hitRigidbody = collision.rigidbody;
		if (hitRigidbody)
		{
			var constraints = hitRigidbody.constraints;
			// Ignore rigidbodies that can fall (move on Y axis)
			if (!constraints.HasFlag(RigidbodyConstraints.FreezePositionY)) return;
		}

		// Is the ground collider still acting as ground?
		if (CheckGrounding(collision))
		{
			if (m_GroundColliders.Contains(collision.collider))
			{
				OnGroundCollisionStay(collision);
			}
			else
			{
				m_GroundColliders.Add(collision.collider);
				OnGroundCollisionEnter(collision);
			}
		}
		else 
		{
			if (m_GroundColliders.Contains(collision.collider))
			{
				// XXX If the player has more than one collider, this doesn't work.
				// because OnCollisionExit is called for any local collider.
				m_GroundColliders.Remove(collision.collider);
				OnGroundCollisionExit(collision);
			}
		}
	}

	void OnGroundCollisionEnter(Collision collision)
	{
		m_IsGrounded = true;
	}

	void OnGroundCollisionStay(Collision collision)
	{
		var hitRigidbody = collision.rigidbody;
		if (hitRigidbody)
		{
			m_GroundVelocity = hitRigidbody.velocity;
		}
		else
		{
			m_GroundVelocity = Vector3.zero;
		}

		m_IsGrounded = true;
	}

	void OnGroundCollisionExit(Collision collision)
	{
		if (m_GroundColliders.Count <= 0)
		{
			m_IsGrounded = false;
		}
	}

	bool CheckGrounding(Collision collision)
	{
		Vector3 expectedImpulse = Rigidbody.mass * -Physics.gravity * Time.fixedDeltaTime;
		float alignment = Vector3.Dot(expectedImpulse, collision.GetImpulse()) / expectedImpulse.sqrMagnitude;
		if (alignment >= 0.5f)
		{
			return true;
		}

		return false;
	}

    

	Vector3 GetMovement()
	{
		Vector3 up = -Physics.gravity.normalized;
			Vector3 direction = new Vector3(m_RelativeMovement.x, 0, m_RelativeMovement.y);
			if (m_Camera != null)
			{
				direction = m_Camera.transform.TransformDirection(direction);
				direction = Vector3.ProjectOnPlane(direction, up);
			}
			direction.Normalize();

			float magnitude = Mathf.Min(m_RelativeMovement.magnitude, 1);
			var movement = direction * magnitude;
			return movement;
	}


    public void ContextEnter(ActionContext A , PlayerController P){
        P.enabled = false;
        this.enabled = true;
    }
}
}