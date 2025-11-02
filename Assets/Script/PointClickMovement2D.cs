

using UnityEngine;
using UnityEngine.Tilemaps;

public enum PlayerClass
{
    Knight,
    Archer
}
public class PointClickMovement4Dir : MonoBehaviour
{
    public PlayerClass currentClass = PlayerClass.Knight;   // <<< ganti dari inspector

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float stopDistance = 0.05f;

    [Header("Combat")]
    public float attackRange = 0.6f;
    public float attackCooldown = 0.6f;
    private float lastAttackTime = -999f;

    [Header("Clicking")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask enemyMask;

    [Header("Archer Settings")]
    [SerializeField] private GameObject arrowPrefab;  // prefab panah
    [SerializeField] private Transform shootOrigin;   // titik keluarnya panah
    [SerializeField] private GameObject aimPrefab;
    private bool isAiming = false;
    private Vector2 aimDirection;
    private GameObject currentAimGO;   // instance yang lagi nongol
    [SerializeField] private float aimOffset = 0.7f;  // jarak dari player

    // 4 arah
    private Vector2 stage1Target;
    private Vector2 stage2Target;
    private int moveStage = 0;

    // combat
    private Enemy currentEnemyTarget = null;
    private bool isChasingEnemy = false;

    void Update()
    {
        HandleClick();

        if (currentClass == PlayerClass.Archer)
            HandleArcherAim();   // <--- tambahan ini

        if (isChasingEnemy)
            HandleChaseEnemy();
        else
            HandleMove();

        // nanti di sini kita taruh HandleArcherAim();
    }

    void HandleClick()
    {
        // kalau archer → klik kiri JANGAN lock enemy
        if (Input.GetMouseButtonDown(0))
        {
            if (currentClass == PlayerClass.Archer)
            {
                // archer: klik kiri = cuma jalan ke tile biasa
                HandleGroundClick();
                return;
            }

            // knight: boleh klik enemy
            HandleEnemyOrGroundClick();
        }

        // klik kanan bakal kita pakai buat archer aim di langkah berikutnya
    }

    //AIM
    void HandleArcherAim()
    {
        // mulai aim
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;

            // spawn prefab aim
            if (aimPrefab != null && currentAimGO == null)
            {
                currentAimGO = Instantiate(aimPrefab, transform.position, Quaternion.identity);
            }
        }

        // tahan buat ngarahin
        if (isAiming && Input.GetMouseButton(1))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = (mouseWorld - transform.position);
            dir.Normalize();

            // snap ke 8 arah
            aimDirection = SnapTo8Directions(dir);

            // hitung posisi offset dari player
            Vector3 offsetPos = (Vector2)transform.position + aimDirection * aimOffset;

            if (currentAimGO != null)
            {
                currentAimGO.transform.position = offsetPos;

                // rotasi supaya ngadep ke arah aim
                float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                currentAimGO.transform.rotation = Quaternion.Euler(0, 0, angle );
            }
        }

        // lepas → tembak
        if (isAiming && Input.GetMouseButtonUp(1))
        {
            ShootArrow();

            isAiming = false;

            // hapus prefab aim
            if (currentAimGO != null)
            {
                Destroy(currentAimGO);
                currentAimGO = null;
            }
        }
    }


    // ============ KNIGHT CLICK ===============
    void HandleEnemyOrGroundClick()
    {
        Vector3 m2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(m2.x, m2.y);

        // cek enemy dulu
        Collider2D enemyCol = Physics2D.OverlapPoint(mousePos2D, enemyMask);
        if (enemyCol != null)
        {
            Enemy enemy = enemyCol.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                currentEnemyTarget = enemy;
                isChasingEnemy = true;

                // bikin path 4 arah ke musuh
                Vector3Int enemyCell = tilemap.WorldToCell(enemy.transform.position);
                Vector3 enemyCenter = tilemap.GetCellCenterWorld(enemyCell);
                SetFourDirPath(enemyCenter);

                Debug.Log("Klik enemy: " + enemy.name);
            }
            return;
        }

        // kalau bukan enemy → klik tanah biasa
        HandleGroundClick();
    }

    // ============ GROUND CLICK (untuk dua class) ===============
    void HandleGroundClick()
    {
        Vector3 m2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(m2.x, m2.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero, 0f, groundMask);
        if (hit.collider != null)
        {
            Vector3Int cellPos = tilemap.WorldToCell(hit.point);
            Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPos);
            SetFourDirPath(cellCenter);

            // klik tanah = batalin chase
            currentEnemyTarget = null;
            isChasingEnemy = false;
        }
    }

    // bikin path 4 arah ke titik tertentu
    void SetFourDirPath(Vector3 worldTarget)
    {
        Vector2 finalTarget = new Vector2(worldTarget.x, worldTarget.y);
        Vector2 current = transform.position;
        float dx = Mathf.Abs(finalTarget.x - current.x);
        float dy = Mathf.Abs(finalTarget.y - current.y);

        if (dx >= dy)
        {
            stage1Target = new Vector2(finalTarget.x, current.y);
            stage2Target = finalTarget;
        }
        else
        {
            stage1Target = new Vector2(current.x, finalTarget.y);
            stage2Target = finalTarget;
        }

        moveStage = 1;
    }

    // ============ movement biasa ===============
    void HandleMove()
    {
        if (moveStage == 0) return;

        Vector2 current = transform.position;
        Vector2 target = (moveStage == 1) ? stage1Target : stage2Target;

        float dist = Vector2.Distance(current, target);
        if (dist <= stopDistance)
        {
            if (moveStage == 1)
                moveStage = 2;
            else
                moveStage = 0;
            return;
        }

        Vector2 newPos = Vector2.MoveTowards(current, target, moveSpeed * Time.deltaTime);
        transform.position = newPos;
    }

    // ============ chase ke musuh (knight) ===============
    void HandleChaseEnemy()
    {
        if (currentEnemyTarget == null)
        {
            isChasingEnemy = false;
            moveStage = 0;
            return;
        }

        float distToEnemy = Vector2.Distance(transform.position, currentEnemyTarget.transform.position);
        if (distToEnemy <= attackRange)
        {
            moveStage = 0;
            TryAttack();
            return;
        }

        HandleMove();
    }

    void TryAttack()
    {
        if (currentEnemyTarget == null) return;
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        currentEnemyTarget.TakeDamage(3);
        Debug.Log("Player hit " + currentEnemyTarget.name);
    }

    Vector2 SnapTo8Directions(Vector2 inputDir)
    {
        Vector2[] dirs = new Vector2[]
        {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right,
        new Vector2(1,1).normalized,
        new Vector2(1,-1).normalized,
        new Vector2(-1,1).normalized,
        new Vector2(-1,-1).normalized
        };

        float bestDot = -999f;
        Vector2 best = Vector2.up;
        foreach (Vector2 d in dirs)
        {
            float dot = Vector2.Dot(inputDir, d);
            if (dot > bestDot)
            {
                bestDot = dot;
                best = d;
            }
        }
        return best;
    }

    void ShootArrow()
    {
        if (arrowPrefab == null) return;

        GameObject arrow = Instantiate(arrowPrefab, shootOrigin.position, Quaternion.identity);
        ArrowProjectile ap = arrow.GetComponent<ArrowProjectile>();
        ap.Launch(aimDirection);
        Debug.Log("Arrow shot toward " + aimDirection);
    }


}
