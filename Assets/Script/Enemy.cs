using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Assign Stats Asset")]
    public EnemyStats stats;   // ← ini yang nanti diisi SlimeStats / GoblinStats / WolfStats

    [HideInInspector] public int currentHP;

    void Awake()
    {
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
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
