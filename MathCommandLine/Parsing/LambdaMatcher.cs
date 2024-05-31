using IML.Parsing.Util;
using IML.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IML.Parsing
{
    public class LambdaMatcher
    {
        public static LambdaMatch MatchLambda(string expression)
        {
            if (expression.Length < 2)
            {
                return LambdaMatch.Failure;
            }

            int i = SkipWhitespace(expression, 0);

            // Parse generics; check if they exist
            string generics = "";
            if (expression[i] == Parser.TYPE_GENERIC_START_WRAPPER)
            {
                int genericStart = i;
                i = Parser.GetBracketEndIndex(expression, i, Parser.TYPE_GENERIC_START_WRAPPER,
                    Parser.TYPE_GENERIC_END_WRAPPER);
                if (i < 0)
                {
                    return LambdaMatch.Failure;
                }
                generics = expression.SubstringBetween(genericStart + 1, i);
                i++;
            }
            i = SkipWhitespace(expression, i);

            // Now do params
            int parenStart = i;
            if (expression[i] != Parser.LAMBDA_TYPE_PARAM_WRAPPERS[0])
            {
                return LambdaMatch.Failure;
            }
            i = Parser.GetBracketEndIndex(expression, i, Parser.LAMBDA_TYPE_PARAM_WRAPPERS[0],
                Parser.LAMBDA_TYPE_PARAM_WRAPPERS[1]);
            if (i < 0)
            {
                return LambdaMatch.Failure;                
            }
            string paramsString = expression.SubstringBetween(parenStart + 1, i);
            i++;
            i = SkipWhitespace(expression, i);

            // Now do return type
            string returnType = "";
            if (expression[i] == Parser.RETURN_TYPE_DELIMITER)
            {
                // Go until we reach the arrow bit
                StringBuilder returnTypeBuilder = new StringBuilder();
                char[] returnTypeTerminators = new char[]
                {
                    Parser.LAMBDA_TYPE_NO_ENVIRONMENT_LINE,
                    Parser.LAMBDA_TYPE_CREATES_ENVIRONMENT_LINE
                };
                i++;
                while (!returnTypeTerminators.Contains(expression[i]))
                {
                    returnTypeBuilder.Append(expression[i]);
                    i++;
                    if (i >= expression.Length)
                    {
                        return LambdaMatch.Failure;
                    }
                }
                returnType = returnTypeBuilder.ToString();
            }
            i = SkipWhitespace(expression, i);

            if (expression[i] != Parser.LAMBDA_TYPE_NO_ENVIRONMENT_LINE &&
                expression[i] != Parser.LAMBDA_TYPE_CREATES_ENVIRONMENT_LINE)
            {
                return LambdaMatch.Failure;
            }

            string arrowBit = expression[i].ToString();
            i++; // Skip to the '>' in the arrow, but verify it
            if (expression[i] != Parser.LAMBDA_TYPE_ARROW_TIP)
            {
                return LambdaMatch.Failure;
            }
            i++;
            i = SkipWhitespace(expression, i);

            if (expression[i] != Parser.LAMBDA_BODY_WRAPPERS[0])
            {
                return LambdaMatch.Failure;
            }
            int bodyStart = i;
            i = Parser.GetBracketEndIndex(expression, i, Parser.LAMBDA_BODY_WRAPPERS[0],
                Parser.LAMBDA_BODY_WRAPPERS[1]);
            if (i < 0)
            {
                return LambdaMatch.Failure;
            }

            string body = expression.SubstringBetween(bodyStart + 1, i);
            i++;

            // Make sure there's nothing else
            i = SkipWhitespace(expression, i);
            if (i != expression.Length)
            {
                return LambdaMatch.Failure;
            }

            return new LambdaMatch(true, generics, paramsString, returnType, arrowBit, body);
        }

        private static int SkipWhitespace(string expression, int i)
        {
            while (i < expression.Length && Regex.Match(expression[i].ToString(), "\\s").Success)
            {
                i++;
            }
            return i;
        }
    }
}
