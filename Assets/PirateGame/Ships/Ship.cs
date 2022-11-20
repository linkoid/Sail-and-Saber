using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PirateGame.Crew;
using Unity.AI.Navigation;
using System.Reflection;

namespace PirateGame.Ships
{
	/// <summary>
	/// The object-oriented class for a ship. 
	/// If you need something from a ship from outside a ship script, use this class.
	/// </summary>
	[SelectionBase]
	[RequireComponent(typeof(Rigidbody))]
	public partial class Ship : MonoBehaviour
	{
		public NavMeshSurface NavMeshSurface => this.GetComponent<NavMeshSurface>();


		public float MaxHealth { get => _maxHealth; protected set => _maxHealth = value; }
		public float Health { get => _health; protected set => _health = value; }
		public float SpeedModifier { get => _speedModifier; protected set => _speedModifier = value; }
		public CrewDirector Crew { get => _crew; protected set => _crew = value; }
		public bool IsRaided { get => _isRaided; protected set => _isRaided = value; }
		public bool IsPlundered { get => _isRaided; protected set => _isRaided = value; }

		[SerializeField] private float _health = 100;
		[SerializeField] private float _maxHealth = 100;
		[SerializeField] private float _speedModifier = 1;
		
		[SerializeField] private CrewDirector _crew;

		[SerializeField, ReadOnly] private bool _isRaided;
		[SerializeField, ReadOnly] private bool _isPlundered;


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
				Sink();
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

		/// <summary>
		/// Assign the specifed crew to the ship.
		/// </summary>
		/// <remarks><see cref="CrewDirector.Board(Ship)"/> should also be called manually.</remarks>
		public void AssignCrew(CrewDirector crew)
		{
			Crew = crew;
		}

		/// <summary>
		/// Call this function when this ship is being raided.
		/// Should prevent the ship from moving and spawn enemy crew.
		/// </summary>
		public virtual void Raid()
		{
			if (IsRaided)
			{
				Debug.LogWarning($"{this} has already been raided", this);
				return;
			}
			IsRaided = true;

			// send callback to internal components
			SendInternalCallback((component) => component.OnRaid());
		}

		/// <summary>
		/// Plunders the ship, awarding gold to the player. (and maybe other stuff?)
		/// </summary>
		/// <remarks>Fails if the ship is not currently being raided.</remarks>
		public virtual void Plunder(Player player)
		{
			if (!IsRaided)
			{
				Debug.LogWarning($"{this} is not being raided, and cannot be plundered", this);
				return;
			}

			if (IsPlundered)
			{
				Debug.LogWarning($"{this} has already been plundered", this);
				return;
			}

			IsPlundered = true;

			// send callback to internal components
			SendInternalCallback((component) => component.OnPlunder(player));
		}

		/// <summary>
		/// Will cause the ship to sink.
		/// </summary>
		public virtual void Sink()
		{
			if (IsRaided && !IsPlundered)
			{
				Debug.LogWarning($"{this} cannot be sunk. It is still being raided, but has not been plundered.", this);
				return;
			}

			// send callback to internal components
			SendInternalCallback((component) => component.OnSink());
		}



		#region Collision Messages

		const int k_ShipLayer = 6; //LayerMask.NameToLayer("Inter-Ship Collision");
		void OnCollisionEnter(Collision collision)
		{
			if (collision.collider.gameObject.layer == k_ShipLayer)
				SendInternalCallback((component) => component.OnShipCollisionEnter(collision));
		}
		void OnCollisionStay(Collision collision)
		{
			if (collision.collider.gameObject.layer == k_ShipLayer)
				SendInternalCallback((component) => component.OnShipCollisionStay(collision));
		}
		void OnCollisionExit(Collision collision)
		{
			if (collision.collider.gameObject.layer == k_ShipLayer)
				SendInternalCallback((component) => component.OnShipCollisionExit(collision));
		}

		#endregion // Unity Messages



		private void SendInternalCallback(System.Action<IShipBehaviourInternal> method)
		{
			foreach (var shipBehaviour in this.GetComponentsInChildren<IShipBehaviourInternal>())
			{
				try
				{
					method.Invoke(shipBehaviour);
				}
				catch (System.Exception e)
				{
					Debug.LogException(e, shipBehaviour as ShipBehaviour);
				}
			}
		}
	}
}

