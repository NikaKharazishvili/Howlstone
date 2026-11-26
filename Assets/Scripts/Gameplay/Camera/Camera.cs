using UnityEngine;
using System.Collections.Generic;
using static GameConstants;

public class Cam : MonoBehaviour
{
    public static Player Player;

    [SerializeField] LayerMask occludableLayer, blockingLayer;

    // The room the camera needs when ground or a wall stops it, and how fast it moves back out
    const float CameraRadius = 0.3f, CameraReturnSpeed = 5;
    float distance = CameraZoom;

    readonly Dictionary<int, Occludable> occluderCache = new();
    HashSet<Occludable> hiddenLastFrame = new();
    HashSet<Occludable> hitThisFrame = new();

    void Start()
    {
        transform.rotation = Quaternion.Euler(CameraPitch, transform.rotation.eulerAngles.y, 0);
        FollowPlayer();
    }

    void LateUpdate()
    {
        HandleRotation();
        FollowPlayer();
        HandleOcclusion();
    }

    void FollowPlayer()
    {
        Vector3 pivot = Player.transform.position + Vector3.up * CameraUpPosition;
        distance = Mathf.Lerp(distance, CameraZoom, Time.deltaTime * CameraReturnSpeed);
        if (Physics.SphereCast(pivot, CameraRadius, -transform.forward, out RaycastHit wall, distance, blockingLayer, QueryTriggerInteraction.Ignore)) distance = wall.distance;
        transform.position = pivot - transform.forward * distance;
    }

    void HandleRotation()
    {
        if (!Player.IsAlive) return;
        if (Mathf.Abs(Player.rotateCameraInput) > 0.1f)
        {
            Quaternion PlayerRotation = Quaternion.Euler(CameraPitch, transform.rotation.eulerAngles.y + Player.rotateCameraInput * CameraHotkeyRotationSpeed, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, PlayerRotation, Time.deltaTime * CameraHotkeyRotationSpeed);
        }
        else if (Mathf.Abs(Player.moveInput.x) > 0.1f)
        {
            Quaternion PlayerRotation = Quaternion.Euler(CameraPitch, transform.rotation.eulerAngles.y + Player.moveInput.x * CameraAutoRotationSpeed, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, PlayerRotation, Time.deltaTime * CameraAutoRotationSpeed);
        }
    }

    void HandleOcclusion()
    {
        hitThisFrame.Clear();
        CastAndCollectHits();
        RestoreNonHitObjects();
        (hiddenLastFrame, hitThisFrame) = (hitThisFrame, hiddenLastFrame);
    }

    void CastAndCollectHits()
    {
        Vector3 direction = Player.transform.position + transform.up - transform.position;
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction.normalized, direction.magnitude, occludableLayer);

        foreach (var hit in hits)
        {
            Occludable occludable = GetOrCacheOccludable(hit.collider);
            if (!occludable) continue;
            hitThisFrame.Add(occludable);
            if (!hiddenLastFrame.Contains(occludable)) occludable.Hide();
        }
        Debug.DrawLine(transform.position, Player.transform.position + transform.up, Color.cyan);
    }

    void RestoreNonHitObjects() { foreach (var occludable in hiddenLastFrame) if (!hitThisFrame.Contains(occludable)) occludable.Show(); }

    Occludable GetOrCacheOccludable(Collider col)
    {
        int id = col.transform.GetInstanceID();
        if (!occluderCache.TryGetValue(id, out var occludable))
        {
            occludable = col.GetComponent<Occludable>();
            if (occludable) occluderCache[id] = occludable;
        }
        return occludable;
    }
}