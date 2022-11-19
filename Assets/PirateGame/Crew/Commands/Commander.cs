using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace PirateGame.Crew
{
	public class Commander : MonoBehaviour
	{
		public Player Player { get => _player; private set => _player = value; }
		[SerializeField] Player _player;

		public CrewDirector Crew { get => _crew; private set => _crew = value; }
		[SerializeField] CrewDirector _crew;

        public GameObject Target { get => _Target; private set => _Target = value; }
        [SerializeField] GameObject _Target;

        [SerializeField] List<Command> m_Command = new List<Command>();

        private GameObject CommandBG;


        void Start()
        {
            CommandBG = transform.GetChild(0).gameObject;
            for (int i = 0; i < m_Command.Count; i++)
            {
                Button.Instantiate(m_Command[i], CommandBG.transform);
            }
        }
    }
}
