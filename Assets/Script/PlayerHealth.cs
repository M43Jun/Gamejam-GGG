using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Player HP")]
    public int maxHP = 20;
    public int currentHP;

    void Awake() => currentHP = maxHP;

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log($"Player took {amount} dmg → {currentHP}/{maxHP}");
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        Debug.Log("Player died");
        // TODO: matikan input / respawn / game over
    }
}
