using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    // List of all the objects that have been instantiated
    [SerializeField] private List<GameObject> _placedGameObjects = new();

    public GameObject PlaceObject(GameObject prefab, Vector3 position, int rotationDegrees)
    {
        GameObject newObject = Instantiate(prefab);
        newObject.transform.position = position;
        RotateObject(newObject, rotationDegrees);
        _placedGameObjects.Add(newObject);
        return newObject;
    }

    internal void RemoveObjectAt(Vector3 position)
    {
        // Loop backward so when we remove an element the others shift safely
        for (int i = _placedGameObjects.Count - 1; i >= 0; i--)
        {
            GameObject placedObj = _placedGameObjects[i];

            if (placedObj == null)
                continue;

            // Compare the current object's position to the target position
            if (Vector3Int.RoundToInt(placedObj.transform.position) == Vector3Int.RoundToInt(position))
            {
                Destroy(placedObj);
                _placedGameObjects.RemoveAt(i);
                break;  // Remove only the first match
            }
        }
    }


    private void RotateObject(GameObject gameObject, int rotationDegrees)
    {
        // Find the child pivot by index
        // prefabs must have a child pivot that contains the renderer
        Transform pivot = gameObject.transform.GetChild(0);

        if (pivot != null)
        {
            // Set local rotation so it spins around its own center
            pivot.localRotation = Quaternion.Euler(0, rotationDegrees, 0);
        }
        else
        {
            Debug.LogWarning("No PivotRoot child found under the preview object!");
        }
    }
    
    public GameObject GetPlacedObjectByPosition(Vector3 position)
    {
        for (int i = 0; i < _placedGameObjects.Count; i++)
        {
            GameObject placedObj = _placedGameObjects[i];

            if (placedObj == null)
                continue;

            // Compare the current object's position to the target position
            if (Vector3Int.RoundToInt(placedObj.transform.position) == Vector3Int.RoundToInt(position))
            {
                return placedObj;
            }
        }
        return null;
    }
}