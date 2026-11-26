using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public static class KeyBindings
{
    const string FileName = "KeyBindings.json";

    static string FilePath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, FileName);

    public static PlayerInputActions CreateActions()
    {
        PlayerInputActions actions = new();
        Apply(actions);
        return actions;
    }

    public static void Apply(PlayerInputActions actions)
    {
        actions.asset.RemoveAllBindingOverrides();
        if (!File.Exists(FilePath)) return;
        try { actions.asset.LoadBindingOverridesFromJson(File.ReadAllText(FilePath)); }
        catch (ArgumentException) { actions.asset.RemoveAllBindingOverrides(); }
    }

    public static void Save(PlayerInputActions actions) => File.WriteAllText(FilePath, actions.asset.SaveBindingOverridesAsJson());
}