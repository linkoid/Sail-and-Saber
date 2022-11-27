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

		[SpanRange( 0, 1)] public Span VolumeRange = new Span(0.1f, .4f);
		[SpanRange(-3, 3)] public Span PitchRange = new Span(0.9f, 1.1f);
		[SpanRange( 0, 1)] public Span DelayRange = new Span(0, 0);

		public List<AudioClip> Sounds = new List<AudioClip>();

		void Awake()
		{
			AudioSource.playOnAwake = false;
		}

		public void Play()
		{
			AudioClip clip = Sounds[Random.Range(0, Sounds.Count)];
			AudioSource.volume = VolumeRange.RandomInRange() * PlayerPrefs.GetFloat("SFX_Volume");

			AudioSource.pitch = PitchRange.RandomInRange();
			AudioSource.clip = clip;
			float delay = DelayRange.RandomInRange();
			if (delay <= 0)
			{
				AudioSource.Play();
			}
			else
			{
				AudioSource.PlayDelayed(delay);
			}
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

