using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator2D : MonoBehaviour
{
    [SerializeField] private EnemyGridChase2D mover;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animator Params (harus sama di Animator Controller)")]
    [SerializeField] private string paramIsMoving = "IsMoving";
    [SerializeField] private string paramDirX = "DirX";
    [SerializeField] private string paramDirY = "DirY";

    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (mover == null) mover = GetComponent<EnemyGridChase2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (mover == null || anim == null) return;

        bool moving = mover.AnimIsMoving; // sinyal yang sudah dismoothing
        anim.SetBool(paramIsMoving, moving);

        if (moving)
        {
            Vector2 dir = mover.CurrentDir.sqrMagnitude > 0.0001f ? mover.CurrentDir : Vector2.down;
            dir = SnapTo4(dir);
            anim.SetFloat(paramDirX, dir.x);
            anim.SetFloat(paramDirY, dir.y);

            // side flip (kanan/kiri) bila dominan horizontal
            if (spriteRenderer != null && Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                spriteRenderer.flipX = dir.x < 0f;
        }
        // saat idle, biarkan DirX/DirY apa adanya (tetap menghadap arah terakhir)
    }

    private Vector2 SnapTo4(Vector2 v)
    {
        // pilih sumbu dominan agar eksklusif 4 arah
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            return new Vector2(Mathf.Sign(v.x), 0f);
        else
            return new Vector2(0f, Mathf.Sign(v.y));
    }
}
