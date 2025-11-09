using UnityEngine;

public class PlayerItemController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            Item item = collision.GetComponent<Item>();
            if (item != null && item.itemData != null)
            {
                bool itemAdded = InventoryController.Instance.AddItemData(item.itemData);
                if (itemAdded)
                {
                    Debug.Log($"Picked up {item.itemData.itemName}");
                    Destroy(collision.gameObject);
                }
                else
                {
                    Debug.Log("Inventory full!");
                }
            }
        }
    }
}