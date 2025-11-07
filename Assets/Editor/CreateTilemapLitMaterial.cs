#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreateTilemapLitMaterial : EditorWindow
{
    [MenuItem("Tools/Fix Tilemap Material for 2D Lighting")]
    public static void FixTilemapMaterial()
    {
        Debug.Log("=== Memperbaiki Material Tilemap ===");
        
        // 1. Cari shader yang tepat untuk Tilemap
        string[] possibleShaders = new string[]
        {
            "Universal Render Pipeline/2D/Sprite-Lit-Default",
            "Shader Graphs/Sprite Lit",
            "Sprites/Default"
        };
        
        Shader foundShader = null;
        string shaderName = "";
        
        foreach (string shader in possibleShaders)
        {
            foundShader = Shader.Find(shader);
            if (foundShader != null)
            {
                shaderName = shader;
                Debug.Log($"? Shader ditemukan: {shader}");
                break;
            }
        }
        
        if (foundShader == null)
        {
            Debug.LogError("? Tidak ada shader 2D yang ditemukan!");
            Debug.LogError("Pastikan URP sudah terinstall: Window > Package Manager > Universal RP");
            return;
        }
        
        // 2. Buat material baru
        Material tilemapMat = new Material(foundShader);
        tilemapMat.name = "TilemapLit";
        
        // Set properties untuk Tilemap
        if (tilemapMat.HasProperty("_Color"))
        {
            tilemapMat.SetColor("_Color", Color.white);
        }
        
        // Simpan material
        string path = "Assets/TilemapLit.mat";
        AssetDatabase.CreateAsset(tilemapMat, path);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"? Material Tilemap dibuat: {path}");
        
        // 3. Apply ke semua TilemapRenderer di scene
        TilemapRenderer[] tilemapRenderers = FindObjectsOfType<TilemapRenderer>();
        
        if (tilemapRenderers.Length == 0)
        {
            Debug.LogWarning("? Tidak ada TilemapRenderer di scene!");
        }
        else
        {
            foreach (TilemapRenderer tr in tilemapRenderers)
            {
                tr.material = tilemapMat;
                EditorUtility.SetDirty(tr);
                Debug.Log($"? Material di-apply ke: {tr.gameObject.name}");
            }
            
            Debug.Log($"? Total {tilemapRenderers.Length} Tilemap berhasil diupdate!");
        }
        
        // 4. Apply ke semua SpriteRenderer (Player, Item, dll)
        SpriteRenderer[] spriteRenderers = FindObjectsOfType<SpriteRenderer>();
        
        if (spriteRenderers.Length > 0)
        {
            foreach (SpriteRenderer sr in spriteRenderers)
            {
                // Skip jika sudah punya material custom
                if (sr.sharedMaterial == null || sr.sharedMaterial.name.Contains("Default"))
                {
                    sr.material = tilemapMat;
                    EditorUtility.SetDirty(sr);
                }
            }
            Debug.Log($"? {spriteRenderers.Length} Sprite juga diupdate!");
        }
        
        // Highlight material
        Selection.activeObject = tilemapMat;
        EditorGUIUtility.PingObject(tilemapMat);
        
        Debug.Log("=== SELESAI ===");
        Debug.Log("Tilemap sekarang sudah support 2D Lighting!");
    }
    
    [MenuItem("Tools/Reset Tilemap to Default Material")]
    public static void ResetTilemapMaterial()
    {
        TilemapRenderer[] tilemapRenderers = FindObjectsOfType<TilemapRenderer>();
        
        foreach (TilemapRenderer tr in tilemapRenderers)
        {
            tr.material = null; // Reset ke default
            EditorUtility.SetDirty(tr);
        }
        
        Debug.Log($"? {tilemapRenderers.Length} Tilemap di-reset ke material default");
        Debug.Log("Tilemap seharusnya sudah terlihat lagi!");
    }
}
#endif