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
	public class Ship : MonoBehaviour
	{
		public Rigidbody Rigidbody => this.GetComponent<Rigidbody>();
		internal ShipInternal Internal => this.GetComponent<ShipInternal>();

	}
}

