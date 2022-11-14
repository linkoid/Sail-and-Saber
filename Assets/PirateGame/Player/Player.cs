using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using PirateGame.Crew;

namespace PirateGame
{
    public class Player : MonoBehaviour
    {
        public int Health;
        public int MaxHealth;
        public long Gold { get => Stats.Gold; set => Stats.Gold = value; }
        public long CrewCount { get => Stats.CrewCount; set => Stats.CrewCount = value; }




		public PlayerStats Stats;
        public Ships.Ship Ship;
        public Ships.Ship Target;

        public Commander Commander { get => _commander; private set => _commander = value; }
        [SerializeField] private Commander _commander;
        

        public int SpeedMod = 0;
        // Start is called before the first frame update
        void Start()
        {
            Health = MaxHealth;

            // Create Commander Here?
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    public struct PlayerStats
    {
        public long Gold;
        public long CrewCount;
    }
}