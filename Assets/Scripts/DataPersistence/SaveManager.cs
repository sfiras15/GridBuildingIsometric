using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private string fileName = "MultiGridSave.json";
    private string filePath;

    private void Awake()
    {
        // Set the file path for saving
        filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
    }

    /// <summary>
    /// Saves multiple GridData objects (each with a unique ID) as well as ShelfData into one JSON file.
    /// </summary>
    public void SaveGrids(Dictionary<string, GridData> grids)
    {
        GameSaveData multiSave = new GameSaveData();
        foreach (var kvp in grids)
        {
            GridDataSaveWrapper wrapper = new GridDataSaveWrapper();
            wrapper.gridID = kvp.Key;
            wrapper.gridData = kvp.Value.ToGridDataSave();
            multiSave.grids.Add(wrapper);
        }

        // save shelf data.
        multiSave.shelfDataSave = ShelfManager.Instance.ToShelfDataSave();

        string json = JsonUtility.ToJson(multiSave, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Saved multi-grid data to " + filePath);
    }

    /// <summary>
    /// Loads all grid data and ShelfData from the JSON file.
    /// If no file exists, returns a new dictionary with default GridData objects using the provided width and length.
    /// </summary>
    public Dictionary<string, GridData> LoadGrids(int defaultWidth, int defaultLength)
    {
        Dictionary<string, GridData> grids = new Dictionary<string, GridData>();

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            GameSaveData multiSave = JsonUtility.FromJson<GameSaveData>(json);
            foreach (var wrapper in multiSave.grids)
            {
                GridData gridData = GridData.LoadFromGridDataSave(wrapper.gridData);
                grids.Add(wrapper.gridID, gridData);
            }
            Debug.Log("Loaded multi-grid data from " + filePath);

            if (multiSave.shelfDataSave != null)
            {
                ShelfManager.Instance.LoadFromShelfDataSave(multiSave.shelfDataSave);
            }
        }
        else
        {
            // Create new default grid data if the file doesn't exist.
            grids.Add("floorData", new GridData(defaultWidth, defaultLength));
            grids.Add("furnitureData", new GridData(defaultWidth, defaultLength));
            Debug.Log("No save file found. Created new grid data.");
        }

        return grids;
    }
}
