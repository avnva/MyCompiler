using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyCompiler;

public class AssignmentOperatorParser
{
    private string result = "";
    private List<Lexeme> tokens;
    private Lexeme CurrToken;
    private int CurrIndex;
    private int MaxIndex;
    private const string sep1 = " → ";
    public string Parse(List<Lexeme> tokensList)
    {
        tokens = tokensList;
        CurrIndex = 0;
        MaxIndex = tokensList.Count - 1;
        CurrToken = tokens[CurrIndex];
        result = string.Empty;

        try
        {
            ArithmeticOperator(false);
        }
        catch (SyntaxErrorException)
        {
            log("Syntax Error: Обнаружено незаконченное выражение.");
        }

        return result;
    }

    private void log(string str, string sep = sep1)
    {
        result += (result == string.Empty) ? str : $"{sep}{str}";
    }

    private void ChangeCurrentToken()
    {
        if (CanGetNext())
        {
            CurrIndex++;
            CurrToken = tokens[CurrIndex];
        }
        else
        {
            throw new SyntaxErrorException();
        }
    }

    private bool CanGetNext() => CurrIndex < MaxIndex;
    private LexemeType GetNextType()
    {
        return CanGetNext() ? tokens[CurrIndex + 1].Type : LexemeType.InvalidCharacter;
    }

    private void ArithmeticOperator(bool get)
    {
        if (get) ChangeCurrentToken();
        log("<AO>", "");
        bool flag = false;
        if (CurrToken.Type == LexemeType.Identifier)
        {
            log("id", sep1);
            Assignment(true);
            flag = true;
        }
        else
        {
            if (CurrToken.Type == LexemeType.AssignmentOperator)
            {
                log("Syntax Error: Ожидалcя идентификатор.");
                Assignment(false);
                flag = true;
            }
            else
            {
                log($"Syntax Error: Ожидался идентификатор, а встречено \"{CurrToken.Value}\"");

                if (GetNextType() == LexemeType.Identifier)
                    ArithmeticOperator(true);
                else
                {
                    Assignment(true);
                    flag = true;
                }
            }
        }
        if (flag)
        {
            if (CurrToken.Type == LexemeType.Semicolon)
            {
                log(";", sep1);
                log("\n", "\n");

                if (CanGetNext())
                {
                    ArithmeticOperator(true);
                }
            }
            else
            {
                log("Syntax Error: Ожидался оператор конца выражения \";\".");
            }
        }
    }
    private void Assignment(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();

        if (CurrToken.Type == LexemeType.AssignmentOperator)
        {
            log("=", sep1);
            Expr(true);
        }
        else
        {

            if (GetNextType() == LexemeType.AssignmentOperator)
            {
                log("Syntax Error: Ожидался оператор присваивания.");
                Assignment(true);
            }
            else
            {
                log("Syntax Error: Пропущен оператор присваивания.");
                Expr(false);
            }    
        }
    }
    private void Expr(bool get, bool isFirstOrSecond = true)
    {
        if (get) ChangeCurrentToken();

        log("<Exp>", sep1);
        Token(false);
        if (CurrToken.Type == LexemeType.Plus)
        {
            log("+", sep1);
            if (CanGetNext() && (GetNextType() == LexemeType.CloseBracket || GetNextType() == LexemeType.Semicolon))
            {
                log("Syntax Error: Ожидался идентификатор или число.");
            }
            Expr(true);
        }
    }
    private void Token(bool get)
    {
        if (get) ChangeCurrentToken();

        log("<Token>", sep1);

        Operand(false);
        ChangeCurrentToken();
        if (CurrToken.Type == LexemeType.Multiply)
        {
            log("*", sep1);
            if (CanGetNext() && (GetNextType() == LexemeType.CloseBracket || GetNextType() == LexemeType.Semicolon))
            {
                log("Syntax Error: Ожидался идентификатор или число.");
            }
            Token(true);
        }
    }
    private void Operand(bool get)
    {
        if (get) ChangeCurrentToken();

        log("<Operand>", sep1);

        switch (CurrToken.Type)
        {
            case LexemeType.Minus:
                log("-", sep1);
                Operand(true);
                break;
            case LexemeType.Identifier:
                log("id", sep1);
                if (CanGetNext() && GetNextType() == LexemeType.Minus)
                    Operand(true);
                break;
            case LexemeType.Integer:
                log("digit", sep1);
                if (CanGetNext() && GetNextType() == LexemeType.Minus)
                    Operand(true);
                break;
            case LexemeType.Float:
                log("digit", sep1);
                if (CanGetNext() && GetNextType() == LexemeType.Minus)
                    Operand(true);
                break;
            case LexemeType.OpenBracket:
                log("(", sep1);
                if (CanGetNext() && GetNextType() != LexemeType.CloseBracket)
                    Expr(true);
                else
                    log("Syntax Error: Ожидалось выражение в скобках.");
                if (CurrToken.Type != LexemeType.CloseBracket)
                    log("Syntax Error: Несоответствие скобок.");
                else
                {
                    log(")", sep1);
                    if (CanGetNext() && GetNextType() == LexemeType.Minus)
                        Operand(true);
                }
                break;
            default:
                log("Syntax Error: Ожидался идентификатор или число.");
                break;

        }
    }
}
