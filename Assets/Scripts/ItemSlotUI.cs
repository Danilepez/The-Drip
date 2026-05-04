using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image itemIconImage;
    public TMP_Text itemName;
    public TMP_Text itemQuantity;

    private ItemData _itemData;

    public void Setup(ItemData data)
    {
        _itemData = data;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_itemData != null)
            ItemDetailUI.Instance?.Show(_itemData);
    }
}
