using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MusicMaster
{
    [CreateAssetMenu(fileName = "SongAsset", menuName = "Music/SongAsset")]
    public class SongAsset : ScriptableObject, ISong
    {
        public string Name          => _name;
        public string Identity      => _identity;
        public int    IDNumber      => this.GetInstanceID();
        public int    LoopbackPoint => _loopbackPoint;
        public Object Prefab        => _prefab;
        
        
        [SerializeField] private string _name = "Song";
        [SerializeField] private string _identity = "song";
        [SerializeField] private int    _loopbackPoint = 0;
        [SerializeField] private Object _prefab;
    }
}

