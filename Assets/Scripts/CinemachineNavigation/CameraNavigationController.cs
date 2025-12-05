using Cinemachine;
using UnityEngine;


public enum CameraState
{
    OVERVIEW,
    SELECTED_OBJECT,
}
/// <summary>
/// This script handles event subscription to the different camera events.
/// Changes the Cinemachine camera depending on the animator state of the camera.
/// </summary>

public class CameraNavigationController : MonoBehaviour
{
    [Header("Virtual Camera references")]
    [SerializeField] private CinemachineVirtualCamera overviewVC;
    [SerializeField] private CinemachineVirtualCamera selectedObjectVC;

    // Note : Panning works for selected objects only so the panningVC need to have the same body configuration as the selectedObjectVC
    [SerializeField] private CinemachineVirtualCamera panningVC; 

    [SerializeField] private float cameraRotationSpeed = 300f;
    
    private CinemachineOrbitalTransposer _selectedObjectVCOrbital; 
    private CinemachineOrbitalTransposer _overviewVCOrbital;
    private Animator _animator;

    // Animator parameters
    private readonly int isObjectSelectedBool = Animator.StringToHash("isObjectSelected");
    private readonly int isPanningBool = Animator.StringToHash("isPanning");

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        // Store the orbital references to affect the input axis and speed later when entering certain states
        if (selectedObjectVC != null ) _selectedObjectVCOrbital = selectedObjectVC.GetCinemachineComponent<CinemachineOrbitalTransposer>();

        if (overviewVC != null) _overviewVCOrbital = overviewVC.GetCinemachineComponent<CinemachineOrbitalTransposer>();
    }

    private void OnEnable()
    {
        CameraEventBus.onOverviewStateEntered += EnterOverViewState;
        CameraEventBus.onSelectedObjectStateEntered += EnterSelectedObjectState;
        CameraEventBus.onFreeLookStateEntered += EnterFreeLookState;
        CameraEventBus.onPanningStateEntered += EnterPanningState;
    }

   
    private void OnDisable()
    {
        CameraEventBus.onOverviewStateEntered -= EnterOverViewState;
        CameraEventBus.onSelectedObjectStateEntered -= EnterSelectedObjectState;
        CameraEventBus.onFreeLookStateEntered -= EnterFreeLookState;
        CameraEventBus.onPanningStateEntered -= EnterPanningState;
    }

    private void EnterPanningState(Transform transform)
    {
        if (panningVC == null) return;

        // Move the Panning camera position and rotation to the selectedObject's camera
        Transform selectedCamTransform = selectedObjectVC.VirtualCameraGameObject.transform;
        Transform panningCamTransform = panningVC.VirtualCameraGameObject.transform;

        panningCamTransform.position = selectedCamTransform.position;
        panningCamTransform.rotation = selectedCamTransform.rotation;

        // Assign the camera target
        panningVC.Follow = transform;
        panningVC.LookAt = transform;

        // Change animation state of the camera
        SetObjectSelectedParameter(false);
        SetPanningParameter(true);
    }

    private void EnterFreeLookState(Transform transform)
    {
        if (selectedObjectVC == null) return;

        if (overviewVC == null) return;

        if (CameraEventBus.cameraState == CameraState.OVERVIEW)
        {
            // Update the follow target (position)
            overviewVC.Follow = transform;

            // Update the look target (rotation)
            overviewVC.LookAt = transform;

            _overviewVCOrbital.m_XAxis.m_MaxSpeed = cameraRotationSpeed;
        }
        else if (CameraEventBus.cameraState == CameraState.SELECTED_OBJECT)
        {
            // Update the follow target (position)
            selectedObjectVC.Follow = transform;

            // Update the look target (rotation)
            selectedObjectVC.LookAt = transform;

            _selectedObjectVCOrbital.m_XAxis.m_MaxSpeed = cameraRotationSpeed;
        }

        // Change animation state of the camera
        SetObjectSelectedParameter(CameraEventBus.cameraState == CameraState.SELECTED_OBJECT);
        SetPanningParameter(false);
    }

    private void EnterSelectedObjectState(Transform transform)
    {
        if (selectedObjectVC == null) return;

        // Force heading to "center" to reset the position of the camera 
        if (selectedObjectVC.Follow != transform) _selectedObjectVCOrbital.m_XAxis.Value = 0f;

        // Update the follow target (position)
        selectedObjectVC.Follow = transform;

        // Update the look target (rotation)
        selectedObjectVC.LookAt = transform;

        // reset the speed to 0 to not orbit
        _selectedObjectVCOrbital.m_XAxis.m_MaxSpeed = 0f;

        // Change animation state of the camera
        SetObjectSelectedParameter(true);
        SetPanningParameter(false);
    }

    private void EnterOverViewState(Transform transform)
    {
        // Update the follow target (position)
        overviewVC.Follow = transform;

        // Update the look target (rotation)
        overviewVC.LookAt = transform;

        // reset the speed to 0 to not orbit
        _overviewVCOrbital.m_XAxis.m_MaxSpeed = 0f;

        // Change animation state of the camera
        SetObjectSelectedParameter(false);
        SetPanningParameter(false);
    }

    private void Start()
    {
        SetObjectSelectedParameter(false);
    }

    private void SetObjectSelectedParameter(bool value)
    {
        if (_animator != null) _animator.SetBool(isObjectSelectedBool, value);
    }

    private void SetPanningParameter(bool value)
    {
        if (_animator != null) _animator.SetBool(isPanningBool, value);
    }
}
