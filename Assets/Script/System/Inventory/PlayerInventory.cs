using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [SerializeField] private int maxSlots = 20;
    [SerializeField] private int startingGold = 100;

    private List<ItemData> items = new List<ItemData>();
    private int currentGold;

    public int CurrentGold => currentGold;
    public List<ItemData> Items => items;
    public int MaxSlots => maxSlots;

    public delegate void OnInventoryChanged();
    public event OnInventoryChanged OnInventoryChangedEvent;

    public delegate void OnGoldChanged(int newGold);
    public event OnGoldChanged OnGoldChangedEvent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        currentGold = startingGold;
    }

    public bool AddItem(ItemData item)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("Inventory is full!");
            return false;
        }

        items.Add(item);
        OnInventoryChangedEvent?.Invoke();
        return true;
    }

    public bool RemoveItem(ItemData item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            OnInventoryChangedEvent?.Invoke();
            return true;
        }
        return false;
    }

    public void RemoveItemAt(int index)
    {
        if (index >= 0 && index < items.Count)
        {
            items.RemoveAt(index);
            OnInventoryChangedEvent?.Invoke();
        }
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        OnGoldChangedEvent?.Invoke(currentGold);
    }

    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            OnGoldChangedEvent?.Invoke(currentGold);
            return true;
        }
        return false;
    }

    public ItemData GetItemAt(int index)
    {
        if (index >= 0 && index < items.Count)
        {
            return items[index];
        }
        return null;
    }
}