#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

/// <summary>
/// Small localization manager. Add it as an autoload and it reads
/// plain json files from res://languages.
///
/// Usage from anywhere:
///     Localization.T("menu.quit")        -> "Quit"
///     Localization.SetLanguage("de")     -> switch language
///     Localization.LanguageChanged       -> event, fired on switch
/// </summary>
public partial class Localization : Node
{
    /// <summary>Fallback language (languages/en.json).</summary>
    public const string DefaultLanguage = "en";

    public static Localization Instance { get; private set; } = null!;

    // key -> string of the currently active language
    private readonly Dictionary<string, string> _strings = new();

    // keys we already complained about. without this the console would
    // spam the same warning every single frame.
    private readonly HashSet<string> _missingWarned = new();

    private static string[] _available = null!;
    private string _current = DefaultLanguage;

    /// <summary>Fires whenever the language changed, UIs should refresh.</summary>
    public static event Action? LanguageChanged;

    public static string CurrentLanguage => Instance != null ? Instance._current : DefaultLanguage;

    public override void _Ready()
    {
        Instance = this;

        // try the system language first. falls back to english on its own
        // if we don't ship a file for it.
        SetLanguage(DetectSystemLanguage());
    }

    /// <summary>Get a string: Localization.T("menu.multiplayer")</summary>
    public static string T(string key)
    {
        if (Instance != null && Instance._strings.TryGetValue(key, out string? value))
            return value;

        // no translation? return the key itself so you can spot it in the UI,
        // and warn exactly once.
        if (Instance != null && Instance._missingWarned.Add(key))
            GD.PushWarning($"[Localization] key '{key}' not found in language '{Instance._current}'!");
        return key;
    }

    /// <summary>
    /// Get a translated string and fill in placeholders.
    /// Example: Localization.T("welcome", "Ahmet", 42)
    /// </summary>
    public static string T(string key, params object[] args)
    {
        string text = T(key);

        return args.Length > 0? string.Format(System.Globalization.CultureInfo.InvariantCulture, text, args) : text;
    }

    /// <summary>Switch language: Localization.SetLanguage("de")</summary>
    public static void SetLanguage(string code)
    {
        if (Instance == null)
        {
            GD.PushError("[Localization] not ready yet (autoload order?)!");
            return;
        }
        Instance.LoadLanguage(code);
    }

    /// <summary>Everything in the languages/ folder: ["en", "de", "tr"]</summary>
    public static string[] GetAvailableLanguages()
    {
        if (_available != null) return _available;

        var list = new List<string>();

        // Open returns null when the folder doesn't exist, so keep an eye on that
        using DirAccess? dir = DirAccess.Open("res://languages");
        if (dir != null)
        {
            dir.ListDirBegin();
            string file = dir.GetNext();
            while (!string.IsNullOrEmpty(file))
            {
                if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    list.Add(file[..^5]); // "tr.json" -> "tr"
                file = dir.GetNext();
            }

            dir.ListDirEnd();
        }

        if (list.Count == 0) list.Add(DefaultLanguage);

        _available = list.ToArray();
        return _available;
    }

    /// <summary>
    /// Read the OS locale: "tr_TR" -> "tr", "de_DE" -> "de".
    /// Returns the default language if we have no file for it.
    /// </summary>
    public static string DetectSystemLanguage()
    {
        string prefix = OS.GetLocale().Split('_')[0].ToLowerInvariant();
        foreach (string lang in GetAvailableLanguages())
            if (lang == prefix)
                return lang;

        return DefaultLanguage;
    }

    // ---------------- internals ----------------

    private void LoadLanguage(string code)
    {
        string path = $"res://languages/{code}.json";

        // no file? quietly fall back to english.
        if (!FileAccess.FileExists(path))
        {
            GD.PushWarning($"[Localization] '{path}' not found, sticking with '{DefaultLanguage}'.");
            if (code == DefaultLanguage) return;
            code = DefaultLanguage;
            path = $"res://languages/{code}.json";
        }

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PushError($"[Localization] can't open '{path}'!");
            return;
        }

        try
        {
            var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(file.GetAsText());
            if (loaded == null) throw new JsonException("file is empty or not valid json.");

            _strings.Clear();
            foreach (var pair in loaded)
                _strings[pair.Key] = pair.Value;

            _current = code;
            _missingWarned.Clear(); // new language, fresh chance for missing keys

            LanguageChanged?.Invoke();
        }
        catch (Exception e)
        {
            GD.PushError($"[Localization] can't parse '{path}': {e.Message}");
        }
    }
}
