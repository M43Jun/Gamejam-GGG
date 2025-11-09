using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerClickHandler
{
    public Image itemIcon; // Reference ke Image component untuk icon
    public Transform itemParent; // Tempat anak item UI (bisa null ? pakai transform sendiri)
    public ItemUI itemPrefab; // Prefab ItemUI
    [HideInInspector] public ItemData currentItem;

    private void Start()
    {
        // Jika tidak ada reference, coba ambil Image component
        if (itemIcon == null)
        {
            itemIcon = GetComponent<Image>();
        }
    }

    // Set item ke slot
    public void SetItem(ItemData data)
    {
        ClearSlot(); // Hapus item lama

        currentItem = data;

        Transform parent = itemParent != null ? itemParent : transform;
        ItemUI itemUI = Instantiate(itemPrefab, parent);
        itemUI.Setup(data);
    }

    public void ClearSlot()
    {
        currentItem = null;

        // Hapus semua anak item (UI)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    // Handle klik
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        // Klik kanan = Drop
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            InventoryController.Instance.ShowDropConfirmation(this);
        }
        // Klik kiri = Use item (optional)
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            UseItem();
        }
    }

    private void UseItem()
    {
        if (currentItem == null) return;

        // Cek apakah item bisa digunakan (misalnya health potion)
        if (currentItem.stats.healthRestore > 0)
        {
            Debug.Log($"Using {currentItem.itemName}, restore {currentItem.stats.healthRestore} HP");
            // Tambahkan logic restore health di sini

            // Hapus item setelah digunakan
            ClearSlot();

            // Trigger inventory update event
            if (InventoryController.Instance != null)
            {
                InventoryController.Instance.TriggerInventoryUpdate();
            }
        }
        else
        {
            Debug.Log($"{currentItem.itemName} tidak bisa digunakan");
        }
    }
}