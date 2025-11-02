using UnityEngine;

public class ClickToPoint2D : MonoBehaviour
{
    [SerializeField] private LayerMask groundMask;  // pilih layer Ground2D
    public Vector2 lastClickPoint;                  // biar keliatan di Inspector

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // klik kiri
        {
            // ambil posisi mouse di world
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // karena 2D kita buang z nya
            Vector2 mousePos2D = new Vector2(mouseWorld.x, mouseWorld.y);

            // raycast 2D dari titik itu
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero, 0f, groundMask);

            if (hit.collider != null)
            {
                lastClickPoint = hit.point;
                Debug.Log("Klik di tanah 2D: " + lastClickPoint);
            }
        }
    }
}
