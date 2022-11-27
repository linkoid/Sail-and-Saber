using System.Collections.Generic;
using UnityEngine;
using PirateGame.Ships;

namespace PirateGame
{
	public class Fortress : Ship
	{
		public bool IsCaptured { get => _isCaptured; set => _isCaptured = value; }
		[SerializeField] private bool _isCaptured = false;

		public string fortName;

		protected void OnEnable()
		{
			IsCaptured = (PlayerPrefs.GetString(fortName) == "Captured");
		}

		public void Capture()
		{
			if (!IsPlundered)
			{
				Debug.LogWarning($"Capture(): {this} has not been plundered", this);
				return;
			}
			if (IsCaptured)
			{
				Debug.LogWarning($"Capture(): {this} has already been captured", this);
				return;
			}

			IsCaptured = true;
			PlayerPrefs.SetString(fortName,"Captured");
		}
	}
}
