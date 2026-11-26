using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static GameConstants;

// Tile size and count; meters before an enemy where the player stands, and between two shown places
const int Width = 640, Height = 360, Columns = 2, Rows = 3;
const float Arrival = 5, Apart = 12;
if (EditorApplication.isPlaying) return "playing";
var shadows = Object.FindFirstObjectByType<BlobShadows>();
var drawShadows = typeof(BlobShadows).GetMethod("LateUpdate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
var environment = (Transform)new SerializedObject(shadows).FindProperty("environment").objectReferenceValue;
var bounds = environment.GetComponentsInChildren<Renderer>().Select(r => r.bounds).Aggregate((a, b) => { a.Encapsulate(b); return a; });
var go = new GameObject("LevelShotsCamera") { hideFlags = HideFlags.HideAndDontSave };
var camera = go.AddComponent<Camera>(); camera.CopyFrom(Camera.main);
var target = new RenderTexture(Width, Height, 24); camera.targetTexture = target;
var sheet = new Texture2D(Width * Columns, Height * Rows, TextureFormat.RGB24, false);
var legend = new List<string>();
void Shot(string name, Vector3 from, Quaternion look, float above = 0)
{
    int tile = legend.Count;
    camera.orthographic = above > 0; camera.orthographicSize = above;
    go.transform.SetPositionAndRotation(from, look);
    camera.Render(); drawShadows.Invoke(shadows, null); camera.Render();
    RenderTexture.active = target;
    sheet.ReadPixels(new Rect(0, 0, Width, Height), tile % Columns * Width, (Rows - 1 - tile / Columns) * Height);
    legend.Add($"{tile + 1} {name}");
}
void Arrive(string name, Vector3 spot, Vector3 forward)
{
    if (Physics.Raycast(spot + Vector3.up * 50, Vector3.down, out var hit, 100, LayerMask.GetMask("Ground"))) spot.y = hit.point.y;
    var look = Quaternion.Euler(CameraPitch, Quaternion.LookRotation(new Vector3(forward.x, 0, forward.z)).eulerAngles.y, 0);
    Shot(name, spot - look * Vector3.forward * CameraZoom + Vector3.up * CameraUpPosition, look);
}
Shot("above", new Vector3(bounds.center.x, bounds.max.y + 50, bounds.center.z), Quaternion.Euler(90, 0, 0), Mathf.Max(bounds.extents.z, bounds.extents.x * Height / Width));
var player = Object.FindFirstObjectByType<Player>().transform;
Arrive("start", player.position, player.forward);
var seen = new List<Vector3> { player.position };
var enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None).Select(e => e.transform.position).ToList();
while (legend.Count < Columns * Rows)
{
    var next = enemies.OrderByDescending(e => seen.Min(s => Vector3.Distance(s, e))).FirstOrDefault();
    if (enemies.Count == 0 || seen.Min(s => Vector3.Distance(s, next)) < Apart) break;
    var path = new UnityEngine.AI.NavMeshPath();
    UnityEngine.AI.NavMesh.SamplePosition(player.position, out var from, 2, UnityEngine.AI.NavMesh.AllAreas);
    UnityEngine.AI.NavMesh.SamplePosition(next, out var to, 2, UnityEngine.AI.NavMesh.AllAreas);
    UnityEngine.AI.NavMesh.CalculatePath(from.position, to.position, UnityEngine.AI.NavMesh.AllAreas, path);
    var spot = path.corners.Length > 1 ? path.corners[0] : player.position;
    float left = Arrival;
    for (int i = path.corners.Length - 1; i > 0; i--)
    {
        float length = Vector3.Distance(path.corners[i], path.corners[i - 1]);
        if (length >= left) { spot = Vector3.MoveTowards(path.corners[i], path.corners[i - 1], left); break; }
        left -= length;
    }
    Arrive($"enemy at ({next.x:F0},{next.z:F0})", spot, next - spot);
    seen.Add(next);
}
sheet.Apply();
var file = System.IO.Path.GetFullPath(Application.dataPath + "/../Temp/LevelShots.png");
System.IO.File.WriteAllBytes(file, ImageConversion.EncodeToPNG(sheet));
RenderTexture.active = null; camera.targetTexture = null;
Object.DestroyImmediate(go); Object.DestroyImmediate(target); Object.DestroyImmediate(sheet);
return file + " | " + string.Join(" | ", legend);