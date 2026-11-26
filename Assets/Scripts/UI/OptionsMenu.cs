using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class OptionsMenu
{
    // A move step of this size is up or down, a step of 1 is left or right
    public const int VerticalStep = 2;

    const string SelectedClass = "selected", PageBeforeClass = "page-before", PageAfterClass = "page-after", OffClass = "off";
    const int SettingsPage = 0, KeysPage = 1, CellsPerRow = 2;

    readonly KeyBindingList keys;
    readonly Setting[] settings = (Setting[])Enum.GetValues(typeof(Setting));
    readonly List<Label> settingLabels = new();
    readonly VisualElement[] pages;
    readonly List<VisualElement>[] pageItems = { new(), new() };
    readonly VisualElement pageLeft, pageRight;
    readonly Label pageNumber;
    int page, selected;

    public bool IsWaitingForKey => keys.IsWaitingForKey;

    List<VisualElement> Items => pageItems[page];
    int PagerItem => Items.Count - 1;
    int ResetItem => Items.Count - 2;

    public OptionsMenu(VisualElement root)
    {
        keys = new KeyBindingList(root);
        pages = new[] { root.Q("SettingsPage"), root.Q("KeysPage") };
        pageLeft = root.Q("PageLeft");
        pageRight = root.Q("PageRight");
        pageNumber = root.Q<Label>("PageNumber");
        VisualElement settingList = root.Q("Settings"), pager = root.Q("Pager");
        foreach (Setting setting in settings)
        {
            Label label = new(GameSettings.Describe(setting)) { pickingMode = PickingMode.Ignore };
            label.AddToClassList("item");
            label.AddToClassList("setting");
            settingList.Add(label);
            settingLabels.Add(label);
        }
        pageItems[SettingsPage].AddRange(settingLabels);
        pageItems[SettingsPage].Add(root.Q("ResetSettings"));
        pageItems[SettingsPage].Add(pager);
        pageItems[KeysPage].AddRange(keys.Cells);
        pageItems[KeysPage].Add(root.Q("ResetBindings"));
        pageItems[KeysPage].Add(pager);
        ShowPage(SettingsPage);
    }

    // The page stays as it was left; the first entry of it is selected
    public void Open() => Select(0);

    public void Close() => Items[selected].RemoveFromClassList(SelectedClass);

    public void Move(int step)
    {
        if (Math.Abs(step) == VerticalStep) Select(NextRow(Math.Sign(step)));
        else if (selected == PagerItem) SwitchPage(page + step);
        else if (selected == ResetItem) return;
        else if (page == SettingsPage) ChangeSelectedSetting(step);
        else Select((selected + step + keys.Cells.Count) % keys.Cells.Count);
    }

    public void Submit(Action finished)
    {
        if (selected == PagerItem) SwitchPage(page == SettingsPage ? KeysPage : SettingsPage);
        else if (page == KeysPage && selected != ResetItem) { keys.WaitForNewKey(selected, finished); return; }
        else if (page == KeysPage) keys.ResetToDefault();
        else if (selected == ResetItem) ResetSettings();
        else if (settings[selected] == Setting.MuteInBackground) ChangeSelectedSetting(1);
        finished();
    }

    // On the keys page a row has two cells, then come the reset entry and the pager
    int NextRow(int direction)
    {
        if (page == SettingsPage) return (selected + direction + Items.Count) % Items.Count;
        if (selected == PagerItem) return direction > 0 ? 0 : ResetItem;
        if (selected == ResetItem) return direction > 0 ? PagerItem : keys.Cells.Count - CellsPerRow;
        int target = selected + direction * CellsPerRow;
        return target < 0 ? PagerItem : target >= keys.Cells.Count ? ResetItem : target;
    }

    void Select(int item)
    {
        Items[selected].RemoveFromClassList(SelectedClass);
        selected = item;
        Items[selected].AddToClassList(SelectedClass);
    }

    void SwitchPage(int newPage)
    {
        if (newPage < 0 || newPage >= pages.Length) return;
        Close();
        ShowPage(newPage);
        selected = PagerItem;
        Items[selected].AddToClassList(SelectedClass);
    }

    void ShowPage(int newPage)
    {
        page = newPage;
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].EnableInClassList(PageBeforeClass, i < page);
            pages[i].EnableInClassList(PageAfterClass, i > page);
        }
        pageLeft.EnableInClassList(OffClass, page == 0);
        pageRight.EnableInClassList(OffClass, page == pages.Length - 1);
        pageNumber.text = $"{page + 1} / {pages.Length}";
    }

    void ChangeSelectedSetting(int direction)
    {
        GameSettings.Change(settings[selected], direction);
        settingLabels[selected].text = GameSettings.Describe(settings[selected]);
    }

    void ResetSettings()
    {
        GameSettings.ResetToDefault();
        for (int i = 0; i < settings.Length; i++) settingLabels[i].text = GameSettings.Describe(settings[i]);
    }
}