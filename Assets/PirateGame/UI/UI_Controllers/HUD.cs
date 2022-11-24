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
		public Slider HealthBar,Target_Health;
		public TMP_Text Loot_Text, Crew_Text, Error_Text,Target_Title;
		[SerializeField] float Error_timer = 5f;
		public GameObject Ship,TargetUI;
		bool isBuying, isError;

		public bool CanBuy(int cost)
		{
			var value = Player.Gold;
			Player.Gold = Player.Gold >= cost ? Player.Gold - cost : Player.Gold;
			if(value >= cost == false){
				Error("Too Poor Need " + (cost - Player.Gold)+ " More Gold");
			}else{
				isError = false;
			}

			return value >= cost;
		}

		public void RepairShip()
		{
			if (!ShipCheck()) return;

			if (Player.Ship.Health == Player.Ship.MaxHealth)
			{
				return;
			}
			if (CanBuy(3))
			{
				Player.Ship.Repair(Mathf.Infinity);
			}
		}

		
		public void BuyShipType(int cost,GameObject ShipType)
		{
			if (CanBuy(cost))
			{
				Instantiate(ShipType,Player.transform);
			}
		}

		public void AddCrew()
		{
			Player.CrewCount += CanBuy(5) ? 1 : 0;
		}


		public void AddSpeed()
		{
			if (!ShipCheck()) return;

			if (CanBuy(20))
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
			if(m_Player.Target){
				Target_Title.text = m_Player.Target.name;
				TargetUI.SetActive(true);

				if (Target_Health.maxValue != m_Player.Target.MaxHealth)
				{
					Target_Health.maxValue = Player.Target.MaxHealth;
				}
				float valueDif = Player.Target.Health - Target_Health.value;
				Target_Health.value += valueDif * .01f;
				
			}else {
				TargetUI.SetActive(false);

			}

			Loot_Text.text = Player.Gold.ToString();

			Crew_Text.text = Player.CrewCount.ToString();

			UpdateHealthBar();


			Error_Text.gameObject.SetActive(isError);
			//change opasity with time 
			if(isError ){
				Error_timer -= Time.deltaTime;
			}

			if(Error_timer <= 0){
				isError = false;
				Error_timer = 5f;
			}
			
		}

		void Error(string error){
			Error_Text.text = error;
			Error_timer = 5f;
			isError = true;
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