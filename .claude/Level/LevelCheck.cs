using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

// Limits in meters (Core: the share of a footprint left out at each side), and examples listed per problem
const float FloatingGap = 0.1f, FootprintHeight = 0.7f, Core = 0.3f;
const int Examples = 8;
if (EditorApplication.isPlaying) return "playing";
float maxClimbAngle = (float)typeof(Player).GetField("MaxClimbAngle", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetRawConstantValue();
Physics.SyncTransforms();
var environment = (Transform)new SerializedObject(Object.FindFirstObjectByType<BlobShadows>()).FindProperty("environment").objectReferenceValue;
int groundMask = LayerMask.GetMask("Ground");
var heights = new Dictionary<Vector2Int, float>();
float GroundAt(float x, float z)
{
    var cell = new Vector2Int(Mathf.RoundToInt(x * 10), Mathf.RoundToInt(z * 10));
    if (!heights.TryGetValue(cell, out var y)) heights[cell] = y = Physics.Raycast(new Vector3(cell.x / 10f, 200, cell.y / 10f), Vector3.down, out var hit, 400, groundMask) ? hit.point.y : float.NaN;
    return y;
}
string At(Transform t) => $"{t.name} ({t.position.x:F0},{t.position.z:F0})";
var report = new System.Text.StringBuilder();
void Report(string title, List<string> found) { if (found.Count > 0) report.AppendLine($"{title} {found.Count}: {string.Join(", ", found.Take(Examples))}"); }

var floating = new List<string>(); var buried = new List<string>(); var outside = new List<string>();
var solids = new List<(Transform item, Quaternion turn, Rect area, bool building)>(); var soft = new List<Transform>();
long triangles = 0; var materials = new HashSet<Material>(); int renderers = 0;
var items = environment.Cast<Transform>().SelectMany(g => g.GetComponent<Renderer>() ? new[] { g } : g.Cast<Transform>().ToArray()).Where(i => i.gameObject.activeInHierarchy).ToList();
foreach (var item in items)
{
    var vertices = new List<Vector3>();
    foreach (var filter in item.GetComponentsInChildren<MeshFilter>())
    {
        var look = filter.GetComponent<Renderer>();
        if (!look || !filter.sharedMesh) continue;
        renderers++; triangles += filter.sharedMesh.triangles.Length / 3;
        foreach (var m in look.sharedMaterials) materials.Add(m);
        vertices.AddRange(filter.sharedMesh.vertices.Select(filter.transform.TransformPoint));
    }
    if (vertices.Count == 0) continue;
    float under = GroundAt(item.position.x, item.position.z);
    if (float.IsNaN(under)) { outside.Add(At(item)); continue; }
    if (item.parent.name.Contains("Water") || item.name.Contains("lily")) continue;
    var lowest = vertices.OrderBy(v => v.y).First();
    float gap = vertices.Where(v => v.y < lowest.y + 0.5f).Min(v => v.y - GroundAt(v.x, v.z));
    if (gap > FloatingGap && !Physics.RaycastAll(lowest + Vector3.up * 0.05f, Vector3.down, 0.25f).Any(h => !h.transform.IsChildOf(item))) floating.Add($"{At(item)} {gap:F2} m");
    if (vertices.Max(v => v.y) - under < FloatingGap) buried.Add(At(item));
    var turn = Quaternion.Euler(0, item.eulerAngles.y, 0);
    var low = vertices.Where(v => v.y < under + FootprintHeight).Select(v => Quaternion.Inverse(turn) * (v - item.position)).ToList();
    if (!item.GetComponentInChildren<Collider>()) { soft.Add(item); continue; }
    if (low.Count > 0) solids.Add((item, turn, Rect.MinMaxRect(low.Min(v => v.x), low.Min(v => v.z), low.Max(v => v.x), low.Max(v => v.z)), item.parent.name.Contains("Building")));
}
bool Inside((Transform item, Quaternion turn, Rect area, bool building) solid, Vector3 point)
{
    var local = Quaternion.Inverse(solid.turn) * (point - solid.item.position);
    return solid.area.Contains(new Vector2(local.x, local.z));
}
bool Crosses((Transform item, Quaternion turn, Rect area, bool building) a, (Transform item, Quaternion turn, Rect area, bool building) b)
{
    foreach (var x in new[] { Core, 0.5f, 1 - Core })
        foreach (var z in new[] { Core, 0.5f, 1 - Core })
            if (Inside(b, a.item.position + a.turn * new Vector3(Mathf.Lerp(a.area.xMin, a.area.xMax, x), 0, Mathf.Lerp(a.area.yMin, a.area.yMax, z)))) return true;
    return false;
}
var crossing = new List<string>();
for (int i = 0; i < solids.Count; i++)
    for (int j = i + 1; j < solids.Count; j++)
        if (!(solids[i].building && solids[j].building) && (Crosses(solids[i], solids[j]) || Crosses(solids[j], solids[i]))) crossing.Add($"{At(solids[i].item)} + {solids[j].item.name}");
var inSolid = new List<string>();
foreach (var plant in soft)
{
    var holder = solids.FirstOrDefault(s => Inside(s, plant.position));
    if (holder.item) inSolid.Add($"{At(plant)} in {holder.item.name}");
}

var unreachable = new List<string>(); var steep = new List<string>();
var player = Object.FindFirstObjectByType<Player>().transform;
if (!NavMesh.SamplePosition(player.position, out var start, 2, NavMesh.AllAreas)) report.AppendLine("PLAYER is off the NavMesh");
var targets = GameObject.FindGameObjectsWithTag("Coin").Select(c => c.transform).Concat(Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None).Select(e => e.transform)).ToList();
foreach (var target in targets)
{
    var path = new NavMeshPath();
    if (!NavMesh.SamplePosition(target.position, out var end, 2, NavMesh.AllAreas) || !NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete) { unreachable.Add(At(target)); continue; }
    float worst = 0;
    for (int i = 1; i < path.corners.Length; i++)
        for (float t = 0; t < 1; t += 0.5f / Mathf.Max(0.5f, Vector3.Distance(path.corners[i - 1], path.corners[i])))
            if (Physics.Raycast(Vector3.Lerp(path.corners[i - 1], path.corners[i], t) + Vector3.up * 2, Vector3.down, out var hit, 5, groundMask)) worst = Mathf.Max(worst, Vector3.Angle(hit.normal, Vector3.up));
    if (worst > maxClimbAngle) steep.Add($"{At(target)} {worst:F0} deg");
}
var dryWater = GameObject.FindGameObjectsWithTag("Water").Where(w => !w.GetComponentsInChildren<Collider>().Any(c => c.isTrigger)).Select(w => At(w.transform)).ToList();

Report("FLOATING", floating); Report("BURIED", buried); Report("OUTSIDE GROUND", outside);
Report("SOLIDS CROSSING", crossing); Report("INSIDE A SOLID", inSolid);
Report("UNREACHABLE", unreachable); Report("PATH TOO STEEP", steep); Report("WATER WITHOUT TRIGGER", dryWater);
report.Append($"totals: objects {items.Count}, renderers {renderers}, triangles {triangles}, materials {materials.Count}, targets {targets.Count}");
return report.ToString();