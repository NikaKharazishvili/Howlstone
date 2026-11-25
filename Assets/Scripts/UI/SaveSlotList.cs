using System.Collections.Generic;
using UnityEngine.UIElements;

public class SaveSlotList
{
    const string ErasePrompt = "Press Delete again to erase", EmptyText = "Empty", EraseClass = "erase";
    readonly Label heading;
    int slotToErase = -1;

    public List<Label> Slots { get; }

    public SaveSlotList(VisualElement root)
    {
        heading = root.Q<Label>("SlotsHeading");
        Slots = root.Query<Label>(className: "slot").ToList();
    }

    public void Show(string headingText)
    {
        heading.text = headingText;
        slotToErase = -1;
        for (int slot = 0; slot < Slots.Count; slot++) ShowSlotText(slot);
    }

    public void CancelErase()
    {
        if (slotToErase < 0) return;
        int slot = slotToErase;
        slotToErase = -1;
        ShowSlotText(slot);
    }

    // The first press asks to press again; returns true when the slot was erased by the second press
    public bool PressErase(int slot)
    {
        if (SaveFile.Read(slot) == null) return false;
        if (slotToErase == slot)
        {
            SaveFile.Delete(slot);
            slotToErase = -1;
            ShowSlotText(slot);
            return true;
        }
        CancelErase();
        slotToErase = slot;
        Slots[slot].text = ErasePrompt;
        Slots[slot].AddToClassList(EraseClass);
        return false;
    }

    void ShowSlotText(int slot)
    {
        Slots[slot].RemoveFromClassList(EraseClass);
        SaveData data = SaveFile.Read(slot);
        Slots[slot].text = data == null ? EmptyText : data.account;
    }
}