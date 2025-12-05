using System.Collections.Generic;
using System;

[Serializable]
public class GameSaveData
{
    // Grids Data
    public List<GridDataSaveWrapper> grids = new List<GridDataSaveWrapper>();

    // Shelves Data
    public ShelfDataSave shelfDataSave;
}