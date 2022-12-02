using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace PirateGame.Weather
{
	[System.Serializable]
	public struct WeatherParams
	{
		public WaveParams Waves0;

		// Maybe custom water material colors?
		// Maybe custom sky colors?

		/// <summary>
		/// Lerps from one weather params to another, based on a factor between 0 and 1.
		/// </summary>
		public static WeatherParams Lerp(WeatherParams from, WeatherParams to, float factor, Vector3 origin)
		{
			var newParams = new WeatherParams();

			newParams.Waves0 = WaveParams.Lerp(from.Waves0, to.Waves0, factor, origin);

			WaveParams.AssertPhase(newParams.Waves0, from.Waves0, origin, "WeatherParams.Lerp");

			return newParams;
		}

		/// <inheritdoc cref="Lerp(WeatherParams, WeatherParams, float, Vector2)"/>
		public static WeatherParams Lerp(WeatherParams from, WeatherParams to, float factor)
		{
			return Lerp(from, to, factor, Vector3.zero);
		}
	}

	[System.Serializable]
	public struct WaveParams
	{
		public float Amplitude;
		public float Distance;
		public float Speed;
		public Vector2 Direction;
		public float PhaseOffset;

		[BurstDiscard]
		public static WaveParams Lerp(in WaveParams from, in WaveParams to, float factor, Vector3 origin)
		{
			var newParams = new WaveParams();

			newParams.Amplitude = Mathf.Lerp   (from.Amplitude, to.Amplitude, factor);
			newParams.Distance  = Mathf.Lerp   (from.Distance , to.Distance , factor);
			newParams.Speed     = Mathf.Lerp   (from.Speed    , to.Speed    , factor);
			newParams.Direction = Vector3.Slerp(from.Direction, to.Direction, factor);
			newParams.PhaseOffset = 0;

			newParams = MatchPhase(from, newParams, origin);

			AssertPhase(newParams, from, origin, "WaveParams.Lerp");
			
			return newParams;
		}

		public static WaveParams MatchPhase(in WaveParams from, WaveParams to, Vector3 origin)
		{
			float fromPhase = from.Phase(Time.timeSinceLevelLoad, origin);
			float toPhase = to.Phase(Time.timeSinceLevelLoad, origin);
			float phaseDiff = toPhase - fromPhase;
			if (!float.IsNaN(phaseDiff))
				to.PhaseOffset -= phaseDiff;

			return to;
		}

		public static void AssertPhase(in WaveParams oldParams, in WaveParams newParams, Vector3 origin, string source)
		{
			//float oldPhase = oldParams.Phase(Time.timeSinceLevelLoad, origin);
			//float newPhase = newParams.Phase(Time.timeSinceLevelLoad, origin);
			//if (newPhase != oldPhase)
			//{
			//	Debug.LogError($"{source}: newPhase ({newPhase}) != oldPhase ({oldPhase})");
			//}
		}

		[BurstDiscard]
		public static WaveParams Lerp(WaveParams from, WaveParams to, float factor)
		{
			return Lerp(from, to, factor, Vector3.zero);
		}
		
		public float Phase(float time, Vector3 position)
		{
			var direction = Direction.normalized;
			return (position.x * direction.x + position.z * direction.y + time * Speed) / Distance + PhaseOffset;
		}

		public Vector3 Position(float time, Vector3 position)
		{
			position.y = Mathf.Sin(Phase(time, position)) * Amplitude;
			return position;
		}
	}
}
