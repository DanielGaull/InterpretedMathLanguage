using IML.Parsing.Util;
using IML.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Parsing
{
    public class CallMatcher
    {
        public static CallMatch MatchCall(string expression)
        {
            if (expression.Length < 2)
            {
                return CallMatch.Failure;
            }

            // What makes a call a call?
            // Well, it's simply some "thing" followed by parentheses, which may have arguments in them
            // It's important to note that the "thing" must have balanced brackets: that is, balanced curly braces
            // and balanced parentheses. That way, '((x)' doesn't count as a call, because it appears to really be
            // the start of a lambda ex. ((x)=>{_add(x,7)})
            // So, lets start from the back. The last character should be a parenthesis. Then, we simply find its
            // matching balanced one. As long as the match is not the first character in the string, then
            // there is something before the pair, and we've found what should be interpreted as a call!
            if (expression[expression.Length - 1] != Parser.CALL_END_WRAPPER)
            {
                // Last character is not a closing parenthesis
                return CallMatch.Failure;
            }
            // Now time to backtrack. We'll go character by character. Depending on parentheses we find, we'll keep a count
            // Need to find the open wrapper when the counter is zero though
            // Start at second-to-last char b/c we know the last one is a ')'
            int wrapperCounter = 0;
            int startWrapperIndex = -1;
            for (int i = expression.Length - 2; i >= 0; i--)
            {
                if (expression[i] == Parser.CALL_END_WRAPPER)
                {
                    wrapperCounter++;
                }
                else if (expression[i] == Parser.CALL_START_WRAPPER)
                {
                    if (wrapperCounter == 0)
                    {
                        // We've found the corresponding location!
                        startWrapperIndex = i;
                        break;
                    }
                    else
                    {
                        wrapperCounter--;
                    }
                }
            }
            if (startWrapperIndex < 0 || startWrapperIndex == 0)
            {
                // Didn't find the matching start in the whole string, or the starting parenthesis was the first character,
                // meaning this is just an expression wrapped in parentheses
                return CallMatch.Failure;
            }

            string beforeParens = expression.Substring(0, startWrapperIndex);
            string genericString = "";
            string calledPart = "";
            // We're not done yet; need to obtain the generics string from this beforeParens part
            if (beforeParens.EndsWith(Parser.TYPE_GENERIC_END_WRAPPER))
            {
                // Need to extract
                int level = 0;
                int startIndex = -1;
                for (int i = beforeParens.Length - 1; i >= 0; i--)
                {
                    if (beforeParens[i] == Parser.TYPE_GENERIC_START_WRAPPER)
                    {
                        level--;
                        if (level == 0)
                        {
                            startIndex = i;
                            break;
                        }
                    }
                    else if (beforeParens[i] == Parser.TYPE_GENERIC_END_WRAPPER)
                    {
                        level++;
                    }
                }
                calledPart = beforeParens.Substring(0, startIndex);
                genericString = beforeParens.SubstringBetween(startIndex + 1, beforeParens.Length - 1);
            }
            else
            {
                calledPart = beforeParens;
            }

            string argsString = expression.Substring(startWrapperIndex + 1, expression.Length - (startWrapperIndex + 1) - 1);

            return new CallMatch(true, calledPart, genericString, argsString);
        }
        
    }
}
