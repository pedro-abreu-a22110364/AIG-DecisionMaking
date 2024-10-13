using UnityEditor;
using UnityEngine;
using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.Formations
{
    public class FormationManager
    {
        public Dictionary<Monster, int> SlotAssignment;

        public List<Monster> Monsters;

        public FormationPattern Pattern;

        // # A Static (i.e., position and orientation) representing the
        // # drift offset for the currently filled slots.

        public Vector3 AnchorPosition;

        public Vector3 Orientation;


        public FormationManager(List<Monster> Monsters, FormationPattern pattern, Vector3 position, Vector3 orientation)
        {
            this.SlotAssignment = new Dictionary<Monster, int>();
            this.Monsters = Monsters;
            this.Pattern = pattern;
            this.AnchorPosition = position;
            this.Orientation = orientation;

            int i = 0;
            foreach (Monster npc in Monsters)
            {
                npc.usingFormation = true;
                npc.FormationManager = this;
                SlotAssignment[npc] = i;

                // Set the leader as the first NPC
                if (SlotAssignment[npc] == 0)
                {
                    npc.formationLeader = true;
                }
                else
                {
                    npc.formationLeader = false;
                }
                i++;
            }
        }


        public void UpdateSlotAssignements()
        {
            int i = 0; 
            foreach(var npc in SlotAssignment.Keys)
            {
                SlotAssignment[npc] = i;
                i++;
            }
        }

        public bool AddCharacter(Monster character)
        {
            var occupiedSlots = this.SlotAssignment.Count;
            if (this.Pattern.SupportSlot(occupiedSlots + 1))
            {
                SlotAssignment.Add(character, occupiedSlots);
                this.UpdateSlotAssignements();
                return true;
            }
            else return false;
        }

        public void RemoveCharacter(Monster character)
        {
            if (!SlotAssignment.ContainsKey(character)) return;
            var slot = SlotAssignment[character];
            SlotAssignment.Remove(character);
            character.usingFormation = false;
            character.formationLeader = false;
            character.FormationManager = null;
            UpdateSlotAssignements();
        }

        public void BreakFormation()
        {
            foreach (var npc in SlotAssignment.Keys) {
                npc.usingFormation = false;
                npc.formationLeader = false;
                npc.FormationManager = null;
            }

            SlotAssignment.Clear();
        }

        public void UpdateSlots()
        {
            // Use the first NPC in the SlotAssignment for anchor and orientation
            var anchor = Pattern.FreeAnchor ? AnchorPosition : SlotAssignment.FirstOrDefault(pair => pair.Value == 0).Key.transform.position;
            AnchorPosition = anchor;
            Orientation = Pattern.FreeAnchor ? AnchorPosition : SlotAssignment.FirstOrDefault(pair => pair.Value == 0).Key.transform.forward;

            // Update positions for all NPCs
            foreach (var npc in SlotAssignment.Keys)
            {
                if (SlotAssignment[npc] > 0 || Pattern.FreeAnchor)
                {
                    int slotNumber = SlotAssignment[npc];
                    // Get the location of the current slot based on the formation pattern
                    var slotLocation = Pattern.GetSlotLocation(this, slotNumber);

                    // Move NPC to the correct slot position using pathfinding
                    npc.StartPathfinding(slotLocation);
                    npc.GetComponent<NavMeshAgent>().updateRotation = true;
                }
            }
        }
    }
}