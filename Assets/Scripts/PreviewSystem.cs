using UnityEngine;

public class PreviewSystem : MonoBehaviour
{
    [Tooltip("How high the preview object should be from the grid")]
    [SerializeField] private float previewYOffset = 0.06f;

    [SerializeField] private GameObject cellIndicator;
    private GameObject _previewObject;
    private Transform _previewObjectPivot;

    [Tooltip("The transparent material that will be applied on the object")]
    [SerializeField] private Material previewMaterialPrefab;
    private Material _previewMaterialInstance;

    private Renderer _cellIndicatorRenderer;

    private void Start()
    {
        _previewMaterialInstance = new Material(previewMaterialPrefab);
        cellIndicator.SetActive(false);
        _cellIndicatorRenderer = cellIndicator.GetComponentInChildren<Renderer>();
    }

    public void StartShowingPlacementPreview(GameObject prefab, Vector2Int size)
    {
        _previewObject = Instantiate(prefab);
        PreparePreview(_previewObject);
        _previewObjectPivot = _previewObject.transform.GetChild(0);
    }

    private void PrepareCursor(Vector2Int size)
    {
        if (size.x > 0 && size.y > 0)
        {
            cellIndicator.transform.localScale = new Vector3(size.x, 1, size.y);
            _cellIndicatorRenderer.material.mainTextureScale = size;
        }
    }

    private void PreparePreview(GameObject previewObject)
    {
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
                materials[i] = _previewMaterialInstance;
            renderer.materials = materials;
        }
    }

    public void StopShowingPreview()
    {
        cellIndicator.SetActive(false);
        if (_previewObject != null)
            Destroy(_previewObject);
    }

    public void UpdatePosition(Vector3 position, bool validity)
    {
        if (_previewObject != null)
        {
            MovePreview(position);
            ApplyFeedbackToPreview(validity);
        }

        MoveCursor(position);
        ApplyFeedbackToCursor(validity);
    }

    private Color GetColorByValidity(bool validity)
    {
        Color color = validity ? Color.white : Color.red;
        color.a = 0.5f;
        return color;
    }

    private void ApplyFeedbackToPreview(bool validity)
    {
        _previewMaterialInstance.color = GetColorByValidity(validity);
    }

    private void ApplyFeedbackToCursor(bool validity)
    {
        _cellIndicatorRenderer.material.color = GetColorByValidity(validity);
    }

    private void MoveCursor(Vector3 position)
    {
        cellIndicator.transform.position = position;
    }

    private void MovePreview(Vector3 position)
    {
        // Add the rotation offset plus a small Y offset
        _previewObject.transform.position = new Vector3(
            position.x,
            position.y + previewYOffset,
            position.z
        );
    }

    internal void StartShowingRemovePreview()
    {
        cellIndicator.SetActive(true);
        PrepareCursor(Vector2Int.one);
        ApplyFeedbackToCursor(false);
    }

    // Called by PlacementSystem when scroll is detected
    public void SetPreviewRotation(int rotationDegrees)
    {
        RotateRenderer(rotationDegrees);
    }
    private void RotateRenderer(int rotationDegrees)
    {
        if (_previewObject == null) return;

        if (_previewObjectPivot ==  null) return;

        // Set local rotation so it spins around its own center
        _previewObjectPivot.localRotation = Quaternion.Euler(0, rotationDegrees, 0);
    }
}
