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

namespace MyCompiler;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    private void textEditor_TextChanged(object sender, EventArgs e)
    {
        undoButton.IsEnabled = textEditor.CanUndo;
        redoButton.IsEnabled = textEditor.CanRedo;

        //undoMenuItem.IsEnabled = textEditor.CanUndo;
        //redoMenuItem.IsEnabled = textEditor.CanRedo;

        GetCaretPosition();
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
}