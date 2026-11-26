using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ConfirmBox
{
    const string OpenClass = "open";
    readonly VisualElement box;
    readonly Label question;
    Action confirmed;

    public static bool IsEraseKeyPressed => Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton2);
    public bool IsOpen => confirmed != null;

    public ConfirmBox(VisualElement box)
    {
        this.box = box;
        question = box.Q<Label>("Question");
    }

    public void Ask(string questionText, Action onYes)
    {
        question.text = questionText;
        confirmed = onYes;
        box.AddToClassList(OpenClass);
    }

    public void ReadAnswer(AudioSource audioSource, AudioClip yesSound, AudioClip noSound)
    {
        bool yes = Input.GetButtonDown("Submit");
        if (!yes && !Input.GetButtonDown("Cancel")) return;
        Action action = confirmed;
        confirmed = null;
        box.RemoveFromClassList(OpenClass);
        audioSource.PlayOneShot(yes ? yesSound : noSound);
        if (yes) action();
    }
}