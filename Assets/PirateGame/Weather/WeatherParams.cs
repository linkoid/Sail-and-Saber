using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Weather
{
	[System.Serializable]
	public struct WeatherParams
	{
		public float WaveAmplitude;
		public float WaveDistance;
		public float WaveSpeed;

		// Maybe custom water material colors?
		// Maybe custom sky colors?

		/// <summary>
		/// Lerps from one weather params to another, based on a factor between 0 and 1.
		/// </summary>
		public static WeatherParams Lerp(WeatherParams from, WeatherParams to, float factor)
		{
			var newParams = new WeatherParams();

			newParams.WaveAmplitude = Mathf.Lerp(from.WaveAmplitude, to.WaveAmplitude, factor);
			newParams.WaveDistance  = Mathf.Lerp(from.WaveDistance , to.WaveDistance , factor);
			newParams.WaveSpeed     = Mathf.Lerp(from.WaveSpeed    , to.WaveSpeed    , factor);

			return newParams;
		}
	}
}
