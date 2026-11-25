using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public static class Utils
{
    public static bool Chance(int percent) => Random.Range(0, 100) < percent;

    public static T PoolComponent<T>(T[] pool) where T : Component
    {
        foreach (var component in pool) if (!component.gameObject.activeInHierarchy) return component;
        return null;
    }

    public static void MoveTransformUp(Transform self, float speed) => self.Translate(Vector3.up * speed * Time.deltaTime);

    public static void SetScreenSizeBillboard(Transform self, Transform camera, float screenSize)
    {
        self.rotation = camera.rotation;
        float scale = Vector3.Dot(self.position - camera.position, camera.forward) * screenSize;
        self.localScale = new Vector3(scale, scale, scale);
    }

    public static bool IsGrounded(CapsuleCollider collider, float rayLength = 0.3f)
    {
        float radius = collider.radius * 0.9f;
        Vector3 bottom = collider.transform.position + collider.center + Vector3.down * (collider.height / 2f - 0.1f);
        bool grounded = false;
        for (int i = 0; i < 4; i++)
        {
            Vector3 origin = bottom + (i < 2 ? Vector3.right : Vector3.forward) * (i % 2 == 0 ? radius : -radius);
            bool hit = Physics.Raycast(origin, Vector3.down, rayLength);
            Debug.DrawRay(origin, Vector3.down * rayLength, hit ? Color.green : Color.red);
            grounded |= hit;
        }
        return grounded;
    }

    public static IEnumerator LoadSceneAsync(string sceneName, Image loadingBar = null, TextMeshProUGUI loadingText = null, float briefDelay = 0)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;
        while (operation.progress < 0.9f)
        {
            float progress = operation.progress / 0.9f;
            if (loadingText) loadingText.text = $"Loading {Mathf.RoundToInt(progress * 100)}%";
            if (loadingBar) loadingBar.fillAmount = progress;
            yield return null;
        }
        if (loadingText) loadingText.text = "Loading 100%";
        if (loadingBar) loadingBar.fillAmount = 1;
        yield return new WaitForSeconds(briefDelay);
        operation.allowSceneActivation = true;
    }
}