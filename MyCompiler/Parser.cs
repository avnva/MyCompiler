using System.Data;

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
            if (CurrToken.Type == LexemeType.AssignmentOperator)
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
            if (CurrToken.Type == LexemeType.Lambda)
            {
                Errors.Add(new ParserError($"Пропущен оператор присваивания", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
                Lambda(false);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидался оператор присваивания, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));

                if (GetNextType() == LexemeType.AssignmentOperator)
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
            else if (GetNextType()== LexemeType.Identifier)
            {
                ChangeCurrentToken();
                Errors.Add(new ParserError($"Пропущена запятая между аргументами", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
                Arg(false);
            }
            else
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
            Expression(true, true, false);
        }
        else
        {
            if (CurrToken.Type == LexemeType.OpenBracket ||
                CurrToken.Type == LexemeType.Identifier ||
                CurrToken.Type == LexemeType.Integer ||
                CurrToken.Type == LexemeType.Float ||
                CurrToken.Type == LexemeType.Minus ||
                CurrToken.Type == LexemeType.Plus ||
                CurrToken.Type == LexemeType.Multiply ||
                CurrToken.Type == LexemeType.Divide ||
                CurrToken.Type == LexemeType.CloseBracket)
            {
                Errors.Add(new ParserError($"Пропущено двоеточие", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
                if (CurrToken.Type == LexemeType.OpenBracket)
                    Expression(false, false, false);
                else
                    Expression(false, true, false);
            }
            else
            {
                Errors.Add(new ParserError($"Ожидалось двоеточие, а встречено \"{CurrToken.Value}\"", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));

                if (GetNextType() == LexemeType.Colon)
                    StartFunction(true);
                else
                    Expression(true, true, false);
            }
        }
    }

    private void Expression(bool get, bool neutralize, bool bracket)
    {
        if (get) ChangeCurrentToken();

        Additive(false, bracket);
        if (neutralize)
            End(false);
    }

    
    private void Additive(bool get, bool bracket)
    {
        if (get) ChangeCurrentToken();

        Multiplicative(false, bracket);

        // Проверка на наличие дополнительных операций сложения и вычитания
        while (CurrToken.Type == LexemeType.Plus || CurrToken.Type == LexemeType.Minus)
        {
            if (CanGetNext() && (GetNextType() == LexemeType.CloseBracket || GetNextType() == LexemeType.Semicolon))
                Errors.Add(new ParserError($"Пропущено число или идентификатор!!", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
            Multiplicative(true, bracket);
   
        }
    }

    private void Multiplicative(bool get, bool bracket)
    {
        if (get) ChangeCurrentToken();

        Primary(false, bracket);

        // Проверка на наличие дополнительных операций умножения и деления
        while (CurrToken.Type == LexemeType.Multiply || CurrToken.Type == LexemeType.Divide)
        {
            if (CanGetNext() && (GetNextType() == LexemeType.CloseBracket || GetNextType() == LexemeType.Semicolon))
                Errors.Add(new ParserError($"Пропущено число или идентификатор!!", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
            Primary(true, bracket);
        }
    }

    private void Primary(bool get, bool bracket)
    {
        if (get) ChangeCurrentToken();

        if (CurrToken.Type == LexemeType.OpenBracket)
        {
            // Если текущий токен - открывающая скобка, вызываем Expression для разбора выражения в скобках
            Expression(true, false, true);
            if (CurrToken.Type != LexemeType.CloseBracket)
            {
                Errors.Add(new ParserError($"Ожидалась закрывающая скобка", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
            }
            ChangeCurrentToken();
            if (CurrToken.Type != LexemeType.Semicolon)
            {
                if (CurrToken.Type != LexemeType.Plus && CurrToken.Type != LexemeType.Minus &&
                            CurrToken.Type != LexemeType.Multiply && CurrToken.Type != LexemeType.Divide)
                {
                    Errors.Add(new ParserError($"Пропущен арифметический оператор", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
                    Expression(false, false, false);
                }
            }
        }
        
        else if (CurrToken.Type == LexemeType.Integer || CurrToken.Type == LexemeType.Float || CurrToken.Type == LexemeType.Identifier)
        {
            if (CanGetNext() && GetNextType() == LexemeType.CloseBracket)
            {
                if (!bracket)
                    Primary(true, bracket);
            }
            else if (CanGetNext() && GetNextType() != LexemeType.Plus && GetNextType() != LexemeType.Minus &&
                                GetNextType() != LexemeType.Multiply && GetNextType() != LexemeType.Divide && GetNextType() != LexemeType.Semicolon)
            {
                ChangeCurrentToken();
                Errors.Add(new ParserError($"Пропущен арифметический оператор", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
                if (GetNextType() != LexemeType.Semicolon)
                    Expression(false, false, false);
                //Primary(false, false);
            }

            if (CurrToken.Type != LexemeType.Semicolon)
                ChangeCurrentToken();
        }
        else if (CurrToken.Type == LexemeType.Minus)
        {
            Primary(true, bracket); // Разбираем выражение после унарного минуса
        }
        else if (CurrToken.Type == LexemeType.CloseBracket)
        {
            if (!bracket)
            {
                Errors.Add(new ParserError($"Несоответствие скобок", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
                ChangeCurrentToken();
                if (CurrToken.Type != LexemeType.Semicolon)
                    Primary(false, false);
            }
            else
            {
                int prevInd = CurrIndex-1;
                if (tokens[prevInd].Type == LexemeType.OpenBracket)
                    Errors.Add(new ParserError($"Ожидалось выражение в скобках", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
            }
        }
        else if (CurrToken.Type == LexemeType.Semicolon)
        {

        }
        else
        {
            Errors.Add(new ParserError($"Пропущено число или идентификатор", CurrToken.StartIndex, tokens[MaxIndex].EndIndex, ErrorType.UnfinishedExpression));
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
            if (CurrToken.Type == LexemeType.Identifier  || GetNextType() == LexemeType.InvalidCharacter)
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