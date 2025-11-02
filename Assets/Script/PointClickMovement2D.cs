using UnityEngine;
using UnityEngine.Tilemaps;

public class PointClickMovement4Dir : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float stopDistance = 0.05f;

    [Header("Combat")]
    public float attackRange = 0.6f;      // jarak berhenti ke musuh
    public float attackCooldown = 0.6f;   // biar ga mukul tiap frame
    private float lastAttackTime = -999f;

    [Header("Clicking")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask enemyMask;

    // 4 arah
    private Vector2 stage1Target;
    private Vector2 stage2Target;
    private int moveStage = 0; // 0 = diam, 1 = ke stage1, 2 = ke stage2

    // combat
    private Enemy currentEnemyTarget = null;
    private bool isChasingEnemy = false;

    void Update()
    {
        HandleClick();

        if (isChasingEnemy)
        {
            HandleChaseEnemy();
        }
        else
        {
            HandleMove();   // movement biasa (klik tanah)
        }
    }

    void HandleClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 m2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(m2.x, m2.y);

        // 1) cek enemy dulu (PAKE IN PARENT ya)
        Collider2D enemyCol = Physics2D.OverlapPoint(mousePos2D, enemyMask);
        if (enemyCol != null)
        {
            Enemy enemy = enemyCol.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                currentEnemyTarget = enemy;
                isChasingEnemy = true;

                // kita langsung set tujuan ke tile tempat musuh berdiri
                Vector3Int enemyCell = tilemap.WorldToCell(enemy.transform.position);
                Vector3 enemyCenter = tilemap.GetCellCenterWorld(enemyCell);
                Vector2 finalTarget = new Vector2(enemyCenter.x, enemyCenter.y);

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

                // cuma buat ngecek
                Debug.Log("Klik enemy: " + enemy.name);
            }
            return; // penting: jangan lanjut ke tanah
        }

        // 2) kalau bukan enemy → jalan biasa ke tile
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero, 0f, groundMask);
        if (hit.collider != null)
        {
            // klik tanah = batalin chase
            currentEnemyTarget = null;
            isChasingEnemy = false;

            Vector3Int cellPos = tilemap.WorldToCell(hit.point);
            Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPos);
            Vector2 finalTarget = new Vector2(cellCenter.x, cellCenter.y);

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
    }

    // gerak biasa (klik tanah)
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

    // gerak khusus ke musuh
    void HandleChaseEnemy()
    {
        // kalau musuh sudah hilang / mati → balik ke mode biasa
        if (currentEnemyTarget == null)
        {
            isChasingEnemy = false;
            moveStage = 0;
            return;
        }

        // cek jarak ke musuh dulu
        float distToEnemy = Vector2.Distance(transform.position, currentEnemyTarget.transform.position);
        if (distToEnemy <= attackRange)
        {
            // udah nyampe jarak pukul → berhenti dan serang
            moveStage = 0;
            TryAttack();
            return;
        }

        // kalau belum nyampe → lanjutkan gerak 4 arah yang tadi kita set
        HandleMove();
    }

    void TryAttack()
    {
        if (currentEnemyTarget == null) return;

        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;
        currentEnemyTarget.TakeDamage(3);
        Debug.Log("Player hit " + currentEnemyTarget.name);
    }
}
