using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PirateGame.Ships;

namespace PirateGame.UI
{
	public class HUD : MonoBehaviour
	{
		public Player Player { get => m_Player; private set => m_Player = value; }

		[SerializeField] private Player m_Player;
        public Slider HealthBar, Target_Health;
		public TMP_Text Loot_Text, Crew_Text, Error_Text, Target_Title,Target_Crew_Text;
		[SerializeField] float Error_timer = 5f;
		public GameObject TargetUI, DeathPanel, winScreen;

		[SerializeField]
		private Ship m_ShipPrefab;

		[SerializeField] private bool NotToggled = true;
		[SerializeField] private SoundEffect buySound, WinSound, LoseSound;
		bool isBuying, isError;


		[SerializeField] private Button m_ShopButton;
		[SerializeField] private GameObject m_ShopPanel;

		[SerializeField] private MiniMapCamera m_MiniMapCamera;

		[SerializeField, ReadOnly] private Ships.Ship m_PreviousTarget;

		[SerializeField] private TMP_Text m_HealthWarningText;
		[SerializeField] private TMP_Text m_CrewWarningText;


		public void Toggle()
		{
			NotToggled = false;
		}

		public bool CanBuy(int cost)
		{
			var value = Player.Gold;
			Player.Gold = Player.Gold >= cost ? Player.Gold - cost : Player.Gold;
			if (value >= cost == false)
			{
				Error("Too Poor Need " + (cost - Player.Gold) + " More Gold");
			}
			else
			{
				buySound.Play();
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
			if (CanBuy(5))
			{
				Player.Ship.Repair(Player.Ship.MaxHealth);
			}
		}




		public void BuyShipType(int cost)
		{
			if (CanBuy(cost))
			{
				Ship oldShip = Player.Ship;
				Ship newShip = Object.Instantiate(m_ShipPrefab, oldShip.transform.position, oldShip.transform.rotation);
				Player.Ship = newShip;
				//Player.Crew.Board(newShip); // Automatically called by Player.set_Ship()
				Object.Destroy(oldShip.gameObject);
			}
		}

		const int k_CrewCost = 5;
		public void AddCrew()
		{
			Player.CrewCount += CanBuy(k_CrewCost) ? 1 : 0;
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
			m_Player = FindObjectOfType<Player>();
			HealthBar.minValue = 0;
		}

		// Update is called once per frame
		void Update()
		{

			Loot_Text.text = Player.Gold.ToString();

			Crew_Text.text = Player.CrewCount.ToString();

			UpdateHealthBar();


			Error_Text.gameObject.SetActive(isError);
			//change opasity with time 
			if (isError)
			{
				Error_timer -= Time.deltaTime;
			}

			if (Error_timer <= 0)
			{
				isError = false;
				Error_timer = 5f;
			}

			if (Player.Ship == null)
			{
				return;
			}

			CheckLooseCondition();

			CheckWinCondition();

			UpdateTargetInfo();

			UpdateShopVisibility();

			UpdateMiniMapCamera();

			UpdateHealthWarning();

			UpdateCrewWarning();
		}

		private void CheckLooseCondition()
		{
			if (Player.Ship == null) goto gameOver;
			if (Player.Ship.Health <= 0) goto gameOver;
			if (Player.Ship.Crew.Count <= 0 && Player.Gold < k_CrewCost) goto gameOver;
			// else
			return;

		gameOver:
			LoseSound.Play();
			DeathPanel.SetActive(true);
		}

		private void CheckWinCondition()
		{
			//Debug.Log(PlayerPrefs.GetString("Fort1") + " " + PlayerPrefs.GetString("Fort2") + " " + PlayerPrefs.GetString("Fort3"));
			if (PlayerPrefs.GetString("Fort1") == "Captured" && PlayerPrefs.GetString("Fort2") == "Captured" && PlayerPrefs.GetString("Fort3") == "Captured")
			{
				PlayerPrefs.SetString("Fort1","unCaptured");
				PlayerPrefs.SetString("Fort2","unCaptured");
				PlayerPrefs.SetString("Fort3","unCaptured");
				winScreen.SetActive(true && NotToggled);
				WinSound.Play();
				
			}
		}

		private void UpdateTargetInfo()
		{
			if (m_Player.Target == null)
			{
				TargetUI.SetActive(false);
				return;
			}

			Target_Crew_Text.text = m_Player.Target.Crew.Count.ToString();
			Target_Title.text = m_Player.Target.Nickname;


			TargetUI.SetActive(true);

			if (Target_Health.maxValue != m_Player.Target.MaxHealth)
			{
				Target_Health.maxValue = Player.Target.MaxHealth;
			}

            //Change the color based on health
            if (m_Player.Target.Health <= m_Player.Target.MaxHealth * .25)
                Target_Health.fillRect.GetComponent<Image>().color = new Color32(0, 95, 255, 255);
            else
                Target_Health.fillRect.GetComponent<Image>().color = new Color32(31, 255, 0, 255);


            if (m_PreviousTarget != m_Player.Target)
			{
				// Don't animate when switching targetss
				Target_Health.value = Player.Target.Health;
				m_PreviousTarget = m_Player.Target;
			}
			else
			{
				float valueDif = Player.Target.Health - Target_Health.value;
				Target_Health.value += valueDif * .05f;
			}
		}

		void Error(string error)
		{
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
			HealthBar.value += valueDif * .05f;

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

		private void UpdateShopVisibility()
		{
			if (!Player.Ship) return;
			//Debug.Log(Player.Ship.Internal.Combat.NearbyShips.Count);
			m_ShopButton.interactable = Player.Ship.Internal.Combat.NearbyShips.Count == 0;
			m_ShopPanel.SetActive(Player.Ship.Internal.Combat.NearbyShips.Count > 0 ? false : m_ShopPanel.activeSelf);

		}

		private void UpdateMiniMapCamera()
		{
			if (!Player.Ship) return;
			m_MiniMapCamera.ToFollow = Player.Ship.transform;
		}

		private void UpdateHealthWarning()
		{
			if (!Player.Ship) return;
			m_HealthWarningText.gameObject.SetActive(Player.Ship.Health < Player.Ship.MaxHealth * 0.333f);
		}

		private void UpdateCrewWarning()
		{
			if (!Player.Ship) return;
			m_CrewWarningText.gameObject.SetActive(Player.CrewCount <= 0);
		}
	}
}