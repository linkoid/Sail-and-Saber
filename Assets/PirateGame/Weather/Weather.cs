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

		[SerializeField] float Transitiontimer,transitionDuration=5f;
		[SerializeField] bool transitionSet = false;

		public void TransitionWeather(WeatherParams newWeather)
		{
			transitionSet = true;
			Transitiontimer = 0f;
			NewWeather = newWeather;
		}

		void Update()
		{
			if(transitionSet){
				Transitiontimer += Time.deltaTime;
			}

			if(Transitiontimer >=transitionDuration){
				transitionSet = false;
				Transitiontimer = 0;
			}
			UpdateWeather();
			UpdateTransition();
		}

		/// <summary>
		/// Applies the params from CurrentWeather to the scene / water.
		/// </summary>
		void UpdateWeather()
		{
			Sea.WaveAmplitude = CurrentWeather.WaveAmplitude;
			Sea.WaveDistance = CurrentWeather.WaveDistance;
			Sea.WaveSpeed = CurrentWeather.WaveSpeed;

		}

		/// <summary>
		/// Calculates CurrentWeather based on the current transition
		/// </summary>
		void UpdateTransition()
		{
			float time = Time.time;
			float lerpFactor = transitionDuration/Transitiontimer; // Some equation based on time
			CurrentWeather = WeatherParams.Lerp(OldWeather, NewWeather, lerpFactor);

		}
	}
}
