using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;

namespace MyCompiler;

public class CompilerViewModel : ViewModelBase
{

    private FileManager _fileManager;
    private string _currentFilePath;
    private string _fileContent;
    private bool _isFileModified;
    private LexicalAnalyzer _lexicalAnalyzer;
    private Parser _parser;
    private string _vmText;
    private AssignmentOperatorParser _aOParser;


    private const string _aboutPath = @"Resources\About.html";
    private const string _helpPath = @"Resources\Help.html";
    private const string _grammarPath = @"Resources\Grammar.html";
    private const string _grammarClassificationPath = @"Resources\GrammarClassification.html";
    private const string _literaturePath = @"Resources\Literature.html";
    private const string _methodAnalysisPath = @"Resources\MethodOfAnalysis.html";
    private const string _neutralizingErrorsPath = @"Resources\NeutralizingErrors.html";
    private const string _taskSettingPath = @"Resources\ProblemStatement.html";
    private const string _correctTestCasePath = @"Resources\correct_test_case.txt";
    private const string _wrongTestCasePath = @"Resources\wrong_test_case.txt";
    private const string _sourceCode = @"https://github.com/avnva/MyCompiler";


    private RelayCommand _createNewFileCommand;
    private RelayCommand _openFileCommand;
    private RelayCommand _saveFileCommand;
    private RelayCommand _saveAsFileCommand;
    private RelayCommand _aboutCommand;
    private RelayCommand _helpCommand;
    private RelayCommand _exitCommand;
    private RelayCommand _startAnalyzersCommand;
    private RelayCommand _grammarClassificationCommand;
    private RelayCommand _grammarCommand;
    private RelayCommand _problemStatementCommand;
    private RelayCommand _literatureCommand;
    private RelayCommand _neutralizingErrorsCommand;
    private RelayCommand _methodOfAnalysisCommand;
    private RelayCommand _viewSourceCodeCommand;
    private RelayCommand _removeErrorsCommand;
    private RelayCommand _RPNConvertCommand;

    public event EventHandler<MessageEventArgs> StringSent;
    public event EventHandler RequestClose;
    public event EventHandler<Lexeme> LexemeSent;
    public event EventHandler<ParserError> ErrorSent;

    private List<Lexeme> _lexemesList;
    private ObservableCollection<Lexeme> _lexemes;
    private ObservableCollection<ParserError> _incorrectLexemes;
    private Lexeme _selectedLexeme;
    private ParserError _selectedError;
    private RelayCommand _openTestCaseCommand;
    private RelayCommand _openWrongTestCaseCommand;
    private RelayCommand _reg1Command;
    private RelayCommand _reg2Command;
    private RelayCommand _reg3Command;
    private RelayCommand _parseAOCommand;

    public ObservableCollection<Lexeme> Lexemes
    {
        get { return _lexemes; }
        set
        {
            _lexemes = value;
            OnPropertyChanged(nameof(Lexemes));
        }
    }

    public ObservableCollection<ParserError> IncorrectLexemes
    {
        get { return _incorrectLexemes; }
        set
        {
            _incorrectLexemes = value;
            OnPropertyChanged(nameof(IncorrectLexemes));
        }
    }

    public Lexeme SelectedLexeme
    {
        get { return _selectedLexeme; }
        set
        {
            _selectedLexeme = value;
            LexemeSent(this, value);
            OnPropertyChanged(nameof(SelectedLexeme));
        }
    }

    public ParserError SelectedError
    {
        get { return _selectedError; }
        set
        {
            _selectedError = value;
            ErrorSent(this, value);
            OnPropertyChanged(nameof(SelectedLexeme));
        }
    }

    public string FileContent
    {
        get { return _fileContent; }
        set
        {
            _fileContent = value;
            IsFileModified = true;
            OnPropertyChanged(nameof(FileContent));
        }
    }

    public bool IsFileModified
    {
        get { return _isFileModified; }
        set
        {
            _isFileModified = value;
            OnPropertyChanged(nameof(IsFileModified));
            OnPropertyChanged(nameof(WindowTitle));
        }
    }

    public string CurrentFilePath
    {
        get { return _currentFilePath; }
        set
        {
            _currentFilePath = value;
            OnPropertyChanged(nameof(CurrentFilePath));
            OnPropertyChanged(nameof(WindowTitle));
        }
    }

    public string VMText
    {
        get { return _vmText; }
        set
        {
            _vmText = value;
            OnPropertyChanged(nameof(VMText));
        }
    }
    public string WindowTitle
    {
        get => $"MyCompiler — {((CurrentFilePath == string.Empty) ? "Новый файл.txt" : "")}{_currentFilePath.Split(@"\").Last()}{(IsFileModified ? "*" : "")} {((CurrentFilePath != string.Empty) ? "(" : "")}{_currentFilePath}{((CurrentFilePath != string.Empty) ? ")" : "")}";
    }


    public CompilerViewModel()
    {
        _fileManager = new FileManager();
        _lexicalAnalyzer = new LexicalAnalyzer();
        IncorrectLexemes = new ObservableCollection<ParserError>();
        _parser = new Parser();
        _fileContent = string.Empty;
        CurrentFilePath = string.Empty;
        IsFileModified = false;
        _aOParser = new AssignmentOperatorParser();
    }

    public RelayCommand CreateNewFileCommand
    {
        get => _createNewFileCommand ??= new RelayCommand(CreateNewFile);
    }

    public RelayCommand OpenFileCommand
    {
        get => _openFileCommand ??= new RelayCommand(OpenFile);
    }

    public RelayCommand SaveFileCommand
    {
        get => _saveFileCommand ??= new RelayCommand(SaveFile, _ => _isFileModified || CurrentFilePath == string.Empty);
    }

    public RelayCommand SaveAsFileCommand
    {
        get => _saveAsFileCommand ??= new RelayCommand(SaveAsFile);
    }

    public RelayCommand ExitCommand
    {
        get => _exitCommand ??= new RelayCommand(Exit);
    }
    public RelayCommand AboutCommand
    {
        get => _aboutCommand ??= new RelayCommand(_ => HTMLManager.OpenInBrowser(_aboutPath));
    }

    public RelayCommand HelpCommand
    {
        get => _helpCommand ??= new RelayCommand(_ => HTMLManager.OpenInBrowser(_helpPath));
    }
    public RelayCommand StartAnalyzersCommand
    {
        get => _startAnalyzersCommand ??= new RelayCommand(StartAnalysis);
    }
    public RelayCommand RemoveErrorsCommand
    {
        get => _removeErrorsCommand ??= new RelayCommand(RemoveErrors);
    }
    public RelayCommand RPNConvertCommand
    {
        get => _RPNConvertCommand ??= new RelayCommand(RPNConvert);
    }






    public RelayCommand NeutralizingErrorsCommand
    {
        get => _neutralizingErrorsCommand ??= new RelayCommand(_ => HTMLManager.OpenInBrowser(_neutralizingErrorsPath));
    }

    public RelayCommand MethodAnalysisCommand
    {
        get => _methodOfAnalysisCommand ??= new RelayCommand(_ => HTMLManager.OpenInBrowser(_methodAnalysisPath));
    }

    public RelayCommand GrammarClassificationCommand
    {
        get => _grammarClassificationCommand ??= new RelayCommand(_ => HTMLManager.OpenInBrowser(_grammarClassificationPath));
    }

    public RelayCommand GrammarCommand
    {
        get => _grammarCommand ??= new RelayCommand(_ => HTMLManager.OpenInBrowser(_grammarPath));
    }

    public RelayCommand TaskSettingCommand
    {
        get => _problemStatementCommand ??= new RelayCommand(_ => HTMLManager.OpenInBrowser(_taskSettingPath));
    }

    public RelayCommand LiteratureCommand
    {
        get => _literatureCommand ??= new RelayCommand(_ => HTMLManager.OpenInBrowser(_literaturePath));
    }

    public RelayCommand ViewSourceCodeCommand
    {
        get => _viewSourceCodeCommand ??= new RelayCommand(_ => HTMLManager.OpenInBrowser(_sourceCode));
    }
    public RelayCommand OpenTestCaseCommand
    {
        get => _openTestCaseCommand ??= new RelayCommand(OpenTestCase);
    }

    public RelayCommand OpenWrongTestCaseCommand
    {
        get => _openWrongTestCaseCommand ??= new RelayCommand(OpenWrongTestCase);
    }
    public RelayCommand Reg1Command
    {
        get => _reg1Command ??= new RelayCommand(FindReg1);
    }

    public RelayCommand Reg2Command
    {
        get => _reg2Command ??= new RelayCommand(FindReg2);
    }

    public RelayCommand Reg3Command
    {
        get => _reg3Command ??= new RelayCommand(FindReg3);
    }
    public RelayCommand ParseAOCommand
    {
        get => _parseAOCommand ??= new RelayCommand(ParseAO);
    }
    public void ParseAO(object obj)
    {
        VMText = _aOParser.Parse(_lexicalAnalyzer.Analyze(_fileContent));
    }
    public void FindReg1(object obj)
    {
        PrintRegResult(RegEx.ValidateTime(_fileContent));
    }

    public void FindReg2(object obj)
    {
        PrintRegResult(RegEx.ValidatePhoneNumber(_fileContent));
    }

    public void FindReg3(object obj)
    {
        PrintRegResult(RegEx.ValidatePassword(_fileContent));
    }

    private void PrintRegResult(List<Match> matches)
    {
        VMText = "";
        bool flag = false;
        foreach (Match match in matches)
        {
            flag = true;
            VMText += $"Найдено: '{match.Value}', позиция: {match.Index}\n";
        }
        if (!flag)
        {
            VMText += $"Ничего не найдено";
        }
    }
    public void OpenTestCase(object obj)
    {
        CurrentFilePath = _correctTestCasePath;
        ReadFileContent();
    }

    public void OpenWrongTestCase(object obj)
    {
        CurrentFilePath = _wrongTestCasePath;
        ReadFileContent();
    }
    public void RemoveErrors(object obj)
    {
        StartAnalysis();
        FileContent = TextCleaner.RemoveIncorrectLexemes(_fileContent, _incorrectLexemes);
        StartAnalysis();

        SendString(_fileContent);
    }

    public void CreateNewFile(object obj)
    {
        if (CancelOperationAfterCheckingForUnsavedChanges())
            return;

        FileContent = string.Empty;
        SendString(_fileContent);
        CurrentFilePath = string.Empty;
        IsFileModified = true;
    }

    public void OpenFile(object obj = null)
    {
        if (CancelOperationAfterCheckingForUnsavedChanges())
            return;

        OpenFileDialog openFileDialog = new OpenFileDialog();
        if (openFileDialog.ShowDialog() == true)
        {
            _currentFilePath = openFileDialog.FileName;
            ReadFileContent();
        }
    }

    public void SaveAsFile(object obj = null)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "Текстовый файл (*.txt)|*.txt|C# файл (*.cs)|*.cs|C файл (*.c)|*.c|C++ файл (*.cpp)|*.cpp|Python файл (*.py)|*.py|JavaScript файл (*.js)|*.js|HTML файл (*.html)|*.html|CSS файл (*.css)|*.css|XML файл (*.xml)|*.xml|JSON файл (*.json)|*.json|Markdown файл (*.md)|*.md|PHP файл (*.php)|*.php|Java файл (*.java)|*.java|Все файлы (*.*)|*.*";

        if (saveFileDialog.ShowDialog() == true)
        {
            _currentFilePath = saveFileDialog.FileName;
            _fileManager.SaveFile(_currentFilePath, FileContent);
            IsFileModified = false;
        }
    }

    public void SaveFile(object obj = null)
    {
        if (string.IsNullOrEmpty(_currentFilePath))
        {
            SaveAsFile();
        }
        else
        {
            _fileManager.SaveFile(_currentFilePath, FileContent);
            IsFileModified = false;
        }
    }
    public void HandleDroppedFiles(string[] files)
    {
        if (CancelOperationAfterCheckingForUnsavedChanges())
            return;

        CurrentFilePath = files[0];
        ReadFileContent();
    }
    public void Exit(object obj = null)
    {
        if (CancelOperationAfterCheckingForUnsavedChanges())
            return;

        OnRequestClose();
    }

    private void OnRequestClose()
    {
        // Получаем текущее приложение
        var currentApplication = Application.Current as App;

        if (currentApplication != null)
        {
            // Закрываем приложение
            currentApplication.Shutdown();
        }

        //RequestClose?.Invoke(this, EventArgs.Empty);
    }
    public void SendString(string message)
    {
        if (StringSent != null)
        {
            StringSent(this, new MessageEventArgs(message));
        }
    }

    public bool CancelOperationAfterCheckingForUnsavedChanges()
    {
        if (_isFileModified)
        {
            var result = MessageBoxEventArgs.ShowUnsavedChangesMessage();

            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveFile();
                    return false;
                case MessageBoxResult.No:
                    return false;
                case MessageBoxResult.Cancel:
                    return true;
            }
        }

        return false;
    }
    public void ReadFileContent()
    {
        FileContent = _fileManager.OpenFile(_currentFilePath);
        SendString(_fileContent);
        IsFileModified = false;
    }
    public void StartAnalysis(object obj = null)
    {
        //LexicalAnalysis();
        Parsing();
    }
    public void LexicalAnalysis()
    {
        _lexemesList = _lexicalAnalyzer.Analyze(FileContent);

        Lexemes = new ObservableCollection<Lexeme>(_lexemesList);
    }
    public void Parsing()
    {
        _lexemesList = _lexicalAnalyzer.Analyze(FileContent);

        Lexemes = new ObservableCollection<Lexeme>(_lexemesList);
        IncorrectLexemes = new ObservableCollection<ParserError>(_parser.Parse(_lexemesList));
    }
    public void RPNConvert(object obj)
    {
        try
        {
            RPNConverter polishNotationCalculator = new(_fileContent.Replace("\n", "").Replace("\r", "").Replace(" ", ""));
            VMText = $"Исходное арифметическое выражение:\n{polishNotationCalculator.input}\n" +
                string.Join(" ", polishNotationCalculator.ConvertToRPN());
        }
        catch (Exception ex)
        {
            VMText = ex.Message;
        }
    }
}
