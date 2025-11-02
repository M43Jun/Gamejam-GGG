using UnityEngine;
using UnityEngine.Tilemaps;

public class TileHoverAndClickOutline : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Transform outline; // sprite border

    [Header("Colors")]
    [SerializeField] private Color selectedColor = Color.red;

    private Vector3Int currentHoverCell;
    private bool hasHover = false;

    private Vector3Int currentSelectedCell;
    private bool hasSelected = false;

    // posisi "buang" kalau mouse keluar
    private readonly Vector3 hiddenPos = new Vector3(9999, 9999, 0);

    void Start()
    {
        // pastikan outline aktif dari awal
        if (outline != null && !outline.gameObject.activeSelf)
            outline.gameObject.SetActive(true);

        // taruh di luar layar dulu
        if (outline != null)
            outline.position = hiddenPos;
    }

    void Update()
    {
        bool inside = IsMouseInsideGameWindow();

        if (!inside)
        {
            // mouse di luar → sembunyikan aja, JANGAN di-setActive(false)
            HideOutlineOnly();
            return;
        }

        HandleHover();
        HandleClick();
    }

    void HandleHover()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector3Int cell = tilemap.WorldToCell(mouseWorld);

        if (tilemap.HasTile(cell))
        {
            if (!hasHover || cell != currentHoverCell)
            {
                currentHoverCell = cell;
                hasHover = true;

                Vector3 center = tilemap.GetCellCenterWorld(cell);
                outline.position = center;
            }
        }
        else
        {
            HideOutlineOnly();
        }
    }

    void HandleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (hasHover && tilemap.HasTile(currentHoverCell))
            {
                // balikin yang lama
                if (hasSelected)
                {
                    tilemap.SetTileFlags(currentSelectedCell, TileFlags.None);
                    tilemap.SetColor(currentSelectedCell, Color.white);
                }

                // set yang baru
                tilemap.SetTileFlags(currentHoverCell, TileFlags.None);
                tilemap.SetColor(currentHoverCell, selectedColor);

                currentSelectedCell = currentHoverCell;
                hasSelected = true;
            }
        }
    }

    void HideOutlineOnly()
    {
        if (outline != null)
            outline.position = hiddenPos;

        hasHover = false;
    }

    bool IsMouseInsideGameWindow()
    {
        Vector3 mp = Input.mousePosition;
        return mp.x >= 0 && mp.x <= Screen.width && mp.y >= 0 && mp.y <= Screen.height;
    }
}
