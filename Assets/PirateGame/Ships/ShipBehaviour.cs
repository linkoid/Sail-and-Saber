using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PirateGame.Ships
{
	partial class Ship
	{
		private interface IShipBehaviourInternal
		{
			void OnShipCollisionEnter(Collision collision);
			void OnShipCollisionStay(Collision collision);
			void OnShipCollisionExit(Collision collision);
		}

		public abstract class ShipBehaviour : MonoBehaviour, IShipBehaviourInternal
		{
			protected Ship Ship => this.GetComponentInParent<Ship>();

			protected virtual void OnShipCollisionEnter(Collision collision) { }
			protected virtual void OnShipCollisionExit (Collision collision) { }
			protected virtual void OnShipCollisionStay (Collision collision) { }

			void IShipBehaviourInternal.OnShipCollisionEnter(Collision collision) => OnShipCollisionEnter(collision);
			void IShipBehaviourInternal.OnShipCollisionExit (Collision collision) => OnShipCollisionExit (collision);
			void IShipBehaviourInternal.OnShipCollisionStay (Collision collision) => OnShipCollisionStay (collision);
		}
	}
}

