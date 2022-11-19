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

		public override bool Poll()
		{
            if (Commander.Target != Commander.Player)
            {
                return true;
            }
            return false;
		}

		protected override void OnExecute()
		{
			throw new System.NotImplementedException();
		}

		protected override void Update()
		{
			base.Update();
		}

		protected override void OnCancel()
		{
			throw new System.NotImplementedException();
		}
	}
}