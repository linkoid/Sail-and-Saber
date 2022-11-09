using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using PirateGame;

using TMPro;
public class UI_Controller : MonoBehaviour
{
    [SerializeField] StatsManager StatsManager;
    public Slider HealthBar;
    public TMP_Text Loot_Text,Crew_Text,Too_Poor;
    
    public bool Buy(int cost){
        int value = StatsManager.Gold;
        StatsManager.Gold  = (StatsManager.Gold >= cost) ? StatsManager.Gold- cost : StatsManager.Gold;
        return value >= cost;
    }

    public void RepairShip(){
        if(StatsManager.Health == StatsManager.MaxHealth){
            return;
        }
        StatsManager.Health = Buy(3) ? StatsManager.MaxHealth : StatsManager.Health;
    }

    public void AddCrew(){
        StatsManager.Crew += Buy(5) ? 1 :0 ;
    }

    
    public void AddSpeed(){
        StatsManager.SpeedMod +=  Buy(20) ? 1 :0 ;;
    }

    // Start is called before the first frame update
    void Start()
    {
        
        HealthBar.minValue = 0;
    }

    // Update is called once per frame
    void Update()
    {

        Loot_Text.text =  StatsManager.Gold.ToString();
        
        Crew_Text.text =  StatsManager.Crew.ToString();

        if(HealthBar.maxValue != StatsManager.MaxHealth){
            HealthBar.maxValue  = StatsManager.MaxHealth;
        }
        float valueDif = (StatsManager.Health- HealthBar.value);
        HealthBar.value += valueDif* .01f ;
    }
}
