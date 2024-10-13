using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Formations
{
    public class TriangleFormation : FormationPattern
    {
        private static readonly float rowOffset = 3.0f;
        private static readonly float columnOffset = 3.0f;  

        public TriangleFormation()
        {
            this.FreeAnchor = false;
        }

        public override Vector3 GetOrientation(FormationManager formation)
        {
            // Orientation is based on the first character's forward direction
            return formation.SlotAssignment.Keys.First().transform.forward;
        }

        public override Vector3 GetSlotLocation(FormationManager formation, int slotNumber)
        {
            Vector3 anchorPosition = formation.AnchorPosition;
            Vector3 orientation = this.GetOrientation(formation);

            if (slotNumber == 0)
            {
                return anchorPosition;
            }

            // Determine the row number based on the triangular pattern
            int row = Mathf.FloorToInt((Mathf.Sqrt(1 + 8 * slotNumber) - 1) / 2);
            int indexInRow = slotNumber - row * (row + 1) / 2;

            float horizontalOffset = (indexInRow - (row / 2.0f)) * columnOffset;

            // Calculate the position for the slot: moving back based on the row number and horizontally within the row
            Vector3 rowPosition = anchorPosition + orientation * row * rowOffset;
            Vector3 horizontalPosition = Vector3.Cross(orientation, Vector3.up) * horizontalOffset;

            return rowPosition + horizontalPosition; 
        }


        public override bool SupportSlot(int slotCount)
        {
            return slotCount <= 6;
        }
    }
}
