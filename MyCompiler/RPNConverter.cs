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
    //public string postfixExpr { get; private set; }
    public RPNConverter(string _input)
    {
        lexicalAnalyzer = new LexicalAnalyzer();
        input = _input;
        //infixExpr = expression;
        //postfixExpr = ToPostfix(infixExpr + "\r");
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
            var stack = new Stack<string>();

            // Флаг, указывающий на то, что предыдущая лексема была операндом
            bool prevOperand = false;

            foreach (var lexeme in lexemes)
            {
                switch (lexeme.Type)
                {
                    case LexemeType.Integer:
                    case LexemeType.Float:
                    case LexemeType.Identifier:
                        // Проверяем, что предыдущая лексема была оператором или открывающей скобкой
                        if (prevOperand)
                            throw new ArgumentException($"Неверный формат выражения: ожидался оператор. \nМестоположение: с {lexeme.StartIndex} по {lexeme.EndIndex} символы");

                        output.Add(lexeme.Value);
                        prevOperand = true;
                        break;
                    case LexemeType.OpenBracket:
                        // Проверяем, что предыдущая лексема была оператором или открывающей скобкой
                        if (prevOperand)
                            throw new ArgumentException($"Неверный формат выражения: ожидался оператор. \nМестоположение: с {lexeme.StartIndex} по {lexeme.EndIndex} символы");

                        stack.Push(lexeme.Value);
                        prevOperand = false;
                        break;
                    case LexemeType.CloseBracket:
                        // Проверяем, что предыдущая лексема была операндом или закрывающей скобкой
                        if (!prevOperand)
                            throw new ArgumentException($"Неверный формат выражения: ожидался операнд. \nМестоположение: с {lexeme.StartIndex} по {lexeme.EndIndex} символы");

                        while (stack.Count > 0 && stack.Peek() != "(")
                        {
                            output.Add(stack.Pop());
                        }
                        if (stack.Count == 0)
                            throw new ArgumentException($"Несогласованные скобки. \nМестоположение: с {lexeme.StartIndex} по {lexeme.EndIndex} символы");
                        stack.Pop(); // Удаляем открывающую скобку из стека
                        prevOperand = true;
                        break;
                    case LexemeType.Plus:
                    case LexemeType.Multiply:
                    case LexemeType.Divide:
                        // Проверяем, что предыдущая лексема была операндом или закрывающей скобкой
                        if (!prevOperand)
                            throw new ArgumentException($"Неверный формат выражения: ожидался операнд. \nМестоположение: с {lexeme.StartIndex} по {lexeme.EndIndex}  символы");

                        while (stack.Count > 0 && Precedence(stack.Peek()) >= Precedence(lexeme.Value))
                        {
                            output.Add(stack.Pop());
                        }
                        stack.Push(lexeme.Value);
                        prevOperand = false;
                        break;
                    case LexemeType.Minus:
                        // Проверяем, является ли минус унарным или бинарным оператором
                        if (!prevOperand && (output.Count == 0 || output[output.Count - 1] == "("))
                        {
                            // Унарный минус
                            stack.Push("~"); // Используем символ "~" для обозначения унарного минуса
                        }
                        else
                        {
                            // Бинарный минус
                            while (stack.Count > 0 && Precedence(stack.Peek()) >= Precedence(lexeme.Value))
                            {
                                output.Add(stack.Pop());
                            }
                            stack.Push(lexeme.Value);
                        }
                        prevOperand = false;
                        break;
                    default:
                        throw new ArgumentException($"Недопустимый символ. \nМестоположение: с {lexeme.StartIndex} по {lexeme.EndIndex}  символы");
                }
            }

            // Проверяем, что последняя лексема была операндом или закрывающей скобкой
            if (!prevOperand)
                throw new ArgumentException($"Неверный формат выражения: ожидался операнд. \nМестоположение: с {lexemes[^1].StartIndex} по {lexemes[^1].EndIndex} символы");

            while (stack.Count > 0)
            {
                if (stack.Peek() == "(")
                    throw new ArgumentException($"Несогласованные скобки. \nМестоположение: с {lexemes[^1].StartIndex} по {lexemes[^1].EndIndex}  символы");
                output.Add(stack.Pop());
            }

            return string.Join(" ", output);
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