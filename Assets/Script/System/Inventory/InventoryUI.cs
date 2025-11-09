using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject inventoryImage;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    [Header("Drop Confirmation")]
    [SerializeField] private GameObject dropConfirmPanel;
    [SerializeField] private TextMeshProUGUI dropConfirmText;
    [SerializeField] private Button dropConfirmButton;
    [SerializeField] private Button dropCancelButton;

    private InventorySlot[] slots;
    private ItemData itemToConfirmDrop;

    private void Start()
    {
        InitializeSlots();

        PlayerInventory.Instance.OnInventoryChangedEvent += RefreshInventory;
        PlayerInventory.Instance.OnGoldChangedEvent += UpdateGoldDisplay;

        dropConfirmButton.onClick.AddListener(ConfirmDrop);
        dropCancelButton.onClick.AddListener(CancelDrop);

        inventoryPanel.SetActive(false);
        dropConfirmPanel.SetActive(false);

        UpdateGoldDisplay(PlayerInventory.Instance.CurrentGold);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    private void InitializeSlots()
    {
        slots = new InventorySlot[PlayerInventory.Instance.MaxSlots];

        for (int i = 0; i < PlayerInventory.Instance.MaxSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            slot.Initialize(i, this);
            slots[i] = slot;
        }
    }

    public void ToggleInventory()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);
        inventoryImage.SetActive(isActive);

        if (isActive)
        {
            GameManager.Instance.PauseGame();
            RefreshInventory();
        }
        else
        {
            GameManager.Instance.ResumeGame();
        }
    }

    private void RefreshInventory()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < PlayerInventory.Instance.Items.Count)
            {
                slots[i].SetItem(PlayerInventory.Instance.Items[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }

    private void UpdateGoldDisplay(int gold)
    {
        goldText.text = $"Gold: {gold}";
    }

    public void ShowDropConfirmation(ItemData item)
    {
        itemToConfirmDrop = item;
        dropConfirmText.text = $"Drop {item.itemName}?";
        dropConfirmPanel.SetActive(true);
    }

    private void ConfirmDrop()
    {
        if (itemToConfirmDrop != null)
        {
            PlayerInventory.Instance.RemoveItem(itemToConfirmDrop);
            Debug.Log($"Dropped {itemToConfirmDrop.itemName}");
        }
        dropConfirmPanel.SetActive(false);
        itemToConfirmDrop = null;
    }

    private void CancelDrop()
    {
        dropConfirmPanel.SetActive(false);
        itemToConfirmDrop = null;
    }

    private void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryChangedEvent -= RefreshInventory;
            PlayerInventory.Instance.OnGoldChangedEvent -= UpdateGoldDisplay;
        }
    }
}