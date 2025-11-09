using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance;

    [Header("UI References")]
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount = 18;
    public GameObject inventoryParent;
    [Header("Player Data")]
    public int currentGold = 100;

    [Header("Drop Confirmation UI")]
    public GameObject dropConfirmPanel; // Panel untuk konfirmasi drop
    public UnityEngine.UI.Button dropYesButton;
    public UnityEngine.UI.Button dropNoButton;

    private List<Slot> slots = new List<Slot>();
    private Slot slotToDropFrom; // Slot yang akan di-drop itemnya

    // Events untuk update UI
    public delegate void OnInventoryChanged();
    public event OnInventoryChanged OnInventoryUpdated;

    public delegate void OnGoldChanged(int newGold);
    public event OnGoldChanged OnGoldUpdated;

    // Public method untuk trigger event dari luar
    public void TriggerInventoryUpdate()
    {
        OnInventoryUpdated?.Invoke();
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Create slots
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, inventoryPanel.transform);
            Slot slot = slotObj.GetComponent<Slot>();
            slots.Add(slot);
        }

        // Setup drop confirmation buttons
        if (dropYesButton != null)
            dropYesButton.onClick.AddListener(ConfirmDrop);

        if (dropNoButton != null)
            dropNoButton.onClick.AddListener(CancelDrop);

        if (dropConfirmPanel != null)
            dropConfirmPanel.SetActive(false);

        // Hide inventory at start
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        // Toggle inventory dengan tombol I
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);
        inventoryParent.SetActive(isActive);
        // Pause game saat inventory dibuka
        Time.timeScale = isActive ? 0f : 1f;
    }

    // Tambah item dari world (pickup)
    public bool AddItem(GameObject itemObject)
    {
        Item item = itemObject.GetComponent<Item>();
        if (item == null || item.itemData == null) return false;

        return AddItemData(item.itemData);
    }

    // Tambah item dari ItemData (untuk shop/system lain)
    public bool AddItemData(ItemData itemData)
    {
        foreach (Slot slot in slots)
        {
            if (slot.currentItem == null)
            {
                slot.SetItem(itemData);
                OnInventoryUpdated?.Invoke();
                return true;
            }
        }

        Debug.Log("Inventory penuh!");
        return false;
    }


    // Panggil ini dari Slot saat klik kanan
    public void ShowDropConfirmation(Slot slot)
    {
        slotToDropFrom = slot;
        if (dropConfirmPanel != null)
        {
            dropConfirmPanel.SetActive(true);
        }
    }

    private void ConfirmDrop()
    {
        if (slotToDropFrom != null)
        {
            slotToDropFrom.ClearSlot();
            OnInventoryUpdated?.Invoke();
        }

        if (dropConfirmPanel != null)
            dropConfirmPanel.SetActive(false);

        slotToDropFrom = null;
    }

    private void CancelDrop()
    {
        if (dropConfirmPanel != null)
            dropConfirmPanel.SetActive(false);

        slotToDropFrom = null;
    }

    // Fungsi untuk gold
    public void AddGold(int amount)
    {
        currentGold += amount;
        OnGoldUpdated?.Invoke(currentGold);
    }

    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            OnGoldUpdated?.Invoke(currentGold);
            return true;
        }
        return false;
    }

    // Get all items (untuk shop sell)
    public List<ItemData> GetAllItems()
    {
        List<ItemData> items = new List<ItemData>();
        foreach (Slot slot in slots)
        {
            if (slot.currentItem != null)
            {
                items.Add(slot.currentItem);
            }
        }
        return items;
    }

    // Remove specific item (untuk shop sell)
    public bool RemoveItem(ItemData itemData)
    {
        foreach (Slot slot in slots)
        {
            if (slot.currentItem == itemData)
            {
                slot.ClearSlot();
                OnInventoryUpdated?.Invoke();
                return true;
            }
        }
        return false;
    }
}