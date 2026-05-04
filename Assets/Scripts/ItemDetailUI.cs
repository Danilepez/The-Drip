using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDetailUI : MonoBehaviour
{
    public static ItemDetailUI Instance { get; private set; }

    public GameObject detailPanel;
    public Image itemIcon;
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;

    private void Awake()
    {
        Instance = this;
        detailPanel.SetActive(false);
    }

    public void Show(ItemData data)
    {
        itemIcon.sprite = data.itemIcon;
        itemNameText.text = data.itemName;
        itemDescriptionText.text = data.itemDescription;
        detailPanel.SetActive(true);
    }

    public void Hide()
    {
        detailPanel.SetActive(false);
    }
}
