using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyAIChase : MonoBehaviour
{
    [SerializeField] private Transform player;   // auto cari by Tag kalau kosong
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float attackRange = 0.8f;

    private Enemy enemy;
    private float nextAttackTime = 0f;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void Update()
    {
        if (!player || enemy.currentHP <= 0) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            TryAttack();
        }
        else if (dist <= detectRange)
        {
            Chase();
        }
    }

    void Chase()
    {
        float speed = enemy.stats ? enemy.stats.movementSpeed : 2f;
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        transform.position = (Vector2)transform.position + dir * speed * Time.deltaTime;

        // opsional: rotasi ngikut arah gerak
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void TryAttack()
    {
        float atkPerSec = (enemy.stats && enemy.stats.attackSpeed > 0f) ? enemy.stats.attackSpeed : 1f;
        float cd = 1f / atkPerSec;
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + cd;

        // cari komponen IDamageable di player (atau ganti sesuai sistem HP kamu)
        var dmgTarget = player.GetComponent<IDamageable>();
        if (dmgTarget != null)
        {
            int dmg = enemy.stats ? enemy.stats.damage : 1;
            dmgTarget.TakeDamage(dmg);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
