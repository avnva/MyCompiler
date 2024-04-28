using Microsoft.VisualBasic;
using MyCompiler.States;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace MyCompiler;


public class Parser
{
    private List<Lexeme> tokens;
    private Lexeme CurrToken;
    private int CurrIndex;
    private int MaxIndex;

    public List<ParserError> Errors { get; set; }

    public Parser()
    {
        Errors = new List<ParserError>();
    }

    public List<ParserError> Parse(List<Lexeme> tokensList)
    {
        Errors.Clear();
        if (tokensList.Count <= 0)
            return Errors;
        GetAndRemoveInvalidLexemes(tokensList);
        tokens = tokensList;
        CurrIndex = 0;
        MaxIndex = tokensList.Count - 1;
        CurrToken = tokens[CurrIndex];

        try
        {
            LF(false);
        }
        catch (SyntaxErrorException)
        {
            if (CurrToken.Type != LexemeType.Semicolon)
                Errors.Add(new ParserError($"Выражение незакончено", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
        }

        return Errors;
    }
    public void GetAndRemoveInvalidLexemes(List<Lexeme> lexemes)
    {
        for (int i = lexemes.Count - 1; i >= 0; i--)
        {
            var lexeme = lexemes[i];
            if (lexeme.Type == LexemeType.Whitespace || lexeme.Type == LexemeType.NewLine || lexeme.Type == LexemeType.InvalidCharacter)
            {
                Errors.Add(new ParserError($"Недопустимый символ \"{lexeme.Value}\"", lexeme.StartIndex, lexeme.EndIndex, ErrorType.UnfinishedExpression));
                lexemes.RemoveAt(i);
            }
        }
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
    private LexemeType GetNextType()
    {
        return CanGetNext() ? tokens[CurrIndex + 1].Type : LexemeType.InvalidCharacter;
    }

    private bool CanGetNext() => CurrIndex < MaxIndex;

    private void LF(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();

        if (CurrToken.Type == LexemeType.Identifier)
        {
            Assignment(true);
        }
        else
        {
            if (CurrToken.Type == LexemeType.AssignmentOperator && GetNextType() == LexemeType.Lambda)
            {
                Errors.Add(new ParserError($"Пропущен идентификатор", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
                Assignment(false);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидался идентификатор, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));

                if (GetNextType() == LexemeType.Identifier)
                    LF(true);
                else
                    Assignment(true);
            }
        }
    }

    private void Assignment(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();

        if (CurrToken.Type == LexemeType.AssignmentOperator)
        {
            Lambda(true);
        }
        else
        {
            if (CurrToken.Type == LexemeType.AssignmentOperator)
            {
                Errors.Add(new ParserError($"Пропущен оператор присваивания", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
                Lambda(false);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидался оператор присваивания, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));

                if (GetNextType() == LexemeType.Identifier)
                    Assignment(true);
                else
                    Lambda(true);
            }
        }
    }
    private void Lambda(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();

        if (CurrToken.Type == LexemeType.Lambda)
        {
            Arg(true);
        }
        else
        {
            if ((CurrToken.Type == LexemeType.Identifier && GetNextType() == LexemeType.Comma) || (CurrToken.Type == LexemeType.Identifier && GetNextType() == LexemeType.Colon))
            {
                Errors.Add(new ParserError($"Пропущено ключевое выражение", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
                Arg(false);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидалось ключевое выражение, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));

                if (GetNextType() == LexemeType.Lambda)
                    Lambda(true);
                else
                    Arg(true);
            }
        }
    }

    private void Arg(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();

        if (CurrToken.Type == LexemeType.Identifier)
        {
            if(GetNextType() == LexemeType.Comma)
            {
                ChangeCurrentToken();
                Arg(true);
            }
            else if (GetNextType() == LexemeType.Colon)
            {
                StartFunction(true);
            }
        }
        else
        {
            if (CurrToken.Type == LexemeType.Comma && GetNextType() == LexemeType.Identifier)
            {
                Errors.Add(new ParserError($"Пропущен идентификатор", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
                Arg(true);
            }
            else if (CurrToken.Type == LexemeType.Colon)
            {
                Errors.Add(new ParserError($"Пропущен идентификатор", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
                StartFunction(false);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидался идентификатор, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));

                if (GetNextType() == LexemeType.Identifier)
                    Arg(true);
                else
                    StartFunction(true);
            }
        }
    }

    private void StartFunction(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();

        if (CurrToken.Type == LexemeType.Colon)
        {
            Expression(true, true);
        }
        else
        {
            if (CurrToken.Type == LexemeType.OpenBracket ||
                CurrToken.Type == LexemeType.Identifier ||
                CurrToken.Type == LexemeType.Integer ||
                CurrToken.Type == LexemeType.Float ||
                CurrToken.Type == LexemeType.Minus)
            {
                Errors.Add(new ParserError($"Пропущено двоеточие", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
                Expression(false, true);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидалось двоеточие, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));

                if (GetNextType() == LexemeType.Colon)
                    StartFunction(true);
                else
                    Expression(true, true);
            }
        }
    }

    private void Expression(bool get, bool neutralize)
    {
        if (get) ChangeCurrentToken();

        Additive(false);
        if (neutralize)
            End(false);
        // Проверяем, если выражение не закончено, генерируем ошибку
        //if (CurrToken.Type != LexemeType.Semicolon && CanGetNext())
        //{
        //    Errors.Add(new ParserError($"Выражение незакончено", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
        //}
    }

    
    private void Additive(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();

        // Разбор первичного выражения
        Multiplicative(false);

        // Проверка на наличие дополнительных операций сложения и вычитания
        while (CurrToken.Type == LexemeType.Plus || CurrToken.Type == LexemeType.Minus)
        {
            Multiplicative(true);
        }
    }

    private void Multiplicative(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();

        // Разбор первичного выражения
        Primary(false);

        // Проверка на наличие дополнительных операций умножения и деления
        while (CurrToken.Type == LexemeType.Multiply || CurrToken.Type == LexemeType.Divide)
        {
            Primary(true);
        }
    }

    private void Primary(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();

        if (CurrToken.Type == LexemeType.OpenBracket)
        {
            // Если текущий токен - открывающая скобка, вызываем Expression для разбора выражения в скобках
            Expression(true, false);
            if (CurrToken.Type != LexemeType.CloseBracket)
            {
                Errors.Add(new ParserError($"Ожидалась закрывающая скобка", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
            }
            ChangeCurrentToken();
        }
        
        else if (CurrToken.Type == LexemeType.Integer || CurrToken.Type == LexemeType.Float || CurrToken.Type == LexemeType.Identifier)
        {
            // Если текущий токен - число или идентификатор, просто переходим к следующему токену
            if (CanGetNext() && GetNextType() != LexemeType.Plus && GetNextType() != LexemeType.Minus &&
                                GetNextType() != LexemeType.Multiply && GetNextType() != LexemeType.Divide &&
                                GetNextType() != LexemeType.Semicolon && GetNextType() != LexemeType.CloseBracket)
            {
                
                ChangeCurrentToken();
                Errors.Add(new ParserError($"Ожидался арифметический оператор", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
            }
            ChangeCurrentToken();
        }
        else if (CurrToken.Type == LexemeType.Minus)
        {
            Primary(true); // Разбираем выражение после унарного минуса
        }
        else
        {
            Errors.Add(new ParserError($"Ожидалось число, идентификатор или открывающая скобка", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
        }
    }

    private void End(bool get, bool neutralize = false)
    {
        if (get) ChangeCurrentToken();

        if (CurrToken.Type == LexemeType.Semicolon)
        {
            LF(true);
        }
        else
        {
            if (CurrToken.Type == LexemeType.Identifier || GetNextType() == LexemeType.InvalidCharacter)
            {
                Errors.Add(new ParserError($"Пропущен оператор конца выражения", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
                if (GetNextType() != LexemeType.InvalidCharacter)
                    LF(false);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидался оператор конца выражения, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));

                if (GetNextType() == LexemeType.Semicolon)
                    End(true);
                else
                    LF(true);
            }
        }
    }


}


//public enum StatesType
//{
//    Identifier = 1,
//    AssignmentOperator = 2,
//    Lambda = 3,
//    Сolon = 4,
//    FirstArgument = 5,
//    Comma = 6,
//    SecondArgument = 8,
//    FirstOperator = 9,
//    ArithmeticOperator =10,
//    SecondOperator = 11,
//    Semicolon = 12
//}

//public Dictionary<StatesType, IState> StateMap;
//public StringHelper StringHelper { get; set; }
//public List<ParserError> Errors { get; set; }

//public List<ParserError> Parse(string text = "")
//{
//    Errors.Clear();
//    StringHelper.Source = text;

//    StateMap[StatesType.Identifier].Handle();
//    return Errors;
//}

//public Parser(string text)
//{
//    Errors = new List<ParserError>();
//    StringHelper = new StringHelper(text);
//    StateMap = new Dictionary<StatesType, IState>();

//    StateMap.Add(StatesType.Identifier, new IdState(Errors, StringHelper, StateMap));
//    StateMap.Add(StatesType.AssignmentOperator, new AssignmentOperatorState(Errors, StringHelper, StateMap));
//    StateMap.Add(StatesType.Lambda, new LambdaState(Errors, StringHelper, StateMap));
//    StateMap.Add(StatesType.FirstArgument, new FirstArgState(Errors, StringHelper, StateMap));
//    StateMap.Add(StatesType.Comma, new CommaState(Errors, StringHelper, StateMap));
//    StateMap.Add(StatesType.SecondArgument, new SecondArgState(Errors, StringHelper, StateMap));
//    StateMap.Add(StatesType.Сolon, new WhitespaceState(Errors, StringHelper, StateMap));
//    StateMap.Add(StatesType.FirstOperator, new FirstOperatorNameState(Errors, StringHelper, StateMap));
//    StateMap.Add(StatesType.ArithmeticOperator, new ArithmeticOperatorState(Errors, StringHelper, StateMap));
//    StateMap.Add(StatesType.SecondOperator, new SecondOperatorNameState(Errors, StringHelper, StateMap));
//    StateMap.Add(StatesType.Semicolon, new SemicolonState(Errors, StringHelper, StateMap));

//}