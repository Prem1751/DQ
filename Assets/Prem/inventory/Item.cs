using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName = "ไอเทมใหม่";
    public Sprite icon;
    [TextArea(3, 5)]
    public string description = "รายละเอียดไอเทม";
}