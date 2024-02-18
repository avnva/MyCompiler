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
        DataContext = viewModel;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            using (StreamReader s = new StreamReader("DeepBlack.xshd"))
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

    public void OnStringReceived(object sender, MessageEventArgs e)
    {
        textEditor.Document.Text = e.Message;
    }

    private void textEditor_TextChanged(object sender, EventArgs e)
    {
        undoButton.IsEnabled = textEditor.CanUndo;
        redoButton.IsEnabled = textEditor.CanRedo;

        //undoMenuItem.IsEnabled = textEditor.CanUndo;
        //redoMenuItem.IsEnabled = textEditor.CanRedo;

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
}