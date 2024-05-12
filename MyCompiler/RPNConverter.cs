using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler;

public class RPNConverter
{
    private readonly LexicalAnalyzer lexicalAnalyzer;
    public string input { get; private set; }
    public RPNConverter(string _input)
    {
        lexicalAnalyzer = new LexicalAnalyzer();
        input = _input;
    }

    public string ConvertToRPN()
    {
        try
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), "Входная строка не может быть null.");
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Входная строка не может быть пустой или содержать только пробельные символы.");

            var lexemes = lexicalAnalyzer.Analyze(input);
            var output = new List<string>();
            var outputExpr = new List<string>();
            var stack = new Stack<string>();

            bool prevOperand = false;
            bool exceptionFlag = false;

            foreach (var lexeme in lexemes)
            {
                switch (lexeme.Type)
                {
                    case LexemeType.Integer:
                    case LexemeType.Float:
                    case LexemeType.Identifier:
                        // Проверяем, что предыдущая лексема была оператором или открывающей скобкой
                        if (prevOperand)
                        {
                            output.Add($"{lexeme.StartIndex} - {lexeme.EndIndex} символы: Неверный формат выражения - ожидался оператор\n");
                            exceptionFlag = true;
                        }
                        outputExpr.Add(lexeme.Value);
                        prevOperand = true;
                        break;
                    case LexemeType.OpenBracket:
                        if (prevOperand)
                        {
                            output.Add($"{lexeme.StartIndex} - {lexeme.EndIndex} символы: Неверный формат выражения - ожидался оператор\n");
                            exceptionFlag = true;
                        }
                        stack.Push(lexeme.Value);
                        prevOperand = false;
                        break;
                    case LexemeType.CloseBracket:
                        if (!prevOperand)
                        {
                            output.Add($"{lexeme.StartIndex} - {lexeme.EndIndex} символы: Неверный формат выражения - ожидалось число или идентификатор\n");
                            exceptionFlag = true;
                        }
                        while (stack.Count > 0 && stack.Peek() != "(")
                        {
                            outputExpr.Add(stack.Pop());
                        }
                        if (stack.Count == 0)
                        {
                            output.Add($"{lexeme.StartIndex} - {lexeme.EndIndex} символы: Несогласованные скобки\n");
                            exceptionFlag = true;
                        }
                        stack.Pop(); // Удаляем открывающую скобку из стека
                        prevOperand = true;
                        break;
                    case LexemeType.Plus:
                    case LexemeType.Multiply:
                    case LexemeType.Divide:
                        // Проверяем, что предыдущая лексема была операндом или закрывающей скобкой
                        if (!prevOperand)
                        {
                            output.Add($"{lexeme.StartIndex} - {lexeme.EndIndex} символы: Неверный формат выражения - ожидалось число или идентификатор\n");
                            exceptionFlag = true;
                        }

                        while (stack.Count > 0 && Precedence(stack.Peek()) >= Precedence(lexeme.Value))
                        {
                            outputExpr.Add(stack.Pop());
                        }
                        stack.Push(lexeme.Value);
                        prevOperand = false;
                        break;
                    case LexemeType.Minus:
                        // Проверяем, является ли минус унарным или бинарным оператором
                        if (!prevOperand && (output.Count == 0 || output[output.Count - 1] == "("))
                        {
                            // Унарный минус
                            stack.Push("~");
                        }
                        else
                        {
                            // Бинарный минус
                            while (stack.Count > 0 && Precedence(stack.Peek()) >= Precedence(lexeme.Value))
                            {
                                outputExpr.Add(stack.Pop());
                            }
                            stack.Push(lexeme.Value);
                        }
                        prevOperand = false;
                        break;
                    default:
                        output.Add($"{lexeme.StartIndex} - {lexeme.EndIndex} символы: Недопустимый символ\n");
                        exceptionFlag = true;
                        break;
                }
            }

            // Проверяем, что последняя лексема была операндом или закрывающей скобкой
            if (!prevOperand)
            {
                output.Add($"{lexemes[^1].StartIndex} - {lexemes[^1].EndIndex} символы: Неверный формат выражения - ожидалось число или идентификатор\n");
                exceptionFlag = true;
            }

            while (stack.Count > 0)
                {
                    if (stack.Peek() == "(")
                    {
                        output.Add($"{lexemes[^1].StartIndex} - {lexemes[^1].EndIndex} символы: Несогласованные скобки\n");
                        exceptionFlag = true;
                    }
                    outputExpr.Add(stack.Pop());
                }
            if (exceptionFlag)
                return string.Join("", output);
            else
                return string.Join(" ", outputExpr);
        }
        catch (Exception ex)
        {
            return $"Ошибка: {ex.Message}";
        }
    }

    // Метод для определения приоритета оператора
    private int Precedence(string op)
    {
        switch (op)
        {
            case "+":
            case "-":
                return 1;
            case "*":
            case "/":
                return 2;
            default:
                return 0;
        }
    }
}