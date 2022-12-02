using System.Collections.Generic;
using UnityEngine;

namespace MusicMaster
{
    public class MusicHelper : MonoBehaviour
    {
        [SerializeField] private List<SongAsset> _songAssets = new List<SongAsset>();

		[SerializeField, ReadOnly] private float _globalVolume;

		private void Start()
        {
            Object.DontDestroyOnLoad(this.gameObject);
            MusicController.Start();
        }

        private void Update()
        {
            MusicController.Update();
            _globalVolume = MusicController.GlobalVolume;
		}
    }
}
