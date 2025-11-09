using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Enemy))]
public class EnemyGridChase2D : MonoBehaviour
{
    [Header("Tags")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string obstacleTag = "Obstacle";

    [Header("Grid")]
    [SerializeField] private float cellSize = 1f;              // ukuran satu grid (umumnya 1)
    [SerializeField] private float minStepDuration = 0.15f;    // durasi minimum 1 langkah (anti teleport)
    [SerializeField] private float maxStepDuration = 0.5f;     // durasi maksimum 1 langkah (opsional)
    [SerializeField]
    private AnimationCurve stepCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);                  // easing biar halus (opsional)

    [Header("Animator Smoothing")]
    [SerializeField] private float animMoveHold = 0.08f;       // tahan anim Walk di jeda antar langkah

    // --- runtime ---
    private Transform player;
    private Enemy enemy;

    private bool isMoving = false;          // status fisik sedang melangkah
    private float nextAttackTime = 0f;      // cooldown serangan
    private int axisBias = 0;               // swap prioritas X/Y tiap langkah

    // sinyal buat Animator (halus)
    private float animMoveHoldTimer = 0f;
    public bool AnimIsMoving { get; private set; } = false;

    // arah lihat/gerak terakhir (buat pilih clip & flip side)
    public Vector2 CurrentDir { get; private set; } = Vector2.down;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) player = p.transform;

        // snap posisi awal ke grid center
        transform.position = SnapToGrid(transform.position);
    }

    void Update()
    {
        if (enemy == null || enemy.currentHP <= 0 || enemy.stats == null) { SmoothAnimator(false); return; }
        if (player == null) { SmoothAnimator(false); return; }

        // kalau lagi melangkah, jangan rencanakan langkah baru
        if (isMoving) { SmoothAnimator(true); return; }

        // ambil range dari stats
        int atkRange = Mathf.Max(0, enemy.stats.attackRange);
        int triggerR = Mathf.Max(0, enemy.stats.chaseTriggerRange);
        int persistR = Mathf.Max(triggerR, enemy.stats.chasePersistRange);

        // jarak Manhattan di grid
        Vector2Int myG = WorldToGrid(transform.position);
        Vector2Int plG = WorldToGrid(player.position);
        int manhattan = Mathf.Abs(myG.x - plG.x) + Mathf.Abs(myG.y - plG.y);

        // serang jika cukup dekat
        if (manhattan <= atkRange)
        {
            TryAttack();
            SmoothAnimator(false); // diam di tempat saat nyerang (kalau mau tetap walk, set true)
            return;
        }

        // state aggro sederhana (trigger & persist)
        bool shouldAggro =
            (AnimIsMoving || manhattan <= triggerR) &&   // sudah ngejar atau masuk trigger
            (manhattan <= persistR);                     // tidak keluar dari persist ring

        if (!shouldAggro)
        {
            hasAggroed = false;
            SmoothAnimator(false);
            return; // idle
        }

        // pilih 1 sel menuju player (prioritas sumbu bergantian)
        Vector2Int next = ChooseNextStep(myG, plG);
        if (next == myG)
        {
            // buntu / ketahan obstacle → tetap anggap jalan sebentar biar anim gak flicker
            SmoothAnimator(false);
            return;
        }

        Vector3 targetPos = GridToWorld(next);

        // speed (cell/s) dari stats → durasi langkah
        float cps = (enemy.stats.movementSpeed > 0f) ? enemy.stats.movementSpeed : 0.01f;
        float duration = cellSize / cps;
        duration = Mathf.Clamp(duration, minStepDuration, maxStepDuration);

        StartCoroutine(MoveTo(targetPos, duration));
        SmoothAnimator(true);
    }

    IEnumerator MoveTo(Vector3 target, float duration)
    {
        isMoving = true;

        Vector3 start = transform.position;
        Vector3 rawDir = (target - start);
        if (rawDir.sqrMagnitude > 0.0001f)
            CurrentDir = ((Vector2)rawDir).normalized;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // easing
            t = stepCurve != null ? stepCurve.Evaluate(t) : t;
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
        isMoving = false;

        // ganti prioritas sumbu tiap langkah biar pola gak kaku
        axisBias = 1 - axisBias;
    }

    [SerializeField] private float firstAttackDelay = 0.8f; // jeda sebelum serangan pertama
    private bool hasAggroed = false;

    void TryAttack()
    {
        // waktu cooldown serangan (berdasarkan attackSpeed)
        float atkPerSec = (enemy.stats.attackSpeed > 0f) ? enemy.stats.attackSpeed : 1f;
        float cd = 1f / atkPerSec;

        // jika baru pertama kali masuk mode serang, kasih jeda awal
        if (!hasAggroed)
        {
            nextAttackTime = Time.time + firstAttackDelay;
            hasAggroed = true;
        }

        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + cd;

        var dmgable = player.GetComponent<IDamageable>();
        if (dmgable != null)
        {
            int dmg = enemy.stats.damage > 0 ? enemy.stats.damage : 1;
            dmgable.TakeDamage(dmg);
        }
    }

    // ---------------- util grid & path 4-arah ----------------

    Vector2Int WorldToGrid(Vector3 pos)
    {
        int gx = Mathf.RoundToInt(pos.x / cellSize);
        int gy = Mathf.RoundToInt(pos.y / cellSize);
        return new Vector2Int(gx, gy);
    }

    Vector3 GridToWorld(Vector2Int g)
    {
        return new Vector3(g.x * cellSize, g.y * cellSize, 0f);
    }

    Vector3 SnapToGrid(Vector3 world)
    {
        return GridToWorld(WorldToGrid(world));
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

        // fallback
        if (dx != 0 && IsWalkable(stepX)) return stepX;
        if (dy != 0 && IsWalkable(stepY)) return stepY;

        return from; // buntu
    }

    bool IsWalkable(Vector2Int grid)
    {
        // cek ada obstacle bertag "Obstacle" di pusat sel
        Vector3 center = GridToWorld(grid);
        // radius 0.3 * cellSize cukup ketat, sesuaikan collider obstacle kamu
        Collider2D hit = Physics2D.OverlapCircle(center, cellSize * 0.3f);
        if (hit != null && hit.CompareTag(obstacleTag)) return false;
        return true;
    }

    // --------------- animator signal smoothing ----------------

    private void SmoothAnimator(bool intendToMove)
    {
        // kalau sedang melangkah, atau akan segera melangkah → set true & reset hold
        if (isMoving || intendToMove)
        {
            AnimIsMoving = true;
            animMoveHoldTimer = animMoveHold;
            return;
        }

        // tidak sedang melangkah: tahan sinyal walk sedikit supaya tidak flicker
        if (animMoveHoldTimer > 0f)
        {
            animMoveHoldTimer -= Time.deltaTime;
            AnimIsMoving = true;
        }
        else
        {
            AnimIsMoving = false;
        }
    }

    // ----------------- debug gizmos -----------------
    void OnDrawGizmosSelected()
    {
        if (enemy != null && enemy.stats != null)
        {
            float cs = (cellSize <= 0f) ? 1f : cellSize;

            // trigger ring
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemy.stats.chaseTriggerRange * cs);

            // persist ring
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
            Gizmos.DrawWireSphere(transform.position, enemy.stats.chasePersistRange * cs);

            // attack ring
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemy.stats.attackRange * cs);
        }
    }
}
