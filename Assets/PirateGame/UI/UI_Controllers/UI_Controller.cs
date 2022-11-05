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
    public TMP_Text Loot_Text,Crew_Text;
    

    public void Buy(int cost){
        StatsManager.Gold  = (StatsManager.Gold > cost) ? StatsManager.Gold- cost : StatsManager.Gold;
    }

    public void RepairShip(){
        Buy(5);
        StatsManager.Health = StatsManager.MaxHealth;
    }

    public void AddCrew(){
        Buy(1);
        StatsManager.Crew += 1;
    }

    
    public void AddSpeed(){
        Buy(20);
        StatsManager.SpeedMod += 1;
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
