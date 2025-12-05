using UnityEngine;

public class PlacementState : IBuildingState
{
    private int selectedObjectIndex = -1;
    int ID;
    private Grid grid;
    private PreviewSystem previewSystem;
    private ObjectsDataMapSO dataMap;
    private GridData floorData;
    private GridData furnitureData;
    private ObjectPlacer objectPlacer;
    private int _currentRotation;
    private GridData _selectedData;
    /// <summary>
    /// Initialize the state and visualize the selected object.
    /// </summary>
    /// <param name="iD"></param>
    /// <param name="grid"></param>
    /// <param name="previewSystem"></param>
    /// <param name="dataMap"></param>
    /// <param name="floorData"></param>
    /// <param name="furnitureData"></param>
    /// <param name="objectPlacer"></param>
    /// <exception cref="System.Exception"></exception>
    public PlacementState(int iD,
                          Grid grid,
                          PreviewSystem previewSystem,
                          ObjectsDataMapSO dataMap,
                          GridData floorData,
                          GridData furnitureData,
                          ObjectPlacer objectPlacer,
                          int rotationDegrees)
    {
        ID = iD;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.dataMap = dataMap;
        this.floorData = floorData;
        this.furnitureData = furnitureData;
        this.objectPlacer = objectPlacer;
        _currentRotation = rotationDegrees;

        selectedObjectIndex = dataMap.objectsData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex > -1)
        {
            // Initialize the current size in case it didn't happen through rotation
            dataMap.objectsData[selectedObjectIndex].CurrentSize = dataMap.objectsData[selectedObjectIndex].Size;

           if (previewSystem != null) previewSystem.StartShowingPlacementPreview(dataMap.objectsData[selectedObjectIndex].Prefab,
                dataMap.objectsData[selectedObjectIndex].CurrentSize);
        }
        else
            throw new System.Exception($"No object with ID {iD}");
    }

    /// <summary>
    ///  Stop showing the selected object.
    /// </summary>
    public void EndState()
    {
        if (previewSystem != null) previewSystem.StopShowingPreview();
    }

    /// <summary>
    ///  Places the selected Object in the grid position and adds it to the grid data.
    /// </summary>
    /// <param name="gridPosition"></param>
    public GridData OnAction(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

        if (placementValidity == false) return null;

        if (objectPlacer != null)
        {
            objectPlacer.PlaceObject(dataMap.objectsData[selectedObjectIndex].Prefab, grid.CellToWorld(gridPosition), _currentRotation);

            // Add the appropriate object to the appropriate gridData
            _selectedData = dataMap.objectsData[selectedObjectIndex].ID == 0 ? floorData : furnitureData;
            _selectedData.AddObjectAt(gridPosition, dataMap.objectsData[selectedObjectIndex].CurrentSize, dataMap.objectsData[selectedObjectIndex].ID, _currentRotation);

            // The index of the object in the placed objects from object placer
            Vector3Int gameObjectGridPosition = _selectedData.GetOriginGridPosition(gridPosition);

            // Get the id of the selected object
            int _gameObjectId = _selectedData.GetRepresentationId(gridPosition);

            // If it's a shelf remove its data as well.
            // check if it's a shelf change later logic
            if (_gameObjectId == 3 || _gameObjectId == 2)
            {
                Shelf shelf = objectPlacer.GetPlacedObjectByPosition(grid.CellToWorld(gameObjectGridPosition)).GetComponent<Shelf>();
                shelf.Init(_selectedData.GetUniqueID(gridPosition), new ShelfData());
            }
        }

        if (previewSystem != null) previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), false);

        return _selectedData;
    }

    /// <summary>
    /// Updates the position of the selected object and the cell indicator
    /// </summary>
    /// <param name="gridPosition"></param>
    public void UpdateState(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);

        if (previewSystem != null) previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);
    }

    public void UpdateSize(int rotation)
    {
        _currentRotation = rotation;

        Vector2Int baseSize = dataMap.objectsData[selectedObjectIndex].Size;

        if (rotation % 180 == 0)
        {
            dataMap.objectsData[selectedObjectIndex].CurrentSize = baseSize;
        }
        else
        {
            dataMap.objectsData[selectedObjectIndex].CurrentSize = new Vector2Int(baseSize.y, baseSize.x);
        }
    }

    // Helper function to check the validity of the position we are trying to build in 
    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        GridData selectedData = dataMap.objectsData[selectedObjectIndex].ID == 0 ? floorData : furnitureData;

        return selectedData.CanPlaceObejctAt(gridPosition, dataMap.objectsData[selectedObjectIndex].CurrentSize, _currentRotation);
    }

    public void UpdateState(Vector3Int gridPosition, int rotation)
    {
        UpdateSize(rotation);
        UpdateState(gridPosition);
    }
}