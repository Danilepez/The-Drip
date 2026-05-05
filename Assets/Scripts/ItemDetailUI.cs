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
    public ScrollRect descriptionScroll;
    public RectTransform descriptionContent;
    public bool resetScrollOnShow = true;
    public float minDescriptionHeight = 0f;
    public float maxDescriptionHeight = 0f;

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
        UpdateDescriptionLayout();
        detailPanel.SetActive(true);
    }

    public void Hide()
    {
        detailPanel.SetActive(false);
    }

    private void UpdateDescriptionLayout()
    {
        if (itemDescriptionText == null) return;

        itemDescriptionText.ForceMeshUpdate();
        float preferredHeight = itemDescriptionText.preferredHeight;
        float targetHeight = preferredHeight;

        if (minDescriptionHeight > 0f)
            targetHeight = Mathf.Max(targetHeight, minDescriptionHeight);
        if (maxDescriptionHeight > 0f)
            targetHeight = Mathf.Min(targetHeight, maxDescriptionHeight);

        RectTransform content = descriptionContent != null
            ? descriptionContent
            : itemDescriptionText.rectTransform;

        if (content != null)
        {
            Vector2 size = content.sizeDelta;
            size.y = targetHeight;
            content.sizeDelta = size;
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }

        if (descriptionScroll != null)
        {
            if (descriptionScroll.content == null && content != null)
                descriptionScroll.content = content;

            if (resetScrollOnShow)
                descriptionScroll.verticalNormalizedPosition = 1f;
        }
    }
}
