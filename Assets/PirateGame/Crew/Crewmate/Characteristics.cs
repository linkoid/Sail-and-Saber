using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PirateGame.Crew.Characteristics;

namespace PirateGame.Crew
{
	public class Characteristics : MonoBehaviour
	{
		[System.Serializable]
		public struct Feature
		{
			public Renderer Renderer;
			[Range(0,1)]
			public float HideChance;
		}

		[System.Serializable]
		public struct MaterialPointer
		{
			public Renderer Renderer;
			public int MaterialIndex;
		}

		[System.Serializable]
		public struct ColorScheme
		{
			public List<MaterialPointer> MaterialPointers;
			public Color ColorVariance;
		}

		public List<Feature> Features = new List<Feature>();
		public List<ColorScheme> ColorSchemes = new List<ColorScheme>();

		// Start is called before the first frame update
		void Start()
		{
			Apply();
		}

		public void Apply(int seed)
		{
			Random.State preState = Random.state;
			Random.InitState(seed);
			Apply();
			Random.state = preState;
		}

		public void Apply()
		{
			foreach (var feature in Features)
			{
				ApplyFeature(feature);
			}
			foreach (var scheme in ColorSchemes)
			{
				ApplyColorScheme(scheme);
			}
		}

		public void ApplyFeature(Feature feature)
		{
			if (feature.HideChance > Random.value)
			{
				feature.Renderer.forceRenderingOff = true;
				return;
			}
		}

		public void ApplyColorScheme(ColorScheme scheme)
		{
			var color = new Color(
					Random.Range(0, scheme.ColorVariance.r),
					Random.Range(0, scheme.ColorVariance.g),
					Random.Range(0, scheme.ColorVariance.b),
					0
				);
			color *= scheme.ColorVariance;

			foreach (var materialPointer in scheme.MaterialPointers)
			{
				var material = materialPointer.Renderer.materials[materialPointer.MaterialIndex];
				material.SetColor("_BaseColor", material.GetColor("_BaseColor") + color);
			}
		}
	}
}