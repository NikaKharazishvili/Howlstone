using UnityEngine;

public class Decoration : MonoBehaviour
{
    enum AnimationState { Dead, Sitting, Lying }
    [SerializeField] GameObject[] gameObjects;
    [SerializeField] ClipAnimator[] animators;
    [SerializeField] AnimationState state;
    [SerializeField] AnimationClip deadClip, sittingClip, lyingClip;

    void Start()
    {
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (!gameObjects[i].activeSelf) continue;
            animators[i].PlayInstant(state switch { AnimationState.Dead => deadClip, AnimationState.Sitting => sittingClip, AnimationState.Lying => lyingClip, _ => throw new System.NotImplementedException() }, state == AnimationState.Dead ? 1.5f : 1);
            return;
        }
    }
}