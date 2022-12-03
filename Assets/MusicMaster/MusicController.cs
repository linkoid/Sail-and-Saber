using System.Collections.Generic;
using UnityEngine;

namespace MusicMaster
{
	public static class MusicController
	{
		public static Song CurrentSong { get; private set; }
		public static Orchestration CurrentOrchestration => CurrentSong?.Orchestration;


		public static float GlobalVolume = 0.4f;

		private static List<Song> _songs = new List<Song>();
		private static bool _hasStarted = false;
		
		
		public static void Start()
		{
			if (_hasStarted)
				return;
			
			// Load songs ?


			_hasStarted = true;
		}

		public static void Update()
		{
			CurrentSong?.Update();
		}


		public static void AddTrack(ISong isong, Track track)
		{
			if (!_hasStarted)
				Start();

			Song song = GetSong(isong);
			if (song != null)
			{
				song.AddTrack(track);
			}
		}

		public static void RemoveTrack(ISong isong, Track track)
		{
			if (!_hasStarted)
				Start();

			Song song = GetSong(isong);
			if (song != null)
			{
				song.RemoveTrack(track);
			}
		}

		public static void PlaySong(ISong isong, bool restart = true) => PlaySongScheduled(isong, AudioSettings.dspTime, restart);
		public static void PlaySong(string songIdentity, bool restart = true) => PlaySongScheduled(songIdentity, AudioSettings.dspTime, restart);

		public static void PlaySongScheduled(ISong isong, double time, bool restart = true)
		{
			//if (!MusicController._hasStarted) return;

			Debug.Log($"Play song \"{isong.Name}\"");

			Song song = GetSong(isong);

			if (CurrentSong != null && (CurrentSong != song || restart))
			{
				CurrentSong.Stop();
			}
			CurrentSong?.ResetOrchestration();

			CurrentSong = song;
			song.PlayScheduled(time, restart);
			song.ResetOrchestration();
		}
		public static void PlaySongScheduled(string songIdentity, double time, bool restart = true)
		{
			//if (!MusicController._hasStarted) return;

			Debug.Log($"Play song \'{songIdentity}\'");

			Song song = GetSong(songIdentity);

			if (CurrentSong != null && (CurrentSong != song || restart))
			{
				CurrentSong.Stop();
			}
			CurrentSong?.ResetOrchestration();

			CurrentSong = song;
			song.PlayScheduled(time, restart);
			song.ResetOrchestration();
		}

		public static void AddSong(Song song)
		{
			Song found = _songs.Find(MusicManager.SongIdentityMatches(song));
			if (found == null)
				_songs.Add(song);
		}
		
		public static bool HasSong(ISong isong)
		{
			return _songs.Find(MusicManager.SongIdentityMatches(isong)) != null;
		}
		public static bool HasSong(string songIdentity)
		{
			return _songs.Find(MusicManager.SongIdentityMatches(songIdentity)) != null;
		}

		// Gets a song. If it can't be found, it will be loaded.
		private static Song GetSong(ISong isong)
		{
			Song song = _songs.Find(MusicManager.SongIdentityMatches(isong));

			if (song != null)
				return song;

			// If the song wasn't found
			return NewSong(isong);
		}
		private static Song GetSong(string songIdentity)
		{
			Song song = _songs.Find(MusicManager.SongIdentityMatches(songIdentity));

			if (song != null)
				return song;

			// If the song wasn't found
			return NewSong(songIdentity);
		}

		private static Song NewSong(ISong isong)
		{
			Debug.Log($"New song \"{isong.Name}\" is auto-loading.");
			if (typeof(Song).IsAssignableFrom(isong.GetType()))
			{
				AddSong(isong as Song);
				return isong as Song;
			}
			else if ( typeof(SongAsset).IsAssignableFrom(isong.GetType()) ) {
				return MusicManager.LoadSong(isong as SongAsset);
			}
			else
			{
				return MusicManager.LoadSong(isong as Song);
			}
		}
		private static Song NewSong(string songIdentity)
		{
			Debug.Log($"New song \'{songIdentity}\' is auto-loading.");
			return MusicManager.LoadSong(songIdentity);
		}

	}
}
