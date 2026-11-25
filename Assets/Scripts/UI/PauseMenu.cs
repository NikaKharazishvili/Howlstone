using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Utils;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class PauseMenu : MonoBehaviour
{
    const string HiddenClass = "hidden", SelectedClass = "selected", MenuScene = "Menu";
    const int ResumeIndex = 0, SaveIndex = 1, LoadIndex = 2, MainMenuIndex = 3, ExitIndex = 4;

    [SerializeField] UIDocument document;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip selectionSound, submitSound, cancelSound;
    VisualElement pause;
    List<Label> items;
    bool isPaused;
    int selected, heldStep;

    void Start()
    {
        pause = document.rootVisualElement.Q("Pause");
        items = pause.Query<Label>(className: "item").ToList();
        items[0].AddToClassList(SelectedClass);
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel")) { SetPaused(!isPaused); return; }
        if (!isPaused) return;
        MoveSelection();
        if (Input.GetButtonDown("Submit")) Submit();
    }

    void MoveSelection()
    {
        float vertical = Input.GetAxisRaw("Vertical");
        int step = vertical > 0 ? -1 : vertical < 0 ? 1 : 0;
        if (step != 0 && step != heldStep) Select((selected + step + items.Count) % items.Count);
        heldStep = step;
    }

    void Select(int index)
    {
        items[selected].RemoveFromClassList(SelectedClass);
        selected = index;
        items[selected].AddToClassList(SelectedClass);
        audioSource.PlayOneShot(selectionSound);
    }

    void SetPaused(bool paused)
    {
        isPaused = paused;
        Time.timeScale = paused ? 0 : 1;
        pause.EnableInClassList(HiddenClass, !paused);
        items[selected].RemoveFromClassList(SelectedClass);
        selected = 0;
        items[selected].AddToClassList(SelectedClass);
        audioSource.PlayOneShot(paused ? submitSound : cancelSound);
    }

    void Submit()
    {
        switch (selected)
        {
            case ResumeIndex: SetPaused(false); break;
            case SaveIndex: SaveGame(); break;
            case LoadIndex: LoadGame(); break;
            case MainMenuIndex: Time.timeScale = 1; UnitySceneManager.LoadScene(MenuScene); break;
            case ExitIndex: QuitGame(); break;
            default: throw new NotImplementedException(selected.ToString());
        }
    }

    void SaveGame()
    {
        SceneManager.Instance.SaveGame();
        SetPaused(false);
        SceneManager.Instance.Hud.ShowEvent("Game saved", Color.green);
    }

    void LoadGame()
    {
        if (!SaveFile.ReloadCurrent()) { audioSource.PlayOneShot(cancelSound); return; }
        Time.timeScale = 1;
        UnitySceneManager.LoadScene(SaveFile.Current.lastScene);
    }
}