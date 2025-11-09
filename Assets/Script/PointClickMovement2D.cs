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
    public float attackCooldown = 1.2f;
    private float lastAttackTime = -999f;

    [Header("Damage")]
    [SerializeField] private int knightDamage = 3;   // NEW: damage knight bisa diatur
    [SerializeField] private int arrowDamage = 3;    // NEW: damage panah bisa diatur

    [Header("Clicking")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask enemyMask;

    [Header("Archer Settings")]
    [SerializeField] private GameObject arrowPrefab;  // prefab panah
    [SerializeField] private Transform shootOrigin;   // titik keluarnya panah
    [SerializeField] private GameObject aimPrefab;
    [SerializeField] private float minArrowRange = 3f;
    [SerializeField] private float maxArrowRange = 10f;
    [SerializeField] private float maxChargeTime = 1.0f; // waktu tahan untuk capai range maksimal
    private float chargeTimer = 0f;
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

    [SerializeField] private FourDirAnimator2D fourDir;
    [SerializeField] private Animator animator;

    void Awake()
    {
        fourDir = GetComponent<FourDirAnimator2D>();
        if (!animator) animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>(true);
    }

    void Update()
    {
        HandleClick();

        if (currentClass == PlayerClass.Archer)
            HandleArcherAim();

        if (isChasingEnemy)
            HandleChaseEnemy();
        else
            HandleMove();
    }

    void HandleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (currentClass == PlayerClass.Archer)
            {
                HandleGroundClick();
                return;
            }
            HandleEnemyOrGroundClick();
        }
    }

    //AIM
    void HandleArcherAim()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            chargeTimer = 0f;
            if (animator) animator.SetBool("IsShooting", true);
            if (aimPrefab != null && currentAimGO == null)
                currentAimGO = Instantiate(aimPrefab, transform.position, Quaternion.identity);
            // setelah aimDirection dihitung
            if (fourDir != null) fourDir.ForceFace(aimDirection); // biar muka mengikuti arah aim
            Debug.Log("IsShooting True" );

        }

        if (isAiming && Input.GetMouseButton(1))
        {
            chargeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(chargeTimer / maxChargeTime);
            float _ = Mathf.Lerp(minArrowRange, maxArrowRange, t); // hanya untuk indikator range

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dir = (mouseWorld - transform.position);
            dir.Normalize();
            aimDirection = SnapTo8Directions(dir);

            Vector3 offsetPos = (Vector2)transform.position + aimDirection * aimOffset;

            if (currentAimGO != null)
            {
                currentAimGO.transform.position = offsetPos;
                float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                currentAimGO.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            Debug.Log("Harusnya Nembak");
        }

        if (isAiming && Input.GetMouseButtonUp(1))
        {
            ShootArrow();
            isAiming = false;
            if (currentAimGO != null)
            {
                Destroy(currentAimGO);
                currentAimGO = null;
            }
            if (animator) animator.SetBool("IsShooting", false);
            Debug.Log("Harusnya Nembak Selesai");
        }

    }

    // ============ KNIGHT CLICK ===============
    void HandleEnemyOrGroundClick()
    {
        Vector3 m2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(m2.x, m2.y);

        Collider2D enemyCol = Physics2D.OverlapPoint(mousePos2D, enemyMask);
        if (enemyCol != null)
        {
            Enemy enemy = enemyCol.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                currentEnemyTarget = enemy;
                isChasingEnemy = true;

                Vector3Int enemyCell = tilemap.WorldToCell(enemy.transform.position);
                Vector3 enemyCenter = tilemap.GetCellCenterWorld(enemyCell);
                SetFourDirPath(enemyCenter);
                Debug.Log("Klik enemy: " + enemy.name);
            }
            return;
        }
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

            currentEnemyTarget = null;
            isChasingEnemy = false;
            animator.SetBool("IsAttacking", false);
        }
    }

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
            if (animator) animator.SetBool("IsAttacking", false);   // <— penting
            return;
        }

        float distToEnemy = Vector2.Distance(transform.position, currentEnemyTarget.transform.position);
        if (distToEnemy <= attackRange)
        {
            moveStage = 0;
            TryAttack();
            return;
        }

        // di luar jangkauan serang, pastikan attack dimatikan
        if (animator) animator.SetBool("IsAttacking", false);       // <— opsional tapi aman
        HandleMove();
    }


    void TryAttack()
    {
        if (currentEnemyTarget == null)
        {
            if (animator) animator.SetBool("IsAttacking", false);
            return;
        }
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;

        Vector2 dir = (currentEnemyTarget.transform.position - transform.position).normalized;
        if (fourDir != null) fourDir.ForceFace(dir);

        if (animator) animator.SetBool("IsAttacking", true);

        currentEnemyTarget.TakeDamage(3);

        // jika musuh mati di serangan ini, hentikan animasi
        if (currentEnemyTarget == null && animator)
            animator.SetBool("IsAttacking", false);

        Debug.Log("Player hit " + currentEnemyTarget?.name);
    }




    Vector2 SnapTo8Directions(Vector2 inputDir)
    {
        Vector2[] dirs = new Vector2[]
        {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right,
            new Vector2(1,1).normalized, new Vector2(1,-1).normalized,
            new Vector2(-1,1).normalized, new Vector2(-1,-1).normalized
        };

        float bestDot = -999f;
        Vector2 best = Vector2.up;
        foreach (Vector2 d in dirs)
        {
            float dot = Vector2.Dot(inputDir, d);
            if (dot > bestDot) { bestDot = dot; best = d; }
        }
        return best;
    }

    void ShootArrow()
    {
        if (arrowPrefab == null || shootOrigin == null) return;

        float t = Mathf.Clamp01(chargeTimer / maxChargeTime);
        float chargedRange = Mathf.Lerp(minArrowRange, maxArrowRange, t);

        GameObject arrow = Instantiate(arrowPrefab, shootOrigin.position, Quaternion.identity);
        ArrowProjectile ap = arrow.GetComponent<ArrowProjectile>();
        if (ap != null)
        {
            ap.damage = arrowDamage;                  // NEW: set damage panah dari sini
            ap.Launch(aimDirection, chargedRange);
        }

        chargeTimer = 0f;
        Debug.Log($"Arrow shot dir {aimDirection} range {chargedRange:0.0} dmg {arrowDamage}");
    }
}
