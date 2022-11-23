using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Weather{
public class Weather_Changer : MonoBehaviour
{

[SerializeField] WeatherParams WP;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("hit");
        Weather.Instance.TransitionWeather(WP);
    }
}
}