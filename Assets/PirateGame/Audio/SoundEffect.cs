using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CustomUtils;

namespace PirateGame
{
	[RequireComponent(typeof(AudioSource))]
	public class SoundEffect : MonoBehaviour
	{
		[SerializeField]
		private bool m_PreviewButton = false;

		public AudioSource AudioSource => this.GetComponent<AudioSource>();

		[SpanRange( 0, 0.1f)] public Span VolumeRange = new Span(0.01f, .04f);
		[SpanRange(-3, 3)] public Span PitchRange = new Span(0.9f, 1.1f);

		public List<AudioClip> Sounds = new List<AudioClip>();



		public void Play()
		{
			AudioClip clip = Sounds[Random.Range(0, Sounds.Count)];
			AudioSource.volume = PlayerPrefs.GetFloat("SFX_Volume");;

			AudioSource.pitch = PitchRange.RandomRange();
			AudioSource.clip = clip;
			AudioSource.Play();
		}
		
		void OnValidate()
		{
			if (m_PreviewButton)
			{
				Play();
				m_PreviewButton = false;
			}
		}
	}
}

