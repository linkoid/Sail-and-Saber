using System.Collections.Generic;
using UnityEngine;

namespace MusicMaster
{
	[CreateAssetMenu(fileName = "SongList", menuName = "Music/MusicPackage", order = 0)]
	public class MusicPackage : ScriptableObject
	{
		public string PackageName;
		public List<SongAsset> SongAssets;
	}
}