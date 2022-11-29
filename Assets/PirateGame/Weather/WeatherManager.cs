using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CustomUtils;
using PirateGame.Sea;

namespace PirateGame.Weather
{
	public class WeatherManager : GlobalSingleton<WeatherManager>
	{
		public static bool Started = false;


		public WeatherParams CurrentWeather { get => _currentWeather; private set => _currentWeather = value; }
		public WeatherParams OldWeather     { get => _oldWeather    ; private set => _oldWeather     = value; }
		public WeatherParams NewWeather     { get => _newWeather    ; private set => _newWeather     = value; }

		[SerializeField          ] private WeatherParams _currentWeather;
		[SerializeField, ReadOnly] private WeatherParams _oldWeather    ;
		[SerializeField, ReadOnly] private WeatherParams _newWeather    ;

		public static BuoyancyEffector Sea => Object.FindObjectOfType<BuoyancyEffector>(false);

		[SerializeField] float m_TransitionTimer, m_TransitionDuration = 5f;
		[SerializeField] bool m_IsTransitioning = false;

		void Start()
		{
			Started = true;
		}

		void Update()
		{
			UpdateTransition();
		}

		public void TransitionWeather(WeatherParams newWeather, float duration = 5f)
		{
			OldWeather = CurrentWeather;
			m_IsTransitioning = true;
			m_TransitionTimer = 0f;
			m_TransitionDuration =	duration;
			NewWeather = newWeather;
		}

		/// <summary>
		/// Calculates CurrentWeather based on the current transition
		/// </summary>
		void UpdateTransition()
		{
			if (m_IsTransitioning)
			{
				m_TransitionTimer += Time.deltaTime;
			}
			else
			{
				return;
			}

			if (m_TransitionTimer > m_TransitionDuration)
			{
				m_TransitionTimer = m_TransitionDuration;
			}

			float lerpFactor = m_TransitionTimer / m_TransitionDuration;
			CurrentWeather = WeatherParams.Lerp(OldWeather, NewWeather, lerpFactor);

			ApplyWeather(CurrentWeather);

			if (m_TransitionTimer >= m_TransitionDuration)
			{
				m_IsTransitioning = false;
				m_TransitionTimer = 0;
			}
		}

		/// <summary>
		/// Applies the weatherParams to the scene / water.
		/// </summary>
		public static void ApplyWeather(WeatherParams weatherParams)
		{
			if (Sea != null)
			{
				Sea.Waves0 = weatherParams.Waves0;
				Sea.UpdateWaveParameters();
			}
		}
	}
}
