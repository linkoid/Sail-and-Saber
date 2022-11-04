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
    public TMP_Text Loot_Text;
    

    // Start is called before the first frame update
    void Start()
    {
        
        HealthBar.minValue = 0;
    }

    // Update is called once per frame
    void Update()
    {

        Loot_Text.text = "Score " +  StatsManager.Score.ToString();
        if(HealthBar.maxValue != StatsManager.MaxHealth){
            HealthBar.maxValue  = StatsManager.MaxHealth;
        }
        float valueDif = (StatsManager.Health- HealthBar.value);
        HealthBar.value += valueDif* .01f ;
    }
}
