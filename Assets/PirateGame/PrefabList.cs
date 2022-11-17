using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PirateGame
{
    [CreateAssetMenu(fileName = "PrefabList", menuName = "Pirate Game/Prefab List")]
    public class PrefabList : ScriptableObject, IReadOnlyList<GameObject>
    {
        [SerializeField]
        private List<GameObject> m_Prefabs;



        public GameObject this[int index] => m_Prefabs[index];
        public int Count => m_Prefabs.Count;
        public IEnumerator<GameObject> GetEnumerator() => m_Prefabs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_Prefabs).GetEnumerator();
    }
}