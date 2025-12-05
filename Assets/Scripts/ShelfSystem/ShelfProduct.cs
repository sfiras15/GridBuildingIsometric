using UnityEngine;

[System.Serializable]
public class ShelfProduct 
{
    [field:SerializeField] public GameObject productPrefab { get; private set; }
    [field: SerializeField] public Sprite productIcon { get; private set; }
}
