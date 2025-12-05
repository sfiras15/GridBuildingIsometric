using System;

public enum userInterface
{
    BUILDING,
    SHELVES,
    NONE
}

public static class UIEventBus
{
    public static bool isBuildingUIActive;

    public static event Action<userInterface> OnStateChange;
    public static event Action<ShelfProduct[]> OnProductChange;
    public static void ActivateCurrentUI(userInterface interfaceToActivate)
    {
        OnStateChange?.Invoke(interfaceToActivate);
    }

    public static void ActivateCurrentUI(userInterface interfaceToActivate, ShelfProduct[] products)
    {
        OnStateChange?.Invoke(interfaceToActivate);
        if (interfaceToActivate == userInterface.SHELVES) OnProductChange?.Invoke(products);
    }
}
