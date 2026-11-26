using System.Linq;
using UnityEngine;
using UnityEditor;

// Tile size and count, and the text the shown prefabs' names hold
const int Width = 213, Height = 270, Columns = 6, Rows = 4;
const string search = "SEARCH_TEXT";
if (EditorApplication.isPlaying) return "playing";
var paths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Objects" }).Select(AssetDatabase.GUIDToAssetPath)
    .Where(p => System.IO.Path.GetFileNameWithoutExtension(p).IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0).ToList();
paths.Sort((a, b) => EditorUtility.NaturalCompare(System.IO.Path.GetFileName(a), System.IO.Path.GetFileName(b)));
var scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
var go = new GameObject("PrefabSheetCamera");
UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
var camera = go.AddComponent<Camera>(); camera.CopyFrom(Camera.main); camera.scene = scene;
camera.orthographic = true; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.45f, 0.45f, 0.45f);
var target = new RenderTexture(Width, Height, 24); camera.targetTexture = target;
var sheet = new Texture2D(Width * Columns, Height * Rows, TextureFormat.RGB24, false);
var list = new System.Text.StringBuilder($"{paths.Count} found, tiles left to right, top to bottom\n");
for (int i = 0; i < Mathf.Min(paths.Count, Columns * Rows); i++)
{
    var made = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]), scene);
    var looks = made.GetComponentsInChildren<Renderer>();
    var size = Vector3.zero;
    if (looks.Length > 0)
    {
        var bounds = looks.Select(r => r.bounds).Aggregate((a, b) => { a.Encapsulate(b); return a; });
        size = bounds.size;
        var look = Quaternion.Euler(15, 30, 0);
        camera.orthographicSize = Mathf.Max(bounds.extents.y, new Vector2(bounds.extents.x, bounds.extents.z).magnitude * Height / Width) * 1.15f;
        go.transform.SetPositionAndRotation(bounds.center - look * Vector3.forward * (bounds.size.magnitude + 5), look);
        camera.farClipPlane = bounds.size.magnitude * 3 + 10;
    }
    camera.Render();
    RenderTexture.active = target;
    sheet.ReadPixels(new Rect(0, 0, Width, Height), i % Columns * Width, (Rows - 1 - i / Columns) * Height);
    list.AppendLine($"{i + 1} {paths[i].Substring("Assets/Objects/".Length)} {size.x:F1}x{size.y:F1}x{size.z:F1} parts {looks.Length}{(made.GetComponentInChildren<Collider>() ? " solid" : "")}");
    Object.DestroyImmediate(made);
}
sheet.Apply();
var file = System.IO.Path.GetFullPath(Application.dataPath + "/../Temp/PrefabSheet.png");
System.IO.File.WriteAllBytes(file, ImageConversion.EncodeToPNG(sheet));
RenderTexture.active = null; camera.targetTexture = null;
UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
Object.DestroyImmediate(target); Object.DestroyImmediate(sheet);
return file + "\n" + list;