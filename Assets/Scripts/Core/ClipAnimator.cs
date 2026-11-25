using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif

[RequireComponent(typeof(Animator)), DisallowMultipleComponent]
public partial class ClipAnimator : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] AnimationClip playOnAwake;
    [SerializeField] float playOnAwakeSpeed = 1;
    [SerializeField] AnimationBar playingAnimation;
    [System.Serializable] struct AnimationBar { }

    public AnimationClip CurrentClip { get; private set; }
    public bool IsBusy => CurrentClip && !CurrentClip.isLooping && Progress < 1;
    public float Progress { get { if (!CurrentClip || !graph.IsValid()) return 0; float done = (float)mixer.GetInput(current).GetTime() / CurrentClip.length; return CurrentClip.isLooping ? done % 1 : Mathf.Min(done, 1); } }

    PlayableGraph graph;
    AnimationMixerPlayable mixer;
    int current;
    float fadeSpeed;

    void Reset() => animator = GetComponent<Animator>();

    void Awake()
    {
        graph = PlayableGraph.Create(name);
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        var output = AnimationPlayableOutput.Create(graph, "Animation", animator);
        mixer = AnimationMixerPlayable.Create(graph, 1);
        mixer.SetInputWeight(0, 1);
        output.SetSourcePlayable(mixer);
        graph.Play();
        if (playOnAwake) Play(playOnAwake, playOnAwakeSpeed, 0);
    }

    void OnDestroy() { if (graph.IsValid()) graph.Destroy(); }

    void Update()
    {
        float weight = mixer.GetInputWeight(current);
        if (weight >= 1) return;
        float newWeight = Mathf.Min(1, weight + fadeSpeed * Time.deltaTime), othersScale = (1 - newWeight) / (1 - weight);
        mixer.SetInputWeight(current, newWeight);
        for (int i = 0; i < mixer.GetInputCount(); i++)
        {
            if (i == current || !mixer.GetInput(i).IsValid()) continue;
            float w = mixer.GetInputWeight(i) * othersScale;
            if (w < 0.01f) RemoveMixerInput(i);
            else mixer.SetInputWeight(i, w);
        }
    }

    public void Play(AnimationClip clip, float speed = 1, float crossfade = GameConstants.DefaultCrossfade)
    {
        CurrentClip = clip;
        int reused = -1;
        bool fadeFromOthers = false;
        for (int i = 0; i < mixer.GetInputCount(); i++)
        {
            var input = mixer.GetInput(i);
            if (!input.IsValid()) continue;
            if (crossfade <= 0) RemoveMixerInput(i);
            else if (reused < 0 && clip.isLooping && ((AnimationClipPlayable)input).GetAnimationClip() == clip) reused = i;
            else fadeFromOthers = true;
        }
        current = reused >= 0 ? reused : GetFreeMixerInput();
        if (reused < 0) graph.Connect(AnimationClipPlayable.Create(graph, clip), 0, mixer, current);
        mixer.GetInput(current).SetSpeed(speed);
        if (fadeFromOthers) fadeSpeed = 1 / crossfade;
        else mixer.SetInputWeight(current, 1);
    }

    public void PlayInstant(AnimationClip clip, float speed = 1) => Play(clip, speed, 0);

    public void SetSpeed(float speed) => mixer.GetInput(current).SetSpeed(speed);

    void RemoveMixerInput(int i) { var input = mixer.GetInput(i); mixer.DisconnectInput(i); input.Destroy(); mixer.SetInputWeight(i, 0); }

    int GetFreeMixerInput()
    {
        int count = mixer.GetInputCount();
        for (int i = 0; i < count; i++) if (!mixer.GetInput(i).IsValid()) return i;
        mixer.SetInputCount(count + 1);
        return count;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(AnimationBar))]
    class AnimationBarDrawer : PropertyDrawer
    {
        static GUIStyle middle, right;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var target = (ClipAnimator)property.serializedObject.targetObject;
            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.DrawRect(rect, Color.black);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width * target.Progress, rect.height), new Color(0f, 0.6f, 0f));
            middle ??= new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
            right ??= new GUIStyle(middle) { alignment = TextAnchor.MiddleRight, padding = { right = 6 } };
            GUI.Label(rect, target.CurrentClip ? target.CurrentClip.name : "No animation", middle);
            GUI.Label(rect, $"{target.Progress * 100:0}%", right);
            if (Application.isPlaying && ActiveEditorTracker.sharedTracker.activeEditors is { Length: > 0 } editors) editors[0].Repaint();
        }
    }
#endif
}

#if UNITY_EDITOR
public partial class ClipAnimator : IAnimationClipSource
{
    void IAnimationClipSource.GetAnimationClips(List<AnimationClip> results)
    {
        foreach (var script in GetComponents<MonoBehaviour>())
        {
            using var serialized = new SerializedObject(script);
            var property = serialized.GetIterator();
            while (property.Next(true)) if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue is AnimationClip clip && !results.Contains(clip)) results.Add(clip);
        }
    }
}
#endif