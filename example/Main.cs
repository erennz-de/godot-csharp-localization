using Godot;

// Minimal demo: a title, a hint and a dropdown to switch the language.
// UI is built in code on purpose, keeps the .tscn from getting noisy.
// Warning: It may not fill your complete screen! Its just a test demo.
public partial class Main : Control
{
    private Label _title = null!;
    private Label _hint = null!;
    private OptionButton _langPicker = null!;

    public override void _Ready()
    {
        _title = new Label();
        _title.AddThemeFontSizeOverride("font_size", 32);
        _title.Position = new Vector2(48, 48);

        _hint = new Label();
        _hint.Position = new Vector2(48, 110);

        _langPicker = new OptionButton();
        _langPicker.Position = new Vector2(48, 160);
        _langPicker.ItemSelected += OnLanguagePicked;

        // fill the dropdown with whatever json files are around.
        // "de" shows up as "Deutsch", "tr" as "Türkçe" and so on.
        foreach (string code in Localization.GetAvailableLanguages())
        {
            _langPicker.AddItem(Localization.T("lang." + code));
            _langPicker.SetItemMetadata(_langPicker.ItemCount - 1, code);
        }

        // preselect whatever is active right now
        for (int i = 0; i < _langPicker.ItemCount; i++)
        {
            if (_langPicker.GetItemMetadata(i).AsString() == Localization.CurrentLanguage)
                _langPicker.Select(i);
        }

        AddChild(_title);
        AddChild(_hint);
        AddChild(_langPicker);

        Localization.LanguageChanged += RefreshTexts;
        RefreshTexts();
    }

    public override void _ExitTree()
    {
        Localization.LanguageChanged -= RefreshTexts;
    }

    private void OnLanguagePicked(long index)
    {
        string code = _langPicker.GetItemMetadata((int)index).AsString();
        Localization.SetLanguage(code);
    }

    private void RefreshTexts()
    {
        _title.Text = Localization.T("demo.title");
        _hint.Text = Localization.T("demo.hint");
    }
}
