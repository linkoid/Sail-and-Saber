using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Ships
{
	/// <summary>
	/// The object-oriented class for a ship. 
	/// If you need something from a ship from outside a ship script, use this class.
	/// </summary>
	[RequireComponent(typeof(Rigidbody), typeof(ShipInternal))]
	public partial class Ship : MonoBehaviour
	{
		public float MaxHealth { get => _maxHealth; protected set => _maxHealth = value; }
		public float Health { get => _health; protected set => _health = value; }

		[SerializeField] private float _health;
		[SerializeField] private float _maxHealth;


		public Rigidbody Rigidbody => this.GetComponent<Rigidbody>();
		internal ShipInternal Internal => this.GetComponent<ShipInternal>();

		void Awake()
		{
			this.gameObject.AddComponent<ShipInternal>();
		}

		public void TakeDamage(float damage)
		{
			Health -= damage;
			if (Health < 0)
			{
				Health = 0;
				OnSink();
			}
		}

		public void Repair(float amount)
		{
			Health += amount;
			if (Health > MaxHealth)
			{
				Health = MaxHealth;
			}
		}

		protected virtual void OnSink()
		{
			// sink the ship
		}



		#region ShipBehaviour Messages

		const int k_ShipLayer = 6; //LayerMask.NameToLayer("Inter-Ship Collision");
		void OnCollisionEnter(Collision collision)
		{
			if (collision.collider.gameObject.layer == k_ShipLayer)
				OnShipCollisionEnter(collision);
		}
		void OnCollisionStay(Collision collision)
		{
			if (collision.collider.gameObject.layer == k_ShipLayer)
				OnShipCollisionStay(collision);
		}
		void OnCollisionExit(Collision collision)
		{
			if (collision.collider.gameObject.layer == k_ShipLayer)
				OnShipCollisionExit(collision);
		}

		void OnShipCollisionEnter(Collision collision)
		{
			foreach (var shipBehaviour in this.GetComponentsInChildren<IShipBehaviourInternal>())
				try { shipBehaviour.OnShipCollisionEnter(collision); }
				catch (Exception e) { Debug.LogException(e, shipBehaviour as ShipBehaviour); }
		}
		void OnShipCollisionStay(Collision collision)
		{
			foreach (var shipBehaviour in this.GetComponentsInChildren<IShipBehaviourInternal>())
				try { shipBehaviour.OnShipCollisionStay(collision); }
				catch (Exception e) { Debug.LogException(e, shipBehaviour as ShipBehaviour); }
		}
		void OnShipCollisionExit(Collision collision)
		{
			foreach (var shipBehaviour in this.GetComponentsInChildren<IShipBehaviourInternal>())
				try { shipBehaviour.OnShipCollisionExit(collision); }
				catch (Exception e) { Debug.LogException(e, shipBehaviour as ShipBehaviour); }
		}

		#endregion // ShipBehaviour Callbacks
	}
}

