using System.Collections.Generic;


[System.Serializable]
public class ShelfData
{
    public List<ShelfItemData> shelfItems = new List<ShelfItemData>();
}

[System.Serializable]
public class ShelfItemData
{
    public int productIndex; // Which product from shelfProducts array was placed.
    public int slotIndex;    // The index of the slot where the product was placed.
}