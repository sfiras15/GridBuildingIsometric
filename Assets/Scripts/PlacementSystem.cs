using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UIElements;

public class PlacementSystem : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PreviewSystem previewSystem;
    [SerializeField] private ObjectPlacer objectPlacer;
    //[SerializeField] private GameObject mouseIndicator;

    [Header("Grid References")]
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject gridVisualization;

    [Tooltip("The width of our current grid")]
    [SerializeField] private int gridWidth = 10;

    [Tooltip("The width of our current grid")]
    [SerializeField] private int gridLength = 10;

    // Position of the mouse in the grid
    private Vector3Int _gridPosition;
    private Vector3 _mousePosition;

    // Objects Map
    [SerializeField] private ObjectsDataMapSO objectsDataMapSO;

    private GridData floorData;
    private const string FLOOR_GRID_ID = "floorData";

    private GridData furnitureData;
    private const string FURNITURE_GRID_ID = "furnitureData";
    

    // Store the last selected position to avoid repeated calculations in update method
    private Vector3Int _lastDetectedPosition = Vector3Int.zero;

    private IBuildingState buildingState;

    // Keep track of the current rotation angle (in degrees)
    private int _currentRotation = 0;

    //Place holder

    public SaveManager saveManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StopPlacement();
        Dictionary<string, GridData> grids = saveManager.LoadGrids(gridWidth, gridLength);
        floorData = grids[FLOOR_GRID_ID];
        furnitureData = grids[FURNITURE_GRID_ID];

        // Instantiate structure prefabs following gridData loading
        LoadStructures();
    }

    private void LoadStructures()
    {
        Dictionary<Vector3Int, PlacementData> floorObjects = floorData.GetPlacedObjects();
        Dictionary<Vector3Int, PlacementData> furnitureObjects = furnitureData.GetPlacedObjects();

        // Use a HashSet to track which unique IDs have been processed
        HashSet<string> processedUniqueIDs = new HashSet<string>();

        // Process floor objects
        foreach (var kvp in floorObjects)
        {
            string uniqueID = kvp.Value.UniqueID;
            if (processedUniqueIDs.Contains(uniqueID))
                continue;

            processedUniqueIDs.Add(uniqueID);

            GameObject prefab = GetPrefabByID(kvp.Value.ID);
            if (prefab != null)
            {
                objectPlacer.PlaceObject(prefab, kvp.Value.occupiedPositions[0], kvp.Value.RotationDegrees);
            }
        }

        // Clear the set for the next grid if necessary
        processedUniqueIDs.Clear();

        // Process furniture objects
        foreach (var kvp in furnitureObjects)
        {
            string uniqueID = kvp.Value.UniqueID;
            if (processedUniqueIDs.Contains(uniqueID))
                continue;

            processedUniqueIDs.Add(uniqueID);

            GameObject prefab = GetPrefabByID(kvp.Value.ID);
            if (prefab != null)
            {
                GameObject obj = objectPlacer.PlaceObject(prefab, kvp.Value.occupiedPositions[0], kvp.Value.RotationDegrees);

                if (obj == null) return;

                // If the placedObject is a shelf, initialize its shelf data and rebuild the shelf
                if (obj.TryGetComponent(out Shelf shelf))
                {
                    if (ShelfManager.Instance != null) ShelfManager.Instance.RebuildShelf(shelf, ShelfManager.Instance.GetShelfDataByUniqueID(uniqueID), uniqueID);
                }
            }
        }
    }

    private GameObject GetPrefabByID(int id)
    {
        if (objectsDataMapSO == null)
            return null;

        var found = objectsDataMapSO.objectsData
            .Find(data => data.ID == id);

        return found?.Prefab;
    }

    public void StopPlacement()
    {
        gridVisualization.SetActive(false);

        if (buildingState != null) buildingState.EndState();

        inputManager.onMouseClick -= PlaceStructure;
        inputManager.onExitBuilding -= StopPlacement;
        inputManager.onMouseWheelScroll -= RotateStructure;

        _lastDetectedPosition = Vector3Int.zero;

        if (!(buildingState is EditState)) inputManager.IsInPlacementMode = false;

        //Resets the building state
        buildingState = null;
    }

    public void ResetPlacementMode()
    {
        UIEventBus.ActivateCurrentUI(userInterface.BUILDING);

        //We need to introduce a delay because input manager check for placement mode right after we reset it
        Invoke(nameof(ResetUI), 0.05f); 
    }

    private void ResetUI()
    {
        inputManager.IsInPlacementMode = false;
    }

    public void StartPlacement(int Id)
    {
        StopPlacement();
        inputManager.IsInPlacementMode = true;
        gridVisualization.SetActive(true);


        buildingState = new PlacementState(Id,
                                           grid,
                                           previewSystem,
                                           objectsDataMapSO,
                                           floorData,
                                           furnitureData,
                                           objectPlacer,
                                           0);

        inputManager.onMouseClick += PlaceStructure;
        inputManager.onExitBuilding += StopPlacement;
        inputManager.onMouseWheelScroll += RotateStructure;
    }

    private void PlaceStructure()
    {
        if (inputManager.IsPointerOverUI())
        {
            return;
        }
        _mousePosition = inputManager.GetSelectedMapPosition();
        _gridPosition = grid.WorldToCell(_mousePosition);

        if (buildingState != null) buildingState.OnAction(_gridPosition);

        UIEventBus.ActivateCurrentUI(userInterface.BUILDING);
    }

    // Called when a mouse wheel scroll event is received
    private void RotateStructure(float scrollValue)
    {
        // Rotate 90° at a time based on the scroll direction.
        if (scrollValue < 0)
            _currentRotation += 90;
        else if (scrollValue > 0)
            _currentRotation -= 90;

        // Normalize the rotation to be between 0 and 360 degrees.
        _currentRotation = (_currentRotation % 360 + 360) % 360;

        if (buildingState != null) buildingState.UpdateState(_gridPosition, _currentRotation);

        // Update the preview rotation.
        if (previewSystem != null) previewSystem.SetPreviewRotation(_currentRotation);
    }

    public void StartRemoving()
    {
        StopPlacement();
        inputManager.IsInPlacementMode = true;
        gridVisualization.SetActive(true);
        buildingState = new RemovingState(grid, previewSystem, floorData, furnitureData, objectPlacer, 0);

        inputManager.onMouseClick += PlaceStructure;
        inputManager.onExitBuilding += StopPlacement;
    }

    public void StartEditing()
    {
        StopPlacement();
        inputManager.IsInPlacementMode = true;
        gridVisualization.SetActive(true);
        buildingState = new EditState(this,grid, previewSystem, floorData, furnitureData, objectPlacer, objectsDataMapSO, 0);

        inputManager.onMouseClick += PlaceStructure;
        inputManager.onExitBuilding += StopPlacement;
        inputManager.onMouseWheelScroll += RotateStructure;

    }

    // Update is called once per frame
    void Update()
    {
        if (buildingState == null) return;

        _mousePosition = inputManager.GetSelectedMapPosition();
        _gridPosition = grid.WorldToCell(_mousePosition);

        if (_lastDetectedPosition != _gridPosition)
        {
            buildingState.UpdateState(_gridPosition);

            _lastDetectedPosition = _gridPosition;
        }
    }

    public void SaveGridData()
    {
        Dictionary<string, GridData> grids = new Dictionary<string, GridData>
        {
            { "floorData", floorData },
            { "furnitureData", furnitureData }
        };
        saveManager.SaveGrids(grids);
    }
}
