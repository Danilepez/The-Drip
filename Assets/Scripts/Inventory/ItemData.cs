using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/New Item")]


public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        Paper,
        Key,
        Flashlight,
        Map,
        Battery,
        Other
    }
    public string itemName;
    public Sprite itemIcon;
    [TextArea(6, 12)]
    public string itemDescription;
    public ItemType itemType;
}
