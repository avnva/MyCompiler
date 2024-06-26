﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler;

public static class TextCleaner
{
    public static string RemoveIncorrectLexemes(string inputString, ObservableCollection<ParserError> _incorrectLexemes)
    {
        if (_incorrectLexemes.Last().ErrorType == ErrorType.UnfinishedExpression)
        {
            _incorrectLexemes.Remove(_incorrectLexemes.Last());
        }

        foreach (var lexeme in _incorrectLexemes.Reverse())
        {
            int fragmentLength = lexeme.EndIndex - lexeme.StartIndex + 1;
            int fragmentStartIndex = lexeme.StartIndex - 1;

            if (fragmentStartIndex > 0 && inputString[fragmentStartIndex - 1] == ' ' &&
                (fragmentStartIndex + fragmentLength) < (inputString.Length - 1) &&
                inputString[fragmentStartIndex + fragmentLength] == ' ')
                fragmentLength++;

            inputString = inputString.Remove(fragmentStartIndex, fragmentLength);
        }

        inputString = inputString.Replace("  ", " ");

        return inputString;
    }
}