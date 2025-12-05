using UnityEngine;
using UnityEngine.UI;

public class ShelfProductUI : MonoBehaviour
{
    [SerializeField] private Image productIcon;
    [SerializeField] private Button productButton;

    public void Init(Sprite icon, int index)
    {
        productButton.onClick.RemoveAllListeners();
        productIcon.sprite = icon;
        productIcon.SetNativeSize();
        productButton.onClick.AddListener(() => ShelfManager.Instance.OnProductSelected(index));
    }
}
