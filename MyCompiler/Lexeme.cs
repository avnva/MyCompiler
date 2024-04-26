using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler;

//public enum LexemeType
//{
//    Lambda = 1,
//    Identifier = 2,
//    Whitespace = 3,
//    NewLine = 4,
//    AssignmentOperator = 5,
//    StartFunction = 6,
//    Comma = 7,
//    IntDigit = 8,
//    FloatDigit = 9,
//    Plus = 10,
//    Minus = 11,
//    Multiply = 12,
//    Divide = 13,
//    Semicolon = 14,
//    InvalidCharacter = 15
//}

public enum LexemeType
{
    Identifier = 1,             // имя переменной
    Lambda = 2,                 // lambda
    Whitespace = 3,             // пробел
    NewLine = 4,                // \n
    AssignmentOperator = 5,     // =
    Comma = 6,                  // ,
    Colon = 7,                  // :
    Integer = 8,                // цифра
    Float = 9,                  // цифра.цифра
    Plus = 10,                  // +
    Minus = 11,                 // -
    Multiply = 12,              // *
    Divide = 13,                // /
    Semicolon = 14,             // ;
    CloseBracket = 15,          // )
    OpenBracket = 16,           // (
    InvalidCharacter = 17
}


public class Lexeme
{
    private string[] lexemeNames;
    private string message;

    public int LexemeId { get => (int)Type; }
    public string LexemeName { get => lexemeNames[LexemeId - 1]; }
    public LexemeType Type { get; set; }
    public string Value { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public string Position { get => $"с {StartIndex} по {EndIndex} символы"; }
    public string Message
    {
        get => $"{message} (Отброшенный фрагмент: \"{Value}\")";
        set => message = value;
    }

    public Lexeme(LexemeType type, string value, int startIndex, int endIndex)
    {
        Type = type;
        Value = value;
        StartIndex = startIndex;
        EndIndex = endIndex;

        lexemeNames =
        [
            "Идентификатор",
            "Ключевое слово",
            "Пробел",
            "Новая строка",
            "Оператор присваивания",
            "Запятая",
            "Двоеточие",
            "Целое число",
            "Вещественное число",
            "Оператор сложения",
            "Оператор вычитания",
            "Оператор умножения",
            "Оператор деления",
            "Конец оператора",
            "Закрывающаяся скобка",
            "Открывающаяся скобка",
            "Недопустимый символ"
        ];
    }
}
