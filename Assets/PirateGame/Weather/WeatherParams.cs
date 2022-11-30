using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using UnityEngine;

namespace PirateGame.Weather
{
	[System.Serializable]
	public struct WeatherParams
	{
		public WaveParams Waves0;

		public float WaveAmplitude;
		public float WaveDistance;
		public float WaveSpeed;
		public Vector2 WaveDirection;

		// Maybe custom water material colors?
		// Maybe custom sky colors?

		/// <summary>
		/// Lerps from one weather params to another, based on a factor between 0 and 1.
		/// </summary>
		public static WeatherParams Lerp(WeatherParams from, WeatherParams to, float factor)
		{
			var newParams = new WeatherParams();

			newParams.Waves0 = WaveParams.Lerp(from.Waves0, to.Waves0, factor);

			return newParams;
		}
	}

	[System.Serializable]
	public struct WaveParams
	{
		public float Amplitude;
		public float Distance;
		public float Speed;
		public Vector2 Direction;

		[BurstDiscard]
		public static WaveParams Lerp(WaveParams from, WaveParams to, float factor)
		{
			var newParams = new WaveParams();

			Vector3 fromDirection = new Vector3(from.Direction.x, 0, from.Direction.y);

			newParams.Amplitude = Mathf.Lerp   (from.Amplitude, to.Amplitude, factor);
			newParams.Distance  = Mathf.Lerp   (from.Distance , to.Distance , factor);
			newParams.Speed     = Mathf.Lerp   (from.Speed    , to.Speed    , factor);
			newParams.Direction = Vector3.Lerp(from.Direction, to.Direction, factor);

			return newParams;
		}
	}
}
