using UnityEngine;
using UnityEngine.Tilemaps;

public class PointClickMovement4Dir : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float stopDistance = 0.05f;

    [Header("Clicking")]
    [SerializeField] private Tilemap tilemap;       // <<< penting
    [SerializeField] private LayerMask groundMask;

    private Vector2 stage1Target;
    private Vector2 stage2Target;
    private int moveStage = 0; // 0 = diam, 1 = ke stage1, 2 = ke stage2

    void Update()
    {
        HandleClick();
        HandleMove();
    }

    void HandleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
            // atau bisa juga:
            // Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // buat raycast cek apakah yang diklik itu area ground
            Vector3 m2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(m2.x, m2.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero, 0f, groundMask);
            if (hit.collider != null)
            {
                // 1) world -> cell
                Vector3Int cellPos = tilemap.WorldToCell(hit.point);

                // 2) ambil tengah cell
                Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPos);
                Vector2 finalTarget = new Vector2(cellCenter.x, cellCenter.y);

                // 3) bikin path 4 arah (horizontal/vertical)
                Vector2 current = transform.position;
                float dx = Mathf.Abs(finalTarget.x - current.x);
                float dy = Mathf.Abs(finalTarget.y - current.y);

                if (dx >= dy)
                {
                    // lebih jauh ke kanan/kiri → jalan horizontal dulu
                    stage1Target = new Vector2(finalTarget.x, current.y);
                    stage2Target = finalTarget;
                }
                else
                {
                    // lebih jauh ke atas/bawah → jalan vertical dulu
                    stage1Target = new Vector2(current.x, finalTarget.y);
                    stage2Target = finalTarget;
                }

                moveStage = 1;
            }
        }
    }

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
}
