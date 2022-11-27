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
			WaveAmplitude = 1,
			WaveDistance = 5,
			WaveSpeed = 5,
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
