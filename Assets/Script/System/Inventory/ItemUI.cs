using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemUI : MonoBehaviour
{
    public Image icon;
    //public TextMeshProUGUI countText;

    private ItemData itemData;

    public void Setup(ItemData data)
    {
        itemData = data;
        icon.sprite = data.icon;
        icon.enabled = true;
        //if (countText != null)
        //    countText.text = data.stackCount > 1 ? data.stackCount.ToString() : "";
    }

    public ItemData GetItemData()
    {
        return itemData;
    }
}
