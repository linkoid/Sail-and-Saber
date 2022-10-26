using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Ships
{
	partial class Ship
	{
		/// <summary>
		/// Functions for internal use by Ship scripts
		/// </summary>
		[RequireComponent(typeof(Ship))]
		public class ShipInternal : Component
		{
			public ShipPhysics Physics => this.GetComponentInChildren<ShipPhysics>();

			public ShipAnimator Animator => this.GetComponentInChildren<ShipAnimator>();

			public ShipCombat Combat => this.GetComponentInChildren<ShipCombat>();
		}
	}
}

