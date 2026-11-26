using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Adds what the game needs to the open scene's ground: the scene manager prefab, the counting parents, blob shadows and the sky
// START_X, START_Z: the player's start in meters, replaced in the script's text before sending it
const float StartX = START_X, StartZ = START_Z, StartLift = 0.1f;
if (EditorApplication.isPlaying) return "playing";
var ground = GameObject.Find("Ground");
if (Object.FindFirstObjectByType<SceneManager>()) return "the scene already has a scene manager";
foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) Object.DestroyImmediate(camera.gameObject);
var manager = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Objects/SceneManager.prefab"));
var fields = new SerializedObject(manager.GetComponent<SceneManager>());
foreach (var (field, name) in new[] { ("coinsParent", "Coins"), ("enemiesParent", "Enemies"), ("breakablesParent", "Breakables") })
    fields.FindProperty(field).objectReferenceValue = new GameObject(name);
fields.ApplyModifiedPropertiesWithoutUndo();
var player = manager.GetComponentInChildren<Player>(true).transform;
Physics.SyncTransforms();
if (!Physics.Raycast(new Vector3(StartX, 200, StartZ), Vector3.down, out var spot, 400, LayerMask.GetMask("Ground"))) return "no ground under the start";
player.position = spot.point + Vector3.up * StartLift;
var environment = ground.transform.Find("Environment");
if (!environment) { environment = new GameObject("Environment").transform; environment.SetParent(ground.transform, false); }
var shadows = ground.AddComponent<BlobShadows>();
var link = new SerializedObject(shadows);
link.FindProperty("environment").objectReferenceValue = environment;
link.ApplyModifiedPropertiesWithoutUndo();
RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/Objects/Environment/PolyverseSkies/Skybox Cubemap Extended Blend.mat");
EditorSceneManager.MarkSceneDirty(ground.scene);
EditorSceneManager.SaveScene(ground.scene);
return "set up: " + string.Join(", ", ground.scene.GetRootGameObjects().Select(r => r.name));