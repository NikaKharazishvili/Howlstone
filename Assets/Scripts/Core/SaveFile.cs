using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class CharacterSave { public int level = 1, xp, maxXp = 50; }

[Serializable]
public class LevelSave { public string scene; public List<string> takenCoins = new(); public int kills; public bool hasPosition; public Vector3 position; }

[Serializable]
public class SaveData
{
    public int version = SaveFile.Version;
    public string account, lastScene = SaveFile.FirstScene;
    public CharacterSave character = new();
    public List<LevelSave> levels = new();

    public LevelSave GetLevel(string scene)
    {
        LevelSave level = levels.Find(l => l.scene == scene);
        if (level != null) return level;
        level = new LevelSave { scene = scene };
        levels.Add(level);
        return level;
    }
}

public static class SaveFile
{
    public const int Version = 1, SlotCount = 5;
    public const string FirstScene = "Level1";
    const string FolderName = "Saves", SlotName = "Slot", Extension = ".json", TemporaryExtension = ".tmp";

    public static SaveData Current { get; private set; }
    static int currentSlot;

    static string Folder => Path.Combine(Directory.GetParent(Application.dataPath).FullName, FolderName);

    static string PathOf(int slot) => Path.Combine(Folder, SlotName + (slot + 1) + Extension);

    // Null for an empty slot and for a file that cannot be read
    public static SaveData Read(int slot)
    {
        string path = PathOf(slot);
        if (!File.Exists(path)) return null;
        SaveData data;
        try { data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path)); }
        catch (ArgumentException) { return null; }
        return data != null && data.version <= Version && !string.IsNullOrEmpty(data.account) ? data : null;
    }

    public static void StartNew(int slot, string account)
    {
        Current = new SaveData { account = account };
        currentSlot = slot;
        Write();
    }

    public static void Delete(int slot) => File.Delete(PathOf(slot));

    public static bool Load(int slot)
    {
        SaveData data = Read(slot);
        if (data == null) return false;
        Current = data;
        currentSlot = slot;
        return true;
    }

    public static bool LoadFirstFilledSlot()
    {
        for (int slot = 0; slot < SlotCount; slot++)
            if (Load(slot)) return true;
        return false;
    }

    public static void Write()
    {
        Directory.CreateDirectory(Folder);
        string path = PathOf(currentSlot);
        string temporaryPath = Path.ChangeExtension(path, TemporaryExtension);
        File.WriteAllText(temporaryPath, JsonUtility.ToJson(Current, true));
        if (File.Exists(path)) File.Replace(temporaryPath, path, null);
        else File.Move(temporaryPath, path);
    }
}