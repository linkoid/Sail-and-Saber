using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MusicMaster
{
    public static class MusicManager
    {
		// cahced level packages
		private static bool _cached = false;
		private static ICollection<MusicPackage> _musicPackages = new List<MusicPackage>();

		private static MusicHelper _musicHelper;

		public static System.Predicate<string> PackageNameMatches(string pattern)
		{
			return (check) =>
			{
				return string.Equals(pattern, check, System.StringComparison.InvariantCultureIgnoreCase);
			};
		}

		public static System.Predicate<ISong> SongIdentityMatches(string pattern)
		{
			return (check) =>
			{
				return string.Equals(pattern, check.Identity, System.StringComparison.InvariantCultureIgnoreCase);
			};
		}
		public static System.Predicate<ISong> SongIdentityMatches(ISong song) {
			if (song != null)
			{
				return SongIdentityMatches(song.Identity);
			}
			// else
			return (check) => false;
		}

		

		public static Song LoadSong(SongAsset songAsset)
		{
			if (MusicController.HasSong(songAsset))
			{
				Debug.LogWarning($"The SongAsset \"{songAsset.Name}\" has already been loaded");
				return null;
			}

			Debug.Log($"Loading SongAsset \"{songAsset.Name}\"");
			GameObject newGameObject = Object.Instantiate(songAsset.Prefab) as GameObject;
			try
			{
				Song newSong = new Song(songAsset, newGameObject);

				Object.DontDestroyOnLoad(newGameObject);

				MusicController.AddSong(newSong);

				CreateMusicHelper();

				return newSong;
			}
			catch (System.Exception e)
			{
				Object.DestroyImmediate(newGameObject);
				Debug.LogError($"Error loading SongAsset \"{songAsset.Name}\": {e.Message}\n{e.StackTrace}");
				return null;
			}
		}

		public static Song LoadSong(Song song)
		{
			Song newSong = song;
			Object.DontDestroyOnLoad(song.GameObject);

			MusicController.AddSong(newSong);
			
			CreateMusicHelper();

			return newSong;
		}

		public static Song LoadSong(string songIdentity)
		{
			if (!FindSong(songIdentity, out SongAsset songAsset))
				return null;

			return LoadSong(songAsset);
		}

		public static bool FindSong(string songIdentity, out SongAsset songAsset, string packageName = null)
		{
			List<SongAsset> songs = (packageName != null) ? GetSongsFrom(packageName) : GetAllSongs();

			songAsset = songs.Find( SongIdentityMatches(songIdentity) );

			return songAsset != null;
		}

		public static List<string> GetPackageNames()
		{
			if (!_cached) SearchForMusicPackages();

			var names = new List<string>();
			foreach (MusicPackage musicPackage in _musicPackages)
			{
				if (names.Find(PackageNameMatches(musicPackage.PackageName)) != null) continue;
				names.Add(musicPackage.PackageName);
			}
			return names;
		}

		public static List<SongAsset> GetAllSongs()
		{
			if (!_cached) SearchForMusicPackages();

			var songs = new List<SongAsset>();
			foreach (MusicPackage musicPackage in _musicPackages)
			{
				foreach (SongAsset songAsset in musicPackage.SongAssets)
				{
					songs.Add(songAsset);
				}
			}
			return songs;
		}
		public static List<SongAsset> GetSongsFrom(string packageName)
		{
			if (!_cached) SearchForMusicPackages();

			System.Predicate<string> matches = PackageNameMatches(packageName);

			var songs = new List<SongAsset>();
			foreach (MusicPackage musicPackage in _musicPackages)
			{
				if (matches(musicPackage.PackageName)) continue;
				foreach (SongAsset songAsset in musicPackage.SongAssets)
				{
					songs.Add(songAsset);
				}
			}
			return songs;
		}

		public static void LoadMusicPackage(MusicPackage musicPackage)
		{
			if (_musicPackages.Contains(musicPackage)) return;
			_musicPackages.Add(musicPackage);
		}
		public static void LoadMusicPackage(string path)
		{
			var musicPackage = UnityEngine.Resources.Load(path) as MusicPackage;
			if (musicPackage == null)
			{
				UnityEngine.Debug.LogError(string.Format("Could not load MusicPackage \"{0}\".", path));
			}
			LoadMusicPackage(musicPackage);
		}

		public static void SearchForMusicPackages()
		{
			LoadMusicPackage("MusicPackages/MainSongs");
			_cached = true;
		}



		private static void CreateMusicHelper()
		{
			Debug.Log("Creating Music Helper");
			if (_musicHelper != null) return;
			GameObject newGameObject = new GameObject("MusicHelper");
			_musicHelper = newGameObject.AddComponent<MusicHelper>();
			Object.DontDestroyOnLoad(newGameObject);
		}
	}
}

