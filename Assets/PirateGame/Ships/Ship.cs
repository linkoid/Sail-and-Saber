using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PirateGame.Crew;
using Unity.AI.Navigation;

namespace PirateGame.Ships
{
	/// <summary>
	/// The object-oriented class for a ship. 
	/// If you need something from a ship from outside a ship script, use this class.
	/// </summary>
	[RequireComponent(typeof(Rigidbody), typeof(ShipInternal))]
	public partial class Ship : MonoBehaviour
	{
		public NavMeshSurface NavMeshSurface => this.GetComponent<NavMeshSurface>();


		public float MaxHealth { get => _maxHealth; protected set => _maxHealth = value; }
		public float Health { get => _health; protected set => _health = value; }
		public float SpeedModifier { get => _speedModifier; protected set => _speedModifier = value; }
		public CrewDirector Crew { get => _crew; protected set => _crew = value; }

		[SerializeField] private float _health = 100;
		[SerializeField] private float _maxHealth = 100;
		[SerializeField] private float _speedModifier = 1;
		[SerializeField] private CrewDirector _crew;


		public Rigidbody Rigidbody => this.GetComponent<Rigidbody>();
		internal ShipInternal Internal => this.GetComponent<ShipInternal>();

		void Awake()
		{
			this.gameObject.AddComponent<ShipInternal>();
		}

		void Start()
		{
			Health = MaxHealth;
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

		public void IncreaseSpeedModifier(float factor)
		{
			SpeedModifier += factor;
		}

		public void AssignCrew(CrewDirector crew)
		{
			Crew = crew;
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

