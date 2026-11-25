using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Utils;

[RequireComponent(typeof(UIDocument))]
public class MenuManager : MonoBehaviour
{
    enum MenuScreen { Main, Account, Difficulty, Tips, Loading, Slots }
    const string HiddenClass = "hidden", PageBeforeClass = "page-before", PageAfterClass = "page-after", SelectedClass = "selected", DimClass = "dim", OffClass = "off", InstantClass = "instant";
    const int MaxAccountNameLength = 16;
    const float SubmitDelay = 0.2f, FirstRepeatDelay = 0.4f, RepeatDelay = 0.12f;
    const int KeyColumns = 7, DeleteKeyIndex = 26, SaveKeyIndex = 27, NewGameIndex = 0, LoadSavedIndex = 1, TipsIndex = 2, ExitIndex = 3;
    static readonly string[] Difficulties = { "Easy", "Medium", "Hard" };

    [SerializeField] AudioClip selectionSound, submitSound, cancelSound, keyboardSound;
    [SerializeField] AudioSource audioSource;
    [SerializeField] UIDocument document;
    static readonly int ScreenCount = Enum.GetValues(typeof(MenuScreen)).Length;
    readonly VisualElement[] screens = new VisualElement[ScreenCount];
    readonly List<Label>[] items = new List<Label>[ScreenCount];
    SaveSlotList slotList;
    List<VisualElement> tipPages;
    List<string> tipTitles;
    VisualElement background, loadingFill, pageLeft, pageRight;
    Label accountName, loadingText, pageNumber, tipTitle;
    MenuScreen current;
    string chosenDifficulty;
    bool isChoosingNewGameSlot;
    int selected, tipPage, heldStep, lastLoadingPercent = -1;
    float submitDelay, repeatTimer;

    void Start()
    {
        VisualElement root = document.rootVisualElement;
        background = root.Q("Background");
        accountName = root.Q<Label>("AccountName");
        pageNumber = root.Q<Label>("PageNumber");
        tipTitle = root.Q<Label>("TipTitle");
        pageLeft = root.Q("PageLeft");
        pageRight = root.Q("PageRight");
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
        screens[(int)MenuScreen.Slots] = root.Q("Slots");
        slotList = new SaveSlotList(screens[(int)MenuScreen.Slots]);
        items[(int)MenuScreen.Slots] = slotList.Slots;
        tipPages = screens[(int)MenuScreen.Tips].Query(className: "tip-page").ToList();
        tipTitles = tipPages.ConvertAll(page => page.Q<Label>(className: "tip-page-title").text);
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
            case MenuScreen.Main: case MenuScreen.Difficulty: case MenuScreen.Slots: return verticalStep;
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
            if (current == MenuScreen.Slots) slotList.CancelOverwrite();
            List<Label> list = items[(int)current];
            list[selected].RemoveFromClassList(SelectedClass);
            selected = (selected + step + list.Count) % list.Count;
            list[selected].AddToClassList(SelectedClass);
        }
        audioSource.PlayOneShot(selectionSound);
    }

    void ShowFirstTipPageWithoutSliding()
    {
        VisualElement tips = screens[(int)MenuScreen.Tips];
        tips.AddToClassList(InstantClass);
        ShowTipPage(0);
        tips.schedule.Execute(() => tips.RemoveFromClassList(InstantClass)).ExecuteLater(60);
    }

    void ShowTipPage(int page)
    {
        tipPage = page;
        for (int i = 0; i < tipPages.Count; i++)
        {
            tipPages[i].EnableInClassList(PageBeforeClass, i < page);
            tipPages[i].EnableInClassList(PageAfterClass, i > page);
        }
        pageLeft.EnableInClassList(OffClass, page == 0);
        pageRight.EnableInClassList(OffClass, page == tipPages.Count - 1);
        tipTitle.text = tipTitles[page];
        pageNumber.text = $"{page + 1} / {tipPages.Count}";
    }

    void Submit()
    {
        submitDelay = SubmitDelay;
        switch (current)
        {
            case MenuScreen.Main: SubmitMainItem(); break;
            case MenuScreen.Account: SubmitKey(); break;
            case MenuScreen.Difficulty: ChooseDifficulty(); break;
            case MenuScreen.Slots: if (isChoosingNewGameSlot) StartGame(); else LoadSelectedSlot(); break;
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
            case LoadSavedIndex: audioSource.PlayOneShot(submitSound); ShowSlots(false); break;
            case TipsIndex: audioSource.PlayOneShot(submitSound); ShowScreen(MenuScreen.Tips); break;
            case ExitIndex: QuitGame(); break;
            default: throw new NotImplementedException(selected.ToString());
        }
    }

    void ChooseDifficulty()
    {
        chosenDifficulty = Difficulties[selected];
        audioSource.PlayOneShot(submitSound);
        ShowSlots(true);
    }

    void ShowSlots(bool forNewGame)
    {
        isChoosingNewGameSlot = forNewGame;
        if (forNewGame) slotList.ShowForNewGame(); else slotList.ShowForLoad();
        ShowScreen(MenuScreen.Slots);
    }

    void LoadSelectedSlot()
    {
        if (!SaveFile.Load(selected)) { audioSource.PlayOneShot(cancelSound); return; }
        audioSource.PlayOneShot(submitSound);
        LoadSavedScene();
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
        else if (accountName.text.Length < MaxAccountNameLength)
        {
            audioSource.PlayOneShot(keyboardSound);
            accountName.text += items[(int)MenuScreen.Account][selected].text;
        }
        else audioSource.PlayOneShot(cancelSound);
    }

    void StartGame()
    {
        if (!slotList.ConfirmOverwrite(selected)) { audioSource.PlayOneShot(cancelSound); return; }
        SaveFile.StartNew(selected, accountName.text, chosenDifficulty);
        audioSource.PlayOneShot(selectionSound);
        LoadSavedScene();
    }

    void LoadSavedScene()
    {
        ShowScreen(MenuScreen.Loading);
        StartCoroutine(LoadSceneAsync(SaveFile.Current.lastScene, ShowLoadingProgress, 0.5f));
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
        if (next == MenuScreen.Tips) ShowFirstTipPageWithoutSliding();
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
}