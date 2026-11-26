using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class KeyBindingList
{
    const string Keyboard = "<Keyboard>", Gamepad = "<Gamepad>", Mouse = "<Mouse>", AnyKey = "<Keyboard>/anyKey", CancelKey = "<Keyboard>/escape";
    const string WaitingText = "...", KeyClass = "keycap", WideKeyClass = "keycap-wide", PadButtonClass = "pad-button", PadTextClass = "pad-shoulder", DuplicateClass = "duplicate";

    static readonly Dictionary<string, string> PadIconClasses = new() { { "buttonSouth", "pad-cross" }, { "buttonEast", "pad-circle" }, { "buttonWest", "pad-square" }, { "buttonNorth", "pad-triangle" } };

    static readonly Dictionary<string, string> PadNames = new()
    {
        { "leftShoulder", "L1" }, { "rightShoulder", "R1" }, { "leftTrigger", "L2" }, { "rightTrigger", "R2" }, { "leftStickPress", "L3" }, { "rightStickPress", "R3" },
        { "dpad/up", "D Up" }, { "dpad/down", "D Down" }, { "dpad/left", "D Left" }, { "dpad/right", "D Right" }, { "start", "Options" }, { "select", "Share" }
    };

    readonly PlayerInputActions actions = KeyBindings.CreateActions();
    readonly List<(InputAction action, int index, string device)> bindings = new();
    InputActionRebindingExtensions.RebindingOperation rebinding;

    // Two cells for each row: keyboard, then gamepad
    public List<Label> Cells { get; } = new();
    public bool IsWaitingForKey => rebinding != null;

    public KeyBindingList(VisualElement root)
    {
        VisualElement table = root.Q("Bindings");
        PlayerInputActions.PlayerActions player = actions.Player;
        AddRow(table, "Move up", player.Move, "up");
        AddRow(table, "Move down", player.Move, "down");
        AddRow(table, "Move left", player.Move, "left");
        AddRow(table, "Move right", player.Move, "right");
        AddRow(table, "Camera left", player.RotateCamera, "negative");
        AddRow(table, "Camera right", player.RotateCamera, "positive");
        AddRow(table, "Sprint", player.Sprint);
        AddRow(table, "Toggle HUD", player.ToggleHud);
        AddRow(table, "Spin", player.Spin);
        AddRow(table, "Shoot", player.Shoot);
        AddRow(table, "Jump", player.Jump);
        ShowAllCells();
    }

    public void WaitForNewKey(int cell, Action finished)
    {
        (InputAction action, int index, string device) = bindings[cell];
        ClearCellLook(Cells[cell]);
        Cells[cell].text = WaitingText;
        Cells[cell].AddToClassList(device == Keyboard ? KeyClass : PadTextClass);
        rebinding = action.PerformInteractiveRebinding(index)
            .WithExpectedControlType("Button")
            .WithControlsHavingToMatchPath(device)
            .WithControlsExcluding(Mouse)
            .WithControlsExcluding(AnyKey)
            .WithCancelingThrough(CancelKey)
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(_ => { KeyBindings.Save(actions); StopWaiting(finished); })
            .OnCancel(_ => StopWaiting(finished))
            .Start();
    }

    void StopWaiting(Action finished)
    {
        rebinding.Dispose();
        rebinding = null;
        ShowAllCells();
        finished();
    }

    public void ResetToDefault()
    {
        actions.asset.RemoveAllBindingOverrides();
        KeyBindings.Save(actions);
        ShowAllCells();
    }

    void AddRow(VisualElement table, string title, InputAction action, string part = null)
    {
        VisualElement row = AddElement(table, new VisualElement(), "hotkey-row");
        AddElement(row, new Label(title), "hotkey-cell", "hotkey-action");
        foreach (string device in new[] { Keyboard, Gamepad })
        {
            VisualElement holder = AddElement(row, new VisualElement(), "hotkey-cell", device == Keyboard ? "hotkey-keys" : "hotkey-pad");
            Cells.Add(AddElement(holder, new Label(), "binding"));
            bindings.Add((action, FindBinding(action, part, device), device));
        }
    }

    static T AddElement<T>(VisualElement parent, T element, params string[] classes) where T : VisualElement
    {
        element.pickingMode = PickingMode.Ignore;
        foreach (string className in classes) element.AddToClassList(className);
        parent.Add(element);
        return element;
    }

    // The first binding of the action for that device: a part of a composite when a part name is given
    static int FindBinding(InputAction action, string part, string device)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            bool isWantedPart = part == null ? !binding.isPartOfComposite : binding.isPartOfComposite && binding.name == part;
            if (!binding.isComposite && isWantedPart && binding.path.StartsWith(device)) return i;
        }
        throw new InvalidOperationException($"{action.name} has no {part} binding for {device}");
    }

    void ShowAllCells()
    {
        for (int cell = 0; cell < bindings.Count; cell++)
        {
            (InputAction action, int index, string device) = bindings[cell];
            string path = action.bindings[index].effectivePath, control = path[(path.IndexOf('/') + 1)..];
            string readableName = InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice);
            Label label = Cells[cell];
            ClearCellLook(label);
            label.EnableInClassList(DuplicateClass, IsKeyUsedTwice(cell));
            if (device == Keyboard)
            {
                label.text = readableName;
                label.AddToClassList(KeyClass);
                label.EnableInClassList(WideKeyClass, readableName.Length > 1);
            }
            else if (PadIconClasses.TryGetValue(control, out string iconClass))
            {
                label.text = "";
                label.AddToClassList(PadButtonClass);
                label.AddToClassList(iconClass);
            }
            else
            {
                label.text = PadNames.TryGetValue(control, out string padName) ? padName : readableName;
                label.AddToClassList(PadTextClass);
            }
        }
    }

    // The same key on two actions of one device: allowed, but both cells are shown red
    bool IsKeyUsedTwice(int cell)
    {
        for (int other = 0; other < bindings.Count; other++)
            if (other != cell && PathOf(other) == PathOf(cell)) return true;
        return false;
    }

    string PathOf(int cell) => bindings[cell].action.bindings[bindings[cell].index].effectivePath;

    static void ClearCellLook(Label label)
    {
        label.RemoveFromClassList(KeyClass);
        label.RemoveFromClassList(WideKeyClass);
        label.RemoveFromClassList(PadButtonClass);
        label.RemoveFromClassList(PadTextClass);
        foreach (string iconClass in PadIconClasses.Values) label.RemoveFromClassList(iconClass);
    }
}