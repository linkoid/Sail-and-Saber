using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Ships
{
	/// <summary>
	/// Functions for internal use by Ship scripts
	/// </summary>
	[RequireComponent(typeof(Ship))]
	internal class ShipInternal : MonoBehaviour
	{
		public Ship Ship => this.GetComponent<Ship>();

		public float Steering;

	}
}

