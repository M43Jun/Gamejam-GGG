using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallHedge.SoundManager;

public class ShopController : MonoBehaviour
{
    [Header("Shop Settings")]
    public List<ItemData> shopItems = new List<ItemData>(); // Item yang dijual di shop
    public float interactionRange = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Panels")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject buyPanel;
    [SerializeField] private GameObject sellPanel;
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private GameObject sellConfirmPanel;

    [Header("UI Buttons")]
    [SerializeField] private Button buyTabButton;
    [SerializeField] private Button sellTabButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button sellConfirmButton;
    [SerializeField] private Button sellCancelButton;

    [Header("Content Containers")]
    [SerializeField] private Transform buyContentContainer;
    [SerializeField] private Transform sellContentContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject shopItemPrefab;

    [Header("Gold Display")]
    [SerializeField] private TextMeshProUGUI goldText;

    [Header("Sell Confirmation")]
    [SerializeField] private TextMeshProUGUI sellConfirmText;

    private Transform playerTransform;
    private bool playerInRange = false;
    private ItemData itemToSell;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Setup buttons
        buyTabButton.onClick.AddListener(() => ShowTab(true));
        sellTabButton.onClick.AddListener(() => ShowTab(false));
        closeButton.onClick.AddListener(CloseShop);
        sellConfirmButton.onClick.AddListener(ConfirmSell);
        sellCancelButton.onClick.AddListener(CancelSell);

        shopPanel.SetActive(false);
        sellConfirmPanel.SetActive(false);

        // Subscribe ke event inventory (kalau ada sistem InventoryController)
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.OnGoldUpdated += UpdateGoldDisplay;
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        playerInRange = distance <= interactionRange;

        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (!shopPanel.activeSelf)
                OpenShop();
        }
    }

    // === SHOP UI HANDLERS ===
    private void OpenShop()
    {
        shopPanel.SetActive(true);
        Time.timeScale = 0f;
        selectionPanel.SetActive(true);
        //ShowTab(true);
        UpdateGoldDisplay(InventoryController.Instance.currentGold);
    }

    public void CloseShop()
    {
        PlaySoundClick();
        shopPanel.SetActive(false);
        selectionPanel.SetActive(false);
        buyPanel.SetActive(false);
        sellPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ShowTab(bool isBuyTab)
    {
        buyPanel.SetActive(isBuyTab);
        sellPanel.SetActive(!isBuyTab);
        selectionPanel.SetActive(false);

        if (isBuyTab)
        {
            PopulateBuyPanel();
            PlaySoundClick();
        }

        else
        {
            PopulateSellPanel();
            PlaySoundClick();

        }
    }

    // === POPULATE PANELS ===
    private void PopulateBuyPanel()
    {
        foreach (Transform child in buyContentContainer)
            Destroy(child.gameObject);

        foreach (ItemData item in shopItems)
        {
            GameObject itemObj = Instantiate(shopItemPrefab, buyContentContainer);
            ShopItemSlot slot = itemObj.GetComponent<ShopItemSlot>();
            if (slot != null)
                slot.Setup(item, this, true);
        }
    }

    private void PopulateSellPanel()
    {
        foreach (Transform child in sellContentContainer)
            Destroy(child.gameObject);

        if (InventoryController.Instance == null) return;

        foreach (ItemData item in InventoryController.Instance.GetAllItems())
        {
            GameObject itemObj = Instantiate(shopItemPrefab, sellContentContainer);
            ShopItemSlot slot = itemObj.GetComponent<ShopItemSlot>();
            if (slot != null)
                slot.Setup(item, this, false);
        }
    }

    // === TRANSACTIONS ===
    public void BuyItem(ItemData item)
    {
        if (InventoryController.Instance.SpendGold(item.buyPrice))
        {
            SoundManager.PlaySound(SoundType.ItemSell);
            if (InventoryController.Instance.AddItemData(item))
                Debug.Log($"Bought {item.itemName} for {item.buyPrice}G");
            else
                InventoryController.Instance.AddGold(item.buyPrice); // Refund jika penuh
        }
        else
        {
            Debug.Log("Not enough gold!");
        }

        UpdateGoldDisplay(InventoryController.Instance.currentGold);
    }

    public void ShowSellConfirmation(ItemData item)
    {
        itemToSell = item;
        sellConfirmText.text = $"Sell {item.itemName} for {item.sellPrice} gold?";
        sellConfirmPanel.SetActive(true);
    }

    private void ConfirmSell()
    {
        PlaySoundClick();
        if (itemToSell != null && InventoryController.Instance.RemoveItem(itemToSell))
        {
            InventoryController.Instance.AddGold(itemToSell.sellPrice);
            SoundManager.PlaySound(SoundType.ItemSell);
            PopulateSellPanel();
        }

        sellConfirmPanel.SetActive(false);
        itemToSell = null;
    }

    private void CancelSell()
    {
        PlaySoundClick();
        sellConfirmPanel.SetActive(false);
        itemToSell = null;
    }

    private void UpdateGoldDisplay(int gold)
    {
        goldText.text = $"Gold: {gold}";
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    public void PlaySoundClick()
    {
        SoundManager.PlaySound(SoundType.CLickSound);
    }
}
