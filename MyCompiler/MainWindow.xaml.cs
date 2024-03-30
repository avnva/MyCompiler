using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.IO;
using System.Xml;

namespace MyCompiler;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        CompilerViewModel viewModel = new CompilerViewModel();
        viewModel.StringSent += OnStringReceived;
        viewModel.LexemeSent += OnLexemeReceived;
        viewModel.ErrorSent += OnErrorReceived;
        DataContext = viewModel;

        Closing += MainWindow_Closing;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            using (StreamReader s = new StreamReader(@"Resources\Python.xshd"))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load syntax highlighting: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        
    }

    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (((CompilerViewModel)DataContext).IsFileModified)
        {
            if (MessageBoxEventArgs.ShowWindowClosingMessage() == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
    public void OnStringReceived(object sender, MessageEventArgs e)
    {
        textEditor.Document.Text = e.Message;
    }
    public void OnLexemeReceived(object sender, Lexeme e)
    {
        if (e != null && e.EndIndex <= textEditor.Document.Text.Length)
        {
            textEditor.Select(e.StartIndex - 1, e.EndIndex - e.StartIndex + 1);
        }
    }
    public void OnErrorReceived(object sender, ParserError e)
    {
        if (e != null && e.EndIndex <= textEditor.Document.Text.Length)
        {
            textEditor.Select(e.StartIndex - 1, e.EndIndex - e.StartIndex + 1);
        }
    }
    private void textEditor_TextChanged(object sender, EventArgs e)
    {
        undoButton.IsEnabled = textEditor.CanUndo;
        redoButton.IsEnabled = textEditor.CanRedo;

        undoMenuItem.IsEnabled = textEditor.CanUndo;
        redoMenuItem.IsEnabled = textEditor.CanRedo;

        GetCaretPosition();
    }
    private void CutText(object sender, RoutedEventArgs e)
    {
        if (textEditor.SelectionLength > 0)
        {
            Clipboard.SetText(textEditor.SelectedText);
            textEditor.Document.Remove(textEditor.SelectionStart, textEditor.SelectionLength);
        }
    }

    private void CopyText(object sender, RoutedEventArgs e)
    {
        if (textEditor.SelectionLength > 0)
        {
            Clipboard.SetText(textEditor.SelectedText);
        }
    }

    private void PasteText(object sender, RoutedEventArgs e)
    {
        if (Clipboard.ContainsText())
        {
            textEditor.Document.Insert(textEditor.CaretOffset, Clipboard.GetText());
        }
    }

    private void DeleteSelectedText(object sender, RoutedEventArgs e)
    {
        if (textEditor.SelectionLength > 0)
        {
            textEditor.Document.Remove(textEditor.SelectionStart, textEditor.SelectionLength);
        }
    }

    private void SelectAllText(object sender, RoutedEventArgs e)
    {
        textEditor.SelectAll();
    }

    private void textEditor_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        GetCaretPosition();
    }

    private void textEditor_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        GetCaretPosition();
    }

    private void GetCaretPosition()
    {
        int offset = textEditor.CaretOffset;
        var location = textEditor.Document.GetLocation(offset);
        CursorPositionTextBlock.Text = $"Строка: {location.Line}, Столбец: {location.Column}";
    }

    private void undoButton_Click(object sender, RoutedEventArgs e)
    {
        textEditor.Undo();
    }

    private void redoButton_Click(object sender, RoutedEventArgs e)
    {
        textEditor.Redo();
    }

    private void Window_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

        if (files != null && files.Length > 0)
        {
            (DataContext as CompilerViewModel)?.HandleDroppedFiles(files);
        }
    }
}