using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;

public class CreateLitMaterial : EditorWindow
{
    [MenuItem("Tools/Create and Apply Sprite Lit Material")]
    public static void CreateAndApplyLitMaterial()
    {
        // Cari shader yang tepat
        Shader litShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
        
        if (litShader == null)
        {
            // Coba shader alternatif
            litShader = Shader.Find("Sprites/Default");
            Debug.LogWarning("URP Sprite-Lit shader tidak ditemukan, menggunakan Sprites/Default");
        }
        
        if (litShader == null)
        {
            Debug.LogError("Tidak dapat menemukan shader yang sesuai!");
            return;
        }

        // Buat material baru
        Material litMaterial = new Material(litShader);
        litMaterial.name = "SpriteLit";
        
        // Simpan material
        string path = "Assets/SpriteLit.mat";
        AssetDatabase.CreateAsset(litMaterial, path);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Material dibuat di: {path}");

        // Apply ke semua sprite dan tilemap
        int changedCount = 0;

        // Ubah semua SpriteRenderer
        SpriteRenderer[] spriteRenderers = FindObjectsOfType<SpriteRenderer>();
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.material = litMaterial;
            EditorUtility.SetDirty(sr);
            changedCount++;
        }

        // Ubah semua TilemapRenderer
        TilemapRenderer[] tilemapRenderers = FindObjectsOfType<TilemapRenderer>();
        foreach (TilemapRenderer tr in tilemapRenderers)
        {
            tr.material = litMaterial;
            EditorUtility.SetDirty(tr);
            changedCount++;
        }

        Debug.Log($"Berhasil apply material ke {changedCount} objects!");
        
        // Highlight material yang baru dibuat
        Selection.activeObject = litMaterial;
        EditorGUIUtility.PingObject(litMaterial);
    }
}
#endif