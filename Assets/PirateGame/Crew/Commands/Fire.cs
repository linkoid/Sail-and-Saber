using PirateGame.Crew;
using PirateGame.Ships;
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
			if (Ship == null) return false;

			// Can't be reloading
			if (Combat.BroadsideCannons.IsReloading) return false;
			if (Combat.DeckCannons.IsReloading) return false;

			// Can't have no cannons in range
			if (!Combat.HasCannonsInRange()) return false;

            //Cannot Fire if no crew
            if (Crew.Count < 1) return false;

            // else
            return true;
		}

		protected override IEnumerable OnExecute()
		{
            Commander.isRepairing = false;

            Combat.Target = Commander.Target;
			Combat.FireBroadsideCannons();

			var cannons = Combat.GetDeckCannonsInRange();
			Crew.ManCannons(cannons);

			// Wait a bit to let them get to the cannons
			yield return new WaitForSeconds(1);

			Combat.FireDeckCannons();
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