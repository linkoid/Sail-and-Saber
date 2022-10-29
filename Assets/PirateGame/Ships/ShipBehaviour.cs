using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PirateGame.Ships
{
	partial class Ship
	{
		public abstract class ShipBehaviour : MonoBehaviour
		{
			protected Ship Ship => this.GetComponentInParent<Ship>();
		}
	}
}

