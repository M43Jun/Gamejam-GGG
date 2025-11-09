using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropEntry
{
    public ItemData itemData;       // Referensi ke ScriptableObject item
    [Range(0, 100)]
    public float dropChance = 50f;  // Persentase peluang drop (0-100)
    public GameObject itemPrefab;   // Prefab fisik item yang muncul di dunia
}

public class DropManager : MonoBehaviour
{
    public static DropManager Instance;

    [Header("Daftar Item Drop")]
    public List<DropEntry> dropTable = new List<DropEntry>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void DropItems(Vector3 position)
    {
        foreach (var entry in dropTable)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= entry.dropChance)
            {
                if (entry.itemPrefab != null)
                {
                    GameObject droppedItem = Instantiate(entry.itemPrefab, position, Quaternion.identity);
                    Item itemComponent = droppedItem.GetComponent<Item>();
                    if (itemComponent != null)
                        itemComponent.itemData = entry.itemData;
                }
                else
                {
                    Debug.LogWarning($"Item prefab untuk {entry.itemData?.itemName} belum di-assign!");
                }
            }
        }
    }
}
