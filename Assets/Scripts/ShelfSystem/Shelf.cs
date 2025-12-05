using System.Collections.Generic;
using UnityEngine;

public class Shelf : Structure
{
    [Header("Product Prefabs")]
    [SerializeField] public ShelfProduct[] shelfProducts; // Change later if needed

    [Header("Slot Setup")]
    [SerializeField] private Transform shelfSlotsHolder;
    [field: SerializeField] public List<Transform> ShelfSlots { get; private set; } = new List<Transform>();

    [Header("Persistent Data")]
    private ShelfData _shelfData; // Holds data about product placements

    private string _uniqueID;

    private void FillShelfSlots()
    {
        for (int i = 0; i < shelfSlotsHolder.childCount; i++)
        {
            ShelfSlots.Add(shelfSlotsHolder.GetChild(i));
        }
    }

    public void Init(string uniqueID, ShelfData data)
    {
        _uniqueID = uniqueID;
        FillShelfSlots();

        // Initialize shelfData if not set (could be loaded from file, etc.)
        if (_shelfData == null)
            _shelfData = new ShelfData();

        SetShelfData(data);
    }

    public ShelfData GetShelfData()
    {
        return _shelfData;
    }
    public string GetShelfID()
    {
        return _uniqueID;
    }

    public void SetShelfData(ShelfData data)
    {
        _shelfData = data;

        ShelfManager.Instance.AddToPersistentShelves(_uniqueID,data);
    }

}

