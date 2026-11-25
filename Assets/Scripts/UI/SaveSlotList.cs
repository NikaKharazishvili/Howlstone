using System.Collections.Generic;
using UnityEngine.UIElements;

public class SaveSlotList
{
    const string LoadHeading = "Load Game", LoadHint = "Enter: load     Esc: back";
    const string NewGameHeading = "New Game: choose a slot", NewGameHint = "Enter: choose     Esc: back";
    const string EmptyText = "Empty";
    readonly Label heading, hint;

    public List<Label> Slots { get; }

    public SaveSlotList(VisualElement root)
    {
        heading = root.Q<Label>("SlotsHeading");
        hint = root.Q<Label>("SlotsHint");
        Slots = root.Query<Label>(className: "slot").ToList();
    }

    public void ShowForLoad() => Show(LoadHeading, LoadHint);

    public void ShowForNewGame() => Show(NewGameHeading, NewGameHint);

    void Show(string headingText, string hintText)
    {
        heading.text = headingText;
        hint.text = hintText;
        for (int slot = 0; slot < Slots.Count; slot++)
        {
            SaveData data = SaveFile.Read(slot);
            Slots[slot].text = data == null ? EmptyText : data.account;
        }
    }
}