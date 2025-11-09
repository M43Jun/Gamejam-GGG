using SmallHedge.SoundManager;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Player Stats")]
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;

    [Header("Flash Settings")]
    public SpriteRenderer spriteRenderer;
    public float flashDuration = 0.1f;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;

    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = Color.white;
        }
    }

    public void Die()
    {
        isDead = true;
        SoundManager.PlaySound(SoundType.PlayerDead);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // optional: disable movement
        var move = GetComponent<PointClickMovement4Dir>();
        if (move != null) move.enabled = false;

        Debug.Log("Game Over!");
    }
}
