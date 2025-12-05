using System;
using UnityEngine;

[Serializable]
public class GridEntry
{
    public Vector3Int gridPosition;            // The grid cell position.
    public PlacementData placementData;  // The associated placement data.
}

