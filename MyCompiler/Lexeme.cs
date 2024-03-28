using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler;

public enum LexemeType
{
    Lambda = 1,
    Identifier = 2,
    Whitespace = 3,
    NewLine = 4,
    AssignmentOperator = 5,
    StartFunction = 6,
    Comma = 7,
    IntDigit = 8,
    FloatDigit = 9,
    Plus = 10,
    Minus = 11,
    Multiply = 12,
    Divide = 13,
    Semicolon = 14,
    InvalidCharacter = 15
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
            "Ключевое слово",
            "Идентификатор",
            "Пробел",
            "Новая строка",
            "Оператор присваивания",
            "Оператор начала функции",
            "Оператор перечисления",
            "Целое число",
            "Вещественное число",
            "Оператор сложения",
            "Оператор вычитания",
            "Оператор умножения",
            "Оператор деления",
            "Конец оператора",
            "Недопустимый символ"
        ];
    }
}
