using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Utils;

[RequireComponent(typeof(UIDocument))]
public class MenuManager : MonoBehaviour
{
    enum MenuScreen { Main, Account, Options, Loading, LoadSlots, NewGameSlots }
    const string HiddenClass = "hidden", SelectedClass = "selected", DimClass = "dim";
    const int MaxAccountNameLength = 16;
    const float SubmitDelay = 0.2f;
    const int KeyColumns = 7, DeleteKeyIndex = 26, SaveKeyIndex = 27, NewGameIndex = 0, LoadSavedIndex = 1, OptionsIndex = 2, ExitIndex = 3;

    [SerializeField] AudioClip selectionSound, submitSound, cancelSound, keyboardSound;
    [SerializeField] AudioSource audioSource, musicSource;
    [SerializeField] UIDocument document;
    static readonly int ScreenCount = Enum.GetValues(typeof(MenuScreen)).Length;
    readonly VisualElement[] screens = new VisualElement[ScreenCount];
    readonly List<Label>[] items = new List<Label>[ScreenCount];
    readonly MenuKeys menuKeys = new();
    SaveSlotList loadList, newGameList;
    OptionsMenu options;
    ConfirmBox confirm;
    VisualElement background, loadingFill;
    Label accountName, loadingText;
    MenuScreen current;
    int chosenSlot, selected, lastLoadingPercent = -1;
    float submitDelay;

    void Start()
    {
        VisualElement root = document.rootVisualElement;
        background = root.Q("Background");
        accountName = root.Q<Label>("AccountName");
        loadingText = root.Q<Label>("LoadingText");
        loadingFill = root.Q("LoadingFill");
        screens[(int)MenuScreen.Main] = root.Q("Main");
        screens[(int)MenuScreen.Account] = root.Q("Account");
        screens[(int)MenuScreen.Options] = root.Q("Options");
        screens[(int)MenuScreen.Loading] = root.Q("Loading");
        screens[(int)MenuScreen.LoadSlots] = root.Q("LoadSlots");
        screens[(int)MenuScreen.NewGameSlots] = root.Q("NewGameSlots");
        loadList = new SaveSlotList(screens[(int)MenuScreen.LoadSlots]);
        newGameList = new SaveSlotList(screens[(int)MenuScreen.NewGameSlots]);
        options = new OptionsMenu(screens[(int)MenuScreen.Options]);
        confirm = new ConfirmBox(root.Q("Confirm"));
        GameSettings.UseMusic(musicSource);
        items[(int)MenuScreen.Main] = screens[(int)MenuScreen.Main].Query<Label>(className: "item").ToList();
        items[(int)MenuScreen.Account] = screens[(int)MenuScreen.Account].Query<Label>(className: "key").ToList();
        items[(int)MenuScreen.Options] = new List<Label>();
        items[(int)MenuScreen.Loading] = new List<Label>();
        items[(int)MenuScreen.LoadSlots] = loadList.Slots;
        items[(int)MenuScreen.NewGameSlots] = newGameList.Slots;
        items[(int)MenuScreen.Main][0].AddToClassList(SelectedClass);
    }

    void Update()
    {
        submitDelay = Mathf.Max(0, submitDelay - Time.deltaTime);
        if (current == MenuScreen.Loading || options.IsBlockingInput) return;
        if (confirm.IsOpen) { confirm.ReadAnswer(audioSource, submitSound, cancelSound); return; }
        if (Input.GetButtonDown("Cancel") && current != MenuScreen.Main) { GoBackToMain(); return; }
        if (ConfirmBox.IsEraseKeyPressed) { AskToErase(); return; }
        int step = menuKeys.Repeat(ReadStep());
        if (step != 0) Move(step);
        bool submitted = current == MenuScreen.Account ? Input.GetButton("Submit") : Input.GetButtonDown("Submit");
        if (submitted && submitDelay == 0) Submit();
    }

    int ReadStep()
    {
        int horizontal = MenuKeys.Horizontal, vertical = MenuKeys.Vertical;
        switch (current)
        {
            case MenuScreen.Main: case MenuScreen.LoadSlots: case MenuScreen.NewGameSlots: return vertical;
            case MenuScreen.Account: return horizontal != 0 ? horizontal : vertical * KeyColumns;
            case MenuScreen.Options: return OptionsMenu.StepFor(horizontal, vertical);
            case MenuScreen.Loading: return 0;
            default: throw new NotImplementedException(current.ToString());
        }
    }

    void Move(int step)
    {
        if (current == MenuScreen.Options) options.Move(step);
        else
        {
            List<Label> list = items[(int)current];
            list[selected].RemoveFromClassList(SelectedClass);
            selected = (selected + step + list.Count) % list.Count;
            list[selected].AddToClassList(SelectedClass);
        }
        audioSource.PlayOneShot(selectionSound);
    }

    void Submit()
    {
        submitDelay = SubmitDelay;
        switch (current)
        {
            case MenuScreen.Main: SubmitMainItem(); break;
            case MenuScreen.Account: SubmitKey(); break;
            case MenuScreen.LoadSlots: LoadSelectedSlot(); break;
            case MenuScreen.NewGameSlots: ChooseNewGameSlot(); break;
            case MenuScreen.Options: audioSource.PlayOneShot(submitSound); options.Submit(); break;
            case MenuScreen.Loading: break;
            default: throw new NotImplementedException(current.ToString());
        }
    }

    void SubmitMainItem()
    {
        switch (selected)
        {
            case NewGameIndex: audioSource.PlayOneShot(submitSound); newGameList.ShowForNewGame(); ShowScreen(MenuScreen.NewGameSlots); break;
            case LoadSavedIndex: audioSource.PlayOneShot(submitSound); loadList.ShowForLoad(); ShowScreen(MenuScreen.LoadSlots); break;
            case OptionsIndex: audioSource.PlayOneShot(submitSound); ShowScreen(MenuScreen.Options); break;
            case ExitIndex: QuitGame(); break;
            default: throw new NotImplementedException(selected.ToString());
        }
    }

    void AskToErase()
    {
        switch (current)
        {
            case MenuScreen.Options: options.AskToReset(confirm); audioSource.PlayOneShot(submitSound); break;
            case MenuScreen.LoadSlots: audioSource.PlayOneShot(loadList.AskToDelete(selected, confirm) ? submitSound : cancelSound); break;
            case MenuScreen.Main: case MenuScreen.Account: case MenuScreen.Loading: case MenuScreen.NewGameSlots: break;
            default: throw new NotImplementedException(current.ToString());
        }
    }

    void ChooseNewGameSlot()
    {
        chosenSlot = selected;
        audioSource.PlayOneShot(submitSound);
        ShowScreen(MenuScreen.Account);
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
            StartGame();
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
        SaveFile.StartNew(chosenSlot, accountName.text);
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
        if (current == MenuScreen.Options) options.Close();
        current = next;
        selected = 0;
        submitDelay = SubmitDelay;
        menuKeys.Release();
        if (next == MenuScreen.Options) options.Open();
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