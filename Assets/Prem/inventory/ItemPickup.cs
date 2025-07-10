using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.F))
        {
            if (InventoryManager.instance != null)
            {
                InventoryManager.instance.AddItem(item);
                Destroy(gameObject);
                Debug.Log("เก็บไอเท็ม: " + item.itemName);
            }
            else
            {
                Debug.LogError("InventoryManager.instance เป็น null!");
            }
        }
    }
}