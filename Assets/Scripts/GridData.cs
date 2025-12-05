using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GridData
{
    private Dictionary<Vector3Int, PlacementData> placedObjects = new();
    private int gridWidth;
    private int gridLength;

    public GridData(int width, int length)
    {
        gridWidth = width;
        gridLength = length;
    }

    public void AddObjectAt(Vector3Int gridPosition,
                            Vector2Int objectSize,
                            int ID,
                            int rotationDegrees)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize, rotationDegrees);
        PlacementData data = new PlacementData(positionToOccupy, ID, rotationDegrees);
        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
                throw new Exception($"Dictionary already contains this cell position {pos}");
            placedObjects[pos] = data;
        }
    }

    private bool IsCellValid(Vector3Int cell)
    {
        // Example gridWidth = 10 gridLength = 10
        int halfWidth = gridWidth / 2;
        int halfLength = gridLength / 2;

        // Check if X is in [-5..4]
        if (cell.x < -halfWidth || cell.x >= halfWidth)
            return false;

        // Check if Z is in [-5..4]
        if (cell.z < -halfLength || cell.z >= halfLength)
            return false;

        return true;
    }

    private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int objectSize, int rotationDegrees)
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        // For square objects, rotation doesn't change the footprint.
        if (objectSize.x == objectSize.y)
        {
            for (int x = 0; x < objectSize.x; x++)
            {
                for (int y = 0; y < objectSize.y; y++)
                {
                    Vector3Int cell = gridPosition + new Vector3Int(x, 0, y);

                    if (!IsCellValid(cell))
                    {
                        // If any cell is invalid, return an empty list.
                        return new List<Vector3Int>();
                    }

                    positions.Add(cell);
                }
            }
        }
        else
        {
            // Normalize rotation to 0, 90, 180, or 270.
            rotationDegrees = ((rotationDegrees % 360) + 360) % 360;

            for (int x = 0; x < objectSize.x; x++)
            {
                for (int y = 0; y < objectSize.y; y++)
                {
                    Vector3Int cell = Vector3Int.zero;

                    if (rotationDegrees == 0 || rotationDegrees == 270)
                    {
                        // 0° and 270° (which is equivalent to -90° normalized)
                        cell = gridPosition + new Vector3Int(x, 0, y);
                    }
                    else if (rotationDegrees == 180)
                    {
                        // 180° rotation: flip the x-offset.
                        cell = gridPosition + new Vector3Int(-x, 0, y);
                    }
                    else if (rotationDegrees == 90)
                    {
                        // 90° rotation: flip the y-offset.
                        cell = gridPosition + new Vector3Int(x, 0, -y);
                    }

                    if (!IsCellValid(cell))
                    {
                        // If any cell is invalid, return an empty list.
                        return new List<Vector3Int>();
                    }

                    positions.Add(cell);
                }
            }
        }
        return positions;
    }

    public bool CanPlaceObejctAt(Vector3Int gridPosition, Vector2Int objectSize, int rotationDegrees)
    {
        List<Vector3Int> positionsToOccupy = CalculatePositions(gridPosition, objectSize, rotationDegrees);

        if (positionsToOccupy.Count == 0)
            return false;

        foreach (var pos in positionsToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
                return false;
        }
        return true;
    }

    internal string GetUniqueID(Vector3Int gridPosition)
    {
        if (placedObjects.ContainsKey(gridPosition) == false)
            return null;
        return placedObjects[gridPosition].UniqueID;
    }

    internal int GetRepresentationId(Vector3Int gridPosition)
    {
        if (placedObjects.ContainsKey(gridPosition) == false)
            return -1;
        return placedObjects[gridPosition].ID;
    }

    internal Vector3Int GetOriginGridPosition(Vector3Int gridPosition)
    {
        return placedObjects[gridPosition].occupiedPositions[0];
    }

    internal int GetOriginalRotation(Vector3Int gridPosition)
    {
        if (placedObjects.ContainsKey(gridPosition) == false)
            return -1;
        return placedObjects[gridPosition].RotationDegrees;
    }

    internal void RemoveObjectAt(Vector3Int gridPosition)
    {
        foreach (var pos in placedObjects[gridPosition].occupiedPositions)
        {
            placedObjects.Remove(pos);
        }
    }

    public Dictionary<Vector3Int, PlacementData> GetPlacedObjects()
    {
        return placedObjects;
    }

    public void LogAllData()
    {
        Debug.Log("=== Logging All GridData ===");

        foreach (var kvp in placedObjects)
        {
            Vector3Int cell = kvp.Key;
            PlacementData data = kvp.Value;

            Debug.Log($"Cell: {cell} | ID: {data.ID}");

            // Convert occupied positions list to a comma-separated string for easy logging
            string occupiedPositionsText = string.Join(", ", data.occupiedPositions);
            Debug.Log($"Occupied Positions: [{occupiedPositionsText}]");
        }

        Debug.Log("=== End of Log ===");
    }

    /// <summary>
    /// Helper method to set a placement entry (used during loading).
    /// </summary>
    public void SetPlacement(Vector3Int key, PlacementData data)
    {
        placedObjects[key] = data;
    }

    public GridDataSave ToGridDataSave()
    {
        GridDataSave save = new GridDataSave();
        save.gridWidth = gridWidth;
        save.gridLength = gridLength;
        save.entries = new List<GridEntry>();

        foreach (var kvp in placedObjects)
        {
            GridEntry entry = new GridEntry();
            entry.gridPosition = kvp.Key;
            entry.placementData = kvp.Value;
            save.entries.Add(entry);
        }
        return save;
    }

    public static GridData LoadFromGridDataSave(GridDataSave save)
    {
        GridData gridData = new GridData(save.gridWidth, save.gridLength);
        foreach (var entry in save.entries)
        {
            gridData.placedObjects.Add(entry.gridPosition, entry.placementData);
        }
        return gridData;
    }

    /// <summary>
    /// Logs all the data of this grid, including the width, length, and each placement’s fields.
    /// </summary>
    public void DebugLogGridContents()
    {
        Debug.Log($"=== Grid Data (Width: {gridWidth}, Length: {gridLength}, Total Placements: {placedObjects.Count}) ===");

        if (placedObjects.Count == 0)
        {
            Debug.Log("No objects placed on this grid yet.");
            return;
        }

        foreach (var kvp in placedObjects)
        {
            Vector3Int cell = kvp.Key;
            PlacementData data = kvp.Value;

            // Basic info about the cell
            Debug.Log($"Cell: {cell} => ID: {data.ID}, Rotation: {data.RotationDegrees}, UniqueID: {data.UniqueID}");

            // Occupied positions in a comma-separated string
            string occupiedPositionsText = string.Join(", ", data.occupiedPositions);
            Debug.Log($"   Occupied Positions: [{occupiedPositionsText}]");
        }

        Debug.Log("=== End of Grid Data ===");
    }
}

[Serializable]
public class PlacementData
{
    public List<Vector3Int> occupiedPositions;

    [SerializeField] private int id;
    [SerializeField] private int rotationDegrees;
    [SerializeField] private string uniqueID;

    // Public getters to access the data
    public int ID { get { return id; } }
    public int RotationDegrees { get { return rotationDegrees; } }
    public string UniqueID { get { return uniqueID; } }

    public PlacementData(List<Vector3Int> occupiedPositions, int iD, int rotationDegrees)
    {
        this.occupiedPositions = occupiedPositions;
        id = iD;
        this.rotationDegrees = rotationDegrees;

        // Build a unique ID using the object's ID, placed object index, and grid position in x and z.
        if (occupiedPositions != null && occupiedPositions.Count > 0)
        {
            Vector3Int pos = occupiedPositions[0];
            StringBuilder sb = new StringBuilder();
            sb.Append(iD);
            sb.Append("_");
            sb.Append(pos.x);
            sb.Append("_");
            sb.Append(pos.z);
            uniqueID = sb.ToString();
        }
        else
        {
            uniqueID = iD + "_";
        }
    }
}