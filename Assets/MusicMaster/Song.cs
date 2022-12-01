using System.Collections.Generic;
using UnityEngine;

namespace MusicMaster
{
	public class Song : ISong
	{
		public string Name          { get; private set; }
		public string Identity      { get; private set; }
		public int    IDNumber      { get; private set; }
		public int    LoopbackPoint { get; set; } = 0;


		public GameObject    GameObject    { get; private set; }
		public bool          IsPlaying     { get; private set; }
		public Orchestration Orchestration { get; private set; } = new Orchestration();


		private List<Track> _tracks = new List<Track>();
		private float _development = 0;

		private int _previousSample = -1;


		public Song(ISong isong, GameObject gameObject)
		{
			this.Name          = isong.Name         ;
			this.Identity      = isong.Identity     ;
			this.IDNumber      = isong.IDNumber     ;
			this.LoopbackPoint = isong.LoopbackPoint;

			this.GameObject = gameObject;
			this.Orchestration.OnUpdate += OnOrchestrationUpdated;
		}


		public void Update()
		{
			// Make sure that all the tracks are on the same sample. (Syncronized)
			int sample = -1;
			foreach (Track track in _tracks)
			{
				if (sample < 0)
				{
					// This is the first track. Align all other tracks to this one.
					sample = track.CurrentSample;

					int samplesPerFrame = (int)(track.Clip.frequency * Time.unscaledDeltaTime);
					
					
					if ((_previousSample > sample && sample < LoopbackPoint) 
						|| (LoopbackPoint > 0 && sample > track.Clip.samples - samplesPerFrame * 4)) // If likely to loop before next update(), change it now.
					{
						// The sound looped, and it did not go back to the LoopbackPoint.
						// If LoopbackPoint == 0 and it looped naturally (via Unity's loop option)
						// the sample would not be < LoopbackPoint.
						// Make sure it goes to the loop-back point.
						sample = LoopbackPoint - (track.Clip.samples - sample);
						track.CurrentSample = sample;
					}
				}
				else
				{
					track.CurrentSample = sample;
				}
			}
			_previousSample = sample;
		}


		public void Play() => PlayScheduled(AudioSettings.dspTime);
		public void PlayScheduled(double time, bool restart = true)
		{
			if (restart || !IsPlaying)
			{
				foreach (Track track in _tracks)
				{
					track.PlayScheduled(time);
					track.Orchestrate(Orchestration);
				}
			}
			else
			{
				Debug.Log($"\"{Name}\" was already playing, and it was requested to not restart.");
			}
			if (restart == false)
			{
				Debug.Log($"Song \"{Name}\".IsPlaying = {IsPlaying}");
			}
			IsPlaying = true;
			//Debug.Log($"Song \"{Name}\".IsPlaying = {IsPlaying}");
		}

		public void Stop()
		{
			foreach (Track track in _tracks)
			{
				track.Stop();
			}
			IsPlaying = false;
		}

		public void AddTrack(Track track)
		{
			//Debug.Log($"Add track \"{track.name}\" to song \"{Name}\"");

			_tracks.Add(track);

			if (IsPlaying)
			{
				track.Play();
				track.Orchestrate(Orchestration);
			}
			else
			{
				track.Stop();
			}
		}

		public void ResetOrchestration()
		{
			Orchestration.Reset();
		}



		protected void OnOrchestrationUpdated(Orchestration orchestration)
		{
			foreach (Track track in _tracks)
			{
				track.Orchestrate(orchestration);
			}
		}
	}
}


