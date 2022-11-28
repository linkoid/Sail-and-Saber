using System.Collections.Generic;
using UnityEngine;
using PirateGame.Ships;

namespace PirateGame
{
	public class Fortress : Ship
	{
		public bool IsCaptured { get => _isCaptured; private set => _isCaptured = value; }
		[SerializeField] private bool _isCaptured = false;

		[SerializeField]
		private string m_FortName;

		protected void OnEnable()
		{
			IsCaptured = (PlayerPrefs.GetString(m_FortName) == "Captured");
		}

		public override void TakeDamage(float damage)
		{
			Health -= damage;
			if (Health < 0)
			{
				Health = 0;
				//Sink(); // Don't call this until plundered!
			}
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
			PlayerPrefs.SetString(m_FortName,"Captured");
		}
	}
}
