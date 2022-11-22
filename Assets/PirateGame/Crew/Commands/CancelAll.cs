using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PirateGame.Crew;

namespace PirateGame.Crew.Commands
{
	[System.Serializable]
	public class CancelAll : Command
	{

		public override string DisplayName => throw new System.NotImplementedException();

		public override bool Poll()
		{
            // Can be raiding
            if (Commander.isRaiding == true)
                return true;
            if (Commander.isRepairing == true)
                return true;
			// else
			return false;
		}

		protected override IEnumerable OnExecute()
		{
            Commander.isRaiding = false;
            Commander.isRepairing = false;

            yield return null;
		}

		protected override void Update()
		{
			base.Update();
		}

		protected override void OnCancel()
		{
            Commander.isRaiding = false;
            Commander.isRepairing = false;
            throw new System.NotImplementedException();
		}
	}
}