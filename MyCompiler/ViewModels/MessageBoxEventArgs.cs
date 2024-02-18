using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyCompiler;

public class MessageBoxEventArgs
{
    public static MessageBoxResult ShowUnsavedChangesMessage()
    {
        MessageBoxResult result = MessageBox.Show("Не все изменения были сохранены. Хотите сохранить их?", "Предупреждение",
            MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

        return result;
    }

    public static MessageBoxResult ShowWindowClosingMessage()
    {
        MessageBoxResult result = MessageBox.Show("Вы точно хотите выйти? Все несохранённые изменения будут утеряны",
            "Подтверждение закрытия", MessageBoxButton.YesNo, MessageBoxImage.Question);

        return result;
    }
}
