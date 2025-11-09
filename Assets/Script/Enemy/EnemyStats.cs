using UnityEngine;

[CreateAssetMenu(menuName = "GGG/Enemy Stats", fileName = "EnemyStats")]
public class EnemyStats : ScriptableObject
{
    [Header("Core Stats")]
    public int health = 10;
    public float movementSpeed = 2f;    // cells per second
    public float attackSpeed = 1f;      // serangan per detik
    public int damage = 2;

    [Header("Ranges (grid/cells)")]
    public int attackRange = 1;         // jarak pukul/serang
    public int chaseTriggerRange = 6;   // jarak untuk MULAI ngejar
    public int chasePersistRange = 10;  // jarak saat SUDAH ngejar (lebih luas)
}
