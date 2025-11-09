using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemSlot : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    private ItemData itemData;
    private ShopController shopUI;
    private bool isBuyMode;

    public void Setup(ItemData item, ShopController ui, bool buyMode)
    {
        itemData = item;
        shopUI = ui;
        isBuyMode = buyMode;

        itemIcon.sprite = item.icon;
        itemNameText.text = item.itemName;

        if (isBuyMode)
        {
            priceText.text = $"Buy: {item.buyPrice}G";
            buttonText.text = "Buy";
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => shopUI.BuyItem(itemData));
        }
        else
        {
            priceText.text = $"Sell: {item.sellPrice}G";
            buttonText.text = "Sell";
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => shopUI.ShowSellConfirmation(itemData));
        }

        // Display stats
        string stats = "";
        if (item.stats.healthRestore > 0)
            stats += $"HP +{item.stats.healthRestore}\n";
        if (item.stats.damageBoost > 0)
            stats += $"DMG +{item.stats.damageBoost}\n";
        if (item.stats.defenseBoost > 0)
            stats += $"DEF +{item.stats.defenseBoost}\n";
        //if (item.stats.speedBoost > 0)
        //    stats += $"SPD +{item.stats.speedBoost}%\n";

        if (statsText != null)
        {
            statsText.text = stats.TrimEnd('\n');
        }
    }
}