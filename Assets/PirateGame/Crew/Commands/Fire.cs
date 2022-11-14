using PirateGame.Crew;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
			throw new System.NotImplementedException();
		}

		public override void OnExecute()
		{
			throw new System.NotImplementedException();
		}

		public override void Update()
		{
			// run command logic here
		}

		protected override void OnCancel()
		{
			throw new System.NotImplementedException();
		}
	}
}