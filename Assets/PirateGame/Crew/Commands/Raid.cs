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
		[SerializeField] private Ships.Ship m_RaidTarget;

		public override string DisplayName => throw new System.NotImplementedException();

		public override bool Poll()
		{
			// Cannot raid nothing
			if (Commander.Target == null) return false;

			// Cannot raid yourself
			if (Commander.Target == Commander.Player.Ship) return false;

			// Must be close enough
			Combat.Target = Commander.Target;
			if (!Combat.CheckCanRaid()) return false;

			// else
			return true;
		}

        protected override IEnumerable OnExecute()
        {
            Commander.isRaiding = true;
            Commander.isRepairing = false;

            // Save target in local variable
            // so even if Commander's target changes, we are still raiding the same ship.
            m_RaidTarget = Commander.Target;

            // Notify the target that it is being raided
            m_RaidTarget.Raid();

            if (!(m_RaidTarget.Crew?.Count == 0))
            {
                // Tell crew to conduct the raid
                Crew.Raid(m_RaidTarget);

                // Loop to do stuff during the raid
                float loopDuration = 11; // how long does the raid last?
                float loopStep = 1f; // how often is the code in the loop run?
                for (float loopTime = 0; loopTime < loopDuration; loopTime += loopStep)
                {
                    if (m_RaidTarget.Crew.CrewRaid.Count == 0 || Crew.CrewRaid.Count == 0)
                        break;
                    yield return new WaitForSeconds(loopStep);
                    // TODO : Maybe do dice-rolls to decide which crewmate & enemy dies or something?

                    //Do Damage to Player Crew
                    Crew.Attack(1);

                    //Do Damage to Enemy Crew
                    m_RaidTarget.Crew.Attack(2);
                }
                // Call crewmates back
                Crew.Board(Ship);
            }

            //If all the enemy crewmates die
            if(m_RaidTarget.Crew.Count == 0)
            {
				// Obtain rewards from the ship
				m_RaidTarget.Plunder(Commander.Player);

				// Sink the raided ship
				m_RaidTarget.Sink();
            }
            else if(Crew.Count != 0)
            {
                m_RaidTarget.RaidCancel();
                foreach(var result in OnExecute())
                {
                    yield return result;
                }
            }
            else
            {
                m_RaidTarget.RaidCancel();
            }

            // Give them time to walk back
            yield return new WaitForSeconds(3);

            Commander.isRaiding = false;
        }

		protected override void Update()
		{
			base.Update();
		}

		protected override void OnCancel()
		{
			// Stop running OnExecute
			StopCoroutine(this.ActiveExecution);
            Commander.isRaiding = false;
			m_RaidTarget.RaidCancel();
		}
	}
}