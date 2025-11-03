using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 3;
    private Vector2 direction;

    public void Launch(Vector2 dir)
    {
        direction = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle ); // rotasi sprite panah
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Enemy e = col.GetComponentInParent<Enemy>();
        if (e != null)
        {
            e.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
