using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Utils;

[RequireComponent(typeof(UIDocument))]
public class MenuManager : MonoBehaviour
{
    enum MenuScreen { Main, Account, Difficulty, Tips, Loading }
    const string HiddenClass = "hidden", PageBeforeClass = "page-before", PageAfterClass = "page-after", SelectedClass = "selected", DimClass = "dim";
    const float SubmitDelay = 0.2f, FirstRepeatDelay = 0.4f, RepeatDelay = 0.12f;
    const int KeyColumns = 7, DeleteKeyIndex = 26, SaveKeyIndex = 27, NewGameIndex = 0, LoadSavedIndex = 1, TipsIndex = 2, ExitIndex = 3;
    static readonly string[] Difficulties = { "Easy", "Medium", "Hard" };

    [SerializeField] AudioClip selectionSound, submitSound, cancelSound, keyboardSound;
    [SerializeField] AudioSource audioSource;
    [SerializeField] UIDocument document;
    readonly VisualElement[] screens = new VisualElement[5];
    readonly List<Label> [] items = new List<Label>[5];
    List<VisualElement> tipPages;
    VisualElement background, loadingFill;
    Label accountName, loadingText, pageNumber;
    MenuScreen current;
    int selected, tipPage, heldStep, lastLoadingPercent = -1;
    float submitDelay, repeatTimer;

    void Start()
    {
        VisualElement root = document.rootVisualElement;
        background = root.Q("Background");
        accountName = root.Q<Label>("AccountName");
        pageNumber = root.Q<Label>("PageNumber");
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
        tipPages = screens[(int)MenuScreen.Tips].Query(className: "tip-page").ToList();
        items[(int)MenuScreen.Main][0].AddToClassList(SelectedClass);
    }

    void Update()
    {
        submitDelay = Mathf.Max(0, submitDelay - Time.deltaTime);
        if (current == MenuScreen.Loading) return;
        if (Input.GetButtonDown("Cancel") && current != MenuScreen.Main) { GoBackToMain(); return; }
        MoveByHeldKey();
        bool submitted = current == MenuScreen.Account ? Input.GetButton("Submit") : Input.GetButtonDown("Submit");
        if (submitted && submitDelay == 0) Submit();
    }

    void MoveByHeldKey()
    {
        int step = ReadStep();
        if (step == 0) { heldStep = 0; return; }
        if (step == heldStep)
        {
            repeatTimer -= Time.deltaTime;
            if (repeatTimer > 0) return;
            repeatTimer = RepeatDelay;
        }
        else
        {
            heldStep = step;
            repeatTimer = FirstRepeatDelay;
        }
        Move(step);
    }

    int ReadStep()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"), vertical = Input.GetAxisRaw("Vertical");
        int horizontalStep = horizontal > 0 ? 1 : horizontal < 0 ? -1 : 0, verticalStep = vertical > 0 ? -1 : vertical < 0 ? 1 : 0;
        switch (current)
        {
            case MenuScreen.Main: case MenuScreen.Difficulty: return verticalStep;
            case MenuScreen.Account: return horizontalStep != 0 ? horizontalStep : verticalStep * KeyColumns;
            case MenuScreen.Tips: return horizontalStep;
            case MenuScreen.Loading: return 0;
            default: throw new NotImplementedException(current.ToString());
        }
    }

    void Move(int step)
    {
        if (current == MenuScreen.Tips)
        {
            int page = tipPage + step;
            if (page < 0 || page >= tipPages.Count) return;
            ShowTipPage(page);
        }
        else
        {
            List<Label> list = items[(int)current];
            list[selected].RemoveFromClassList(SelectedClass);
            selected = (selected + step + list.Count) % list.Count;
            list[selected].AddToClassList(SelectedClass);
        }
        audioSource.PlayOneShot(selectionSound);
    }

    void ShowTipPage(int page)
    {
        tipPage = page;
        for (int i = 0; i < tipPages.Count; i++)
        {
            tipPages[i].EnableInClassList(PageBeforeClass, i < page);
            tipPages[i].EnableInClassList(PageAfterClass, i > page);
        }
        pageNumber.text = $"{page + 1} / {tipPages.Count}";
    }

    void Submit()
    {
        submitDelay = SubmitDelay;
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
        submitDelay = SubmitDelay;
        heldStep = 0;
        if (next == MenuScreen.Tips) ShowTipPage(0);
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