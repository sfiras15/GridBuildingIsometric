using System;

[Serializable]
public class GridDataSaveWrapper
{
    public string gridID;       // e.g., "floorData" or "furnitureData"
    public GridDataSave gridData;
}