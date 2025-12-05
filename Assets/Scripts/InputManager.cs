using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField] private LayerMask placementLayerMask;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask shelfLayerMask;

    [Tooltip("The maximum distance we can raycast from our camera to the grid")]
    [SerializeField] private float maxRayCastDistance = 100f;

    private Camera _mainCamera;
    private Vector3 _lastPosition;

    [Header("Structure Rotation Settings")]
    [Tooltip("The minimum Time between each scrollWheel Detection")]
    [SerializeField] private float scrollWheelCooldown = 0.25f;
    private float _mouseScrollDelta;
    private float _currentScrollCooldown = 0f;

    public event Action<float> onMouseWheelScroll;
    public event Action onMouseClick, onExitBuilding;

    private Shelf _latestSelectedShelf;
    public bool IsInPlacementMode { get; set; }

    private Transform _lastCameraTarget;

    // This is the currently-selected structure.
    // We'll reset its layer if a new structure is selected or if we go to overview.
    private Structure _structure;

    [Tooltip("The ground Transform")]
    [SerializeField] private Transform groundCameraTarget;

    [Header("Camera Panning Settings")]
    [Tooltip("The object the panning camera will follow when panning")]
    [SerializeField] private GameObject panningCameraTarget; // just an empty gameObject
    [SerializeField] private float panSpeed = 1.2f;
    [SerializeField] private Vector3 planeNormal = Vector3.up;  // For a top-down camera, Y is up
    [SerializeField] private float planeHeight = 0f; // If the plane is at y=0
    private Vector3 _dragStartWorldPos;

    void Start()
    {
        _mainCamera = Camera.main;
    }

    public Shelf GetSelectedShelf()
    {
        return _latestSelectedShelf;
    }

    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = _mainCamera.nearClipPlane;
        Ray ray = _mainCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayCastDistance, placementLayerMask))
        {
            _lastPosition = hit.point;
        }
        return _lastPosition;
    }

    private void AdjustUI()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayCastDistance, groundLayerMask))
        {
            // If the grid UI (buildingUI) is not active, activate it and hide shelfUI
            if (!UIEventBus.isBuildingUIActive)
            {
                // Reset the shelf reference
                _latestSelectedShelf = null;
                UIEventBus.ActivateCurrentUI(userInterface.BUILDING);
            }
        }

        // If we hit a shelf layer, switch to shelf UI
        if (Physics.Raycast(ray, out hit, maxRayCastDistance, shelfLayerMask))
        {
            // Initialize the shelf reference
            _latestSelectedShelf = hit.collider.gameObject.GetComponentInParent<Shelf>();
            UIEventBus.ActivateCurrentUI(userInterface.SHELVES, _latestSelectedShelf.shelfProducts);
        }
    }

    /// <summary>
    /// Checks what we hit with the mouse click, and adjusts camera/structure selection.
    /// </summary>
    private void AdjustView()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayCastDistance))
        {
            Structure clickedStructure = hit.collider.gameObject.GetComponentInParent<Structure>();

            if (clickedStructure != null)
            {
                if (clickedStructure != _structure)
                {
                    // If there's a previously selected structure, reset its layer
                    if (_structure != null)
                    {
                        _structure.ResetToOriginalLayer();
                    }

                    // Mark the new structure and set its layer to outline
                    clickedStructure.SetOutlineLayer();
                    _structure = clickedStructure;
                }

                // Broadcast a state change
                _lastCameraTarget = clickedStructure.GetCameraTarget();
                CameraEventBus.BroadcastSelectedObjectState(_lastCameraTarget);
            }
            else
            {
                // We hit the ground
                if (_structure != null)
                {
                    // Reset the old structure's layer if any was selected
                    _structure.ResetToOriginalLayer();
                    _structure = null;
                }

                // Switch to overview state
                if (CameraEventBus.cameraState == CameraState.SELECTED_OBJECT)
                {
                    _lastCameraTarget = null;
                    CameraEventBus.BroadcastOverviewState(groundCameraTarget);
                }
            }
        }
    }

    private void EnterFreeLookView()
    {
        if (CameraEventBus.cameraState == CameraState.OVERVIEW)
            CameraEventBus.BroadcastFreeLookState(groundCameraTarget);
        else
            CameraEventBus.BroadcastFreeLookState(_lastCameraTarget);
    }

    private void ExitFreeLookView()
    {
        if (CameraEventBus.cameraState == CameraState.OVERVIEW)
            CameraEventBus.BroadcastOverviewState(groundCameraTarget);
        else
            CameraEventBus.BroadcastSelectedObjectState(_lastCameraTarget);
    }

    // Returns whether the mouse is over a UI gameObject or not
    public bool IsPointerOverUI()
        => EventSystem.current.IsPointerOverGameObject();

    private void Update()
    {
        // Decrease scroll cooldown
        if (_currentScrollCooldown > 0f)
        {
            _currentScrollCooldown -= Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0))
        {
            onMouseClick?.Invoke();
        }

        // Exit building mode or go to overview state
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            onExitBuilding?.Invoke();

            if (!UIEventBus.isBuildingUIActive)
            {
                _latestSelectedShelf = null;
                UIEventBus.ActivateCurrentUI(userInterface.BUILDING);
            }

            // If we were in "selected object" camera state, go back to overview
            if (CameraEventBus.cameraState == CameraState.SELECTED_OBJECT)
            {
                CameraEventBus.BroadcastOverviewState(groundCameraTarget);

                // Also reset layer if something was selected
                if (_structure != null)
                {
                    _structure.ResetToOriginalLayer();
                    _structure = null;
                }
            }
        }

        // Structure Rotation
        _mouseScrollDelta = Input.GetAxis("Mouse ScrollWheel");
        if (_mouseScrollDelta != 0 && _currentScrollCooldown <= 0f)
        {
            onMouseWheelScroll?.Invoke(_mouseScrollDelta);
            _currentScrollCooldown = scrollWheelCooldown;
        }

        // If we are placing a structure, ignore the rest
        if (IsInPlacementMode) return;

        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            AdjustUI();
            AdjustView();
        }

        // Camera rotation
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            EnterFreeLookView();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            ExitFreeLookView();
        }

        // Camera Panning
        if (Input.GetMouseButtonDown(1) && CameraEventBus.cameraState == CameraState.SELECTED_OBJECT)
        {
            EnterPanningView();
        }
        if (Input.GetMouseButton(1) && CameraEventBus.cameraState == CameraState.SELECTED_OBJECT)
        {
            Vector3 currentMouseWorldPos = GetWorldPositionOnPlane(Input.mousePosition, planeHeight);
            Vector3 worldDelta = _dragStartWorldPos - currentMouseWorldPos;
            panningCameraTarget.transform.position += worldDelta * panSpeed;
            _dragStartWorldPos = currentMouseWorldPos;
        }
    }

    private void EnterPanningView()
    {
        _dragStartWorldPos = GetWorldPositionOnPlane(Input.mousePosition, planeHeight);
        CameraEventBus.BroadcastPanningState(panningCameraTarget.transform);
    }

    private Vector3 GetWorldPositionOnPlane(Vector3 screenPos, float y)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPos);
        Plane groundPlane = new Plane(planeNormal, new Vector3(0, y, 0));
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}
