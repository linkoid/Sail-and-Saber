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
			// Cannot target nothing
			if (Commander.Target == null) return false;

			// Cannot target yourself
			if (Commander.Target == Commander.Player.Ship) return false;

			// Must have a ship
			if (Commander.Player.Ship == null) return false;

			// else
			return true;
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