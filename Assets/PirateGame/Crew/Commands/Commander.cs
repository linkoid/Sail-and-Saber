using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using PirateGame.UI;

namespace PirateGame.Crew
{
	public class Commander : MonoBehaviour
	{
		public Player Player { get => _player; private set => _player = value; }
		[SerializeField] Player _player;

		public CrewDirector Crew => Player.Crew;
		public Ships.Ship Target => Player.Target;

		[SerializeField] List<Command> m_CommandPrefabs = new List<Command>();
		[SerializeField, ReadOnly] List<Command> m_Commands = new List<Command>();

		private GameObject CommandBG;


		void Start()
		{
			FindPlayer();

			CommandBG = transform.GetChild(0).gameObject;
			for (int i = 0; i < m_CommandPrefabs.Count; i++)
			{
				var newCommand = Object.Instantiate(m_CommandPrefabs[i], CommandBG.transform);
				m_Commands.Add(newCommand);
			}
		}

		/// <summary>
		/// Automatically try to find the player if someone forgot to assign one manually.
		/// Only works if the commander script is a child of the HUD.
		/// </summary>
		private void FindPlayer()
		{
			var hud = this.GetComponentInParent<HUD>();
			if (hud == null) return;

			if (Player == null)
			{
				Player = hud.Player;
			}
		}
	}
}
