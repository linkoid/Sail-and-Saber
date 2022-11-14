using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CustomUtils;
using PirateGame.Sea;

namespace PirateGame.Weather
{
	public class Weather : GlobalSingleton<Weather>
	{
		public WeatherParams CurrentWeather { get => _currentWeather; private set => _currentWeather = value; }
		public WeatherParams OldWeather     { get => _oldWeather    ; private set => _oldWeather     = value; }
		public WeatherParams NewWeather     { get => _newWeather    ; private set => _newWeather     = value; }

		[SerializeField          ] private WeatherParams _currentWeather;
		[SerializeField, ReadOnly] private WeatherParams _oldWeather    ;
		[SerializeField, ReadOnly] private WeatherParams _newWeather    ;

		private BuoyancyEffector Sea => Object.FindObjectOfType<BuoyancyEffector>(false);

		public void TransitionWeather(WeatherParams newWeather)
		{
			NewWeather = newWeather;
		}

		void Update()
		{
			UpdateWeather();
		}

		/// <summary>
		/// Applies the params from CurrentWeather to the scene / water.
		/// </summary>
		void UpdateWeather()
		{
			// Sea.something = something;
		}

		/// <summary>
		/// Calculates CurrentWeather based on the current transition
		/// </summary>
		void UpdateTransition()
		{
			float time = Time.time;
			float lerpFactor = time; // Some equation based on time
			CurrentWeather = WeatherParams.Lerp(OldWeather, NewWeather, lerpFactor);
		}
	}
}
