using PirateGame.Crew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Crew.Commands
{
	[System.Serializable]
	public class RepairShip : Command
	{
		public override string DisplayName => throw new System.NotImplementedException();

		public override bool Poll()
		{
            // Cannot target nothing
            //if (Commander.Target == null) return false;

            // Must have a ship
            if (Ship == null) return false;

            // Can target yourself
            if (Commander.Target == Commander.Player.Ship) return true;

            // else
            return true;
        }

		protected override IEnumerable OnExecute()
        {
            //Set the state of the crew
            Commander.isRaiding = false;
            Commander.isRepairing = true;

            // Save target in local variable
            // so even if target changes, we are still repairing the same ship.
            var target = Commander.Target;
            
            Crew.Support();

            yield return new WaitForSeconds(3);

            // Loop to do stuff during the repair
            float loopStep = 0.5f; // how often is the code in the loop run?


            //The ship will be repaired at a maximum of 6 HP/sec
            int repairAmount = 1 * Crew.Count;
            if (repairAmount >= 4)
                repairAmount = 4;

            while(Ship.Health < Ship.MaxHealth && Commander.isRepairing)
            {
                Ship.Repair(repairAmount);
                yield return new WaitForSeconds(loopStep);
            }

            Crew.Board(Ship);
        }

        protected override void Update()
        {
            base.Update();
            // run command logic here
        }

        protected override void OnCancel()
        {
            Commander.isRepairing = false;
            throw new System.NotImplementedException();
        }
    }
}