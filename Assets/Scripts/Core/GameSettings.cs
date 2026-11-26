using System;
using System.IO;
using UnityEngine;

public enum Setting { MusicVolume, EffectsVolume, MuteInBackground, Contrast, Saturation }

public static class GameSettings
{
    [Serializable]
    class SettingsData
    {
        public int musicVolume = 100, effectsVolume = 100, contrast = (int)ShaderGlobals.DefaultContrast, saturation = (int)ShaderGlobals.DefaultSaturation;
        public bool muteInBackground = true;
    }

    const string FileName = "Settings.json";
    const int Step = 10, MaxVolume = 100, MaxLook = 100;
    const float FullMusicVolume = 0.5f;

    static SettingsData data;
    static AudioSource music;
    static bool inBackground;

    static string FilePath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, FileName);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void LoadForPlay()
    {
        data = Load();
        music = null;
        inBackground = false;
        Application.focusChanged -= ApplyForFocus;
        Application.focusChanged += ApplyForFocus;
        Application.quitting -= StopAfterPlay;
        Application.quitting += StopAfterPlay;
    }

    // Without the domain reload an event stays subscribed after Play and would overwrite what is tuned in the editor
    static void StopAfterPlay()
    {
        Application.focusChanged -= ApplyForFocus;
        AudioListener.volume = 1;
    }

    public static void UseMusic(AudioSource source)
    {
        music = source;
        source.ignoreListenerVolume = true;
        Apply();
    }

    public static void Apply()
    {
        bool muted = inBackground && data.muteInBackground;
        AudioListener.volume = muted ? 0 : data.effectsVolume / (float)MaxVolume;
        if (music) music.volume = muted ? 0 : FullMusicVolume * data.musicVolume / MaxVolume;
        Application.runInBackground = !data.muteInBackground;
        (ShaderGlobals.Contrast, ShaderGlobals.Saturation) = (data.contrast, data.saturation);
    }

    public static string NameOf(Setting setting)
    {
        switch (setting)
        {
            case Setting.MusicVolume: return "Music";
            case Setting.EffectsVolume: return "Effects";
            case Setting.MuteInBackground: return "Mute in background";
            case Setting.Contrast: return "Contrast";
            case Setting.Saturation: return "Saturation";
            default: throw new NotImplementedException(setting.ToString());
        }
    }

    public static string ValueText(Setting setting)
    {
        switch (setting)
        {
            case Setting.MusicVolume: return $"< {data.musicVolume} >";
            case Setting.EffectsVolume: return $"< {data.effectsVolume} >";
            case Setting.MuteInBackground: return data.muteInBackground ? "< On >" : "< Off >";
            case Setting.Contrast: return $"< {data.contrast} >";
            case Setting.Saturation: return $"< {data.saturation} >";
            default: throw new NotImplementedException(setting.ToString());
        }
    }

    public static void Change(Setting setting, int direction)
    {
        switch (setting)
        {
            case Setting.MusicVolume: data.musicVolume = Mathf.Clamp(data.musicVolume + direction * Step, 0, MaxVolume); break;
            case Setting.EffectsVolume: data.effectsVolume = Mathf.Clamp(data.effectsVolume + direction * Step, 0, MaxVolume); break;
            case Setting.MuteInBackground: data.muteInBackground = !data.muteInBackground; break;
            case Setting.Contrast: data.contrast = Mathf.Clamp(data.contrast + direction * Step, -MaxLook, MaxLook); break;
            case Setting.Saturation: data.saturation = Mathf.Clamp(data.saturation + direction * Step, -MaxLook, MaxLook); break;
            default: throw new NotImplementedException(setting.ToString());
        }
        ApplyAndSave();
    }

    public static void ResetToDefault()
    {
        data = new SettingsData();
        ApplyAndSave();
    }

    static void ApplyAndSave()
    {
        Apply();
        File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
    }

    static void ApplyForFocus(bool focused)
    {
        inBackground = !focused;
        Apply();
    }

    // A missing or unreadable file means the default settings
    static SettingsData Load()
    {
        if (!File.Exists(FilePath)) return new SettingsData();
        try { return JsonUtility.FromJson<SettingsData>(File.ReadAllText(FilePath)) ?? new SettingsData(); }
        catch (ArgumentException) { return new SettingsData(); }
    }
}