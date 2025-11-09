using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private GameObject emptySlotIndicator;
    [SerializeField] private TextMeshProUGUI itemNameText;

    private ItemData currentItem;
    private int slotIndex;
    private InventoryUI inventoryUI;

    public ItemData CurrentItem => currentItem;

    public void Initialize(int index, InventoryUI ui)
    {
        slotIndex = index;
        inventoryUI = ui;
        ClearSlot();
    }

    public void SetItem(ItemData item)
    {
        currentItem = item;

        if (item != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;

            if (itemNameText != null)
            {
                itemNameText.text = item.itemName;
            }

            if (emptySlotIndicator != null)
            {
                emptySlotIndicator.SetActive(false);
            }
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        itemIcon.enabled = false;

        if (itemNameText != null)
        {
            itemNameText.text = "";
        }

        if (emptySlotIndicator != null)
        {
            emptySlotIndicator.SetActive(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        // Left click - Use item
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            UseItem();
        }
        // Right click - Drop item
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            inventoryUI.ShowDropConfirmation(currentItem);
        }
    }

    private void UseItem()
    {
       
            // Apply item effects
            Debug.Log($"Using {currentItem.itemName}");

            if (currentItem.stats.healthRestore > 0)
            {
                Debug.Log($"Restored {currentItem.stats.healthRestore} health");
                // Add your health restore logic here
            }

            PlayerInventory.Instance.RemoveItem(currentItem);
        
        
    }
}