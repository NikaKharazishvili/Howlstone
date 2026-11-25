using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Utils;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class PauseMenu : MonoBehaviour
{
    const string HiddenClass = "hidden", SelectedClass = "selected", MenuScene = "Menu", LoadHeading = "Load Game";
    const int ResumeIndex = 0, LoadIndex = 1, MainMenuIndex = 2, ExitIndex = 3;

    [SerializeField] UIDocument document;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip selectionSound, submitSound, cancelSound;
    VisualElement pause, slots;
    SaveSlotList slotList;
    List<Label> pauseItems;
    bool isPaused, isShowingSlots;
    int selected, heldStep;

    List<Label> CurrentItems => isShowingSlots ? slotList.Slots : pauseItems;

    void Start()
    {
        VisualElement root = document.rootVisualElement;
        pause = root.Q("Pause");
        slots = root.Q("Slots");
        slotList = new SaveSlotList(slots);
        pauseItems = pause.Query<Label>(className: "item").ToList();
        pauseItems[0].AddToClassList(SelectedClass);
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel")) { if (isShowingSlots) ShowSlots(false); else SetPaused(!isPaused); return; }
        if (!isPaused) return;
        MoveSelection();
        if (isShowingSlots && Input.GetKeyDown(KeyCode.Delete)) EraseSelectedSlot();
        if (Input.GetButtonDown("Submit")) Submit();
    }

    void MoveSelection()
    {
        float vertical = Input.GetAxisRaw("Vertical");
        int step = vertical > 0 ? -1 : vertical < 0 ? 1 : 0;
        if (step != 0 && step != heldStep)
        {
            if (isShowingSlots) slotList.CancelErase();
            Select((selected + step + CurrentItems.Count) % CurrentItems.Count);
            audioSource.PlayOneShot(selectionSound);
        }
        heldStep = step;
    }

    void Select(int index)
    {
        CurrentItems[selected].RemoveFromClassList(SelectedClass);
        selected = index;
        CurrentItems[selected].AddToClassList(SelectedClass);
    }

    void SetPaused(bool paused)
    {
        isPaused = paused;
        SetFrozen(paused);
        pause.EnableInClassList(HiddenClass, !paused);
        Select(0);
        audioSource.PlayOneShot(paused ? submitSound : cancelSound);
    }

    void SetFrozen(bool frozen)
    {
        Time.timeScale = frozen ? 0 : 1;
        AudioListener.pause = frozen;
        SceneManager.Instance.Player.enabled = !frozen;
    }

    void ShowSlots(bool shown)
    {
        if (shown) slotList.Show(LoadHeading);
        CurrentItems[selected].RemoveFromClassList(SelectedClass);
        isShowingSlots = shown;
        selected = 0;
        CurrentItems[selected].AddToClassList(SelectedClass);
        slots.EnableInClassList(HiddenClass, !shown);
        pause.EnableInClassList(HiddenClass, shown);
    }

    void Submit()
    {
        if (isShowingSlots) { LoadSelectedSlot(); return; }
        switch (selected)
        {
            case ResumeIndex: SetPaused(false); break;
            case LoadIndex: audioSource.PlayOneShot(submitSound); ShowSlots(true); break;
            case MainMenuIndex: SetFrozen(false); UnitySceneManager.LoadScene(MenuScene); break;
            case ExitIndex: QuitGame(); break;
            default: throw new NotImplementedException(selected.ToString());
        }
    }

    void LoadSelectedSlot()
    {
        if (!SaveFile.Load(selected)) { audioSource.PlayOneShot(cancelSound); return; }
        SetFrozen(false);
        UnitySceneManager.LoadScene(SaveFile.Current.lastScene);
    }

    void EraseSelectedSlot()
    {
        slotList.PressErase(selected);
        audioSource.PlayOneShot(cancelSound);
    }
}