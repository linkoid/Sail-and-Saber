using PirateGame.Crew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PirateGame.Crew.Commands
{
	[System.Serializable]
	public class Fire : Command
	{
		public override string DisplayName => throw new System.NotImplementedException();
		public Fire(Commander commander) :
			base(commander)
		{
            
        }

		public override bool Poll()
		{
			//throw new System.NotImplementedException();
            if(Commander.Target != Commander.Player)
            {
                return true;
            }
            return false;
		}

		public override void OnExecute()
		{
			throw new System.NotImplementedException();
		}

		public override void Update()
		{
			if(Poll())
            {
                ButtonObject.interactable = false;
            }
		}

		protected override void OnCancel()
		{
			throw new System.NotImplementedException();
		}
	}
}