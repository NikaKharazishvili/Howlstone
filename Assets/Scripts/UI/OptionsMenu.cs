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
    readonly List<Label> valueLabels = new();
    readonly VisualElement[] pages;
    readonly List<VisualElement>[] pageItems = { new(), new() };
    readonly VisualElement pageLeft, pageRight;
    readonly Label pageNumber;
    int page, selected;

    public bool IsWaitingForKey => keys.IsWaitingForKey;

    List<VisualElement> Items => pageItems[page];
    int ResetItem => Items.Count - 2;
    int PagerItem => Items.Count - 1;

    public OptionsMenu(VisualElement root)
    {
        keys = new KeyBindingList(root);
        pages = new[] { root.Q("SettingsPage"), root.Q("KeysPage") };
        pageLeft = root.Q("PageLeft");
        pageRight = root.Q("PageRight");
        pageNumber = root.Q<Label>("PageNumber");
        AddSettingRows(root.Q("Settings"));
        VisualElement pager = root.Q("Pager"), reset = root.Q("ResetOptions");
        pageItems[SettingsPage].AddRange(valueLabels);
        pageItems[KeysPage].AddRange(keys.Cells);
        foreach (List<VisualElement> items in pageItems) { items.Add(reset); items.Add(pager); }
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
        else if (selected == ResetItem) ResetShownPage();
        else if (page == KeysPage) { keys.WaitForNewKey(selected, finished); return; }
        else if (settings[selected] == Setting.MuteInBackground) ChangeSelectedSetting(1);
        finished();
    }

    // On the keys page a row has two cells; after the rows come the reset entry and the pager
    int NextRow(int direction)
    {
        if (page == SettingsPage) return (selected + direction + Items.Count) % Items.Count;
        if (selected == ResetItem) return direction > 0 ? PagerItem : keys.Cells.Count - CellsPerRow;
        if (selected == PagerItem) return direction > 0 ? 0 : ResetItem;
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

    void AddSettingRows(VisualElement table)
    {
        foreach (Setting setting in settings)
        {
            VisualElement row = AddElement(table, new VisualElement(), "hotkey-row");
            Label name = AddElement(row, new Label(GameSettings.NameOf(setting)), "hotkey-cell");
            name.AddToClassList("setting-name");
            VisualElement holder = AddElement(row, new VisualElement(), "hotkey-cell");
            holder.AddToClassList("setting-value");
            valueLabels.Add(AddElement(holder, new Label(GameSettings.ValueText(setting)), "binding"));
        }
    }

    static T AddElement<T>(VisualElement parent, T element, string className) where T : VisualElement
    {
        element.pickingMode = PickingMode.Ignore;
        element.AddToClassList(className);
        parent.Add(element);
        return element;
    }

    void ChangeSelectedSetting(int direction)
    {
        GameSettings.Change(settings[selected], direction);
        valueLabels[selected].text = GameSettings.ValueText(settings[selected]);
    }

    void ResetShownPage()
    {
        if (page == KeysPage) { keys.ResetToDefault(); return; }
        GameSettings.ResetToDefault();
        for (int i = 0; i < settings.Length; i++) valueLabels[i].text = GameSettings.ValueText(settings[i]);
    }
}