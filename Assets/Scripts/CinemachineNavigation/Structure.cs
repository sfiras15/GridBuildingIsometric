using UnityEngine;
using System.Collections.Generic;

public class Structure : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;

    // Dictionary to remember each Transform's original layer
    private Dictionary<Transform, int> originalLayers;

    private void Start()
    {
        originalLayers = new Dictionary<Transform, int>();

        // Capture the original layer of this object AND all its children
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            originalLayers[child] = child.gameObject.layer;
        }
    }

    public Transform GetCameraTarget()
    {
        return cameraTarget;
    }

    // set the object (and children) to layer index 9 (outline)
    public void SetOutlineLayer()
    {
        foreach (KeyValuePair<Transform, int> kvp in originalLayers)
        {
            kvp.Key.gameObject.layer = 9;
        }
    }

    // Restore the original layers
    public void ResetToOriginalLayer()
    {
        foreach (KeyValuePair<Transform, int> kvp in originalLayers)
        {
            kvp.Key.gameObject.layer = kvp.Value;
        }
    }
}
