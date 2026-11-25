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
    public string account, difficulty, lastScene = SaveFile.FirstScene;
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
    public const int Version = 1;
    public const string FirstScene = "Level1";
    const string FolderName = "Saves", Extension = ".json", TemporaryExtension = ".tmp";

    public static SaveData Current { get; private set; }

    static string Folder => Path.Combine(Directory.GetParent(Application.dataPath).FullName, FolderName);

    public static bool AnySaveExists => Directory.Exists(Folder) && Directory.GetFiles(Folder, "*" + Extension).Length > 0;

    public static void StartNew(string account, string difficulty)
    {
        Current = new SaveData { account = account, difficulty = difficulty };
        Write();
    }

    public static bool LoadLatest()
    {
        if (!Directory.Exists(Folder)) return false;
        FileInfo[] files = new DirectoryInfo(Folder).GetFiles("*" + Extension);
        Array.Sort(files, (a, b) => b.LastWriteTimeUtc.CompareTo(a.LastWriteTimeUtc));
        foreach (FileInfo file in files)
            if (TryRead(file.FullName, out SaveData data)) { Current = data; return true; }
        return false;
    }

    public static bool ReloadCurrent()
    {
        if (!TryRead(PathOf(Current.account), out SaveData data)) return false;
        Current = data;
        return true;
    }

    static string PathOf(string account) => Path.Combine(Folder, account + Extension);

    public static void Write()
    {
        Directory.CreateDirectory(Folder);
        string path = PathOf(Current.account);
        string temporaryPath = Path.ChangeExtension(path, TemporaryExtension);
        File.WriteAllText(temporaryPath, JsonUtility.ToJson(Current, true));
        if (File.Exists(path)) File.Replace(temporaryPath, path, null);
        else File.Move(temporaryPath, path);
    }

    static bool TryRead(string path, out SaveData data)
    {
        data = null;
        try { data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path)); }
        catch (ArgumentException) { return false; }
        return data != null && data.version <= Version && !string.IsNullOrEmpty(data.account);
    }
}