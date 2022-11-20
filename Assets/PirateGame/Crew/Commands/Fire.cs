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
		// Shortcuts because I was tired of typing this out so much
		protected Ships.Ship Ship => Commander.Crew.Ship;
		protected Ships.ShipCombat Combat => Ship.Internal.Combat;

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

			// else
			return true;
		}

		protected override IEnumerable OnExecute()
		{
			var crew = Commander.Crew;
			var ship = Commander.Crew.Ship;

			ship.Internal.Combat.Target = Commander.Target;
			ship.Internal.Combat.FireBroadsideCannons();

			var cannons = ship.Internal.Combat.GetDeckCannonsInRange();
			crew.ManCannons(cannons);

			// Wait a bit to let them get to the cannons
			yield return new WaitForSeconds(1);

			ship.Internal.Combat.FireDeckCannons();
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