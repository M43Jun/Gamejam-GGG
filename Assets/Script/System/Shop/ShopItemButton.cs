using UnityEngine;
using UnityEngine.UI;

public class ShopItemButton : MonoBehaviour
{
    [Header("UI Elements")]
    public Image itemIcon;
    public TMPro.TextMeshProUGUI itemNameText;
    public TMPro.TextMeshProUGUI priceText;
    public TMPro.TextMeshProUGUI statsText;
    public Button actionButton;

    private ItemData itemData;
    private bool isBuyMode;
    private ShopController shopController;

    public void Setup(ItemData item, bool buyMode, ShopController controller)
    {
        itemData = item;
        isBuyMode = buyMode;
        shopController = controller;

        // Set icon
        if (itemIcon != null && item.icon != null)
        {
            itemIcon.sprite = item.icon;
        }

        // Set name
        if (itemNameText != null)
        {
            itemNameText.text = item.itemName;
        }

        // Set price
        if (priceText != null)
        {
            if (isBuyMode)
            {
                priceText.text = $"Buy: {item.buyPrice}G";
            }
            else
            {
                priceText.text = $"Sell: {item.sellPrice}G";
            }
        }

        // Set stats
        if (statsText != null)
        {
            string stats = "";
            if (item.stats.healthRestore > 0)
                stats += $"HP +{item.stats.healthRestore} ";
            if (item.stats.damageBoost > 0)
                stats += $"DMG +{item.stats.damageBoost} ";
            if (item.stats.defenseBoost > 0)
                stats += $"DEF +{item.stats.defenseBoost} ";

            statsText.text = stats;
        }

        // Setup button
        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();

            if (isBuyMode)
            {
                actionButton.onClick.AddListener(() => shopController.BuyItem(itemData));
            }
            else
            {
                actionButton.onClick.AddListener(() => shopController.ShowSellConfirmation(itemData));
            }
        }
    }
}