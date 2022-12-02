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

        //Current State of the Crew
        public bool isRaiding { get => _isRaiding; set => _isRaiding = value; }
        [SerializeField] bool _isRaiding;
        public bool isRepairing { get => _isRepairing; set => _isRepairing = value; }
        [SerializeField] bool _isRepairing;
        public bool isFiring { get => _isFiring; set => _isFiring = value; }
        [SerializeField] bool _isFiring;

        [SerializeField] List<Command> m_CommandPrefabs = new List<Command>();
		[SerializeField, ReadOnly] List<Command> m_Commands = new List<Command>();

		private GameObject CommandBG;
        private RectTransform rt;


		void Start()
		{
			FindPlayer();

			CommandBG = transform.GetChild(0).gameObject;
            rt = CommandBG.GetComponent<RectTransform>();

            for (int i = 0; i < m_CommandPrefabs.Count; i++)
			{
				var newCommand = Object.Instantiate(m_CommandPrefabs[i], CommandBG.transform);
				m_Commands.Add(newCommand);
			}

            //Set the scale of the command background based on how many commands are in the menu
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 45 * m_CommandPrefabs.Count + 80);          
		}

        void Update()
        {
            if(CommandBG.transform.childCount == 0)
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 125);
            else
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50 * CommandBG.transform.childCount + 80);
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
