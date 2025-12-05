using System;
using UnityEngine;

public static class CameraEventBus
{
    public static CameraState cameraState = CameraState.OVERVIEW;

    public static event Action<Transform> onOverviewStateEntered;

    public static void BroadcastOverviewState(Transform transform)
    {
        cameraState = CameraState.OVERVIEW;
        onOverviewStateEntered?.Invoke(transform);
    }

    public static event Action<Transform> onSelectedObjectStateEntered;

    public static void BroadcastSelectedObjectState(Transform transform)
    {
        cameraState = CameraState.SELECTED_OBJECT;
        onSelectedObjectStateEntered?.Invoke(transform);
    }

    public static event Action<Transform> onFreeLookStateEntered;

    public static void BroadcastFreeLookState(Transform transform)
    {
        onFreeLookStateEntered?.Invoke(transform);
    }

    public static event Action<Transform> onPanningStateEntered;

    public static void BroadcastPanningState(Transform transform)
    {
        onPanningStateEntered?.Invoke(transform);
    }
}
