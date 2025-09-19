using UnityEngine;
using UnityEditor;
using System.IO;

public class MaskPainterWindow : EditorWindow
{
    private Texture2D mask;
    private Color paintColor = Color.white;
    private int brushSize = 16;
    private bool painting = false;
    private Vector2 scroll;

    private const int DEFAULT_RES = 512;

    [MenuItem("Tools/Grass Mask Painter")]
    static void Init()
    {
        MaskPainterWindow window = (MaskPainterWindow)GetWindow(typeof(MaskPainterWindow));
        window.titleContent = new GUIContent("Grass Mask Painter");
        window.Show();
    }

    void OnEnable()
    {
        if (mask == null)
        {
            CreateNewMask();
        }
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("New Mask", GUILayout.Width(100)))
        {
            CreateNewMask();
        }
        if (GUILayout.Button("Save PNG", GUILayout.Width(100)))
        {
            SaveMaskToPNG();
        }
        if (GUILayout.Button("Clear", GUILayout.Width(100)))
        {
            ClearMask();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        paintColor = EditorGUILayout.ColorField("Brush Color", paintColor);
        brushSize = EditorGUILayout.IntSlider("Brush Size", brushSize, 1, 128);

        GUILayout.Space(10);

        // Vẽ vùng canvas để paint
        scroll = GUILayout.BeginScrollView(scroll, true, true);
        Rect canvasRect = GUILayoutUtility.GetRect(mask.width, mask.height, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
        EditorGUI.DrawPreviewTexture(canvasRect, mask);

        // bắt sự kiện chuột
        Event e = Event.current;
        if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
        {
            if (canvasRect.Contains(e.mousePosition))
            {
                Vector2 local = e.mousePosition - canvasRect.position;
                PaintAt(local, brushSize, paintColor);
                mask.Apply();
                Repaint();
            }
        }
        GUILayout.EndScrollView();
    }

    void CreateNewMask()
    {
        mask = new Texture2D(DEFAULT_RES, DEFAULT_RES, TextureFormat.RGBA32, false);
        ClearMask();
    }

    void ClearMask()
    {
        Color[] cols = new Color[mask.width * mask.height];
        for (int i = 0; i < cols.Length; i++) cols[i] = Color.black;
        mask.SetPixels(cols);
        mask.Apply();
    }

    void PaintAt(Vector2 pos, int radius, Color col)
    {
        int cx = (int)pos.x;
        int cy = (int)pos.y;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int px = cx + x;
                int py = cy + y;
                if (px >= 0 && px < mask.width && py >= 0 && py < mask.height)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        mask.SetPixel(px, mask.height - py, col);
                    }
                }
            }
        }
    }

    void SaveMaskToPNG()
    {
        string path = EditorUtility.SaveFilePanel("Save Mask", "Assets", "grass_mask.png", "png");
        if (!string.IsNullOrEmpty(path))
        {
            byte[] png = mask.EncodeToPNG();
            File.WriteAllBytes(path, png);
            AssetDatabase.Refresh();
            Debug.Log("Saved mask to: " + path);

            string assetPath = "Assets" + path.Replace(Application.dataPath, "");
            Texture2D loaded = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

            // gán lại cho spawner nếu đang chọn
            GrassPainter spawner = Selection.activeGameObject?.GetComponent<GrassPainter>();
            if (spawner != null)
            {
                // spawner.SetMask(loaded);
                Debug.Log("Assigned mask to " + spawner.name);
            }
        }
    }
}

