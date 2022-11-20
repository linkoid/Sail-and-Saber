using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PirateGame.Crew;

namespace PirateGame.Crew.Commands
{
	[System.Serializable]
	public class Raid : Command
	{

		public override string DisplayName => throw new System.NotImplementedException();

		public override bool Poll()
		{
			// Cannot target nothing
			if (Commander.Target == null) return false;

			// Cannot target yourself
			if (Commander.Target == Commander.Player.Ship) return false;

			// Must be close enough
			Combat.Target = Commander.Target;
			if (!Combat.CheckCanRaid()) return false;

			// else
			return true;
		}

		protected override IEnumerable OnExecute()
		{
			// Save target in local variable
			// so even if target changes, we are still raiding the same ship.
			var target = Commander.Target;

			// Notify the target that it is being raided
			target.Raid();

			// Tell crew to conduct the raid
			Crew.Raid(target);

			// Loop to do stuff during the raid
			float loopDuration = 30; // how long does the raid last?
			float loopStep = 0.8f; // how often is the code in the loop run?
			for (float loopTime = 0; loopTime < loopDuration; loopTime += loopStep)
			{
				yield return new WaitForSeconds(loopStep);

				// TODO : Maybe do dice-rolls to decide which crewmate & enemy dies or something?
			}

			// Call crewmates back
			Crew.Board(Ship);

			// Obtain rewards from the ship
			target.Plunder(Commander.Player);

			// Give them time to walk back
			yield return new WaitForSeconds(3);

			// Sink the raided ship
			target.TakeDamage(Mathf.Infinity);
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