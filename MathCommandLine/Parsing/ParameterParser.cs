using IML.CoreDataTypes;
using IML.Exceptions;
using IML.Parsing.AST;
using IML.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static IML.Parsing.Parser;

namespace IML.Parsing
{
    public class ParameterParser
    {
        #region Parsing Parameters
        /// <summary>
        /// Parses a single parameter into an AstParameter object
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public AstParameter ParseParameter(string parameter)
        {
            parameter = parameter.Trim();
            // Check if no type provided. If parameter simply matches valid variable name, then it is of the any type
            if (SYMBOL_NAME_REGEX.IsMatch(parameter))
            {
                // Name is the parameter string, and type is of the any type
                return new AstParameter(parameter, AstType.Any);
            }
            // Need to get the colon to find the name and type
            int colonIndex = parameter.IndexOf(':');
            string paramName = parameter.Substring(0, colonIndex);
            string typeString = parameter.Substring(colonIndex + 1);
            AstType type = ParseType(typeString.Trim());
            return new AstParameter(paramName.Trim(), type);
        }
        public AstType ParseType(string typeStr)
        {
            try
            {
                if (IsParenWrapped(typeStr))
                {
                    return ParseType(typeStr.SubstringBetween(1, typeStr.Length - 1));
                }
                // typeStr can have unions and restrictions to process
                // Split the type on pipe (excluding things in brackets [] to avoid types provided to restrictions)
                string[] types = SplitByDelimiter(typeStr, TYPE_UNION_DELIMITER,
                    new string[] { TYPE_GENERICS_WRAPPERS, LAMBDA_TYPE_PARAM_WRAPPERS });
                if (types.Length <= 0)
                {
                    throw new InvalidParseException("Type has no entries", typeStr);
                }
                // Now for each of these, we need to parse out the datatype + restrictions
                AstType type = AstType.UNION_BASE;
                for (int i = 0; i < types.Length; i++)
                {
                    types[i] = types[i].Trim();
                    AstTypeEntry entry = ParseTypeEntry(types[i]);
                    type = type.Union(new AstType(entry));
                }
                return type;
            }
            catch
            {
                throw new InvalidParseException(typeStr);
            }
        }
        private AstTypeEntry ParseTypeEntry(string entryStr)
        {
            entryStr = entryStr.Trim();
            if (IsParenWrapped(entryStr))
            {
                return ParseTypeEntry(entryStr.SubstringBetween(1, entryStr.Length - 1));
            }
            // Check if this matches the lambda regex; if so, we need to parse it as a lambda type
            if (LAMBDA_TYPE_REGEX.IsMatch(entryStr))
            {
                return ParseLambdaTypeEntry(entryStr);
            }
            // See if we even have restrictions
            if (!entryStr.Contains(TYPE_GENERICS_WRAPPERS[0]))
            {
                // Simply stores a type. We should pass that up.
                if (!SYMBOL_NAME_REGEX.IsMatch(entryStr))
                {
                    throw new InvalidParseException("Invalid type name", entryStr);
                }
                return AstTypeEntry.Simple(entryStr);
            }
            // Need to get everything between the first and last brackets
            int bracketStart = entryStr.IndexOf(TYPE_GENERICS_WRAPPERS[0]);
            int bracketEnd = entryStr.LastIndexOf(TYPE_GENERICS_WRAPPERS[1]);
            string name = entryStr.Substring(0, bracketStart).Trim();
            string genericsString = entryStr.SubstringBetween(bracketStart + 1, bracketEnd).Trim();
            List<AstType> generics = new List<AstType>();
            if (genericsString.Length > 0)
            {
                // Now need to split this up
                string[] genericStrings = SplitByDelimiter(genericsString, TYPE_GENERICS_DELIMITER,
                    new string[] { TYPE_GENERICS_WRAPPERS });

                for (int i = 0; i < genericStrings.Length; i++)
                {
                    generics.Add(ParseType(genericStrings[i].Trim()));
                }
            }
            return new AstTypeEntry(name, generics);
        }
        private LambdaAstTypeEntry ParseLambdaTypeEntry(string str)
        {
            // Check if the type starts with generic wrappers
            // For example, we could have [T](x:T)=>T or something like (x:any)=>any
            List<string> genericNames = new List<string>();
            int startParen = 0;
            if (str.StartsWith(TYPE_GENERICS_WRAPPERS[0]))
            {
                // Parse our generics first
                int startBracket = 0;
                int endBracket = GetBracketEndIndex(str, 0, TYPE_GENERICS_WRAPPERS[0], TYPE_GENERICS_WRAPPERS[1]);
                string generics = str.SubstringBetween(startBracket + 1, endBracket);
                genericNames = ParseGenericNames(generics);
                // Now move the startParen to the proper place. We allow whitespace in between the brackets and paren
                startParen = endBracket;
                char c = str[startParen];
                string inBetweenStuff = "";
                while (c != LAMBDA_TYPE_PARAM_WRAPPERS[0])
                {
                    c = str[startParen];
                    inBetweenStuff += c;
                    startParen++;
                    if (startParen > str.Length)
                    {
                        throw new InvalidParseException("No argument wrapper present in function type declaration", str);
                    }
                }
                // Undo last iteration to keep it in the right spot
                startParen--;
                // Make sure everything in between is whitespace
                Regex whitespaceRegex = new Regex(@"\s*");
                if (!whitespaceRegex.IsMatch(inBetweenStuff))
                {
                    throw new InvalidParseException($"Unrecognized token(s): \"{inBetweenStuff}\"", str);
                }
            }

            // Get the param types, return type, and any requirements on environments/purity
            int endParen = GetBracketEndIndex(str, startParen, LAMBDA_TYPE_PARAM_WRAPPERS[0], LAMBDA_TYPE_PARAM_WRAPPERS[1]);
            string args = str.SubstringBetween(startParen + 1, endParen);
            string[] paramTypeStrings = SplitByDelimiter(args, LAMBDA_TYPE_PARAM_DELMITER,
                new string[] { LAMBDA_TYPE_PARAM_WRAPPERS, TYPE_GENERICS_WRAPPERS });
            List<AstType> parameterTypes = new List<AstType>();
            bool isVarArgs = false;
            for (int i = 0; i < paramTypeStrings.Length; i++)
            {
                paramTypeStrings[i] = paramTypeStrings[i].Trim();
                if (paramTypeStrings[i].EndsWith(LAMBDA_TYPE_VARARGS_SYMBOL))
                {
                    // Better be the last parameter
                    if (i + 1 < paramTypeStrings.Length)
                    {
                        // Trying to use varargs with the not-last argument; illegal
                        // Prevent this by throwing an exception
                        throw new InvalidParseException("Varargs can only be used with the last parameter", str);
                    }
                    // Ok, now parse the type and make sure it's a list of something
                    string varArgsTypeString = paramTypeStrings[i]
                        .SubstringBetween(0, paramTypeStrings[i].Length - LAMBDA_TYPE_VARARGS_SYMBOL.Length)
                        .Trim();
                    AstType type = ParseType(varArgsTypeString);
                    if (!type.Entries.All(e => e.DataTypeName == MDataType.LIST_TYPE_NAME))
                    {
                        // We have an entry for the type of the varargs that is not a list
                        throw new InvalidParseException("Varargs must be of type list or a union type of lists", str);
                    }
                    // Okay, if we're here then it was legal, so lets add the varargs
                    parameterTypes.Add(type);
                    isVarArgs = true;
                }
                else
                {
                    parameterTypes.Add(ParseType(paramTypeStrings[i]));
                }
            }
            // Ok, now handle the "arrow" and cover any special behaviors here
            // Start at end paren in case any lambdas in the params (can't use LastIndexOf either since there could
            // be one in the return type)
            int arrowTipIndex = str.IndexOf(LAMBDA_TYPE_ARROW_TIP, endParen);
            string arrow = str.SubstringBetween(endParen + 1, arrowTipIndex).Trim();
            bool forceEnv = false;
            bool createsEnv;
            if (arrow.StartsWith(LAMBDA_TYPE_REQ_ENV_CHARACTER))
            {
                forceEnv = true;
                arrow = arrow.Substring(1); // Pop off first character
            }
            if (arrow.StartsWith(LAMBDA_TYPE_CREATES_ENVIRONMENT_LINE))
            {
                createsEnv = true;
            }
            else if (arrow.StartsWith(LAMBDA_TYPE_NO_ENVIRONMENT_LINE))
            {
                createsEnv = false;
            }
            else
            {
                // Should never get here since we regexed, but just in case...
                throw new InvalidParseException($"'{str}' could not be parsed to a function type. {arrow[0]} is an invalid " +
                    "environment specifier.");
            }
            LambdaEnvironmentType envType;
            if (forceEnv)
            {
                if (createsEnv)
                {
                    envType = LambdaEnvironmentType.ForceEnvironment;
                }
                else
                {
                    envType = LambdaEnvironmentType.ForceNoEnvironment;
                }
            }
            else if (!forceEnv && !createsEnv)
            {
                // Still force no environment, since user specified (like "()~>void")
                envType = LambdaEnvironmentType.ForceNoEnvironment;
            }
            else
            {
                // No env forced at all
                envType = LambdaEnvironmentType.AllowAny;
            }

            // Finally, we need the return type
            string returnTypeString = str.Substring(arrowTipIndex + 1).Trim();
            AstType returnType = ParseType(returnTypeString);
            // Now we can construct this thing and return it
            return new LambdaAstTypeEntry(returnType, parameterTypes, envType, false, isVarArgs, genericNames);
        }
        #endregion

        #region Unparsing Parameters
        public string UnparseParameter(AstParameter parameter)
        {
            return parameter.Name + PARAM_NAME_TYPE_SEPARATOR + UnparseType(parameter.Type);
        }
        public string UnparseType(AstType type)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < type.Entries.Count; i++)
            {
                builder.Append(UnparseTypeEntry(type.Entries[i]));
                if (i + 1 < type.Entries.Count)
                {
                    builder.Append(TYPE_UNION_DELIMITER);
                }
            }
            return builder.ToString();
        }
        private string UnparseTypeEntry(AstTypeEntry entry)
        {
            if (entry is LambdaAstTypeEntry)
            {
                return UnparseLambdaTypeEntry(entry as LambdaAstTypeEntry);
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(entry.DataTypeName);
            builder.Append(TYPE_GENERICS_WRAPPERS[0]);
            for (int i = 0; i < entry.Generics.Count; i++)
            {
                builder.Append(UnparseType(entry.Generics[i]));
                if (i + 1 < entry.Generics.Count)
                {
                    builder.Append(TYPE_GENERICS_DELIMITER);
                }
            }
            builder.Append(TYPE_GENERICS_WRAPPERS[1]);
            return builder.ToString();
        }
        private string UnparseLambdaTypeEntry(LambdaAstTypeEntry entry)
        {
            StringBuilder builder = new StringBuilder();
            if (entry.GenericNames.Count > 0)
            {
                // Need to include generic names out in front
                builder.Append(TYPE_GENERICS_WRAPPERS[0]);
                for (int i = 0; i < entry.GenericNames.Count; i++)
                {
                    builder.Append(entry.GenericNames[i]);
                    if (i + 1 < entry.GenericNames.Count)
                    {
                        builder.Append(TYPE_GENERICS_DELIMITER);
                    }
                }
                builder.Append(TYPE_GENERICS_WRAPPERS[1]);
            }
            builder.Append(LAMBDA_TYPE_PARAM_WRAPPERS[0]);
            for (int i = 0; i < entry.ParamTypes.Count; i++)
            {
                builder.Append(UnparseType(entry.ParamTypes[i]));
                if (i + 1 < entry.ParamTypes.Count)
                {
                    builder.Append(LAMBDA_TYPE_PARAM_DELMITER);
                }
                else if (entry.IsLastVarArgs)
                {
                    builder.Append(LAMBDA_TYPE_VARARGS_SYMBOL);
                }
            }
            builder.Append(LAMBDA_TYPE_PARAM_WRAPPERS[1]);
            // Now build the arrow
            switch (entry.EnvironmentType)
            {
                case LambdaEnvironmentType.ForceEnvironment:
                    builder.Append(LAMBDA_TYPE_REQ_ENV_CHARACTER);
                    builder.Append(LAMBDA_TYPE_CREATES_ENVIRONMENT_LINE);
                    break;
                case LambdaEnvironmentType.ForceNoEnvironment:
                    builder.Append(LAMBDA_TYPE_REQ_ENV_CHARACTER);
                    builder.Append(LAMBDA_TYPE_NO_ENVIRONMENT_LINE);
                    break;
                case LambdaEnvironmentType.AllowAny:
                    builder.Append(LAMBDA_TYPE_CREATES_ENVIRONMENT_LINE);
                    break;
            }
            builder.Append(LAMBDA_TYPE_ARROW_TIP);
            // Finally, the return type
            builder.Append(UnparseType(entry.ReturnType));
            return builder.ToString();
        }
        #endregion
    }
}
