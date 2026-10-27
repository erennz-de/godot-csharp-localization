# Localization.cs

Small localization setup for Godot 4 (.NET / C#). One script, one folder of json files, that's it.

I wrote this because I didn't want to pull a whole addon into my project just to translate a menu.

## What it does

- reads plain `.json` files from `res://languages/` (`en.json`, `de.json`, ...)
- picks the system language on startup, falls back to english if there's no file for it
- `Localization.T("menu.quit")` returns the string in the current language
- supports placeholders for dynamic values, e.g. `Localization.T("welcome", "Dennis", 42)`
- missing keys return the key itself and print one warning (once, not every frame)
- `LanguageChanged` event so your UI can rebuild itself after a language switch

## Setup

1. Drop `Localization.cs` somewhere in your project, e.g. `res://scripts/`.
2. Project Settings -> Autoload -> add it. Name it `Localization`.
3. Create a `languages` folder and put one json file per language in it:

```
res://languages/en.json
res://languages/de.json
res://languages/tr.json
```

```json
{
    "menu.quit": "Quit",
    "menu.settings": "Settings",
    "menu.multiplayer": "Multiplayer",
    "welcome": "Welcome {0}, your score is {1}!"
}
```

That's all. New files are picked up automatically, nothing to register.

## Usage

```csharp
// from anywhere
GetNode<Label>("Labels/Quit").Text = Localization.T("menu.quit");

// formatted translations
string welcome = Localization.T("welcome", "Ahmet", 42);

// switch language at runtime
Localization.SetLanguage("de");

// and react to it
public override void _Ready()
{
    Localization.LanguageChanged += RefreshTexts;
    RefreshTexts();
}

public override void _ExitTree()
{
    Localization.LanguageChanged -= RefreshTexts;
}
```

## Notes

- the file name is the language code: `tr.json` means `tr`. keep it short, `en_US` gets cut down to `en` automatically
- a missing translation is never fatal, the raw key shows up so you notice it in the UI
- `Localization.GetAvailableLanguages()` gives you everything in the folder, handy for a language dropdown
- placeholders like {0} and {1} are replaced with the values passed to Localization.T()

## Example

There's a tiny runnable project under `example/`. Open it in Godot 4 (.NET), hit F5 and switch languages with the dropdown.

New languages are welcome, just send a PR with your `xx.json`.

## License

MIT, see LICENSE.
