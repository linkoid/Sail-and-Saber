using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicMaster;

public class PlaySongOnLoad : MonoBehaviour
{

	[SerializeField] private string _songIdentity;
	[SerializeField] private SongAsset _songAsset;
	private Song _song;

	[Header("On Play")]
	[SerializeField] private bool _restart = true;
	[SerializeField] private bool _develop = true;
	[SerializeField] private float _developDuration = 120;

	[Header("Orchestrate Sections")]
	[SerializeField] private bool _removeAllSections = false;
	[SerializeField] private Sections[] _removeSections;
	[SerializeField] private Sections[] _addSections;
	

	[Header("Orchestrate Elements")]
	[SerializeField] private bool _removeAllElements = false;
	[SerializeField] private Elements[] _removeElements;
	[SerializeField] private Elements[] _addElements;

	private float _developStartTime;

	// Start is called before the first frame update
	void Start()
	{
		StartSong();
		OrchestrateSong();

		_developStartTime = Time.realtimeSinceStartup;
	}

	// Update live as they are being changed in the inspector
	void OnValidate()
	{
		OrchestrateSong();

		_developStartTime = Time.realtimeSinceStartup;
	}

	void Update()
	{
		if (_develop)
		{
			float elapsed = Time.realtimeSinceStartup; //(Time.realtimeSinceStartup - _developStartTime);
			float development = elapsed / _developDuration;
			MusicMaster.MusicController.CurrentOrchestration.SetDevelopment(development);
		}
	}

	// Start playing the song.
	private void StartSong()
	{
		// We need either the Song itself, or a SongAsset to identify it.
		ISong isong = (_song != null) ? _song : _songAsset as ISong;

		// If you want access to the Song object after it loads, it must be loaded explicitly.
		if (isong == null)
		{
			// Find the SongAsset using it's string identity
			if (!MusicMaster.MusicManager.FindSong(_songIdentity, out _songAsset))
			{
				Debug.LogError($"Could not find the song \"{_songIdentity}\"");
				return;
			}
		}

		if (_song == null)
		{
			_song = MusicMaster.MusicManager.LoadSong(_songAsset);
			MusicMaster.MusicController.AddSong(_song);
		}

		// If the song isn't already loaded, it will load when PlaySong() is called.
		if (isong == null)
		{
			// This is the easiest way to play a song, using a string identifier.
			MusicMaster.MusicController.PlaySong(_songIdentity, _restart);
		}
		else
		{
			// But if you have access to some ISong, this is the safer method.
			MusicMaster.MusicController.PlaySong(isong, _restart);
		}
		
	}

	// Enables and disables tracks according to settings using the MusicMaster API.
	private void OrchestrateSong()
	{
		Orchestration orch = MusicController.CurrentOrchestration;

		if (orch == null) return;

		// Enable/disable tracks by section
		orch.RemoveSections(_removeAllSections ? Sections.All : Combine(_removeSections));
		orch.AddSections(Combine(_addSections));

		// Enable/disable tracks by element
		orch.RemoveElements(_removeAllElements ? Elements.All : Combine(_removeElements));
		orch.AddElements(Combine(_addElements));
	}

	// Because Sections uses bit-flags, we can combine multiple into one.
	private Sections Combine(Sections[] sections)
	{
		Sections combined = Sections.None;
		foreach (Sections section in sections)
		{
			combined |= section;
		}
		return combined;
	}

	// Because Elements uses bit-flags, we can combine multiple into one.
	private Elements Combine(Elements[] elements)
	{
		Elements combined = Elements.None;
		foreach (Elements element in elements)
		{
			combined |= element;
		}
		return combined;
	}
}
