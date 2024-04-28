using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler;

public class LexicalAnalyzer
{
    List<Lexeme> Lexemes;

    public List<Lexeme> Analyze(string input)
    {
        int i;
        string value;

        Lexemes.Clear();

        for (i = 0; i < input.Length; i++)
        {
            value = string.Empty + input[i];

            if (char.IsLetter(input[i]))
            {
                int startIndex = i;

                while ((i + 1) < input.Length && (char.IsLetterOrDigit(input[i + 1]) || input[i + 1] == '_'))
                {
                    i++;
                    value += input[i];
                }

                switch (value)
                {
                    case "lambda":
                        Lexemes.Add(new Lexeme(LexemeType.Lambda, value, startIndex + 1, i + 1));
                        break;
                    default:
                        Lexemes.Add(new Lexeme(LexemeType.Identifier, value, startIndex + 1, i + 1));
                        break;
                }
            }
            else
            {
                if (char.IsDigit(input[i]))
                {
                    int startIndex = i;

                    while ((i + 1) < input.Length && char.IsDigit(input[i + 1]))
                    {
                        i++;
                        value += input[i];
                    }
                    if ((i + 2) < input.Length && input[i + 1] == '.' && char.IsDigit(input[i + 2]))
                    {
                        i++;
                        value += input[i];
                        while ((i + 1) < input.Length && char.IsDigit(input[i + 1]))
                        {
                            i++;
                            value += input[i];
                        }
                        Lexemes.Add(new Lexeme(LexemeType.Float, value, startIndex + 1, i + 1));
                    }
                    else 
                        Lexemes.Add(new Lexeme(LexemeType.Integer, value, startIndex + 1, i + 1));
                }
                else
                {
                    switch (input[i])
                    {
                        case '\t': //tab
                        case ' ':
                            //Lexemes.Add(new Lexeme(LexemeType.Whitespace, value, i + 1, i + 1));
                            break;
                        case (char)13:
                            if ((i + 1) < input.Length && input[i + 1] == (char)10)
                            {
                                i++;
                                value = "\\n";
                                //Lexemes.Add(new Lexeme(LexemeType.NewLine, value, i, i + 1));
                            }
                            else
                            {
                                Lexemes.Add(new Lexeme(LexemeType.InvalidCharacter, value, i + 1, i + 1));
                            }
                            break;
                        case ':':
                            Lexemes.Add(new Lexeme(LexemeType.Colon, value, i + 1, i + 1));
                            break;
                        case '=':
                            Lexemes.Add(new Lexeme(LexemeType.AssignmentOperator, value, i + 1, i + 1));
                            break;
                        case '+':
                            Lexemes.Add(new Lexeme(LexemeType.Plus, value, i + 1, i + 1));
                            break;
                        case '-':
                            Lexemes.Add(new Lexeme(LexemeType.Minus, value, i + 1, i + 1));
                            break;
                        case '*':
                            Lexemes.Add(new Lexeme(LexemeType.Multiply, value, i + 1, i + 1));
                            break;
                        case '/':
                            Lexemes.Add(new Lexeme(LexemeType.Divide, value, i + 1, i + 1));
                            break;
                        case ';':
                            Lexemes.Add(new Lexeme(LexemeType.Semicolon, value, i + 1, i + 1));
                            break;
                        case ',':
                            Lexemes.Add(new Lexeme(LexemeType.Comma, value, i + 1, i + 1));
                            break;
                        case '(':
                            Lexemes.Add(new Lexeme(LexemeType.OpenBracket, value, i + 1, i + 1));
                            break;
                        case ')':
                            Lexemes.Add(new Lexeme(LexemeType.CloseBracket, value, i + 1, i + 1));
                            break;
                        default:
                            Lexemes.Add(new Lexeme(LexemeType.InvalidCharacter, value, i + 1, i + 1));
                            break;
                    }
                }
            }
        }

        return Lexemes;
    }

    public LexicalAnalyzer()
    {
        Lexemes = new List<Lexeme>();
    }
}