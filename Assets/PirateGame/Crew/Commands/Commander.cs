using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Crew
{
	public class Commander : MonoBehaviour
	{
		public Player Player { get => _player; private set => _player = value; }
		[SerializeField] Player _player;

		public CrewDirector Crew { get => _crew; private set => _crew = value; }
		[SerializeField] CrewDirector _crew;

		private List<Command> m_Commands = new List<Command>();
	}
}
