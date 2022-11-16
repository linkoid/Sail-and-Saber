using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PirateGame.UI
{
	public class HUD : MonoBehaviour
	{
		[SerializeField] Player m_Player;
		public Slider HealthBar;
		public TMP_Text Loot_Text, Crew_Text, Too_Poor;

		public bool Buy(int cost)
		{
			var value = m_Player.Gold;
			m_Player.Gold = m_Player.Gold >= cost ? m_Player.Gold - cost : m_Player.Gold;
			return value >= cost;
		}

		public void RepairShip()
		{
			if (!ShipCheck()) return;

			if (m_Player.Ship.Health == m_Player.Ship.MaxHealth)
			{
				return;
			}
			if (Buy(3))
			{
				m_Player.Ship.Repair(Mathf.Infinity);
			}
		}

		public void AddCrew()
		{
			m_Player.CrewCount += Buy(5) ? 1 : 0;
		}


		public void AddSpeed()
		{
			if (!ShipCheck()) return;

			if (Buy(20))
			{
				m_Player.Ship.IncreaseSpeedModifier(0.1f);
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

			Loot_Text.text = m_Player.Gold.ToString();

			Crew_Text.text = m_Player.CrewCount.ToString();

			UpdateHealthBar();
		}

		private void UpdateHealthBar()
		{
			if (m_Player.Ship == null)
			{
				// Maybe hide the health bar?
				return;
			}

			if (HealthBar.maxValue != m_Player.Ship.MaxHealth)
			{
				HealthBar.maxValue = m_Player.Ship.MaxHealth;
			}
			float valueDif = m_Player.Ship.Health - HealthBar.value;
			HealthBar.value += valueDif * .01f;
		}

		private bool ShipCheck()
		{
			if (m_Player == null)
			{
				Debug.LogWarning("Player has no ship to repair");
				return false;
			}
			return true;
		}
	}
}