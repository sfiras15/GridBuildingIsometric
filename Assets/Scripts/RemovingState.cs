using UnityEngine;

public class RemovingState : IBuildingState
{
    Grid grid;
    PreviewSystem previewSystem;
    GridData floorData;
    GridData furnitureData;
    ObjectPlacer objectPlacer;
    private int _currentRotation;

    public RemovingState(Grid grid,
                         PreviewSystem previewSystem,
                         GridData floorData,
                         GridData furnitureData,
                         ObjectPlacer objectPlacer,
                         int rotationDegrees)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.floorData = floorData;
        this.furnitureData = furnitureData;
        this.objectPlacer = objectPlacer;
        _currentRotation = rotationDegrees;
        previewSystem.StartShowingRemovePreview();
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    public GridData OnAction(Vector3Int gridPosition)
    {
        
        GridData selectedData = null;

        // Select the appropriate object type
        if (furnitureData.CanPlaceObejctAt(gridPosition, Vector2Int.one, _currentRotation) == false)
        {
            selectedData = furnitureData;
        }
        else if (floorData.CanPlaceObejctAt(gridPosition, Vector2Int.one, _currentRotation) == false)
        {
            selectedData = floorData;
        }

        if (selectedData == null) return null;
        else
        {
            Debug.Log(selectedData.GetUniqueID(gridPosition));

            // Get the id of the selected object
            int _gameObjectId = selectedData.GetRepresentationId(gridPosition);

            Vector3Int originalPosition = selectedData.GetOriginGridPosition(gridPosition);

            // If it's a shelf remove its data as well.
            // check if it's a shelf change later logic
            if (_gameObjectId == 3 || _gameObjectId == 2)
            {
                ShelfManager.Instance.RemoveShelf(selectedData.GetUniqueID(gridPosition));
            }

            // Remove the objectData from the gridData and from the list of placed objects
            selectedData.RemoveObjectAt(gridPosition);
            objectPlacer.RemoveObjectAt(grid.CellToWorld(originalPosition));            
        }

        Vector3 cellPosition = grid.CellToWorld(gridPosition);
        if (previewSystem != null) previewSystem.UpdatePosition(cellPosition, CheckIfSelectionIsValid(gridPosition));

        return selectedData;
    }

    private bool CheckIfSelectionIsValid(Vector3Int gridPosition)
    {
        return !(furnitureData.CanPlaceObejctAt(gridPosition, Vector2Int.one, _currentRotation) &&
            floorData.CanPlaceObejctAt(gridPosition, Vector2Int.one, _currentRotation));
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool validity = CheckIfSelectionIsValid(gridPosition);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), validity);
    }

    public void UpdateState(Vector3Int gridPosition, int rotation)
    {
        _currentRotation = rotation;
        // we can't rotate when we are in the removing state
        return;
    }
}