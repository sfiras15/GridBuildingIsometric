using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject buildingUI;
    [SerializeField] private GameObject shelfUI;

    private ShelfProductUI[] _shelfProducts;


    private void Awake()
    {
        _shelfProducts = shelfUI.GetComponentsInChildren<ShelfProductUI>(true);
    }

    private void OnEnable()
    {
        UIEventBus.OnStateChange += ActivateUserInterface;
        UIEventBus.OnProductChange += UpdateShelfUI;
    }

    private void OnDisable()
    {
        UIEventBus.OnStateChange -= ActivateUserInterface;
        UIEventBus.OnProductChange -= UpdateShelfUI;
    }

    private void ActivateUserInterface(userInterface interfaceToActivate)
    {
        if (interfaceToActivate == userInterface.BUILDING)
        {
            buildingUI.SetActive(true);
            shelfUI.SetActive(false);
            UIEventBus.isBuildingUIActive = true;
        }
        else if (interfaceToActivate == userInterface.SHELVES)
        {
            buildingUI.SetActive(false);
            shelfUI.SetActive(true);
            UIEventBus.isBuildingUIActive = false;
        }
        else
        {
            buildingUI.SetActive(false);
            shelfUI.SetActive(false);
            UIEventBus.isBuildingUIActive = false;
        }
    }

    private void UpdateShelfUI(ShelfProduct[] products)
    {
        if (_shelfProducts == null || products == null) return;

        // Loop through the minimum of UI slots and the incoming array of products
        int count = Mathf.Min(_shelfProducts.Length, products.Length);
        for (int i = 0; i < count; i++)
        {
            _shelfProducts[i].gameObject.SetActive(true);
            _shelfProducts[i].Init(products[i].productIcon, i);
        }

        // If we have more UI slots than products, we want to disable the extra slots:
        for (int i = count; i < _shelfProducts.Length; i++)
        {
            _shelfProducts[i].gameObject.SetActive(false);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActivateUserInterface(userInterface.NONE);
    }
}
