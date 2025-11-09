using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject buyPanel;
    [SerializeField] private GameObject sellPanel;
    [SerializeField] private GameObject sellectionPanel;

    [Header("UI Buttons")]
    [SerializeField] private Button buyTabButton;
    [SerializeField] private Button sellTabButton;
    [SerializeField] private Button closeButton;

    [Header("Content Containers")]
    [SerializeField] private Transform buyContentContainer;
    [SerializeField] private Transform sellContentContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private GameObject sellItemPrefab;

    [Header("Gold Display")]
    [SerializeField] private TextMeshProUGUI shopGoldText;

    [Header("Sell Confirmation")]
    [SerializeField] private GameObject sellConfirmPanel;
    [SerializeField] private TextMeshProUGUI sellConfirmText;
    [SerializeField] private Button sellConfirmButton;
    [SerializeField] private Button sellCancelButton;

    private ShopController currentShop;
    private ItemData itemToSell;

    private void Start()
    {
        buyTabButton.onClick.AddListener(() => ShowTab(true));
        sellTabButton.onClick.AddListener(() => ShowTab(false));
        closeButton.onClick.AddListener(CloseShop);

        sellConfirmButton.onClick.AddListener(ConfirmSell);
        sellCancelButton.onClick.AddListener(CancelSell);

        shopPanel.SetActive(false);
        sellConfirmPanel.SetActive(false);

        PlayerInventory.Instance.OnGoldChangedEvent += UpdateGoldDisplay;
        PlayerInventory.Instance.OnInventoryChangedEvent += RefreshSellPanel;
    }

    public void OpenShop(ShopController shop)
    {
        currentShop = shop;
        shopPanel.SetActive(true);
        GameManager.Instance.PauseGame();
        sellectionPanel.SetActive(true);
        ShowTab(true); // Show buy tab by default
        UpdateGoldDisplay(PlayerInventory.Instance.CurrentGold);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        GameManager.Instance.ResumeGame();
        currentShop = null;
    }

    private void ShowTab(bool isBuyTab)
    {
        buyPanel.SetActive(isBuyTab);
        sellPanel.SetActive(!isBuyTab);

        if (isBuyTab)
        {
            PopulateBuyPanel();
            sellectionPanel.SetActive(false);

        }
        else
        {
            PopulateSellPanel();
            sellectionPanel.SetActive(false);
        }
    }

    private void PopulateBuyPanel()
    {
        // Clear existing items
        foreach (Transform child in buyContentContainer)
        {
            Destroy(child.gameObject);
        }

        // Create shop item slots
        if (currentShop != null)
        {

            GameObject itemObj = Instantiate(shopItemPrefab, buyContentContainer);
            ShopItemSlot slot = itemObj.GetComponent<ShopItemSlot>();
            if (slot != null)
            {
                //slot.Setup(item, this, true);
            }

        }
    }

    private void PopulateSellPanel()
    {
        // Clear existing items
        foreach (Transform child in sellContentContainer)
        {
            Destroy(child.gameObject);
        }

        // Create sell item slots from inventory
        foreach (ItemData item in PlayerInventory.Instance.Items)
        {
            GameObject itemObj = Instantiate(sellItemPrefab, sellContentContainer);
            ShopItemSlot slot = itemObj.GetComponent<ShopItemSlot>();
            if (slot != null)
            {
                //slot.Setup(item, this, false);
            }
        }
    }

    private void RefreshSellPanel()
    {
        if (sellPanel.activeSelf)
        {
            PopulateSellPanel();
        }
    }

    public void BuyItem(ItemData item)
    {
        if (currentShop != null)
        {

        }
    }

    public void ShowSellConfirmation(ItemData item)
    {
        itemToSell = item;
        sellConfirmText.text = $"Sell {item.itemName} for {item.sellPrice} gold?";
        sellConfirmPanel.SetActive(true);
    }

    private void ConfirmSell()
    {
        if (itemToSell != null && currentShop != null)
        {
        }
        sellConfirmPanel.SetActive(false);
        itemToSell = null;
    }

    private void CancelSell()
    {
        sellConfirmPanel.SetActive(false);
        itemToSell = null;
    }

    private void UpdateGoldDisplay(int gold)
    {
        shopGoldText.text = $"Gold: {gold}";
    }

    private void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnGoldChangedEvent -= UpdateGoldDisplay;
            PlayerInventory.Instance.OnInventoryChangedEvent -= RefreshSellPanel;
        }
    }
}