using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PirateGame.UI
{
	public class HUD : MonoBehaviour
	{
		public Player Player { get => m_Player; private set => m_Player = value; }

		[SerializeField] private Player m_Player;
		public Slider HealthBar;
		public TMP_Text Loot_Text, Crew_Text, Too_Poor;

		public bool Buy(int cost)
		{
			var value = Player.Gold;
			Player.Gold = Player.Gold >= cost ? Player.Gold - cost : Player.Gold;
			return value >= cost;
		}

		public void RepairShip()
		{
			if (!ShipCheck()) return;

			if (Player.Ship.Health == Player.Ship.MaxHealth)
			{
				return;
			}
			if (Buy(3))
			{
				Player.Ship.Repair(Mathf.Infinity);
			}
		}

		public void AddCrew()
		{
			Player.CrewCount += Buy(5) ? 1 : 0;
		}


		public void AddSpeed()
		{
			if (!ShipCheck()) return;

			if (Buy(20))
			{
				Player.Ship.IncreaseSpeedModifier(0.1f);
			}
		}

		// Start is called before the first frame update
		void Start()
		{
			HealthBar.minValue = 0;
		}

		// Update is called once per frame
		void Update()
		{

			Loot_Text.text = Player.Gold.ToString();

			Crew_Text.text = Player.CrewCount.ToString();

			UpdateHealthBar();
		}

		private void UpdateHealthBar()
		{
			if (Player.Ship == null)
			{
				// Maybe hide the health bar?
				return;
			}

			if (HealthBar.maxValue != Player.Ship.MaxHealth)
			{
				HealthBar.maxValue = Player.Ship.MaxHealth;
			}
			float valueDif = Player.Ship.Health - HealthBar.value;
			HealthBar.value += valueDif * .01f;
		}

		private bool ShipCheck()
		{
			if (Player == null)
			{
				Debug.LogWarning("Player has no ship to repair");
				return false;
			}
			return true;
		}
	}
}