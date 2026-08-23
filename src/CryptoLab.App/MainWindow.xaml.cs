
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CryptoLab.App.Services;
using CryptoLab.Core.Cryptography;
using CryptoLab.Core.Education;
using CryptoLab.Core.Models;
using CryptoLab.Core.Storage;
using Microsoft.Win32;

namespace CryptoLab.App;

public partial class MainWindow : Window
{
    private readonly CryptoEngine _engine = new();
    private readonly JsonFileStore _store = new();
    private readonly LocalizationService _localizer = new();
    private readonly Dictionary<string, Button> _navButtons = new(StringComparer.OrdinalIgnoreCase);
    private AppSettings _settings;
    private List<OperationHistoryEntry> _history;
    private string _currentPage = "dashboard";
    private string _selectedAlgorithmId = "caesar";

    private TextBox? _inputText;
    private TextBox? _outputText;
    private TextBox? _shiftText;
    private TextBox? _keywordText;
    private TextBox? _affineAText;
    private TextBox? _affineBText;
    private TextBox? _railsText;
    private TextBox? _columnarKeyText;
    private TextBox? _keyText;
    private TextBox? _ivText;
    private TextBox? _publicKeyText;
    private TextBox? _privateKeyText;
    private ComboBox? _algorithmCombo;
    private ComboBox? _operationCombo;
    private ComboBox? _modeCombo;
    private ComboBox? _rsaKeySizeCombo;
    private StackPanel? _configPanel;
    private StackPanel? _visualizationPanel;
    private TextBlock? _resultMetaText;
    private TextBlock? _explanationText;

    public MainWindow()
    {
        InitializeComponent();
        _settings = _store.LoadSettings();
        _history = _store.LoadHistory().ToList();
        _localizer.SetLanguage(_settings.Language);
        ApplyTheme();
        BuildNavigation();
        ApplyLocalizationShell();
        Navigate("dashboard");
    }

    private string T(string key) => _localizer.T(key);

    private bool Arabic => _localizer.IsRightToLeft;

    private string OperationName(CryptoOperation operation)
    {
        return operation switch
        {
            CryptoOperation.Encrypt => T("encrypt"),
            CryptoOperation.Decrypt => T("decrypt"),
            _ => T("hash")
        };
    }

    private string Unit(string unit) => Arabic && unit == "characters" ? "حرف" : unit;

    private void ApplyLocalizationShell()
    {
        FlowDirection = _localizer.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        SidebarSubtitle.Text = T("app_subtitle");
        SafetyNotice.Text = T("educational_notice");
        StatusText.Text = $"Status: {T("ready")}";
        BuildNavigation();
    }

    private void ApplyTheme()
    {
        var light = string.Equals(_settings.Theme, "Light", StringComparison.OrdinalIgnoreCase);
        Resources["AppBackgroundBrush"] = Brush(light ? "#f4f7fb" : "#08111f");
        Resources["SidebarBrush"] = Brush(light ? "#ffffff" : "#0d1b2f");
        Resources["HeaderBrush"] = Brush(light ? "#eef3f9" : "#0b1627");
        Resources["CardBrush"] = Brush(light ? "#ffffff" : "#13233a");
        Resources["CardAltBrush"] = Brush(light ? "#f8fbff" : "#172b45");
        Resources["ButtonBrush"] = Brush(light ? "#e5edf7" : "#223856");
        Resources["PrimaryTextBrush"] = Brush(light ? "#102033" : "#edf6ff");
        Resources["MutedTextBrush"] = Brush(light ? "#5d6b7e" : "#9db0c8");
        Resources["AccentBrush"] = Brush("#19c8a7");
        Resources["AccentAltBrush"] = Brush("#4da3ff");
        Resources["DangerBrush"] = Brush("#ff5c7a");
        Resources["NoticeBrush"] = Brush(light ? "#dff7ef" : "#14392f");
        Resources["ToastBrush"] = Brush("#101827");
    }

    private static Brush Brush(string value) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
    private Brush R(string key) => (Brush)FindResource(key);

    private void BuildNavigation()
    {
        if (NavItems is null) return;
        NavItems.Children.Clear();
        _navButtons.Clear();
        var pages = new[]
        {
            ("dashboard", T("dashboard")),
            ("crypto", T("encrypt_decrypt")),
            ("algorithms", T("algorithms")),
            ("learning", T("learning")),
            ("compare", T("compare")),
            ("history", T("history")),
            ("settings", T("settings")),
            ("about", T("about"))
        };

        foreach (var (id, label) in pages)
        {
            var button = new Button
            {
                Content = label,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Background = R("ButtonBrush"),
                Foreground = R("PrimaryTextBrush"),
                Margin = new Thickness(0, 4, 0, 4),
                Tag = id
            };
            button.Click += (_, _) => Navigate(id);
            _navButtons[id] = button;
            NavItems.Children.Add(button);
        }
        HighlightActiveNav();
    }

    private void HighlightActiveNav()
    {
        foreach (var pair in _navButtons)
        {
            pair.Value.Background = pair.Key == _currentPage ? R("AccentBrush") : R("ButtonBrush");
            pair.Value.Foreground = pair.Key == _currentPage ? Brushes.Black : R("PrimaryTextBrush");
        }
    }

    private void Navigate(string page)
    {
        _currentPage = page;
        HighlightActiveNav();
        PageHost.Children.Clear();
        switch (page)
        {
            case "crypto": RenderCryptoPage(); break;
            case "algorithms": RenderAlgorithmsPage(); break;
            case "learning": RenderLearningPage(); break;
            case "compare": RenderComparePage(); break;
            case "history": RenderHistoryPage(); break;
            case "settings": RenderSettingsPage(); break;
            case "about": RenderAboutPage(); break;
            default: RenderDashboardPage(); break;
        }
    }

    private void SetHeader(string title, string subtitle)
    {
        PageTitle.Text = title;
        PageSubtitle.Text = subtitle;
    }

    private StackPanel AddCard(string title)
    {
        var stack = new StackPanel();
        if (!string.IsNullOrWhiteSpace(title))
        {
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = R("PrimaryTextBrush"),
                Margin = new Thickness(0, 0, 0, 12)
            });
        }

        PageHost.Children.Add(new Border
        {
            CornerRadius = new CornerRadius(8),
            Background = R("CardBrush"),
            Padding = new Thickness(18),
            Margin = new Thickness(0, 0, 0, 16),
            Child = stack
        });
        return stack;
    }

    private Border Card(UIElement child, Thickness? margin = null)
    {
        return new Border
        {
            CornerRadius = new CornerRadius(8),
            Background = R("CardBrush"),
            Padding = new Thickness(16),
            Margin = margin ?? new Thickness(0, 0, 12, 12),
            Child = child
        };
    }

    private TextBlock Text(string text, double size = 14, bool bold = false, string brush = "PrimaryTextBrush")
    {
        return new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            FontSize = size,
            FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal,
            Foreground = R(brush),
            Margin = new Thickness(0, 3, 0, 5)
        };
    }

    private Button ActionButton(string text, RoutedEventHandler handler, bool primary = false, bool danger = false)
    {
        var button = new Button
        {
            Content = text,
            Background = danger ? R("DangerBrush") : primary ? R("AccentBrush") : R("ButtonBrush"),
            Foreground = primary ? Brushes.Black : R("PrimaryTextBrush"),
            MinWidth = 120
        };
        button.Click += handler;
        return button;
    }

    private TextBox SingleLine(string text = "")
    {
        return new TextBox
        {
            Text = text,
            Background = R("CardAltBrush"),
            Foreground = R("PrimaryTextBrush"),
            BorderBrush = R("ButtonBrush"),
            CaretBrush = R("AccentBrush")
        };
    }

    private TextBox MultiLine(string text = "", int minHeight = 150)
    {
        return new TextBox
        {
            Text = text,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MinHeight = minHeight,
            Background = R("CardAltBrush"),
            Foreground = R("PrimaryTextBrush"),
            BorderBrush = R("ButtonBrush"),
            CaretBrush = R("AccentBrush")
        };
    }

private void RenderDashboardPage()
    {
        SetHeader(T("welcome"), T("interactive"));
        var stats = new WrapPanel();
        stats.Children.Add(StatCard(T("stat_algorithms"), _engine.Algorithms.Count.ToString(), T("stat_algorithms_sub")));
        stats.Children.Add(StatCard(T("stat_encryption"), _engine.Algorithms.Count(a => a.SupportsEncryption).ToString(), T("stat_encryption_sub")));
        stats.Children.Add(StatCard(T("stat_lessons"), EducationCatalog.Lessons.Count.ToString(), T("stat_lessons_sub")));
        stats.Children.Add(StatCard(T("stat_operations"), _history.Count.ToString(), T("stat_operations_sub")));
        PageHost.Children.Add(stats);

        var actions = AddCard(T("quick_actions"));
        var buttons = new WrapPanel();
        buttons.Children.Add(ActionButton(T("start_encryption"), (_, _) => Navigate("crypto"), true));
        buttons.Children.Add(ActionButton(T("explore_algorithms"), (_, _) => Navigate("algorithms")));
        buttons.Children.Add(ActionButton(T("learn_cryptography"), (_, _) => Navigate("learning")));
        buttons.Children.Add(ActionButton(T("compare_algorithms"), (_, _) => Navigate("compare")));
        actions.Children.Add(buttons);

        var notice = AddCard(T("educational_notice"));
        notice.Children.Add(Text(T("dashboard_notice"), 14, false, "MutedTextBrush"));
    }

    private Border StatCard(string title, string value, string subtitle)
    {
        var stack = new StackPanel { Width = 230 };
        stack.Children.Add(Text(title, 13, true, "MutedTextBrush"));
        stack.Children.Add(Text(value, 34, true, "AccentBrush"));
        stack.Children.Add(Text(subtitle, 13, false, "MutedTextBrush"));
        return Card(stack);
    }
private void RenderCryptoPage()
    {
        SetHeader(T("encrypt_decrypt"), T("crypto_subtitle"));
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(390) });

        var left = new StackPanel();
        Grid.SetColumn(left, 0);
        grid.Children.Add(left);
        var right = new StackPanel();
        Grid.SetColumn(right, 1);
        grid.Children.Add(right);
        PageHost.Children.Add(grid);

        var inputCard = new StackPanel();
        inputCard.Children.Add(Text(T("input_text"), 18, true));
        _inputText = MultiLine("HELLO CryptoLab", 170);
        inputCard.Children.Add(_inputText);
        var inputButtons = new WrapPanel();
        inputButtons.Children.Add(ActionButton(T("paste"), (_, _) => PasteInput()));
        inputButtons.Children.Add(ActionButton(T("clear"), (_, _) => _inputText.Clear()));
        inputButtons.Children.Add(ActionButton(T("load_file"), (_, _) => LoadInputFile()));
        inputCard.Children.Add(inputButtons);
        left.Children.Add(Card(inputCard));

        var outputCard = new StackPanel();
        outputCard.Children.Add(Text(T("output"), 18, true));
        _outputText = MultiLine(string.Empty, 170);
        _outputText.IsReadOnly = true;
        outputCard.Children.Add(_outputText);
        var outputButtons = new WrapPanel();
        outputButtons.Children.Add(ActionButton(T("copy"), (_, _) => CopyOutput()));
        outputButtons.Children.Add(ActionButton(T("clear"), (_, _) => _outputText.Clear()));
        outputButtons.Children.Add(ActionButton(T("save"), (_, _) => SaveOutput()));
        outputButtons.Children.Add(ActionButton(T("export"), (_, _) => SaveOutput()));
        outputCard.Children.Add(outputButtons);
_resultMetaText = Text($"{T("meta_algorithm")}: -\n{T("meta_operation")}: -\n{T("meta_input_length")}: 0\n{T("meta_output_length")}: 0\n{T("meta_time")}: 0 ms\n{T("meta_status")}: {T("ready")}", 13, false, "MutedTextBrush");
        outputCard.Children.Add(_resultMetaText);
        left.Children.Add(Card(outputCard));

        var explainCard = new StackPanel();
        explainCard.Children.Add(Text(T("how_it_works"), 18, true));
        _explanationText = Text(T("explanation_placeholder"), 14, false, "MutedTextBrush");
        explainCard.Children.Add(_explanationText);
        _visualizationPanel = new StackPanel();
        explainCard.Children.Add(_visualizationPanel);
        left.Children.Add(Card(explainCard));

        var selectCard = new StackPanel();
        selectCard.Children.Add(Text(T("select_algorithm"), 18, true));
        _algorithmCombo = new ComboBox
        {
            ItemsSource = _engine.Algorithms,
            DisplayMemberPath = "Name",
            SelectedValuePath = "Id",
            SelectedValue = _selectedAlgorithmId,
            Background = R("CardAltBrush"),
            Foreground = R("PrimaryTextBrush")
        };
        _algorithmCombo.SelectionChanged += (_, _) =>
        {
            if (_algorithmCombo.SelectedValue is string id)
            {
                _selectedAlgorithmId = id;
                RebuildConfigPanel();
            }
        };
        selectCard.Children.Add(_algorithmCombo);
        selectCard.Children.Add(Text(T("operation"), 14, true));
        _operationCombo = new ComboBox { Background = R("CardAltBrush"), Foreground = R("PrimaryTextBrush") };
        selectCard.Children.Add(_operationCombo);
        _configPanel = new StackPanel();
        selectCard.Children.Add(Text(T("configuration"), 14, true));
        selectCard.Children.Add(_configPanel);
var operationButtons = new WrapPanel { Margin = new Thickness(0, 12, 0, 0) };
        operationButtons.Children.Add(ActionButton(T("encrypt"), (_, _) => ExecuteOperation(CryptoOperation.Encrypt), true));
        operationButtons.Children.Add(ActionButton(T("decrypt"), (_, _) => ExecuteOperation(CryptoOperation.Decrypt)));
        operationButtons.Children.Add(ActionButton(T("hash"), (_, _) => ExecuteOperation(CryptoOperation.Hash)));
        selectCard.Children.Add(operationButtons);
        right.Children.Add(Card(selectCard, new Thickness(12, 0, 0, 12)));
        RebuildConfigPanel();
    }

    private void RebuildConfigPanel()
    {
        if (_configPanel is null || _operationCombo is null) return;
        _configPanel.Children.Clear();
        _operationCombo.Items.Clear();
        var metadata = _engine.FindMetadata(_selectedAlgorithmId);
        if (metadata is null) return;

if (metadata.SupportsHashing)
        {
            _operationCombo.Items.Add(OperationItem(T("hash"), CryptoOperation.Hash));
            _operationCombo.SelectedIndex = 0;
            _configPanel.Children.Add(Text(T("hashing_note"), 13, false, "MutedTextBrush"));
            if (Arabic)
            {
                _configPanel.Children.Add(Text(ArabicContent.Notice(_selectedAlgorithmId), 12, false, "MutedTextBrush"));
            }
            else
            {
                _configPanel.Children.Add(Text(metadata.Notice, 12, false, "MutedTextBrush"));
            }
            return;
        }

        _operationCombo.Items.Add(OperationItem(T("encrypt"), CryptoOperation.Encrypt));
        _operationCombo.Items.Add(OperationItem(T("decrypt"), CryptoOperation.Decrypt));
        _operationCombo.SelectedIndex = 0;

switch (_selectedAlgorithmId)
        {
            case "caesar":
                _shiftText = AddLabeledTextBox(_configPanel, T("label_shift"), "3");
                break;
            case "vigenere":
                _keywordText = AddLabeledTextBox(_configPanel, T("label_keyword"), "LEMON");
                break;
            case "affine":
                _affineAText = AddLabeledTextBox(_configPanel, T("label_key_a"), "5");
                _affineBText = AddLabeledTextBox(_configPanel, T("label_key_b"), "8");
                break;
            case "rail-fence":
                _railsText = AddLabeledTextBox(_configPanel, T("label_rails"), "3");
                break;
            case "columnar":
                _columnarKeyText = AddLabeledTextBox(_configPanel, T("label_keyword"), "ZEBRA");
                break;
            case "aes":
            case "des":
            case "3des":
                _keyText = AddLabeledTextBox(_configPanel, T("label_key"), _selectedAlgorithmId == "aes" ? "1234567890abcdef" : _selectedAlgorithmId == "des" ? "8bytekey" : "1234567890abcdef12345678");
                _configPanel.Children.Add(Text(T("label_mode"), 13, true));
                _modeCombo = new ComboBox { ItemsSource = new[] { "CBC", "ECB" }, SelectedIndex = 0, Background = R("CardAltBrush"), Foreground = R("PrimaryTextBrush") };
                _configPanel.Children.Add(_modeCombo);
                _ivText = AddLabeledTextBox(_configPanel, T("label_iv"), string.Empty);
                break;
            case "rsa":
                _configPanel.Children.Add(Text(T("label_key_size"), 13, true));
                _rsaKeySizeCombo = new ComboBox { ItemsSource = new[] { 2048, 3072, 4096 }, SelectedIndex = 0, Background = R("CardAltBrush"), Foreground = R("PrimaryTextBrush") };
                _configPanel.Children.Add(_rsaKeySizeCombo);
                _configPanel.Children.Add(ActionButton(T("generate_keys"), (_, _) => GenerateRsaKeys(), true));
                _publicKeyText = AddLabeledMultiline(_configPanel, T("label_public_key"), string.Empty, 95);
                _privateKeyText = AddLabeledMultiline(_configPanel, T("label_private_key"), string.Empty, 120);
                break;
            default:
                _configPanel.Children.Add(Text(Arabic ? ArabicContent.KeyHint(_selectedAlgorithmId) : metadata.KeyHint, 13, false, "MutedTextBrush"));
                break;
        }
        _configPanel.Children.Add(Text(Arabic ? ArabicContent.Notice(_selectedAlgorithmId) : metadata.Notice, 12, false, "MutedTextBrush"));
    }

    private ComboBoxItem OperationItem(string label, CryptoOperation operation) => new() { Content = label, Tag = operation };

    private TextBox AddLabeledTextBox(StackPanel panel, string label, string value)
    {
        panel.Children.Add(Text(label, 13, true));
        var textBox = SingleLine(value);
        panel.Children.Add(textBox);
        return textBox;
    }

    private TextBox AddLabeledMultiline(StackPanel panel, string label, string value, int height)
    {
        panel.Children.Add(Text(label, 13, true));
        var textBox = MultiLine(value, height);
        panel.Children.Add(textBox);
        return textBox;
    }
    private CryptoOperation SelectedOperation()
    {
        return _operationCombo?.SelectedItem is ComboBoxItem item && item.Tag is CryptoOperation operation ? operation : CryptoOperation.Encrypt;
    }

    private void ExecuteOperation(CryptoOperation requested)
    {
        if (_inputText is null || _outputText is null) return;
        var metadata = _engine.FindMetadata(_selectedAlgorithmId);
        var operation = metadata?.SupportsHashing == true ? CryptoOperation.Hash : requested;
        if (requested == CryptoOperation.Hash && metadata?.SupportsHashing != true)
        {
            operation = SelectedOperation();
        }

        var request = new CryptoRequest
        {
            AlgorithmId = _selectedAlgorithmId,
            Operation = operation,
            InputText = _inputText.Text,
            Parameters = BuildParameters()
        };
var result = _engine.Execute(request);
        _outputText.Text = result.Output;
        RenderResult(result);
        AddHistory(result);
        if (result.Success && _settings.AutoClearInput) _inputText.Clear();
        ShowToast(result.Success ? T("toast_operation_done") : result.StatusMessage, result.Success);
    }

    private Dictionary<string, string> BuildParameters()
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        switch (_selectedAlgorithmId)
        {
            case "caesar": parameters["shift"] = _shiftText?.Text ?? "3"; break;
            case "vigenere": parameters["keyword"] = _keywordText?.Text ?? string.Empty; break;
            case "affine": parameters["a"] = _affineAText?.Text ?? "5"; parameters["b"] = _affineBText?.Text ?? "8"; break;
            case "rail-fence": parameters["rails"] = _railsText?.Text ?? "3"; break;
            case "columnar": parameters["key"] = _columnarKeyText?.Text ?? string.Empty; break;
            case "aes":
            case "des":
            case "3des":
                parameters["key"] = _keyText?.Text ?? string.Empty;
                parameters["mode"] = _modeCombo?.SelectedItem?.ToString() ?? "CBC";
                parameters["iv"] = _ivText?.Text ?? string.Empty;
                break;
            case "rsa":
                parameters["publicKey"] = _publicKeyText?.Text ?? string.Empty;
                parameters["privateKey"] = _privateKeyText?.Text ?? string.Empty;
                break;
        }
        return parameters;
    }

private void RenderResult(CryptoResult result)
    {
        if (_resultMetaText is not null)
        {
            _resultMetaText.Text = $"{T("meta_algorithm")}: {result.Algorithm?.Name ?? "-"}\n{T("meta_operation")}: {OperationName(result.Operation)}\n{T("meta_input_length")}: {result.InputLength} {Unit("characters")}\n{T("meta_output_length")}: {result.OutputLength} {Unit("characters")}\n{T("meta_time")}: {result.ExecutionTimeMs:0.###} ms\n{T("meta_encoding")}: {result.Encoding}\n{T("meta_status")}: {(result.Success ? T("status_success") : result.StatusMessage)}";
        }
        if (_explanationText is not null)
        {
            _explanationText.Text = result.Success
                ? (Arabic ? ArabicContent.ExplainResult(result.Algorithm?.Id ?? string.Empty, result.Operation) : result.Explanation)
                : result.StatusMessage;
        }
        if (_visualizationPanel is not null)
        {
            _visualizationPanel.Children.Clear();
            _visualizationPanel.Children.Add(Text(T("visualize"), 16, true));
            if (result.Steps.Count == 0)
            {
                _visualizationPanel.Children.Add(Text(T("no_steps"), 13, false, "MutedTextBrush"));
            }
            else
            {
                foreach (var step in result.Steps.Take(24))
                {
                    var row = new Grid { Margin = new Thickness(0, 4, 0, 4) };
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    row.Children.Add(Cell(Arabic ? ArabicContent.StepLabel(step.Input) : step.Input, 0));
                    row.Children.Add(Cell(Arabic ? ArabicContent.StepLabel(step.Key) : step.Key, 1));
                    row.Children.Add(Cell(Arabic ? ArabicContent.StepLabel(step.Operation) : step.Operation, 2));
                    row.Children.Add(Cell(Arabic ? ArabicContent.StepLabel(step.Output) : step.Output, 3));
                    _visualizationPanel.Children.Add(row);
                }
            }
        }
    }

    private Border Cell(string text, int column)
    {
        var border = new Border { Background = R("CardAltBrush"), CornerRadius = new CornerRadius(6), Padding = new Thickness(8), Margin = new Thickness(3) };
        border.Child = Text(text, 12, false, "PrimaryTextBrush");
        Grid.SetColumn(border, column);
        return border;
    }

    private void AddHistory(CryptoResult result)
    {
        if (!_settings.SaveHistory) return;
        _history.Insert(0, new OperationHistoryEntry
        {
            Timestamp = DateTimeOffset.Now,
            Algorithm = result.Algorithm?.Name ?? "Unknown",
            AlgorithmId = result.Algorithm?.Id ?? string.Empty,
            Operation = result.Operation.ToString(),
            Success = result.Success,
            InputLength = result.InputLength,
            OutputLength = result.OutputLength,
            ExecutionTimeMs = result.ExecutionTimeMs,
            Status = result.StatusMessage
        });
        _history = _history.Take(250).ToList();
        _store.SaveHistory(_history);
    }

    private void GenerateRsaKeys()
    {
        var keySize = _rsaKeySizeCombo?.SelectedItem is int value ? value : 2048;
        var keys = RsaAlgorithm.GenerateKeyPair(keySize);
if (_publicKeyText is not null) _publicKeyText.Text = keys.PublicKeyPem;
        if (_privateKeyText is not null) _privateKeyText.Text = keys.PrivateKeyPem;
        ShowToast(T("toast_keys_generated"), true);
    }

    private void PasteInput()
    {
        if (_inputText is not null && Clipboard.ContainsText()) _inputText.Text = Clipboard.GetText();
    }

    private void CopyOutput()
    {
if (!string.IsNullOrWhiteSpace(_outputText?.Text))
        {
            Clipboard.SetText(_outputText.Text);
            ShowToast(T("toast_copied"), true);
        }
    }

    private void LoadInputFile()
    {
        var dialog = new OpenFileDialog { Filter = "Text files|*.txt;*.md;*.csv;*.json|All files|*.*" };
        if (dialog.ShowDialog(this) == true && _inputText is not null)
        {
            _inputText.Text = File.ReadAllText(dialog.FileName);
        }
    }

    private void SaveOutput()
    {
        if (_outputText is null || string.IsNullOrWhiteSpace(_outputText.Text)) return;
        var dialog = new SaveFileDialog { FileName = "cryptolab-result.txt", Filter = "Text files|*.txt|All files|*.*" };
if (dialog.ShowDialog(this) == true)
        {
            File.WriteAllText(dialog.FileName, _outputText.Text);
            ShowToast(T("toast_saved"), true);
        }
    }

    private async void ShowToast(string message, bool success)
    {
        ToastText.Text = message;
        ToastHost.Background = success ? R("AccentBrush") : R("DangerBrush");
        ToastHost.Visibility = Visibility.Visible;
        await Task.Delay(_settings.AnimationEffects ? 2200 : 1200);
        ToastHost.Visibility = Visibility.Collapsed;
    }
private void RenderAlgorithmsPage()
    {
        SetHeader(T("algorithms_title"), T("algorithms_subtitle"));
        var wrap = new WrapPanel();
        PageHost.Children.Add(wrap);
        foreach (var algorithm in _engine.Algorithms)
        {
            var stack = new StackPanel { Width = 330 };
            stack.Children.Add(Text(algorithm.Name, 20, true));
            stack.Children.Add(Text($"{T("label_category")}: {CategoryName(algorithm.Category)}", 13, true, "AccentBrush"));
            stack.Children.Add(Text($"{T("label_security")}: {SecurityName(algorithm.Security)}", 13, true, algorithm.Security == SecurityLevel.Modern ? "AccentBrush" : "DangerBrush"));
            stack.Children.Add(Text(Arabic ? ArabicContent.Description(algorithm.Id) : algorithm.Description, 13, false, "MutedTextBrush"));
            stack.Children.Add(Text(Arabic ? ArabicContent.Notice(algorithm.Id) : algorithm.Notice, 12, false, "MutedTextBrush"));
            var buttons = new WrapPanel();
            buttons.Children.Add(ActionButton(T("learn_more"), (_, _) => RenderAlgorithmDetail(algorithm.Id)));
            buttons.Children.Add(ActionButton(algorithm.SupportsHashing ? T("hash_it") : T("try_it"), (_, _) => { _selectedAlgorithmId = algorithm.Id; Navigate("crypto"); }, true));
            stack.Children.Add(buttons);
            wrap.Children.Add(Card(stack));
        }
    }

    private void RenderAlgorithmDetail(string algorithmId)
    {
        var algorithm = _engine.FindMetadata(algorithmId);
        if (algorithm is null) return;
        PageHost.Children.Clear();
        SetHeader(algorithm.Name, Arabic ? ArabicContent.Description(algorithmId) : algorithm.Description);
        var detail = AddCard(T("overview"));
        detail.Children.Add(Text(Arabic ? ArabicContent.Description(algorithmId) : algorithm.Description, 15));
        detail.Children.Add(Text(T("how_it_works_title"), 18, true));
        detail.Children.Add(Text(Arabic ? ArabicContent.ExplainAlgorithm(algorithmId) : EducationCatalog.ExplainAlgorithm(algorithmId), 14, false, "MutedTextBrush"));
        detail.Children.Add(Text(T("math_concept_title"), 18, true));
        detail.Children.Add(Text(Arabic ? ArabicContent.MathConcept(algorithmId) : MathConcept(algorithmId), 14, false, "MutedTextBrush"));
        detail.Children.Add(Text(T("advantages"), 18, true));
        var characteristics = Arabic ? ArabicContent.Advantages(algorithmId) : algorithm.KeyCharacteristics;
        detail.Children.Add(Text(string.Join("\n", characteristics.Select(c => "- " + c)), 13, false, "MutedTextBrush"));
        detail.Children.Add(Text(T("security_analysis"), 18, true));
        detail.Children.Add(Text(Arabic ? ArabicContent.Notice(algorithmId) : algorithm.Notice, 14, false, algorithm.Security == SecurityLevel.Modern ? "MutedTextBrush" : "DangerBrush"));
        var buttons = new WrapPanel();
        buttons.Children.Add(ActionButton(T("try_this"), (_, _) => { _selectedAlgorithmId = algorithm.Id; Navigate("crypto"); }, true));
        buttons.Children.Add(ActionButton(T("export_pdf"), (_, _) => ExportAlgorithmPdf(algorithm.Id)));
        buttons.Children.Add(ActionButton(T("back_to_algorithms"), (_, _) => Navigate("algorithms")));
        detail.Children.Add(buttons);

        var python = AddCard(T("python_title"));
        python.Children.Add(Text(PythonContent.Intro(algorithmId, Arabic), 14, false, "MutedTextBrush"));
        python.Children.Add(Text(T("python_verified"), 12, false, "AccentBrush"));
        python.Children.Add(Text(T("python_libraries"), 18, true));
        python.Children.Add(Text(PythonContent.Libraries(algorithmId, Arabic), 13, false, "MutedTextBrush"));
        python.Children.Add(Text(T("python_functions"), 18, true));
        foreach (var function in PythonContent.Functions(algorithmId, Arabic))
        {
            python.Children.Add(Text("• " + function, 13, false, "MutedTextBrush"));
        }
        python.Children.Add(Text(T("python_code"), 18, true));
        var codeBox = PythonHighlighter.Build(PythonContent.Code(algorithmId));
        codeBox.BorderBrush = R("ButtonBrush");
        codeBox.CaretBrush = R("AccentBrush");
        python.Children.Add(codeBox);
        python.Children.Add(ActionButton(T("copy_code"), (_, _) => { Clipboard.SetText(PythonContent.Code(algorithmId)); ShowToast(T("toast_code_copied"), true); }));
    }

    private void ExportAlgorithmPdf(string algorithmId)
    {
        try
        {
            var algorithm = _engine.FindMetadata(algorithmId);
            if (algorithm is null) return;
            var dialog = new SaveFileDialog { FileName = algorithmId + ".pdf", Filter = "PDF files|*.pdf" };
            if (dialog.ShowDialog(this) != true) return;

            byte[]? logo = null;
            try
            {
                using var stream = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/UniversityLogo.png"))?.Stream;
                if (stream is not null)
                {
                    using var memory = new MemoryStream();
                    stream.CopyTo(memory);
                    logo = memory.ToArray();
                }
            }
            catch (Exception)
            {
                // Logo is optional; the document still renders without it.
            }

            var data = new AlgorithmPdfData
            {
                AppTitle = "CryptoLab",
                AppSubtitle = T("app_subtitle"),
                AlgorithmName = algorithm.Name,
                MetaLine = $"{T("label_category")}: {CategoryName(algorithm.Category)}   |   {T("label_security")}: {SecurityName(algorithm.Security)}",
                OverviewTitle = T("overview"),
                Overview = Arabic ? ArabicContent.Description(algorithmId) : algorithm.Description,
                HowItWorksTitle = T("how_it_works_title"),
                HowItWorks = Arabic ? ArabicContent.ExplainAlgorithm(algorithmId) : EducationCatalog.ExplainAlgorithm(algorithmId),
                MathConceptTitle = T("math_concept_title"),
                MathConcept = Arabic ? ArabicContent.MathConcept(algorithmId) : MathConcept(algorithmId),
                AdvantagesTitle = T("advantages"),
                Advantages = Arabic ? ArabicContent.Advantages(algorithmId) : algorithm.KeyCharacteristics,
                SecurityAnalysisTitle = T("security_analysis"),
                SecurityNotice = Arabic ? ArabicContent.Notice(algorithmId) : algorithm.Notice,
                PythonTitle = T("python_title"),
                PythonIntro = PythonContent.Intro(algorithmId, Arabic),
                PythonVerified = T("python_verified"),
                PythonLibrariesTitle = T("python_libraries"),
                PythonLibraries = PythonContent.Libraries(algorithmId, Arabic),
                PythonFunctionsTitle = T("python_functions"),
                PythonFunctions = PythonContent.Functions(algorithmId, Arabic),
                PythonCodeTitle = T("python_code"),
                PythonCode = PythonContent.Code(algorithmId),
                Arabic = Arabic,
                LogoBytes = logo
            };
            File.WriteAllBytes(dialog.FileName, PdfExportService.Build(data));
            ShowToast(T("toast_pdf_exported"), true);
        }
        catch (Exception)
        {
            ShowToast(T("toast_pdf_failed"), false);
        }
    }

    private string CategoryName(AlgorithmCategory category) => Arabic ? ArabicContent.CategoryName(category) : category.ToString();

    private string SecurityName(SecurityLevel security) => Arabic ? ArabicContent.SecurityName(security) : security.ToString();

    private string MathConcept(string algorithmId)
    {
        return algorithmId switch
        {
            "caesar" => "E(x) = (x + shift) mod 26; D(y) = (y - shift) mod 26.",
            "affine" => "E(x) = (a*x + b) mod 26, where gcd(a, 26) = 1.",
            "vigenere" => "Each key character provides a different Caesar shift repeated across the message.",
            "rsa" => "RSA is based on modular exponentiation and the difficulty of factoring large composite numbers.",
            "aes" => "AES applies repeated substitution-permutation rounds over fixed-size blocks.",
            "md5" or "sha1" or "sha256" or "sha512" => "Hash functions compress arbitrary-length input into a fixed-size digest using one-way mixing operations.",
            _ => "This algorithm demonstrates substitution, transposition, keying, or digest construction concepts."
        };
    }

private void RenderLearningPage()
    {
        SetHeader(T("learning"), T("learning_subtitle"));
        var lessons = Arabic ? ArabicContent.Lessons : EducationCatalog.Lessons;
        foreach (var lesson in lessons)
        {
            var card = AddCard(lesson.Title);
            card.Children.Add(Text(lesson.Summary, 14, false, "MutedTextBrush"));
            foreach (var section in lesson.Sections)
            {
                card.Children.Add(Text(section, 13));
            }
            card.Children.Add(Text(T("interactive_exercises"), 15, true));
            foreach (var exercise in lesson.Exercises)
            {
                card.Children.Add(Text("- " + exercise, 13, false, "MutedTextBrush"));
            }
        }
    }

private void RenderComparePage()
    {
        SetHeader(T("compare"), T("compare_subtitle"));
        var card = AddCard(T("select_algorithms"));
        var algorithms = _engine.Algorithms.ToList();
        var first = new ComboBox { ItemsSource = algorithms, DisplayMemberPath = "Name", SelectedIndex = 0, Background = R("CardAltBrush"), Foreground = R("PrimaryTextBrush") };
        var second = new ComboBox { ItemsSource = algorithms, DisplayMemberPath = "Name", SelectedIndex = Math.Min(1, algorithms.Count - 1), Background = R("CardAltBrush"), Foreground = R("PrimaryTextBrush") };
        var result = new StackPanel { Margin = new Thickness(0, 14, 0, 0) };
        void Refresh()
        {
            result.Children.Clear();
            if (first.SelectedItem is AlgorithmMetadata a && second.SelectedItem is AlgorithmMetadata b)
            {
                result.Children.Add(ComparisonRow(T("compare_property"), a.Name, b.Name, true));
                result.Children.Add(ComparisonRow(T("compare_type"), CategoryName(a.Category), CategoryName(b.Category)));
                result.Children.Add(ComparisonRow(T("compare_security"), SecurityName(a.Security), SecurityName(b.Security)));
                result.Children.Add(ComparisonRow(T("compare_key_type"), KeyHintFor(a), KeyHintFor(b)));
                result.Children.Add(ComparisonRow(T("compare_main_use"), Arabic ? ArabicContent.ModernUsage(a.Id) : a.ModernUsage, Arabic ? ArabicContent.ModernUsage(b.Id) : b.ModernUsage));
                result.Children.Add(ComparisonRow(T("compare_reversible"), a.IsReversible ? T("yes") : T("no"), b.IsReversible ? T("yes") : T("no")));
                result.Children.Add(ComparisonRow(T("compare_notice"), Arabic ? ArabicContent.Notice(a.Id) : a.Notice, Arabic ? ArabicContent.Notice(b.Id) : b.Notice));
            }
        }
        first.SelectionChanged += (_, _) => Refresh();
        second.SelectionChanged += (_, _) => Refresh();
        card.Children.Add(Text(T("first_algorithm"), 13, true));
        card.Children.Add(first);
        card.Children.Add(Text(T("second_algorithm"), 13, true));
        card.Children.Add(second);
        card.Children.Add(result);
        Refresh();
    }

    private string KeyHintFor(AlgorithmMetadata metadata)
    {
        if (!metadata.RequiresKey) return T("no_key");
        return Arabic ? ArabicContent.KeyHint(metadata.Id) : metadata.KeyHint;
    }

    private Grid ComparisonRow(string property, string first, string second, bool header = false)
    {
        var grid = new Grid { Margin = new Thickness(0, 3, 0, 3) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(170) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.Children.Add(ComparisonCell(property, 0, header));
        grid.Children.Add(ComparisonCell(first, 1, header));
        grid.Children.Add(ComparisonCell(second, 2, header));
        return grid;
    }

    private Border ComparisonCell(string value, int column, bool header)
    {
        var border = new Border { Background = header ? R("AccentBrush") : R("CardAltBrush"), CornerRadius = new CornerRadius(4), Padding = new Thickness(8), Margin = new Thickness(2) };
        border.Child = new TextBlock { Text = value, TextWrapping = TextWrapping.Wrap, Foreground = header ? Brushes.Black : R("PrimaryTextBrush"), FontWeight = header ? FontWeights.Bold : FontWeights.Normal };
        Grid.SetColumn(border, column);
        return border;
    }
private void RenderHistoryPage()
    {
        SetHeader(T("history_title"), T("history_subtitle"));
        var card = AddCard(T("operations"));
        if (_history.Count == 0)
        {
            card.Children.Add(Text(T("empty_history"), 14, false, "MutedTextBrush"));
        }
        else
        {
            foreach (var item in _history.Take(80))
            {
                var status = string.Equals(item.Status, "Success", StringComparison.OrdinalIgnoreCase) ? T("status_success") : item.Status;
                card.Children.Add(Text($"{item.Timestamp:HH:mm:ss}   {item.Algorithm}   {OperationNameFromString(item.Operation)}   {status}   {item.ExecutionTimeMs:0.###} ms", 13, false, item.Success ? "PrimaryTextBrush" : "DangerBrush"));
            }
        }
        var buttons = new WrapPanel();
        buttons.Children.Add(ActionButton(T("clear_history"), (_, _) => { _history.Clear(); _store.SaveHistory(_history); Navigate("history"); }, danger: true));
        card.Children.Add(buttons);
    }

    private string OperationNameFromString(string operation)
    {
        return operation switch
        {
            "Encrypt" => T("encrypt"),
            "Decrypt" => T("decrypt"),
            "Hash" => T("hash"),
            _ => operation
        };
    }

private void RenderSettingsPage()
    {
        SetHeader(T("settings"), T("settings_subtitle"));
        var appearance = AddCard(T("appearance"));
        appearance.Children.Add(Text(T("appearance"), 13, true));
        var themeCombo = new ComboBox { Background = R("CardAltBrush"), Foreground = R("PrimaryTextBrush") };
        themeCombo.Items.Add(new ComboBoxItem { Content = T("dark"), Tag = "Dark" });
        themeCombo.Items.Add(new ComboBoxItem { Content = T("light"), Tag = "Light" });
        themeCombo.Items.Add(new ComboBoxItem { Content = T("system"), Tag = "System" });
        themeCombo.SelectedIndex = _settings.Theme == "Light" ? 1 : _settings.Theme == "System" ? 2 : 0;
        themeCombo.SelectionChanged += (_, _) =>
        {
            var theme = (themeCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Dark";
            _settings.Theme = theme;
            if (_settings.Theme == "System") _settings.Theme = "Dark";
            _store.SaveSettings(_settings);
            ApplyTheme();
            Navigate("settings");
        };
        appearance.Children.Add(themeCombo);

        appearance.Children.Add(Text(T("language"), 13, true));
        var languageCombo = new ComboBox { Background = R("CardAltBrush"), Foreground = R("PrimaryTextBrush") };
        languageCombo.Items.Add(new ComboBoxItem { Content = T("language_english"), Tag = "en" });
        languageCombo.Items.Add(new ComboBoxItem { Content = T("language_arabic"), Tag = "ar" });
        languageCombo.SelectedIndex = _settings.Language == "ar" ? 1 : 0;
        languageCombo.SelectionChanged += (_, _) =>
        {
            _settings.Language = (languageCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "ar" ? "ar" : "en";
            _store.SaveSettings(_settings);
            _localizer.SetLanguage(_settings.Language);
            ApplyLocalizationShell();
            Navigate("settings");
        };
        appearance.Children.Add(languageCombo);

        var general = AddCard(T("general"));
        AddSettingsCheck(general, T("auto_clear"), _settings.AutoClearInput, value => _settings.AutoClearInput = value);
        AddSettingsCheck(general, T("save_history"), _settings.SaveHistory, value => _settings.SaveHistory = value);
        AddSettingsCheck(general, T("animations"), _settings.AnimationEffects, value => _settings.AnimationEffects = value);

        var security = AddCard(T("security"));
        security.Children.Add(Text(T("security_note"), 13, false, "MutedTextBrush"));
        var buttons = new WrapPanel();
        buttons.Children.Add(ActionButton(T("clear_history"), (_, _) => { _history.Clear(); _store.SaveHistory(_history); ShowToast(T("toast_history_cleared"), true); }, danger: true));
        buttons.Children.Add(ActionButton(T("reset_app"), (_, _) => { _history.Clear(); _settings = new AppSettings(); _store.SaveSettings(_settings); _store.SaveHistory(_history); _localizer.SetLanguage(_settings.Language); ApplyTheme(); ApplyLocalizationShell(); Navigate("settings"); }, danger: true));
        security.Children.Add(buttons);
    }

    private void AddSettingsCheck(StackPanel panel, string label, bool value, Action<bool> update)
    {
        var check = new CheckBox { Content = label, IsChecked = value, Foreground = R("PrimaryTextBrush") };
        check.Checked += (_, _) => { update(true); _store.SaveSettings(_settings); };
        check.Unchecked += (_, _) => { update(false); _store.SaveSettings(_settings); };
        panel.Children.Add(check);
    }

private void RenderAboutPage()
    {
        SetHeader("CryptoLab", T("app_subtitle"));
        var card = AddCard("CryptoLab");
        var layout = new DockPanel();

        var universityBlock = new StackPanel
        {
            Margin = new Thickness(0, 0, 28, 0),
            Width = 220,
            VerticalAlignment = VerticalAlignment.Top
        };
        universityBlock.Children.Add(new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/Assets/UniversityLogo.png")),
            Width = 200,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center
        });
        var universityTitle = Text("جامعة الجزيرة", 16, true, "PrimaryTextBrush");
        universityTitle.TextAlignment = TextAlignment.Center;
        universityBlock.Children.Add(universityTitle);
        var universityDepartment = Text("قسم تقنية المعلومات", 14, false, "MutedTextBrush");
        universityDepartment.TextAlignment = TextAlignment.Center;
        universityBlock.Children.Add(universityDepartment);
        DockPanel.SetDock(universityBlock, Dock.Left);
        layout.Children.Add(universityBlock);

        var info = new StackPanel();
        info.Children.Add(Text(T("about_tagline"), 18, true, "AccentBrush"));
        info.Children.Add(Text($"{T("about_version")}: 1.0.0"));
        info.Children.Add(Text($"{T("about_developer")}: Abdulrahman Al-Zubaidi"));
        info.Children.Add(Text($"{T("about_technology")}: C# + .NET + WPF"));
        info.Children.Add(Text($"{T("about_purpose")}: {T("about_purpose_text")}"));
        info.Children.Add(Text($"{T("about_license")}: Educational use"));
        layout.Children.Add(info);
        card.Children.Add(layout);
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Enter)
        {
            ExecuteOperation(CryptoOperation.Encrypt);
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.Enter)
        {
            ExecuteOperation(CryptoOperation.Decrypt);
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.L)
        {
            _inputText?.Clear();
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
        {
            SaveOutput();
            e.Handled = true;
        }
    }
}
