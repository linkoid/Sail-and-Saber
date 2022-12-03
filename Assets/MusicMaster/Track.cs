using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CustomUtils;

namespace MusicMaster
{
	public class Track : MonoBehaviour, IOrchestratable
	{
		//public int SongIndexNumber => _songAsset.Index;
		//public int TrackIndexNumber;

		public ISong Song { get; private set; }
		public bool IsSyncTrack { get => _isSyncTrack; private set => _isSyncTrack = value; }



		public IReadOnlyList<ArrangementOption> MusicArrangementOptions => _musicArrangementOptions;
		public Elements MusicElement => _musicElement;
		public Sections MusicSection => _musicSection;

		public int CurrentSample
		{
			get => _audioSource != null ? _audioSource.timeSamples + _sampleOffset : -1;
			set { if (_audioSource != null) _audioSource.timeSamples = value - _sampleOffset; }
		}
		public float MixVolume { get => _mixVolume; set { _mixVolume = value; UpdateTargetVolume(); } }
		public float TrueVolume => _audioSource.volume;
		protected float FadeVolume
		{
			get => _audioSource.volume / MusicController.GlobalVolume;
			private set => _audioSource.volume = value * MusicController.GlobalVolume;
		}



		//public bool Loop => this._audioSource.loop;

		[SerializeReference]
		private SongAsset _songAsset;

		[SerializeField]
		private bool _isSyncTrack = false;

		[SerializeField]
		private Sections _musicSection = Sections.Piano;
		[SerializeField]
		private Elements _musicElement = Elements.Melody;
		[SerializeField]
		private List<ArrangementOption> _musicArrangementOptions = new List<ArrangementOption>() { ArrangementOption.Default };


		[SerializeField, SpanRange(0, 1)]
		private Span _developmentFadeIn = new Span(0, 0);
		[SerializeField, SpanRange(0, 1.1f)]
		private Span _developmentFadeOut = new Span(1.1f, 1.1f);

		[SerializeField, Range(0, 1)]
		private float _mixVolume = 1;


		[SerializeField]
		private bool _mute;

		[SerializeField, Tooltip("In %volume per second.")]
		private float _fadeSpeed = 1;

		[SerializeField]
		private int _sampleOffset = 0;

		[SerializeField]
		private bool _randomSampleOffset = false;
		[SerializeField, SpanRange(-10000, 10000), SpanInt]
		private Span _randomSampleOffsetRange = new Span(0, 0);

		private float _orchestratedVolume = 1;
		private float _targetVolume = 1;

		private Arrangements _oldArrangements = Arrangements.Base;


		private AudioSource _audioSource;


		// XXX This is used by Song, but it would be ideal if it wasn't needed.
		public AudioClip Clip => _audioSource.clip;



		void Awake()
		{
			_audioSource = this.gameObject.GetComponent<AudioSource>();
			Song = _songAsset;
		}

		void Start()
		{
			//Object.DontDestroyOnLoad((Object)this);
			_audioSource.loop = true;
			MusicController.AddTrack(Song, this);

			if (_randomSampleOffset)
			{
				_sampleOffset = (int)_randomSampleOffsetRange.RandomInRange();
			}
		}

		void OnDestroy()
		{
			MusicController.RemoveTrack(Song, this);
		}

		void Update()
		{
			float deltaFade = _fadeSpeed * Time.unscaledDeltaTime;

			if (!_mute)
			{
				_audioSource.mute = false;

				if (FadeVolume < _targetVolume)
				{
					FadeVolume += deltaFade;
					// Make sure we don't go over it.
					if (FadeVolume > _targetVolume)
						FadeVolume = _targetVolume;
				}
				else if (FadeVolume > _targetVolume)
				{
					FadeVolume -= deltaFade;
					// Make sure we don't go under it.
					if (FadeVolume < _targetVolume)
						FadeVolume = _targetVolume;
				}
			}

			else if (_mute)
			{
				if (FadeVolume > 0)
				{
					FadeVolume -= deltaFade;
				}
				else
				{
					FadeVolume = 0;
					_audioSource.mute = true;
				}
			}
				
		}

		// For changing _mixVolume in the editor
		void OnValidate()
		{
			UpdateTargetVolume();
		}


		public void Play() => PlayScheduled(AudioSettings.dspTime);
		public void PlayScheduled(double time)
		{
			if (!this.isActiveAndEnabled) return;

			//Debug.Log($"Play track {this.name}", this);
			_audioSource.PlayScheduled(time);
			_mute = false;
			_fadeSpeed = 1;
			_targetVolume = _mixVolume;
			FadeVolume = 0.0f;
			CurrentSample = 0;

			UpdateTargetVolume();
		}

		public void Stop()
		{
			//Debug.Log($"Play track {this.name}", this);
			_audioSource.Stop();
			_mute = true;
			_fadeSpeed = 0.01666667f;
			FadeVolume = 0.0f;

			UpdateTargetVolume();
		}

		public void FadeInTrack(float duration)
		{
			//Volume = 0.0f;
			_mute = false;
			_fadeSpeed = 1/duration;
		}

		public void FadeOutTrack(float duration)
		{
			_mute = true;
			_fadeSpeed = 1/duration;
		}


		
		public void Orchestrate(in Orchestration orchestration)
		{
			// Handle muting by section
			if (!orchestration.Sections.HasFlag(this.MusicSection))
			{
				_orchestratedVolume = 0;
				UpdateTargetVolume();
				return;
			}

			// Handle muting by element
			if (!orchestration.Elements.HasFlag(this.MusicElement))
			{
				_orchestratedVolume = 0;
				UpdateTargetVolume();
				return;
			}


			// Handle development
			float inFactor = _developmentFadeIn.InverseMap(orchestration.Development);
			float outFactor = _developmentFadeOut.InverseMap(orchestration.Development);
			outFactor = (outFactor - 0.5f) * -1 + 0.5f;
			float developmentVolume = inFactor * outFactor;

			Arrangements changed = _oldArrangements ^ orchestration.Arrangements;
			CombineMode combineMode = CombineMode.Multiply;

			// Handle arrangements
			float arrangementVolume = 0;
			foreach (ArrangementOption arr in _musicArrangementOptions)
			{
				if (orchestration.Arrangements.HasFlag(arr.Arrangements))
				{
					if (changed.HasFlag(arr.Arrangements))
						// It was just added, use fade to
						_fadeSpeed = arr.FadeTo;

					if (arr.Include)
						arrangementVolume = 1;
					if (arr.Exclude)
						arrangementVolume = 0;

					if (arr.Include || arr.Exclude)
						combineMode = arr.DevelopmentCombine;
				}
				else
				{
					if (changed.HasFlag(arr.Arrangements))
						// It was just removed, use fade from
						_fadeSpeed = arr.FadeFrom;
				}
			}

			_oldArrangements = orchestration.Arrangements;

			switch(combineMode)
			{
				default:
				case CombineMode.Multiply:
					_orchestratedVolume = developmentVolume * arrangementVolume;
					break;
				case CombineMode.Max:
					_orchestratedVolume = Mathf.Max(developmentVolume, arrangementVolume);
					break;
			}


			UpdateTargetVolume();
		}
		


		protected void UpdateTargetVolume()
		{
			_targetVolume = _orchestratedVolume * _mixVolume;
		}
	}
}
