using SmallHedge.SoundManager;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Assign Stats Asset")]
    public EnemyStats stats;   // ← ini yang nanti diisi SlimeStats / GoblinStats / WolfStats
    private InventoryController inventoryController;
    [HideInInspector] public int currentHP;

    void Awake()
    {
        inventoryController = FindAnyObjectByType<InventoryController>();
        if (stats == null)
        {
            Debug.LogWarning($"Enemy '{name}' belum punya EnemyStats. Pakai fallback health=10.");
            currentHP = 10;
        }
        else
        {
            currentHP = stats.health;
        }
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        SoundManager.PlaySound(SoundType.EnemyHurt);
        if (currentHP <= 0)
        {
            SoundManager.PlaySound(SoundType.EnemyDead);
            inventoryController.AddGold(10);
            if (DropManager.Instance != null)
                DropManager.Instance.DropItems(transform.position);
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
