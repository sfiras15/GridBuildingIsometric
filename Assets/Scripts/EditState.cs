using System;
using UnityEngine;

public class EditState : IBuildingState
{
    PlacementSystem placementSystem;
    Grid grid;
    PreviewSystem previewSystem;
    GridData floorData;
    GridData furnitureData;
    ObjectPlacer objectPlacer;
    private ObjectsDataMapSO dataMap;

    private IBuildingState _currentBuildingState;
    private int _gameObjectId = -1;
    private bool _isRemoving;
    private int _currentRotation;
    private GridData _selectedData;
    private int _previousRotation;
    private Vector3Int _previousGridPosition;
    private Vector2Int _previousSize;
    // the index of the object in the dataMap
    private int selectedObjectIndex = -1;

    private Shelf _shelf;
    private ShelfData _shelfData;


    public EditState(PlacementSystem placementSystem,
                         Grid grid,
                         PreviewSystem previewSystem,
                         GridData floorData,
                         GridData furnitureData,
                         ObjectPlacer objectPlacer,
                         ObjectsDataMapSO dataMap,
                         int rotationDegrees)
    {
        this.placementSystem = placementSystem;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.floorData = floorData;
        this.furnitureData = furnitureData;
        this.objectPlacer = objectPlacer;
        this.dataMap = dataMap;
        _currentRotation = rotationDegrees;

        _isRemoving = true;
        _currentBuildingState = new RemovingState(grid, previewSystem, floorData, furnitureData, objectPlacer, rotationDegrees);
    }

    public void EndState()
    {
        // if the object has not been removed place it back
        // else move it to the new position
        if (!_isRemoving && selectedObjectIndex > -1)
        {
            bool placementValidity = CheckPlacementValidity(_previousGridPosition, selectedObjectIndex);

            if (placementValidity == false) return;

            if (objectPlacer != null)
            {
                objectPlacer.PlaceObject(dataMap.objectsData[selectedObjectIndex].Prefab, grid.CellToWorld(_previousGridPosition), _previousRotation);

                // Add the appropriate object to the appropriate gridData
                if (_selectedData == null) _selectedData = dataMap.objectsData[selectedObjectIndex].ID == 0 ? floorData : furnitureData; 

                _selectedData.AddObjectAt(_previousGridPosition, _previousSize, dataMap.objectsData[selectedObjectIndex].ID, _previousRotation);

                if (_gameObjectId == 3 || _gameObjectId == 2)// check if it's a shelf change later logic
                {
                    RebuildExistingShelf(grid.CellToWorld(_previousGridPosition));
                }
            }
        }
        if (previewSystem != null) previewSystem.StopShowingPreview();

        placementSystem.ResetPlacementMode();
    }

    private void RebuildExistingShelf(Vector3 position)
    {
        _shelf = objectPlacer.GetPlacedObjectByPosition(position).GetComponent<Shelf>();
        ShelfManager.Instance.RebuildShelf(_shelf, _shelfData, _shelf.GetShelfID());
    }

    public GridData OnAction(Vector3Int gridPosition)
    {
        if (_isRemoving)// Remove mode
        {
            // Select the appropriate object type
            if (furnitureData.CanPlaceObejctAt(gridPosition, Vector2Int.one, _currentRotation) == false)
            {
                _selectedData = furnitureData;
            }
            else if (floorData.CanPlaceObejctAt(gridPosition, Vector2Int.one, _currentRotation) == false)
            {
                _selectedData = floorData;
            }

            if (_selectedData == null) return null;
            else
            {
                // Get the id of the selected object
                _gameObjectId = _selectedData.GetRepresentationId(gridPosition);

                // Get the original position in case we need it for adding the object back when we cancel the edit mode
                _previousGridPosition = _selectedData.GetOriginGridPosition(gridPosition);

                // Get the original rotation as well
                _previousRotation = _selectedData.GetOriginalRotation(gridPosition);

                // Check if the selected data is a shelf, if it is store it's shelf data so that it can be rebuilt later
                if (_gameObjectId == 3 || _gameObjectId == 2)
                {
                    _shelf = objectPlacer.GetPlacedObjectByPosition(grid.CellToWorld(_previousGridPosition)).GetComponent<Shelf>();
                    _shelfData = _shelf.GetShelfData();
                }

                _selectedData = _currentBuildingState.OnAction(gridPosition);
            }

            _isRemoving = false;

            // Use the other state
            _currentBuildingState = null;
            _currentBuildingState = new PlacementState(_gameObjectId,
                                           grid,
                                           previewSystem,
                                           dataMap,
                                           floorData,
                                           furnitureData,
                                           objectPlacer,
                                           0);

            // Activate the preview of the item 
            selectedObjectIndex = dataMap.objectsData.FindIndex(data => data.ID == _gameObjectId);

            // Store previous values in case we cancel the edit mode
            _previousSize = dataMap.objectsData[selectedObjectIndex].CurrentSize;
        }
        else // Placement mode
        {         
            _selectedData = _currentBuildingState.OnAction(gridPosition);

            if (_selectedData != null)
            {
                if (_gameObjectId == 3 || _gameObjectId == 2)// check if it's a shelf change later logic
                {
                    Vector3 placedObjectPosition = _selectedData.GetOriginGridPosition(gridPosition);
                    RebuildExistingShelf(placedObjectPosition);
                }
                _isRemoving = true;
                placementSystem.StopPlacement();
            }
        }

        return _selectedData;
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        GridData selectedData = dataMap.objectsData[selectedObjectIndex].ID == 0 ? floorData : furnitureData;

        return selectedData.CanPlaceObejctAt(gridPosition, dataMap.objectsData[selectedObjectIndex].CurrentSize, _currentRotation);
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        if (_currentBuildingState != null) _currentBuildingState.UpdateState(gridPosition);
    }

    public void UpdateState(Vector3Int gridPosition, int rotation)
    {
        if (_isRemoving) return;

        if (_currentBuildingState != null) _currentBuildingState.UpdateState(gridPosition, rotation);
    }
}
