using UnityEngine;

[System.Serializable]
public class ItemStats
{
    public int healthRestore;
    public int damageBoost;
    public int defenseBoost;
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public int id;
    public string itemName;
    public Sprite icon;
    public int buyPrice;
    public int sellPrice;
    public ItemStats stats;
    public string description;
}