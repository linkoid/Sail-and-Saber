using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CustomUtils;
using PirateGame.Sea;
using static UnityEngine.UI.Image;

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
		[SerializeField] Vector3 m_TransitionOrigin = Vector3.zero;

		void Start()
		{
			Started = true;
		}

		void Update()
		{
			UpdateTransition();
		}

		public void TransitionWeather(WeatherParams newWeather, Vector3 origin, float duration = 5f)
		{
			OldWeather = CurrentWeather;
			m_IsTransitioning = true;
			m_TransitionTimer = 0f;
			m_TransitionDuration = duration;
			NewWeather = newWeather;
			m_TransitionOrigin = origin;
		}

		public void TransitionWeather(WeatherParams newWeather, float duration = 5f)
		{
			TransitionWeather(newWeather, Vector3.zero, duration);
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


			if (m_TransitionDuration > 0 && m_TransitionTimer > 0)
			{
				float lerpFactor = m_TransitionTimer / m_TransitionDuration;
				CurrentWeather = WeatherParams.Lerp(OldWeather, NewWeather, lerpFactor, m_TransitionOrigin);

				float fromPhase = OldWeather.Waves0.Phase(Time.timeSinceLevelLoad, m_TransitionOrigin);
				float newPhase = CurrentWeather.Waves0.Phase(Time.timeSinceLevelLoad, m_TransitionOrigin);
				if (newPhase != fromPhase)
				{
					Debug.LogError($"newPhaseWeatherManager ({newPhase}) != fromPhase ({fromPhase})");
				}

				ApplyWeather(CurrentWeather);

			}


			if (m_TransitionTimer >= m_TransitionDuration)
			{
				m_IsTransitioning = false;
				m_TransitionTimer = 0;
				CurrentWeather = NewWeather;
				_currentWeather.Waves0 = WaveParams.MatchPhase(OldWeather.Waves0, NewWeather.Waves0, m_TransitionOrigin);
				ApplyWeather(CurrentWeather);
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

		[SerializeField, ReadOnly] Vector3 m_OldPosition;
		[SerializeField, ReadOnly] Vector3 m_NewPosition;
		[SerializeField, ReadOnly] Vector3 m_CurPosition;
		[SerializeField, ReadOnly] Vector3 m_TmpPosition;

		[SerializeField, ReadOnly] float m_OldPhase;
		[SerializeField, ReadOnly] float m_NewPhase;
		[SerializeField, ReadOnly] float m_CurPhase;
		[SerializeField, ReadOnly] float m_TmpPhase;

		void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(m_TransitionOrigin, 5);

			float lerpFactor = m_TransitionTimer / m_TransitionDuration;
			var tmpWeather = WeatherParams.Lerp(OldWeather, NewWeather, lerpFactor, m_TransitionOrigin);

			m_OldPhase = OldWeather    .Waves0.Phase(Time.timeSinceLevelLoad, m_TransitionOrigin);
			m_NewPhase = NewWeather    .Waves0.Phase(Time.timeSinceLevelLoad, m_TransitionOrigin);
			m_CurPhase = CurrentWeather.Waves0.Phase(Time.timeSinceLevelLoad, m_TransitionOrigin);
			m_TmpPhase = tmpWeather    .Waves0.Phase(Time.timeSinceLevelLoad, m_TransitionOrigin);

			m_OldPosition = OldWeather    .Waves0.Position(Time.timeSinceLevelLoad, m_TransitionOrigin);
			m_NewPosition = NewWeather    .Waves0.Position(Time.timeSinceLevelLoad, m_TransitionOrigin);
			m_CurPosition = CurrentWeather.Waves0.Position(Time.timeSinceLevelLoad, m_TransitionOrigin);
			m_TmpPosition = tmpWeather    .Waves0.Position(Time.timeSinceLevelLoad, m_TransitionOrigin);

			m_OldPosition.y /= OldWeather    .Waves0.Amplitude;
			m_NewPosition.y /= NewWeather    .Waves0.Amplitude;
			m_CurPosition.y /= CurrentWeather.Waves0.Amplitude;
			m_TmpPosition.y /= tmpWeather    .Waves0.Amplitude;

			Gizmos.color = Color.magenta; 																										  
			Gizmos.DrawWireSphere(m_OldPosition, 1f);
			Gizmos.color = Color.yellow; 																										  
			Gizmos.DrawWireSphere(m_CurPosition, m_TransitionTimer / m_TransitionDuration + 1);
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(m_NewPosition, 2f);
		}
	}
}
