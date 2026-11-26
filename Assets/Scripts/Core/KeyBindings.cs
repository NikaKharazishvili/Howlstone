using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public static class KeyBindings
{
    const string FileName = "KeyBindings.json";

    static string FilePath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, FileName);

    // The game's actions with the player's own keys; a missing or unreadable file means the default keys
    public static PlayerInputActions CreateActions()
    {
        PlayerInputActions actions = new();
        if (!File.Exists(FilePath)) return actions;
        try { actions.asset.LoadBindingOverridesFromJson(File.ReadAllText(FilePath)); }
        catch (ArgumentException) { actions.asset.RemoveAllBindingOverrides(); }
        return actions;
    }

    public static void Save(PlayerInputActions actions) => File.WriteAllText(FilePath, actions.asset.SaveBindingOverridesAsJson());
}