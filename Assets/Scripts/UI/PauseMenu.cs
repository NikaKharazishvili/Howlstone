using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Utils;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class PauseMenu : MonoBehaviour
{
    enum PauseScreen { Main, Slots, Options }
    const string HiddenClass = "hidden", SelectedClass = "selected", MenuScene = "Menu";
    const int ResumeIndex = 0, LoadIndex = 1, OptionsIndex = 2, MainMenuIndex = 3, ExitIndex = 4;

    [SerializeField] UIDocument document;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip selectionSound, submitSound, cancelSound;
    readonly MenuKeys menuKeys = new();
    VisualElement pause, slots, optionsScreen;
    SaveSlotList slotList;
    OptionsMenu options;
    ConfirmBox confirm;
    List<Label> pauseItems;
    PauseScreen shown;
    bool isPaused;
    int selected;

    List<Label> CurrentItems => shown == PauseScreen.Slots ? slotList.Slots : pauseItems;

    void Start()
    {
        VisualElement root = document.rootVisualElement;
        audioSource.ignoreListenerPause = true;
        pause = root.Q("Pause");
        slots = root.Q("Slots");
        optionsScreen = root.Q("Options");
        slotList = new SaveSlotList(slots);
        options = new OptionsMenu(optionsScreen);
        confirm = new ConfirmBox(root.Q("Confirm"));
        pauseItems = pause.Query<Label>(className: "item").ToList();
        pauseItems[0].AddToClassList(SelectedClass);
    }

    void Update()
    {
        if (options.IsBlockingInput) return;
        if (confirm.IsOpen) { confirm.ReadAnswer(audioSource, submitSound, cancelSound); return; }
        if (Input.GetButtonDown("Cancel")) { if (shown == PauseScreen.Main) SetPaused(!isPaused); else GoBackToPause(); return; }
        if (!isPaused) return;
        if (ConfirmBox.IsEraseKeyPressed) { AskToErase(); return; }
        MoveSelection();
        if (Input.GetButtonDown("Submit")) Submit();
    }

    void MoveSelection()
    {
        int step = menuKeys.Repeat(shown == PauseScreen.Options ? OptionsMenu.StepFor(MenuKeys.Horizontal, MenuKeys.Vertical) : MenuKeys.Vertical);
        if (step == 0) return;
        if (shown == PauseScreen.Options) options.Move(step);
        else Select((selected + step + CurrentItems.Count) % CurrentItems.Count);
        audioSource.PlayOneShot(selectionSound);
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

    void GoBackToPause()
    {
        audioSource.PlayOneShot(cancelSound);
        ShowScreen(PauseScreen.Main);
    }

    void ShowScreen(PauseScreen next)
    {
        if (shown == PauseScreen.Options)
        {
            options.Close();
            SceneManager.Instance.Player.ApplyKeyBindings();
        }
        else CurrentItems[selected].RemoveFromClassList(SelectedClass);
        shown = next;
        selected = 0;
        menuKeys.Release();
        if (next == PauseScreen.Options) options.Open();
        else CurrentItems[selected].AddToClassList(SelectedClass);
        pause.EnableInClassList(HiddenClass, next != PauseScreen.Main);
        slots.EnableInClassList(HiddenClass, next != PauseScreen.Slots);
        optionsScreen.EnableInClassList(HiddenClass, next != PauseScreen.Options);
    }

    void Submit()
    {
        switch (shown)
        {
            case PauseScreen.Main: SubmitPauseItem(); break;
            case PauseScreen.Slots: LoadSelectedSlot(); break;
            case PauseScreen.Options: audioSource.PlayOneShot(submitSound); options.Submit(); break;
            default: throw new NotImplementedException(shown.ToString());
        }
    }

    void SubmitPauseItem()
    {
        switch (selected)
        {
            case ResumeIndex: SetPaused(false); break;
            case LoadIndex: audioSource.PlayOneShot(submitSound); slotList.ShowForLoad(); ShowScreen(PauseScreen.Slots); break;
            case OptionsIndex: audioSource.PlayOneShot(submitSound); ShowScreen(PauseScreen.Options); break;
            case MainMenuIndex: SetFrozen(false); UnitySceneManager.LoadScene(MenuScene); break;
            case ExitIndex: QuitGame(); break;
            default: throw new NotImplementedException(selected.ToString());
        }
    }

    void AskToErase()
    {
        switch (shown)
        {
            case PauseScreen.Options: options.AskToReset(confirm); audioSource.PlayOneShot(submitSound); break;
            case PauseScreen.Slots: audioSource.PlayOneShot(slotList.AskToDelete(selected, confirm) ? submitSound : cancelSound); break;
            case PauseScreen.Main: break;
            default: throw new NotImplementedException(shown.ToString());
        }
    }

    void LoadSelectedSlot()
    {
        if (!SaveFile.Load(selected)) { audioSource.PlayOneShot(cancelSound); return; }
        SetFrozen(false);
        UnitySceneManager.LoadScene(SaveFile.Current.lastScene);
    }
}