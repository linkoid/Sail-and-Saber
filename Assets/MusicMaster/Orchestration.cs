using System;
using System.Collections.Generic;
using UnityEngine;

namespace MusicMaster
{
	public enum Sections : ushort
	{
		None = 0,
		
		Piano      = 1 <<  0,
		Chromatic  = 1 <<  1,
		Organ      = 1 <<  2,
		Guitar     = 1 <<  3,
		Bass       = 1 <<  4,
		Strings    = 1 <<  5,
		Ensemble   = 1 <<  6,
		Brass      = 1 <<  7,
		Reed       = 1 <<  8,
		Pipe       = 1 <<  9,
		SynthLead  = 1 << 10,
		SynthPad   = 1 << 11,
		SynthFXs   = 1 << 12,
		Ethnic     = 1 << 13,
		Percussion = 1 << 14,
		SFX        = 1 << 15,

		//[HideInInspector]
		All = ushort.MaxValue,
	}

	public enum Elements : ushort
	{
		None = 0,

		Melody     = 1 <<  0,
		Harmony    = 1 <<  1,
		Lead       = 1 <<  2,
		Chords     = 1 <<  3,
		Rhythm     = 1 <<  4,
		Bass       = 1 <<  5,
		SFX        = 1 <<  6,

		//[HideInInspector]
		All = ushort.MaxValue,
	}

	public enum Arrangements : uint 
	{
		None = 0,

		Base = 1,

		Inside     = 1 <<  1,
		Outside    = 1 <<  2,

		Idle       = 1 <<  3,
		Slow       = 1 <<  4,
		Fast       = 1 <<  5,

		Driving    = 1 <<  6,
		Swimming   = 1 <<  7,
		Flying     = 1 <<  8,
		
		Rock       = 1 << 20,
		Techno     = 1 << 21,
		Disco      = 1 << 22,

		Aliens     = 1 << 30,

		//[HideInInspector]
		All = uint.MaxValue,
	}

	[System.Serializable]
	public class Orchestration
	{

		public Arrangements Arrangements => _arrangements;
		public Elements Elements => _elements;
		public Sections Sections => _sections;
		public float Development => _development;

		public bool AlmostAllTracks { get => _almostAllTracks; set { _almostAllTracks = value; OnUpdate?.Invoke(this); } }
		public bool ForceAllTracks  { get => _forceAllTracks ; set { _forceAllTracks  = value; OnUpdate?.Invoke(this); } }
		


		[SerializeField] private Arrangements _arrangements;
		[SerializeField] private Elements _elements;
		[SerializeField] private Sections _sections;
		[SerializeField] private float _development;

		private bool _almostAllTracks;
		private bool _forceAllTracks;

		public event System.Action<Orchestration> OnUpdate;

		//public static Orchestration Default = new Orchestration();

		public Orchestration() :
			this(Arrangements.Base, Elements.All, Sections.All)
		{
			_almostAllTracks = false;
			_forceAllTracks = false;
		}

		public Orchestration(
			Arrangements arrangements = Arrangements.None,
			Elements elements = Elements.All,
			Sections sections = Sections.All
		)
		{
			_arrangements = arrangements;
			_elements = elements;
			_sections = sections;
			_development = 0;
			_almostAllTracks = false;
			_forceAllTracks = false;
			OnUpdate = null;
		}

		public Orchestration(Orchestration orchestration) :
			this(
				orchestration.Arrangements,
				orchestration.Elements,
				orchestration.Sections
			)
		{
			AlmostAllTracks = orchestration.AlmostAllTracks;
			ForceAllTracks = orchestration.ForceAllTracks;
		}



		public void AddArrangements(Arrangements arrangements)
		{
			_arrangements |= arrangements;
			OnUpdate?.Invoke(this);
		}
		public void RemoveArrangements(Arrangements arrangements)
		{
			_arrangements &= ~arrangements;
			OnUpdate?.Invoke(this);
		}



		public void AddElements(Elements elements)
		{
			_elements |= elements;
			OnUpdate?.Invoke(this);
		}
		public void RemoveElements(Elements elements)
		{
			_elements &= ~elements;
			OnUpdate?.Invoke(this);
		}



		public void AddSections(Sections sections)
		{
			_sections |= sections;
			OnUpdate?.Invoke(this);
		}

		public void RemoveSections(Sections sections)
		{
			_sections &= ~sections;
			OnUpdate?.Invoke(this);
		}


		public void SetDevelopment(float development)
		{
			_development = Mathf.Clamp01(development);
			OnUpdate?.Invoke(this);
		}

		public void Reset()
		{
			_arrangements = Arrangements.Base;
			_elements = Elements.All;
			_sections = Sections.All;
			_development = 0;
			_almostAllTracks = false;
			_forceAllTracks = false;
			OnUpdate?.Invoke(this);
		}
	}

	public enum CombineMode
	{
		Multiply,
		Max,
	}

	[System.Serializable]
	public struct ArrangementOption
	{
		public Arrangements Arrangements;
		public CombineMode DevelopmentCombine;
		public bool Include;
		public bool Exclude;
		public float Volume;
		public float FadeTo;
		public float FadeFrom;

		public static ArrangementOption Default = new ArrangementOption()
		{
			Arrangements = Arrangements.Base,
			DevelopmentCombine = CombineMode.Multiply,
			Volume = 1,
			Include = true,
			Exclude = false,
			FadeTo = 1,
			FadeFrom = 1,
		};
	}
	
	public interface IOrchestratable
	{
		IReadOnlyList<ArrangementOption> MusicArrangementOptions { get; }
		Elements MusicElement { get; }
		Sections MusicSection { get; }

		void Orchestrate(in Orchestration orchestration);
	}
}
