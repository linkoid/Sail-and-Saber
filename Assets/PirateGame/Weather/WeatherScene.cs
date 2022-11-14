using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Weather
{
	public class WeatherScene : MonoBehaviour
	{
		public WeatherParams Parameters => m_Parameters;
		[SerializeField] private WeatherParams m_Parameters;
	}
}
