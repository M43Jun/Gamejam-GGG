using UnityEngine;

[DisallowMultipleComponent]
public class FourDirAnimator2D : MonoBehaviour
{
    [Header("Refs (auto-ambil jika kosong)")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animator Param Names")]
    [SerializeField] private string isMovingParam = "IsMoving"; // bool
    [SerializeField] private string faceParam = "Face";     // int: 0=Down,1=Left,2=Right,3=Up

    [Header("Tuning")]
    [SerializeField] private float moveThreshold = 0.03f; // m/s
    [SerializeField] private float dirDeadZone = 0.01f;

    private Vector3 _lastPos;
    private int _face = 2; // default kanan

    void Awake()
    {
        // >>> robust: cari juga di children
        if (!animator) animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>(true);
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>(true);

        if (!animator) Debug.LogWarning("[FourDirAnimator2D] Animator NOT found on this object or children.");
        if (!spriteRenderer) Debug.LogWarning("[FourDirAnimator2D] SpriteRenderer NOT found on this object or children.");

        _lastPos = transform.position;
        ApplyFacing(); // set awal supaya Face di Animator langsung keisi
    }

    void Update()
    {
        Vector3 delta = transform.position - _lastPos;
        float dt = Mathf.Max(Time.deltaTime, 1e-4f);
        Vector2 v = (Vector2)(delta / dt);
        float speed = v.magnitude;

        bool moving = speed > moveThreshold;
        if (animator) animator.SetBool(isMovingParam, moving);

        // Tentukan face saat bergerak
        if (moving)
        {
            if (Mathf.Abs(v.x) >= Mathf.Abs(v.y))
            {
                // Horizontal
                if (v.x > dirDeadZone) _face = 2; // Right
                else if (v.x < -dirDeadZone) _face = 1; // Left
            }
            else
            {
                // Vertikal
                if (v.y > dirDeadZone) _face = 3; // Up
                else if (v.y < -dirDeadZone) _face = 0; // Down
            }
        }

        // >>> selalu kirim Face ke Animator tiap frame
        ApplyFacing();

        _lastPos = transform.position;
    }

    /// Paksa arah dari script lain (mis. arah klik/aim)
    public void ForceFace(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            _face = (dir.x >= 0f) ? 2 : 1;
        else
            _face = (dir.y >= 0f) ? 3 : 0;

        ApplyFacing();
    }

    private void ApplyFacing()
    {
        if (animator) animator.SetInteger(faceParam, _face);

        if (spriteRenderer)
        {
            if (_face == 1)
            {
                spriteRenderer.flipX = true;
                // Debug.Log("Facing LEFT");
            }
            else if (_face == 2)
            {
                spriteRenderer.flipX = false;
                // Debug.Log("Facing RIGHT");
            }
        }
    }

#if UNITY_EDITOR
    // bantu auto-hook saat di inspector
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (!animator) animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>(true);
            if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>(true);
        }
    }
#endif
}
