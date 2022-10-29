using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.CustomUtils;

namespace PirateGame
{
	[RequireComponent(typeof(Collider))]
	public class HumanoidCollider : MonoBehaviour
	{
		[System.Serializable] 
		private class RigidbodyDictionary : UnityDictionary<int, Rigidbody> {}

		public Rigidbody Rigidbody => Collider.attachedRigidbody;
		public Collider Collider => this.GetComponent<Collider>();
		public bool IsGrounded => m_IsGrounded;
		public Vector3 GroundVelocity => m_GroundVelocity;

		public Vector3 TargetVelocity = Vector3.zero;


		[Header("Dynamic Info")]
		[SerializeField, ReadOnly] private Vector3 m_GroundVelocity = Vector3.zero;
		[SerializeField, ReadOnly] private Vector3 m_PrevGroundVelocity = Vector3.zero;
		[SerializeField, ReadOnly] private bool m_IsGrounded = false;
		[SerializeField, ReadOnly] private List<Collider> m_GroundColliders = new List<Collider>();

		[SerializeField, ReadOnly, UnityDictionary] 
		private RigidbodyDictionary m_RigidbodyDictionary = new RigidbodyDictionary();

		[Header("Static Info")]
		[SerializeField, ReadOnly] private Collider m_Collider;
		[SerializeField, ReadOnly] private int m_ColliderID;
		[SerializeField, ReadOnly] private float m_FixedTime;
		[SerializeField, ReadOnly] private float m_FixedDeltaTime;



		void Awake()
		{
			m_GroundVelocity = Vector3.zero;
			m_PrevGroundVelocity = Vector3.zero;
			m_IsGrounded = false;
		}

		void OnEnable()
		{
			m_Collider = this.GetComponent<Collider>();
			m_ColliderID = m_Collider.GetInstanceID();
			m_Collider.hasModifiableContacts = true;
			Physics.ContactModifyEvent += OnContactModifyEvent;
		}

		void OnDisable()
		{
			Physics.ContactModifyEvent -= OnContactModifyEvent;
		}

		void FixedUpdate()
		{
			m_FixedTime = Time.fixedTime;
			m_FixedDeltaTime = Time.fixedDeltaTime;

			m_PrevGroundVelocity = m_GroundVelocity;
		}



		public void OnCollisionEnter(Collision collision)
		{
			//Debug.Log($"{collision.collider} {collision.rigidbody}");
			var hitRigidbody = collision.rigidbody;
			if (hitRigidbody != null)
			{
				m_RigidbodyDictionary[hitRigidbody.GetInstanceID()] = hitRigidbody;
			}
		}

		public void OnCollisionStay(Collision collision)
		{
			OnGroundCollision(collision);

			// Hitting something while in the air resets ground velocity.
			if (!m_IsGrounded)
			{
				m_GroundVelocity = Vector3.zero;
			}
		}

		public void OnCollisionExit(Collision collision)
		{
			var hitRigidbody = collision.rigidbody;
			if (hitRigidbody != null)
			{
				int key = hitRigidbody.GetInstanceID();
				if (m_RigidbodyDictionary.ContainsKey(key))
				{
					m_RigidbodyDictionary.Remove(key);
				}
			}
			OnGroundCollision(collision);
		}
		


		void AddGroundForce(Vector3 groundDeltaV, ContactPoint contact, float factor)
		{
			var groundRigidbody = contact.otherCollider.attachedRigidbody;
			Vector3 groundVelocityAtContact = groundRigidbody.GetPointVelocity(contact.point);
			Vector3 humanoidVelocityAtContact = Rigidbody.GetPointVelocity(contact.point);
			Vector3 relativeVelocityAtContact = groundVelocityAtContact - humanoidVelocityAtContact;

			//Rigidbody.AddForceAtPosition(groundDeltaV * factor, contact.point, ForceMode.VelocityChange);
		}

		/// <summary>
		/// Returns true if the given collider is a ground collider
		/// </summary>
		bool CheckIsGroundCollider(Collider collider)
		{
			var hitRigidbody = collider.attachedRigidbody;
			if (hitRigidbody)
			{
				var constraints = hitRigidbody.constraints;

				// Ignore rigidbodies that can fall (move on Y axis)
				//if (!constraints.HasFlag(RigidbodyConstraints.FreezePositionY)) return false;
			}

			return true;
		}

		void OnGroundCollision(Collision collision)
		{
			if (!CheckIsGroundCollider(collision.collider)) return;

			// Is the ground collider still acting as ground?
			if (CheckIsGrounding(collision))
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
				//Debug.Log("Not grounding!");
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
			HandleGroundFriction(collision);
			m_IsGrounded = true;
		}

		void OnGroundCollisionStay(Collision collision)
		{
			HandleGroundFriction(collision);

			m_IsGrounded = true;
		}

		void OnGroundCollisionExit(Collision collision)
		{
			if (m_GroundColliders.Count <= 0)
			{
				m_IsGrounded = false;
			}
		}

		void HandleGroundFriction(Collision collision)
		{
			var groundRigidbody = collision.rigidbody;
			if (groundRigidbody == null)
			{
				m_GroundVelocity = Vector3.zero;
				return;
			}
			// else

			Vector3 meanContactPoint = Vector3.zero;

			var contacts = new ContactPoint[collision.contactCount];
			collision.GetContacts(contacts);
			foreach (var contact in contacts)
			{
				meanContactPoint += contact.point;
			}
			meanContactPoint /= collision.contactCount;

			m_GroundVelocity = groundRigidbody.GetPointVelocity(meanContactPoint);


			Vector3 groundDeltaV = m_GroundVelocity - m_PrevGroundVelocity;
			foreach (var contact in contacts)
			{
				AddGroundForce(groundDeltaV, contact, 1.0f / collision.contactCount);
			}
		}

		bool CheckIsGrounding(Collision collision)
		{
			Vector3 expectedImpulse = Rigidbody.mass * -Physics.gravity * Time.fixedDeltaTime;
			float alignment = Vector3.Dot(expectedImpulse, collision.GetImpulse()) / expectedImpulse.sqrMagnitude;
			if (Mathf.Abs(alignment) >= 0.5f)
			{
				return true;
			}

			return false;
		}



		void OnContactModifyEvent(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs)
		{
			// NOTE: This function does not run on the main thread,
			// so almost everything not defined in this script is not accessible

			// For each contact pair
			for (int j = 0; j < pairs.Length; j++)
			{
				ModifiableContactPair pair = pairs[j];

				// Check to see if the pair involves this collider
				bool isCollider = (pair.colliderInstanceID == m_ColliderID);
				bool isOtherCollider = (pair.otherColliderInstanceID == m_ColliderID);
				if (!isCollider && !isOtherCollider) continue;
				
				Vector3 thisPosition = isOtherCollider ? pair.otherPosition : pair.position;

				Rigidbody otherRigidbody = null;
				if (isCollider)
				{
					m_RigidbodyDictionary.TryGetValue(pair.otherBodyInstanceID, out otherRigidbody);
				}
				else
				{
					m_RigidbodyDictionary.TryGetValue(pair.bodyInstanceID, out otherRigidbody);
				}

				float flip = 1;
				if (isOtherCollider)
				{
					flip = -1;
				}

				for (int i = 0; i < pair.contactCount; ++i)
				{
					Vector3 contactPoint = pair.GetPoint(i);
					Vector3 contactVelocity = Vector3.zero;
					if (otherRigidbody != null)
					{
						//contactVelocity = TangentVelocity(otherRigidbody, contactPoint);	
						//contactVelocity = otherRigidbody.GetPointVelocity(contactPoint);
					}

					//Debug.Log($"{pair.GetTargetVelocity(i)}");

					Vector3 normal = pair.GetNormal(i) * flip;

					float slopeFactor = Vector3.Dot(normal, -Physics.gravity.normalized);
					//wwDebug.Log(slopeFactor);

					var targetVelocity = TargetVelocity * flip * slopeFactor;
					if (slopeFactor > 0.5)
					{
						pair.SetTargetVelocity(i, targetVelocity);
					}
				}
			}
		}

		private static Vector3 TangentVelocity(Rigidbody rigidbody, Vector3 point)
		{
			Vector3 offset = point - rigidbody.worldCenterOfMass;
			Vector3 tangentVelocity = offset.magnitude * rigidbody.angularVelocity;
			return tangentVelocity;
		}
	}

}

