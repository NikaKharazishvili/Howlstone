using System.Collections.Generic;
using UnityEngine.UIElements;

public class SaveSlotList
{
    const string LoadHint = "Load: Enter /", NewGameHint = "Choose: Enter /", EmptyText = "Empty";
    readonly Label submitHint;

    public List<Label> Slots { get; }

    public SaveSlotList(VisualElement root)
    {
        submitHint = root.Q<Label>("SlotsSubmitHint");
        Slots = root.Query<Label>(className: "slot").ToList();
    }

    public void ShowForLoad() => Show(LoadHint);

    public void ShowForNewGame() => Show(NewGameHint);

    void Show(string submitHintText)
    {
        submitHint.text = submitHintText;
        for (int slot = 0; slot < Slots.Count; slot++)
        {
            SaveData data = SaveFile.Read(slot);
            Slots[slot].text = data == null ? EmptyText : data.account;
        }
    }
}