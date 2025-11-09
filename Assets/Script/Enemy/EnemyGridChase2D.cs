using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Enemy))]
public class EnemyGridChase2D : MonoBehaviour
{
    [Header("Tag Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string obstacleTag = "Obstacle";

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f; // ukuran satu grid

    private Transform player;
    private Enemy enemy;
    private bool isMoving = false;
    private float nextAttackTime = 0f;

    // Aggro state
    private bool isAggro = false; // false: idle, true: mengejar
    private int axisBias = 0;     // gonta-ganti prioritas sumbu biar gerak lebih natural

    void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) player = p.transform;

        transform.position = SnapToGrid(transform.position);
    }

    void Update()
    {
        if (!player || isMoving || enemy.currentHP <= 0 || enemy.stats == null) return;

        int atkRange = Mathf.Max(0, enemy.stats.attackRange);
        int triggerR = Mathf.Max(0, enemy.stats.chaseTriggerRange);
        int persistR = Mathf.Max(triggerR, enemy.stats.chasePersistRange); // jaga ≥ trigger

        Vector2Int myG = WorldToGrid(transform.position);
        Vector2Int plyG = WorldToGrid(player.position);
        int manhattan = Mathf.Abs(myG.x - plyG.x) + Mathf.Abs(myG.y - plyG.y);

        // selalu boleh nyerang kalau sudah cukup dekat
        if (manhattan <= atkRange)
        {
            TryAttack();
            return;
        }

        // state machine sederhana: Idle -> Aggro (trigger), Aggro -> Idle (keluar persist)
        if (!isAggro)
        {
            // Belum aggro: cek trigger
            if (manhattan <= triggerR)
                isAggro = true;
            else
                return; // tetap idle
        }
        else
        {
            // Sedang aggro: kalau terlalu jauh, lepas aggro
            if (manhattan > persistR)
            {
                isAggro = false;
                return;
            }
        }

        // Kalau di sini: sedang aggro → maju 1 sel mendekati player
        Vector2Int next = ChooseNextStep(myG, plyG);
        if (next != myG)
        {
            Vector3 targetPos = GridToWorld(next);
            float cellsPerSec = (enemy.stats ? enemy.stats.movementSpeed : 2f);
            float duration = (cellsPerSec > 0f) ? (1f / cellsPerSec) : 0.35f;
            StartCoroutine(MoveTo(targetPos, duration));
        }
    }

    IEnumerator MoveTo(Vector3 target, float duration)
    {
        isMoving = true;
        Vector3 start = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(start, target, Mathf.Clamp01(t));
            yield return null;
        }

        transform.position = target;
        isMoving = false;
        axisBias = 1 - axisBias; // tukar prioritas sumbu tiap langkah
    }

    void TryAttack()
    {
        float atkPerSec = (enemy.stats && enemy.stats.attackSpeed > 0f) ? enemy.stats.attackSpeed : 1f;
        float cd = 1f / atkPerSec;
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + cd;

        var dmgable = player.GetComponent<IDamageable>();
        if (dmgable != null)
        {
            int dmg = enemy.stats ? enemy.stats.damage : 1;
            dmgable.TakeDamage(dmg);
        }
    }

    Vector2Int WorldToGrid(Vector3 pos)
    {
        int gx = Mathf.RoundToInt(pos.x / cellSize);
        int gy = Mathf.RoundToInt(pos.y / cellSize);
        return new Vector2Int(gx, gy);
    }

    Vector3 GridToWorld(Vector2Int grid)
    {
        return new Vector3(grid.x * cellSize, grid.y * cellSize, 0f);
    }

    Vector3 SnapToGrid(Vector3 pos)
    {
        Vector2Int grid = WorldToGrid(pos);
        return GridToWorld(grid);
    }

    Vector2Int ChooseNextStep(Vector2Int from, Vector2Int to)
    {
        int dx = to.x - from.x;
        int dy = to.y - from.y;

        Vector2Int stepX = new Vector2Int(from.x + Mathf.Clamp(dx, -1, 1), from.y);
        Vector2Int stepY = new Vector2Int(from.x, from.y + Mathf.Clamp(dy, -1, 1));

        if (axisBias == 0)
        {
            if (dx != 0 && IsWalkable(stepX)) return stepX;
            if (dy != 0 && IsWalkable(stepY)) return stepY;
        }
        else
        {
            if (dy != 0 && IsWalkable(stepY)) return stepY;
            if (dx != 0 && IsWalkable(stepX)) return stepX;
        }

        if (dx != 0 && IsWalkable(stepX)) return stepX;
        if (dy != 0 && IsWalkable(stepY)) return stepY;

        return from;
    }

    bool IsWalkable(Vector2Int grid)
    {
        Vector3 world = GridToWorld(grid);
        Collider2D hit = Physics2D.OverlapCircle(world, cellSize * 0.3f);

        // ada collider bertag Obstacle → tidak bisa dilalui
        if (hit != null && hit.CompareTag(obstacleTag))
            return false;

        return true;
    }

    void OnDrawGizmosSelected()
    {
        if (enemy != null && enemy.stats != null)
        {
            float cs = cellSize <= 0f ? 1f : cellSize;

            // Trigger (kecil)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemy.stats.chaseTriggerRange * cs);

            // Persist (lebih besar)
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
            Gizmos.DrawWireSphere(transform.position, enemy.stats.chasePersistRange * cs);

            // Attack
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemy.stats.attackRange * cs);
        }
    }
}
