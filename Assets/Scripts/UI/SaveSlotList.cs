using System.Collections.Generic;
using UnityEngine.UIElements;

public class SaveSlotList
{
    const string LoadHeading = "Load Game", LoadHint = "Enter: load     Esc: back";
    const string NewGameHeading = "New Game: choose a slot", NewGameHint = "Enter: start here     Esc: back";
    const string OverwritePrompt = "Enter again: overwrite", EmptyText = "Empty", WarningClass = "warning";
    readonly Label heading, hint;
    int slotToOverwrite = -1;

    public List<Label> Slots { get; }

    public SaveSlotList(VisualElement root)
    {
        heading = root.Q<Label>("SlotsHeading");
        hint = root.Q<Label>("SlotsHint");
        Slots = root.Query<Label>(className: "slot").ToList();
    }

    public void ShowForLoad() => Show(LoadHeading, LoadHint);

    public void ShowForNewGame() => Show(NewGameHeading, NewGameHint);

    public void CancelOverwrite()
    {
        if (slotToOverwrite < 0) return;
        int slot = slotToOverwrite;
        slotToOverwrite = -1;
        ShowSlotText(slot);
    }

    // An empty slot is free at once; a filled one asks for a second press and returns true only on that second press
    public bool ConfirmOverwrite(int slot)
    {
        if (SaveFile.Read(slot) == null || slotToOverwrite == slot) return true;
        CancelOverwrite();
        slotToOverwrite = slot;
        Slots[slot].text = OverwritePrompt;
        Slots[slot].AddToClassList(WarningClass);
        return false;
    }

    void Show(string headingText, string hintText)
    {
        heading.text = headingText;
        hint.text = hintText;
        slotToOverwrite = -1;
        for (int slot = 0; slot < Slots.Count; slot++) ShowSlotText(slot);
    }

    void ShowSlotText(int slot)
    {
        Slots[slot].RemoveFromClassList(WarningClass);
        SaveData data = SaveFile.Read(slot);
        Slots[slot].text = data == null ? EmptyText : data.account;
    }
}