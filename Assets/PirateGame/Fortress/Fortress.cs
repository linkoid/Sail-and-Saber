using System.Collections.Generic;
using UnityEngine;
using PirateGame.Ships;

namespace PirateGame
{
	public class Fortress : Ship
	{
		public bool IsCaptured { get => _isCaptured; set => _isCaptured = value; }
		[SerializeField] private bool _isCaptured = false;
	}
}
