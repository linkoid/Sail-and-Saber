using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Weather
{
	public class WeatherScene : MonoBehaviour
	{
		public WeatherParams Parameters => m_Parameters;

		[SerializeField] 
		private WeatherParams m_Parameters = new WeatherParams()
		{
			Waves0 = new WaveParams()
			{
				Amplitude = 0.5f,
				Distance = 5,
				Speed = 5,
				Direction = new Vector3(1, 1).normalized,
			},
		};

		void Start()
		{
			if (!WeatherManager.Started)
			{
				WeatherManager.Instance.TransitionWeather(Parameters, 0.1f);
			}
		}

		void OnValidate()
		{
			WeatherManager.ApplyWeather(m_Parameters);
		}
	}
}
