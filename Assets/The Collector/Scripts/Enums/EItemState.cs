using UnityEngine;

namespace FoxX.TheCollector
{
    public enum EItemState
    {
        Active = 0,
        MovingToSpot = 1,
        OnSpot = 2,
        Merging = 3,
        Merged = 4
    }
}