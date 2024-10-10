using Assets.Scripts.Game;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    class Shout : IAction
    {
        private NPC sourceOrc; // The Orc that shouts
        private List<NPC> nearbyOrcs; // The other Orcs that hear the shout
        private Vector3 shoutPosition; // The position from where the shout originates

        public Shout(NPC orc, List<NPC> orcList, Vector3 position)
        {
            this.sourceOrc = orc;
            this.nearbyOrcs = orcList;
            this.shoutPosition = position;
        }

        public void Execute()
        {
            // The source Orc makes the shout
            Debug.Log(sourceOrc.name + " shouted from " + shoutPosition);

            // All nearby Orcs move toward the shout position
            foreach (var orc in nearbyOrcs)
            {
                if (orc != sourceOrc)
                {
                    Debug.Log(orc.name + " is moving toward the shout at " + shoutPosition);
                    orc.StartPathfinding(shoutPosition); // Move to the shout position
                }
            }
        }
    }
}
