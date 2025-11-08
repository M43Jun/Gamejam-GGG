// ArrowProjectile.cs
using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 3;

    private Vector2 direction;
    private float maxTravelDistance = Mathf.Infinity;
    private Vector3 startPos;

    // panggil ini saat spawn
    public void Launch(Vector2 dir, float range)
    {
        direction = dir.normalized;
        maxTravelDistance = Mathf.Max(0f, range);
        startPos = transform.position;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // stop bila sudah mencapai jarak maksimum
        if (Vector2.Distance(transform.position, startPos) >= maxTravelDistance)
            Destroy(gameObject);
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
