using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    public float pickupDistance = 3f;

    private GameObject player;
    private bool canPickup = false;

    void Start()
    {
        // หา Player โดยใช้ Tag
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("找不到玩家对象！请确保玩家有 'Player' 标签");
        }

        // เพิ่ม Collider อัตโนมัติถ้ายังไม่มี
        if (GetComponent<Collider>() == null)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(2f, 2f, 2f);
        }
        else
        {
            // 确保 Collider เป็น Trigger
            GetComponent<Collider>().isTrigger = true;
        }
    }

    void Update()
    {
        // ตรวจสอบระยะทางระหว่างไอเท็มกับผู้เล่น
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance <= pickupDistance)
            {
                canPickup = true;
                Debug.Log("สามารถเก็บไอเท็มได้: " + item.itemName + " (กด F)");

                if (Input.GetKeyDown(KeyCode.F))
                {
                    PickupItem();
                }
            }
            else
            {
                canPickup = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canPickup = true;
            Debug.Log("กด F เพื่อเก็บ: " + item.itemName);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canPickup = false;
            Debug.Log("ออกจากระยะเก็บไอเท็ม: " + item.itemName);
        }
    }

    void PickupItem()
    {
        if (canPickup && InventoryManager.instance != null)
        {
            InventoryManager.instance.AddItem(item);
            Debug.Log("เก็บไอเท็มเรียบร้อย: " + item.itemName);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("ไม่สามารถเก็บไอเท็มได้: InventoryManager.instance เป็น null!");
        }
    }

    void OnDrawGizmosSelected()
    {
        // วาดเส้นแสดงระยะการเก็บ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }

    // 显示GUI提示（可选）
    void OnGUI()
    {
        if (canPickup && player != null)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 50, 200, 30), "กด F เพื่อเก็บ: " + item.itemName);
        }
    }
}