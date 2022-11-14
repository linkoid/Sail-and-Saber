using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Crew
{
    public class Anchor : Command
    {
        public Anchor(Commander commander) :
            base(commander)
        {

        }

        public override string DisplayName => throw new System.NotImplementedException();

        public override void OnExecute()
        {
            throw new System.NotImplementedException();
        }

        public override bool Poll()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnCancel()
        {
            throw new System.NotImplementedException();
        }
    }
}