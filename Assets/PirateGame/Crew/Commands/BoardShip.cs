using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Crew.Commands
{
	[System.Serializable]
	public class BoardShip : Command
    {
        public override string DisplayName => throw new System.NotImplementedException();


        public override bool Poll()
        {
            throw new System.NotImplementedException();
        }
		protected override IEnumerable OnExecute()
        {
            throw new System.NotImplementedException();
        }

		protected override void Update()
        {
            base.Update();
            // run command logic here
        }

        protected override void OnCancel()
        {
            throw new System.NotImplementedException();
        }
    }
}