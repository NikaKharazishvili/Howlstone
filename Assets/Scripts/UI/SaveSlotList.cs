using System.Collections.Generic;
using System.Globalization;
using UnityEngine.UIElements;

public class SaveSlotList
{
    const string LoadHint = "Load: Enter /", NewGameHint = "Choose: Enter /", EmptyText = "------", TimeFormat = "d MMM yyyy,  HH:mm", OffClass = "off", EmptyClass = "empty";
    readonly Label submitHint;
    readonly VisualElement deleteHint;

    public List<Label> Slots { get; }

    public SaveSlotList(VisualElement root)
    {
        submitHint = root.Q<Label>("SlotsSubmitHint");
        deleteHint = root.Q("SlotsDeleteHint");
        Slots = root.Query<Label>(className: "slot").ToList();
    }

    public void ShowForLoad() => Show(LoadHint, true);

    public void ShowForNewGame() => Show(NewGameHint, false);

    public bool AskToDelete(int slot, ConfirmBox confirm)
    {
        if (SaveFile.Read(slot) == null) return false;
        confirm.Ask("Do you really want to delete this save?", () => { SaveFile.Delete(slot); ShowForLoad(); });
        return true;
    }

    void Show(string submitHintText, bool canDelete)
    {
        submitHint.text = submitHintText;
        deleteHint.EnableInClassList(OffClass, !canDelete);
        for (int slot = 0; slot < Slots.Count; slot++)
        {
            SaveData data = SaveFile.Read(slot);
            Slots[slot].EnableInClassList(EmptyClass, data == null);
            Slots[slot].text = data == null ? EmptyText : $"{data.account}\n<size=60%>{data.WrittenAt.ToString(TimeFormat, CultureInfo.InvariantCulture)}</size>";
        }
    }
}