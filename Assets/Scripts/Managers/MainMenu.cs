using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Utils;

[RequireComponent(typeof(UIDocument))]
public class MainMenu : MonoBehaviour
{
    enum MenuScreen { Main, Account, Difficulty, Tips, Loading }
    const string HiddenClass = "hidden", SelectedClass = "selected", DimClass = "dim";
    const float SelectionDelay = 0.2f;
    const int KeyColumns = 7, DeleteKeyIndex = 26, SaveKeyIndex = 27, NewGameIndex = 0, LoadSavedIndex = 1, TipsIndex = 2, ExitIndex = 3;
    static readonly string[] Difficulties = { "Easy", "Medium", "Hard" };

    [SerializeField] AudioClip selectionSound, submitSound, cancelSound, keyboardSound;
    [SerializeField] AudioSource audioSource;
    readonly VisualElement[] screens = new VisualElement[5];
    readonly List<Label>[] items = new List<Label>[5];
    VisualElement background, loadingFill;
    Label accountName, loadingText;
    MenuScreen current;
    int selected, lastLoadingPercent = -1;
    float selectionDelay;

    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        background = root.Q("Background");
        accountName = root.Q<Label>("AccountName");
        loadingText = root.Q<Label>("LoadingText");
        loadingFill = root.Q("LoadingFill");
        screens[(int)MenuScreen.Main] = root.Q("Main");
        screens[(int)MenuScreen.Account] = root.Q("Account");
        screens[(int)MenuScreen.Difficulty] = root.Q("Difficulty");
        screens[(int)MenuScreen.Tips] = root.Q("Tips");
        screens[(int)MenuScreen.Loading] = root.Q("Loading");
        items[(int)MenuScreen.Main] = screens[(int)MenuScreen.Main].Query<Label>(className: "item").ToList();
        items[(int)MenuScreen.Account] = screens[(int)MenuScreen.Account].Query<Label>(className: "key").ToList();
        items[(int)MenuScreen.Difficulty] = screens[(int)MenuScreen.Difficulty].Query<Label>(className: "item").ToList();
        items[(int)MenuScreen.Tips] = new List<Label>();
        items[(int)MenuScreen.Loading] = new List<Label>();
        items[(int)MenuScreen.Main][0].AddToClassList(SelectedClass);
    }

    void Update()
    {
        selectionDelay = Mathf.Max(0, selectionDelay - Time.deltaTime);
        if (current == MenuScreen.Loading) return;
        if (Input.GetButtonDown("Cancel") && current != MenuScreen.Main) { GoBackToMain(); return; }
        Navigate();
        bool submitted = current == MenuScreen.Account ? Input.GetButton("Submit") : Input.GetButtonDown("Submit");
        if (submitted && selectionDelay == 0) Submit();
    }

    void Navigate()
    {
        List<Label> list = items[(int)current];
        if (selectionDelay > 0 || list.Count == 0) return;
        int columns = current == MenuScreen.Account ? KeyColumns : 1;
        float horizontal = Input.GetAxis("Horizontal"), vertical = Input.GetAxis("Vertical");
        int step = columns > 1 && horizontal != 0 ? (horizontal > 0 ? 1 : -1) : vertical != 0 ? (vertical > 0 ? -columns : columns) : 0;
        if (step == 0) return;
        selectionDelay = SelectionDelay;
        list[selected].RemoveFromClassList(SelectedClass);
        selected = (selected + step + list.Count) % list.Count;
        list[selected].AddToClassList(SelectedClass);
        audioSource.PlayOneShot(selectionSound);
    }

    void Submit()
    {
        selectionDelay = SelectionDelay;
        switch (current)
        {
            case MenuScreen.Main: SubmitMainItem(); break;
            case MenuScreen.Account: SubmitKey(); break;
            case MenuScreen.Difficulty: StartGame(); break;
            case MenuScreen.Tips: break;
            case MenuScreen.Loading: break;
            default: throw new NotImplementedException(current.ToString());
        }
    }

    void SubmitMainItem()
    {
        switch (selected)
        {
            case NewGameIndex: audioSource.PlayOneShot(submitSound); ShowScreen(MenuScreen.Account); break;
            case LoadSavedIndex: audioSource.PlayOneShot(cancelSound); break;
            case TipsIndex: audioSource.PlayOneShot(submitSound); ShowScreen(MenuScreen.Tips); break;
            case ExitIndex: Exit(); break;
            default: throw new NotImplementedException(selected.ToString());
        }
    }

    void SubmitKey()
    {
        if (selected == DeleteKeyIndex)
        {
            audioSource.PlayOneShot(cancelSound);
            if (accountName.text.Length > 0) accountName.text = accountName.text[..^1];
        }
        else if (selected == SaveKeyIndex)
        {
            if (accountName.text.Length == 0) { audioSource.PlayOneShot(cancelSound); return; }
            audioSource.PlayOneShot(submitSound);
            ShowScreen(MenuScreen.Difficulty);
        }
        else
        {
            audioSource.PlayOneShot(keyboardSound);
            accountName.text += items[(int)MenuScreen.Account][selected].text;
        }
    }

    void StartGame()
    {
        PlayerPrefs.SetString("Account", accountName.text);
        PlayerPrefs.SetString(accountName.text + "_Difficulty", Difficulties[selected]);
        audioSource.PlayOneShot(selectionSound);
        ShowScreen(MenuScreen.Loading);
        StartCoroutine(LoadSceneAsync("Level1", ShowLoadingProgress, 0.5f));
    }

    void GoBackToMain()
    {
        accountName.text = "";
        audioSource.PlayOneShot(cancelSound);
        ShowScreen(MenuScreen.Main);
    }

    void ShowScreen(MenuScreen next)
    {
        screens[(int)current].AddToClassList(HiddenClass);
        screens[(int)next].RemoveFromClassList(HiddenClass);
        background.EnableInClassList(DimClass, next != MenuScreen.Main);
        foreach (Label item in items[(int)current]) item.RemoveFromClassList(SelectedClass);
        current = next;
        selected = 0;
        selectionDelay = SelectionDelay;
        if (items[(int)next].Count > 0) items[(int)next][0].AddToClassList(SelectedClass);
    }

    void ShowLoadingProgress(float progress)
    {
        int percent = Mathf.RoundToInt(progress * 100);
        if (percent == lastLoadingPercent) return;
        lastLoadingPercent = percent;
        loadingText.text = $"Loading {percent}%";
        loadingFill.style.width = Length.Percent(percent);
    }

    void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}