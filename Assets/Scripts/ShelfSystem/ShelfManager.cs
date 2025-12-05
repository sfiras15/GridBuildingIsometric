using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShelfManager : MonoBehaviour
{
    public static ShelfManager Instance;
    [Header("References")]
    [SerializeField] private InputManager inputManager; // Used to get the current (selected) shelf

    // Reference to the currently spawned (unconfirmed) product instance.
    private GameObject _currentProductInstance;
    // The index of the slot where the current product is placed.
    private int _currentSlotIndex = 0;

    // The shelf that is locked in when a product is selected.
    private Shelf _lockedShelf;
    // Data for the current unconfirmed product placement.
    private ShelfItemData _currentItemData;

    // Dictionary to store shelves and their ShelfData for later persistence.
    private Dictionary<string, ShelfData> _persistedShelfData = new Dictionary<string, ShelfData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Called from ShelfUI Buttons.
    /// When a product is selected, instantiates the product in the first available slot of the selected shelf.
    /// Note: The ShelfItemData is created but not added to the shelf’s data until confirmation.
    /// </summary>
    public void OnProductSelected(int productIndex)
    {
        // Lock in the current shelf from the InputManager.
        Shelf selectedShelf = inputManager.GetSelectedShelf();
        if (selectedShelf == null)
        {
            Debug.LogError("No shelf selected");
            return;
        }
        // If a product is already unconfirmed, do nothing.
        if (_currentProductInstance != null)
            return;

        _lockedShelf = selectedShelf;

        // Find the first available slot.
        for (int i = 0; i < _lockedShelf.ShelfSlots.Count; i++)
        {
            if (_lockedShelf.ShelfSlots[i].childCount == 0)
            {
                _currentSlotIndex = i;
                _currentProductInstance = Instantiate(
                    _lockedShelf.shelfProducts[productIndex].productPrefab,
                    _lockedShelf.ShelfSlots[i].position,
                    Quaternion.identity,
                    _lockedShelf.ShelfSlots[i]
                );
                // Create a new data entry for this product placement.
                _currentItemData = new ShelfItemData
                {
                    productIndex = productIndex,
                    slotIndex = _currentSlotIndex
                };

                break;
            }
        }
    }

    /// <summary>
    /// Moves the product to the next available slot in the locked shelf.
    /// Updates the unconfirmed data accordingly.
    /// </summary>
    public void MoveToNextSlot()
    {
        if (_currentProductInstance != null && _lockedShelf != null)
        {
            int startingIndex = _currentSlotIndex;
            bool found = false;
            do
            {
                _currentSlotIndex = (_currentSlotIndex + 1) % _lockedShelf.ShelfSlots.Count;
                if (_lockedShelf.ShelfSlots[_currentSlotIndex].childCount == 0)
                {
                    found = true;
                    break;
                }
            } while (_currentSlotIndex != startingIndex);

            if (found)
            {
                _currentProductInstance.transform.SetParent(_lockedShelf.ShelfSlots[_currentSlotIndex]);
                _currentProductInstance.transform.position = _lockedShelf.ShelfSlots[_currentSlotIndex].position;

                // Update the unconfirmed data.
                if (_currentItemData != null)
                {
                    _currentItemData.slotIndex = _currentSlotIndex;
                }
            }
        }
    }

    /// <summary>
    /// Moves the product to the previous available slot in the locked shelf.
    /// Updates the unconfirmed data accordingly.
    /// </summary>
    public void MoveToPrevSlot()
    {
        if (_currentProductInstance != null && _lockedShelf != null)
        {
            int startingIndex = _currentSlotIndex;
            bool found = false;
            do
            {
                _currentSlotIndex = (_currentSlotIndex - 1 + _lockedShelf.ShelfSlots.Count) % _lockedShelf.ShelfSlots.Count;
                if (_lockedShelf.ShelfSlots[_currentSlotIndex].childCount == 0)
                {
                    found = true;
                    break;
                }
            } while (_currentSlotIndex != startingIndex);

            if (found)
            {
                _currentProductInstance.transform.SetParent(_lockedShelf.ShelfSlots[_currentSlotIndex]);
                _currentProductInstance.transform.position = _lockedShelf.ShelfSlots[_currentSlotIndex].position;
                if (_currentItemData != null)
                {
                    _currentItemData.slotIndex = _currentSlotIndex;
                }
            }
        }
    }

    /// <summary>
    /// Confirms the placement of the product.
    /// At this point, the unconfirmed product data is added to the shelf’s ShelfData,
    /// and the shelf along with its updated data is stored in a dictionary for persistence.
    /// Also triggers an event to notify listeners.
    /// </summary>
    public void ConfirmPlacement()
    {
        if (_lockedShelf != null && _currentItemData != null)
        {
            // Ensure shelfData exists.
            if (_lockedShelf.GetShelfData() == null)
            {
                _lockedShelf.SetShelfData(new ShelfData());
            }
            // Add the unconfirmed item data to the shelfData.
            _lockedShelf.GetShelfData().shelfItems.Add(_currentItemData);

            string shelfID = _lockedShelf.GetShelfID();
            // Store the shelf and its new ShelfData in the dictionary.
            if (_persistedShelfData.ContainsKey(shelfID))
            {
                _persistedShelfData[shelfID] = _lockedShelf.GetShelfData();
            }
            else
            {
                _persistedShelfData.Add(shelfID, _lockedShelf.GetShelfData());
            }
        }

        // Clear temporary unconfirmed references.
        _currentProductInstance = null;
        _lockedShelf = null;
        _currentItemData = null;

        // Optionally update UI.
        UIEventBus.ActivateCurrentUI(userInterface.NONE);
    }

    private void Update()
    {
        // Get the current shelf selection from the InputManager.
        Shelf currentSelectedShelf = inputManager.GetSelectedShelf();

        // If there is an unconfirmed product and the current selection changes,
        // remove the unconfirmed product and its data.
        if (_currentProductInstance != null && currentSelectedShelf != _lockedShelf)
        {
            if (_lockedShelf != null && _lockedShelf.GetShelfData() != null)
            {
                _lockedShelf.GetShelfData().shelfItems.RemoveAll(item => item.slotIndex == _currentSlotIndex);
            }
            Destroy(_currentProductInstance);
            _currentProductInstance = null;
            _lockedShelf = currentSelectedShelf;
            _currentItemData = null;
        }
    }

    /// <summary>
    /// Rebuilds the shelf by instantiating products from the provided ShelfData.
    /// </summary>
    /// <param name="shelf">The Shelf to be rebuilt.</param>
    /// <param name="data">The ShelfData containing product placement info.</param>
    public void RebuildShelf(Shelf shelf, ShelfData data, string uniqueID)
    {
        if (data != null)
        {
            shelf.Init(uniqueID, data);
            foreach (var item in data.shelfItems)
            {
                if (item.productIndex >= 0 && item.productIndex < shelf.shelfProducts.Length)
                {
                    Transform targetSlot = shelf.ShelfSlots[item.slotIndex];
                    Instantiate(shelf.shelfProducts[item.productIndex].productPrefab, targetSlot.position, Quaternion.identity, targetSlot);
                }
            }
        }
    }

    /// <summary>
    /// Remove the shelf from the list of persistentShelves.
    /// </summary>
    /// <param name="shelf">The Shelf to be removed.</param>
    public void RemoveShelf(string shelfID)
    {
        if (shelfID == null) return;

        if (!_persistedShelfData.ContainsKey(shelfID)) return;

        _persistedShelfData.Remove(shelfID);
    }

    public void AddToPersistentShelves(string shelfID, ShelfData data)
    {
        if (shelfID == null) return;

        if (!_persistedShelfData.ContainsKey(shelfID)) _persistedShelfData.Add(shelfID, data);
        else _persistedShelfData[shelfID] = data;
    }

    public ShelfData GetShelfDataByUniqueID(string uniqueID)
    {
        if (!_persistedShelfData.ContainsKey(uniqueID)) return null;
        
        return _persistedShelfData[uniqueID];
    }

    /// <summary>
    /// Creates a shelfDataSave from the _persistedShelfData
    /// </summary>
    public ShelfDataSave ToShelfDataSave()
    {
        ShelfDataSave save = new ShelfDataSave();

        save.shelfEntries = new List<ShelfDataEntry>();

        foreach (var kvp in _persistedShelfData)
        {
            ShelfDataEntry entry = new ShelfDataEntry();
            entry.shelfUniqueID = kvp.Key;
            entry.shelfData = kvp.Value;

            // Add the entry
            save.shelfEntries.Add(entry);
        }

        return save;
    }

    public Dictionary<string, ShelfData> LoadFromShelfDataSave(ShelfDataSave save)
    {
        _persistedShelfData = new Dictionary<string, ShelfData>();

        foreach(var entry in save.shelfEntries)
        {
            _persistedShelfData[entry.shelfUniqueID] = entry.shelfData;
        }

        return _persistedShelfData;
    }

    /// <summary>
    /// Logs all shelves and their current ShelfData in the persisted shelves dictionary.
    /// </summary>
    private void LogPersistedShelves()
    {
        Debug.Log("Persisted Shelves (" + _persistedShelfData.Count + "):");
        foreach (var kvp in _persistedShelfData)
        {
            string shelf = kvp.Key;
            ShelfData data = kvp.Value;
            string shelfIdentifier = shelf; // assuming each shelf has a unique name
            string logMessage = "Shelf: " + shelfIdentifier + " | Items Count: " + data.shelfItems.Count;

            foreach (var item in data.shelfItems)
            {
                logMessage += "\n   - Product Index: " + item.productIndex + ", Slot Index: " + item.slotIndex;
            }

            Debug.Log(logMessage);
        }
    }

}
