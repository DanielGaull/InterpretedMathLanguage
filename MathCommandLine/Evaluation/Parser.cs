using IML.CoreDataTypes;
using IML.Environments;
using IML.Evaluation.AST.ValueAsts;
using IML.Exceptions;
using IML.Functions;
using IML.Structure;
using IML.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IML.Evaluation
{
    /// <summary>
    /// Parses strings to ASTs. Capable of parsing many different types of strings, such as expressions and declarations.
    /// </summary>
    public class Parser
    {
        // Regexes for common necessities
        private const string NUMBER_REGEX_PATTERN = @"[+-]?(([0-9]+(\.[0-9]*)?)|(\.[0-9]+))";
        private const string SYMBOL_PATTERN = @"[a-zA-Z_][a-zA-Z0-9_]*";
        private static readonly char MEMBER_ACCESS_TOKEN = '.';

        // Regexes for matching language symbols
        private static readonly Regex NUMBER_REGEX = new Regex("^" + NUMBER_REGEX_PATTERN + "$");
        private static readonly Regex REFERENCE_REGEX = new Regex(@"^\&(" + SYMBOL_PATTERN + ")$");
        private static readonly Regex LIST_REGEX = new Regex(@"^\{(.*)\}$");
        private static readonly Regex SIMPLE_LAMBDA_REGEX = new Regex(@"^\[(.*)\]$");
        // Group for params. Non capturing to make the ':' optional, capturing for the type
        // Capturing for the arrow type and the return type
        private static readonly Regex LAMBDA_REGEX = new Regex(@"^(?:\[(.*)\])?\s*\((.*)\)(?:\s*\:(.+))?\s*([=~])>\s*\{(.*)\}$");
        private static readonly Regex STRING_REGEX = new Regex("^\"([^\"]*)\"$");
        private static readonly Regex MEMBER_ACCESS_REGEX = new Regex(@$"^(.*)\{MEMBER_ACCESS_TOKEN}(" + SYMBOL_PATTERN + ")$");

        // Call parsing values
        private const char CALL_END_WRAPPER = ')';
        private const char CALL_START_WRAPPER = '('; 
        private const char ARG_DELIMITER = ',';

        private readonly string LAMBDA_BODY_WRAPPERS = "{}";
        private const string VAR_ARGS_SYMBOL = "...";

        // Param parsing values
        private const char GENERIC_END_WRAPPER = ')';
        private const char GENERIC_START_WRAPPER = '(';
        private readonly string GENERIC_WRAPPERS = $"{GENERIC_START_WRAPPER}{GENERIC_END_WRAPPER}";

        // Simple lambda parsing values
        private const char SIMPLE_LAMBDA_START_WRAPPER = '[';
        private const char SIMPLE_LAMBDA_END_WRAPPER = ']';
        private readonly string SIMPLE_LAMBDA_WRAPPERS = $"{SIMPLE_LAMBDA_START_WRAPPER}{SIMPLE_LAMBDA_END_WRAPPER}";

        // List parsing values
        private const char LIST_START_WRAPPER = '{';
        private const char LIST_END_WRAPPER = '}';
        private const char LIST_DELIMITER = ',';
        private readonly string LIST_WRAPPERS = $"{LIST_START_WRAPPER}{LIST_END_WRAPPER}";

        // String parsing values
        private const char STRING_START_WRAPPER = '"';
        private const char STRING_END_WRAPPER = '"';
        private const char STRING_ESCAPE_STARTER = '\\';

        // Parameter parsing regexes
        private const char PARAM_DELIMITER = ',';
        private const string PARAM_NAME_TYPE_SEPARATOR = ":";
        private const char TYPE_UNION_DELIMITER = '|';
        private const char TYPE_GENERICS_DELIMITER = ',';
        private const string TYPE_GENERICS_WRAPPERS = "[]";

        private const char CODE_LINE_DELIMITER = ';';

        private const char LAMBDA_TYPE_NO_ENVIRONMENT_LINE = '~';
        private const char LAMBDA_TYPE_CREATES_ENVIRONMENT_LINE = '=';
        private const char LAMBDA_TYPE_ARROW_TIP = '>';
        private const char LAMBDA_TYPE_REQ_ENV_CHARACTER = '!'; // Character for requiring lambda to create env
        private const string LAMBDA_TYPE_PARAM_WRAPPERS = "()";
        private const char LAMBDA_TYPE_PARAM_DELMITER = ',';
        private const string LAMBDA_TYPE_VARARGS_SYMBOL = "...";
        private readonly Regex LAMBDA_TYPE_REGEX = 
            new Regex($@"({TYPE_GENERICS_WRAPPERS[0]}(.*){TYPE_GENERICS_WRAPPERS[1]})?\s*" +
                $@"{LAMBDA_TYPE_PARAM_WRAPPERS[0]}(.*){LAMBDA_TYPE_PARAM_WRAPPERS[1]}\s*" + 
                $@"[{LAMBDA_TYPE_REQ_ENV_CHARACTER}]?[{LAMBDA_TYPE_NO_ENVIRONMENT_LINE}|{LAMBDA_TYPE_CREATES_ENVIRONMENT_LINE}]" +
                $@"{LAMBDA_TYPE_ARROW_TIP}\s*(.*)");

        // Variable declaration and assigment syntax
        private const char ASSIGNMENT_TOKEN = '=';
        private const string DECLARATION_VAR_KEYWORD = "var";
        private const string DECLARATION_CONST_KEYWORD = "const";
        private static readonly Regex DECLARATION_REGEX = 
            new Regex($@"^({DECLARATION_VAR_KEYWORD}|{DECLARATION_CONST_KEYWORD})\s+({SYMBOL_PATTERN})" + 
                $@"(?:\s*\:(.*))?\s*{ASSIGNMENT_TOKEN}\s*(.*)$");

        private const string RETURN_KEYWORD = "return";
        private static readonly Regex RETURN_REGEX =
            new Regex(@$"^{RETURN_KEYWORD}\s+(.*)$");

        private const string DEREFERENCE_TOKEN = "*";

        // Other useful regexes
        private static readonly Regex SYMBOL_NAME_REGEX = new Regex(@$"^{SYMBOL_PATTERN}$");
        private static readonly List<string> RESERVED_KEYWORDS = new List<string>()
        {
            DECLARATION_VAR_KEYWORD,
            DECLARATION_CONST_KEYWORD,
            RETURN_KEYWORD,
        };

        private TypeDeterminer typeDeterminer;
        private VariableAstTypeMap defaultVariables;

        public Parser(VariableAstTypeMap defaultVariables)
        {
            typeDeterminer = new TypeDeterminer();
            this.defaultVariables = defaultVariables;
        }

        public virtual Ast Parse(string expression)
        {
            return Parse(expression, defaultVariables);
        }

        /// <summary>
        /// Parses a finalized expression into an AST
        /// Recall: Finalized expressions consist only of functions, literals, and variables
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Ast Parse(string expression, VariableAstTypeMap typeMap)
        {
            try
            {
                // TODO: Add support for big_decimal and big_int (require D or L at the end of the number literal, i.e. 750L or 0.642D)

                // Initially, attempt to extract an expression from parentheses
                while (IsParenWrapped(expression))
                {
                    // Pull out the expression without the first and last characters
                    expression = expression.Substring(1, expression.Length - 2);
                    return Parse(expression, typeMap);
                }

                // 'expression' is either a call, variable, or literal
                // May be something that is wrapped entirely in parenthesis
                CallMatch attempedCallMatch = MatchCall(expression);
                int attemptedAssignmentMatchIndex = TryMatchAssignment(expression);

                if (attempedCallMatch.IsMatch)
                {
                    // Performing some sort of call
                    string callerString = attempedCallMatch.Caller;
                    string argsString = attempedCallMatch.Args;

                    Ast caller = Parse(callerString, typeMap);
                    if (caller.Type == AstTypes.Invalid)
                    {
                        // Invalid caller means we need to return an invalid overall, instead of a valid call
                        return Ast.Invalid(expression);
                    }

                    string[] argStrings = argsString.Length > 0 ? SplitByDelimiter(argsString, ARG_DELIMITER) : new string[0];
                    // Parse all the arguments
                    Ast[] args = argStrings.Select(x => Parse(x, typeMap)).ToArray();

                    return Ast.Call(caller, args);
                }
                else if (NUMBER_REGEX.IsMatch(expression))
                {
                    // Parse the double and throw it into the AST
                    double number = double.Parse(expression);
                    return Ast.NumberLiteral(number);
                }
                else if (STRING_REGEX.IsMatch(expression))
                {
                    string str = STRING_REGEX.Match(expression).Groups[1].Value;
                    return Ast.StringLiteral(str);
                }
                else if (REFERENCE_REGEX.IsMatch(expression))
                {
                    string varName = REFERENCE_REGEX.Match(expression).Groups[1].Value;
                    return Ast.ReferenceLiteral(varName);
                }
                else if (MatchesList(expression)) //LIST_REGEX.IsMatch(expression))
                {
                    // Extract the elements of the list
                    string elements = LIST_REGEX.Match(expression).Groups[1].Value;
                    // Separate by the list delimiter
                    string[] elementStrings = elements.Length > 0 ? SplitByDelimiter(elements, LIST_DELIMITER) : new string[0];
                    List<Ast> elementAsts = new List<Ast>();
                    foreach (string str in elementStrings)
                    {
                        elementAsts.Add(Parse(str.Trim(), typeMap));
                    }
                    return Ast.ListLiteral(elementAsts.ToArray());
                }
                else if (LAMBDA_REGEX.IsMatch(expression))
                {
                    // 3-4 parts: parameters, return type (optional), type of lambda (environment/no environment), expression/body
                    var groups = LAMBDA_REGEX.Match(expression).Groups;
                    string genericsString = groups[1].Value;
                    string paramsString = groups[2].Value;
                    string returnTypeString = groups[3].Value;
                    string arrowBit = groups[4].Value;
                    string exprString = groups[5].Value;

                    bool provideReturnType = returnTypeString.Length > 0;

                    // Parse Parameters
                    List<AstParameter> parsedParamsList = new List<AstParameter>();
                    AstParameter[] parsedParams;
                    bool hasVarArgs = false;
                    if (paramsString.Length > 0)
                    {
                        string[] splitParamsStrings = SplitByDelimiter(paramsString, PARAM_DELIMITER);
                        for (int i = 0; i < splitParamsStrings.Length; i++)
                        {
                            if (i + 1 == splitParamsStrings.Length)
                            {
                                // This is the last parameter; check for varargs
                                if (splitParamsStrings[i].EndsWith(VAR_ARGS_SYMBOL))
                                {
                                    hasVarArgs = true;
                                    splitParamsStrings[i] = splitParamsStrings[i].SubstringBetween(0,
                                        splitParamsStrings[i].Length - VAR_ARGS_SYMBOL.Length);
                                }
                            }
                            parsedParamsList.Add(ParseParameter(splitParamsStrings[i]));
                        }
                        parsedParams = parsedParamsList.ToArray();
                    }
                    else
                    {
                        parsedParams = new AstParameter[0];
                    }

                    bool createsEnv = arrowBit == "=";

                    VariableAstTypeMap mapToUseForBody = typeMap;
                    if (createsEnv)
                    {
                        mapToUseForBody = typeMap.Clone();
                        // Need to add all the params to it
                        for (int i = 0; i < parsedParams.Length; i++)
                        {
                            mapToUseForBody.Add(parsedParams[i].Name, parsedParams[i].Type);
                        }
                    }

                    // In the event that we don't create an env, the typeMap will be updated by parsing the body
                    // We clone the map to use for body return type since we will always have our own type map updated by parsing
                    List<Ast> body = ParseBody(exprString, mapToUseForBody);
                    AstType bodyReturnType = typeDeterminer.DetermineDataType(body, mapToUseForBody.Clone());
                    AstType returnType;
                    if (provideReturnType)
                    {
                        returnType = ParseType(returnTypeString);
                        if (bodyReturnType != returnType)
                        {
                            throw new InvalidParseException("Function return type does not match returned value(s)", expression);
                        }
                    }
                    else
                    {
                        returnType = bodyReturnType;
                    }

                    List<string> generics = ParseGenericNames(genericsString);

                    return Ast.LambdaLiteral(parsedParams, body, returnType, createsEnv, false, hasVarArgs, generics);
                }
                else if (MatchesSimpleLambda(expression))
                {
                    // Simple lambda; no params, does not create body
                    string contents = SIMPLE_LAMBDA_REGEX.Match(expression).Groups[1].Value;
                    Ast body = Parse(contents, typeMap); // Don't clone the type map since we don't create env
                    return Ast.LambdaLiteral(new AstParameter[0], new List<Ast>() { body }, AstType.Any, false, false, false,
                        new List<string>());
                }
                else if (MEMBER_ACCESS_REGEX.IsMatch(expression))
                {
                    var groups = MEMBER_ACCESS_REGEX.Match(expression).Groups;
                    string parentStr = groups[1].Value;
                    string name = groups[2].Value;
                    Ast parent = Parse(parentStr, typeMap);
                    return Ast.MemberAccess(parent, name);
                }
                else if (RETURN_REGEX.IsMatch(expression))
                {
                    var groups = RETURN_REGEX.Match(expression).Groups;
                    string body = groups[1].Value;
                    Ast bodyAst = Parse(body, typeMap);
                    return Ast.Return(bodyAst);
                }
                else if (DECLARATION_REGEX.IsMatch(expression))
                {
                    var groups = DECLARATION_REGEX.Match(expression).Groups;
                    string varName = groups[2].Value;
                    // Make sure the variable name isn't reserved
                    if (RESERVED_KEYWORDS.Contains(varName))
                    {
                        // Attempting to use a reserved keyword as a variable
                        throw new InvalidParseException(expression);
                    }
                    string declarationType = groups[1].Value;
                    VariableType varType = 0;
                    if (declarationType == DECLARATION_VAR_KEYWORD)
                    {
                        varType = VariableType.Variable;
                    }
                    else if (declarationType == DECLARATION_CONST_KEYWORD)
                    {
                        varType = VariableType.Constant;
                    }
                    else
                    {
                        // Should never get here, we'd have not matched the regex
                        throw new InvalidParseException(expression);
                    }
                    string assignedExpr = groups[4].Value;
                    Ast assigned = Parse(assignedExpr, typeMap);

                    // See if we've been given a type; if so, verify that it matches; if not, infer the type
                    string variableType = groups[3].Value;
                    AstType varValType;
                    AstType bodyType = typeDeterminer.DetermineDataType(assigned, typeMap);
                    if (variableType.Length > 0)
                    {
                        varValType = ParseType(variableType);
                        if (varValType != bodyType)
                        {
                            throw new InvalidParseException($"Variable {varName} is not assigned the type it is declared.", 
                                expression);
                        }
                    }
                    else
                    {
                        varValType = bodyType;
                    }

                    return Ast.VariableDeclaration(varName, assigned, varType, varValType);
                }
                else if (attemptedAssignmentMatchIndex >= 0)
                {
                    string identifierExpr = expression.Substring(0, attemptedAssignmentMatchIndex).Trim();
                    string assignedExpr = expression.Substring(attemptedAssignmentMatchIndex + 1,
                        expression.Length - attemptedAssignmentMatchIndex - 1).Trim();
                    // Parse the assigned expression
                    Ast assigned = Parse(assignedExpr, typeMap);
                    // Parse the identifier
                    IdentifierAst identifier = ParseIdentifier(identifierExpr, typeMap);
                    // TODO: Add operator-assignments, ex. +=, &&=, etc.
                    // Should be valid for ANY binary operator; even doing something like !== or === should work,
                    // though they'd be pretty rare to want to do
                    return Ast.VariableAssignment(identifier, assigned);
                }
                else if (SYMBOL_NAME_REGEX.IsMatch(expression))
                {
                    // We've got a variable, or an attempt at one
                    if (RESERVED_KEYWORDS.Contains(expression))
                    {
                        // Attempting to use a reserved keyword as a variable
                        throw new InvalidParseException(expression);
                    }
                    // Valid variable
                    return Ast.Variable(expression);
                }
                else
                {
                    // We don't recognize this, so call it "invalid" and chuck it in here
                    // Someone else will either handle it or throw an error
                    return Ast.Invalid(expression);
                }
            }
            catch (InvalidParseException)
            {
                return Ast.Invalid(expression);
            }
        }

        private List<Ast> ParseBody(string bodyExpr, VariableAstTypeMap typeMap)
        {
            string[] lines = SplitByDelimiter(bodyExpr, CODE_LINE_DELIMITER,
                LAMBDA_BODY_WRAPPERS, LIST_WRAPPERS, SIMPLE_LAMBDA_WRAPPERS, GENERIC_WRAPPERS);

            if (lines.Length == 1)
            {
                string line = lines[0].Trim();
                if (line.Length > 0)
                {
                    // Single line, no semicolon after
                    // This is valid syntax
                    // Special case so the code below doesn't give invalid for missing a semicolon
                    return new List<Ast>() 
                    { 
                        Parse(line, typeMap)
                    };
                }
                else
                {
                    // Lambda has an empty body
                    throw new InvalidOperationException("Lambda has an empty body; Currently UNIMPLEMENTED");
                }
            }

            // The last line should be empty, denoting that a semicolon was used after the last actual line of code
            // If not, then we didn't have a final semicolon
            if (lines[lines.Length - 1].Trim().Length > 0)
            {
                // Last "line" is not empty, meaning we did not have a semicolon after the final function line
                throw new InvalidParseException($"Statement not terminated (missing '{CODE_LINE_DELIMITER}')", bodyExpr);
            }

            List<Ast> bodyLines = new List<Ast>();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                // Don't add the last (empty) line
                if (i + 1 < lines.Length)
                {
                    bodyLines.Add(Parse(line, typeMap));
                }
            }
            return bodyLines;
        }

        //public string Unparse(Ast ast)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    switch (ast.Type)
        //    {
        //        case AstTypes.NumberLiteral:
        //            return ast.NumberArg.ToString();
        //        case AstTypes.ListLiteral:
        //            builder.Append(LIST_START_WRAPPER);
        //            for (int i = 0; i < ast.AstCollectionArg.Length; i++)
        //            {
        //                builder.Append(Unparse(ast.AstCollectionArg[i]));
        //                if (i + 1 < ast.AstCollectionArg.Length)
        //                {
        //                    builder.Append(LIST_DELIMITER);
        //                }
        //            }
        //            builder.Append(LIST_END_WRAPPER);
        //            return builder.ToString();
        //        case AstTypes.StringLiteral:
        //            builder.Append(STRING_START_WRAPPER);
        //            builder.Append(ast.Expression);
        //            builder.Append(STRING_END_WRAPPER);
        //            return builder.ToString();
        //        case AstTypes.Variable:
        //            return ast.Name;
        //        case AstTypes.Call:
        //            builder.Append(Unparse(ast.ParentAst));
        //            builder.Append(CALL_START_WRAPPER);
        //            for (int i = 0; i < ast.AstCollectionArg.Length; i++)
        //            {
        //                builder.Append(Unparse(ast.AstCollectionArg[i]));
        //                if (i + 1 < ast.AstCollectionArg.Length)
        //                {
        //                    builder.Append(ARG_DELIMITER);
        //                }
        //            }
        //            builder.Append(CALL_END_WRAPPER);
        //            return builder.ToString();
        //        case AstTypes.Invalid:
        //            return ast.Expression;
        //        case AstTypes.LambdaLiteral:
        //            return "(" + string.Join(',', ast.Parameters.Select(x => UnparseParameter(x)).ToArray()) + ")" +
        //                (ast.CreatesEnv ? "=>" : "~>") + "{" +
        //                Unparse(ast.Body) + "}";
        //    }
        //    return "";
        //}

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
                    TYPE_GENERICS_WRAPPERS, LAMBDA_TYPE_PARAM_WRAPPERS);
                if (types.Length <= 0)
                {
                    throw new InvalidParseException("Type has no entries", typeStr);
                }
                // Now for each of these, we need to parse out the datatype + restrictions
                AstType type = AstType.UNION_BASE;
                for (int i = 0; i < types.Length; i++)
                {
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
                return AstTypeEntry.Simple(entryStr);
            }
            // Need to get everything between the first and last brackets
            int bracketStart = entryStr.IndexOf(TYPE_GENERICS_WRAPPERS[0]);
            int bracketEnd = entryStr.LastIndexOf(TYPE_GENERICS_WRAPPERS[1]);
            string name = entryStr.Substring(0, bracketStart).Trim();
            string genericsString = entryStr.SubstringBetween(bracketStart + 1, bracketEnd);
            List<AstType> generics = new List<AstType>();
            if (genericsString.Length > 0)
            {
                // Now need to split this up
                string[] genericStrings = SplitByDelimiter(genericsString, TYPE_GENERICS_DELIMITER,
                    TYPE_GENERICS_WRAPPERS);

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
                LAMBDA_TYPE_PARAM_WRAPPERS, TYPE_GENERICS_WRAPPERS);
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
                    AstType type = ParseType(paramTypeStrings[i]
                        .SubstringBetween(0, paramTypeStrings[i].Length - LAMBDA_TYPE_VARARGS_SYMBOL.Length));
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
            string arrow = str.SubstringBetween(endParen + 1, arrowTipIndex);
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
            string returnTypeString = str.Substring(arrowTipIndex + 1);
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

        public List<string> ParseGenericNames(string expression)
        {
            string[] genericNamesArray = SplitByDelimiter(expression, TYPE_GENERICS_DELIMITER, TYPE_GENERICS_WRAPPERS);
            List<string> genericNames = new List<string>(genericNamesArray);
            // Make sure all the names are valid, also trim out whitespace
            for (int i = 0; i < genericNames.Count; i++)
            {
                genericNames[i] = genericNames[i].Trim();
                if (!SYMBOL_NAME_REGEX.IsMatch(genericNames[i]))
                {
                    throw new InvalidParseException($"{genericNames[i]} is not a valid symbol name", genericNames[i]);
                }
            }
            return genericNames;
        }

        public IdentifierAst ParseIdentifier(string expression, VariableAstTypeMap typeMap)
        {
            if (IsParenWrapped(expression))
            {
                // Pull out the expression without the first and last characters
                expression = expression.Substring(1, expression.Length - 2);
                return ParseIdentifier(expression, typeMap);
            }

            // Check if this is just a symbol string
            if (SYMBOL_NAME_REGEX.IsMatch(expression))
            {
                if (RESERVED_KEYWORDS.Contains(expression))
                {
                    // Attempting to use a reserved keyword as a variable
                    throw new InvalidParseException(expression);
                }
                return IdentifierAst.RawVariable(expression);
            }

            // Try to do a dereference, if the string starts with it
            if (expression.StartsWith(DEREFERENCE_TOKEN))
            {
                // Everything else gets turned into the AST
                Ast r = Parse(expression.Substring(DEREFERENCE_TOKEN.Length), typeMap);
                return IdentifierAst.Dereference(r);
            }
            
            // Try to do member access
            if (MEMBER_ACCESS_REGEX.IsMatch(expression))
            {
                var groups = MEMBER_ACCESS_REGEX.Match(expression).Groups;
                string parentExpr = groups[1].Value;
                string varName = groups[2].Value;
                Ast parent = Parse(parentExpr, typeMap);
                return IdentifierAst.MemberAccess(parent, varName);
            }

            throw new InvalidParseException(expression);
        }

        /// <summary>
        /// Returns true if the expression is wrapped in a pair of matching parentheses
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private bool IsParenWrapped(string expression)
        {
            if (expression.Length < 2)
            {
                return false;
            }

            if (expression[0] != GENERIC_START_WRAPPER || expression[expression.Length - 1] != GENERIC_END_WRAPPER)
            {
                return false;
            }

            int wrapperCounter = 0;
            int startWrapperIndex = -1;
            for (int i = expression.Length - 2; i >= 0; i--)
            {
                if (expression[i] == CALL_END_WRAPPER)
                {
                    wrapperCounter++;
                }
                else if (expression[i] == CALL_START_WRAPPER)
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
            if (startWrapperIndex == 0)
            {
                // Starting parenthesis was the first character, so this is an expression wrapped in parentheses
                return true;
            }
            return false;
        }

        /// <summary>
        /// Splits "expr" into many strings delimited by "delimiter", but not if the delimiter is wrapped in parentheses or braces
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        private static string[] SplitByDelimiter(string expr, char delimiter)
        {
            int parenCounter = 0;
            int braceCounter = 0;
            int bracketCounter = 0;
            List<string> substrings = new List<string>();
            StringBuilder currentString = new StringBuilder();
            for (int i = 0; i < expr.Length; i++)
            {
                char c = expr[i];
                if (c == delimiter)
                {
                    // If not in parentheses or braces, then we add this as a split string
                    if (parenCounter == 0 && braceCounter == 0 && bracketCounter == 0)
                    {
                        substrings.Add(currentString.ToString());
                        currentString.Clear();
                    }
                    else
                    {
                        // The delimiter is part of a currently-being-built string
                        currentString.Append(c);
                    }
                }
                else
                {
                    // Add the character to our current string, update the counts as necessary
                    currentString.Append(c);
                    if (c == GENERIC_START_WRAPPER)
                    {
                        parenCounter++;
                    }
                    else if (c == GENERIC_END_WRAPPER)
                    {
                        parenCounter--;
                    }
                    else if (c == LIST_START_WRAPPER)
                    {
                        braceCounter++;
                    }
                    else if (c == LIST_END_WRAPPER)
                    {
                        braceCounter--;
                    }
                    else if (c == SIMPLE_LAMBDA_START_WRAPPER)
                    {
                        bracketCounter++;
                    }
                    else if (c == SIMPLE_LAMBDA_END_WRAPPER)
                    {
                        bracketCounter--;
                    }
                }
            }
            // Remember to add the string that we are in the middle of building
            substrings.Add(currentString.ToString());
            return substrings.ToArray();
        }

        // Wrapper pairs in the form of "{}" or "()"
        // Need to ignore strings as well - that's hard-coded into this function
        private static string[] SplitByDelimiter(string expr, char delimiter, params string[] wrapperPairs)
        {
            if (expr.Length == 0)
            {
                return new string[0];
            }

            List<string> substrings = new List<string>();
            StringBuilder current = new StringBuilder();
            for (int i = 0; i < expr.Length; i++)
            {
                char c = expr[i];
                if (c == delimiter)
                {
                    // Split here
                    substrings.Add(current.ToString());
                    current = new StringBuilder();
                }
                else if (c == STRING_START_WRAPPER)
                {
                    char prevChar = c;
                    while (!(c == STRING_END_WRAPPER && prevChar != STRING_ESCAPE_STARTER))
                    {
                        prevChar = c;
                        c = expr[i];
                        current.Append(c);
                        i++;
                        if (i >= expr.Length)
                        {
                            throw new InvalidParseException("String does not terminate", expr);
                        }
                    }
                    current.Append(c);
                }
                else
                {
                    bool foundWrapper = false;
                    for (int j = 0; j < wrapperPairs.Length; j++)
                    {
                        if (c == wrapperPairs[j][0])
                        {
                            foundWrapper = true;
                            int end = GetBracketEndIndex(expr, i, wrapperPairs[j][0], wrapperPairs[j][1]);
                            while (i != end)
                            {
                                c = expr[i];
                                current.Append(c);
                                i++;
                            }
                            // Make sure we add the end wrapper, by adding one more character
                            c = expr[i];
                            current.Append(c);
                            break;
                        }
                    }
                    if (!foundWrapper)
                    {
                        current.Append(c);
                    }
                }
            }
            substrings.Add(current.ToString());
            return substrings.ToArray();
        }

        private static bool IsWrappedBy(string expression, char start, char end)
        {
            if (expression.Length < 2)
            {
                return false;
            }
            if (expression[0] != start)
            {
                return false;
            }
            if (expression[expression.Length - 1] != end)
            {
                return false;
            }
            int levels = 0;
            for (int i = 1; i < expression.Length - 1; i++)
            {
                char c = expression[i];
                if (c == GENERIC_START_WRAPPER && start != GENERIC_START_WRAPPER)
                {
                    // We want to jump to the end of the parentheses
                    int newI = GetBracketEndIndex(expression, i, GENERIC_START_WRAPPER, GENERIC_END_WRAPPER);
                    if (newI < 0)
                    {
                        // If no end to the paren, we have bigger problems
                        return false;
                    }
                    i = newI;
                }
                if (c == SIMPLE_LAMBDA_START_WRAPPER && start != SIMPLE_LAMBDA_START_WRAPPER)
                {
                    int newI = GetBracketEndIndex(expression, i, SIMPLE_LAMBDA_START_WRAPPER, 
                        SIMPLE_LAMBDA_END_WRAPPER);
                    // We want to jump to the end of the brackets
                    if (newI < 0)
                    {
                        // If no end to the bracket, we have bigger problems
                        return false;
                    }
                    i = newI;
                }
                if (c == LIST_START_WRAPPER && start != LIST_START_WRAPPER)
                {
                    int newI = GetBracketEndIndex(expression, i, LIST_START_WRAPPER, LIST_END_WRAPPER);
                    // We want to jump to the end of the list
                    if (newI < 0)
                    {
                        // If no end to the brace, we have bigger problems
                        return false;
                    }
                    i = newI;
                }
                if (c == STRING_START_WRAPPER && start != STRING_START_WRAPPER)
                {
                    int newI = GetBracketEndIndex(expression, i, STRING_START_WRAPPER, STRING_END_WRAPPER);
                    // We want to jump to the end of the string
                    if (newI < 0)
                    {
                        // If no end to the string, we have bigger problems
                        return false;
                    }
                    i = newI;
                }
                if (c == start)
                {
                    levels++;
                }
                if (c == end)
                {
                    // If we're closing the current "thing", then that's a problem
                    if (levels == 0)
                    {
                        return false;
                    }
                    levels--;
                }
            }
            // If we haven't failed yet, then just return
            return true;
        }

        private static int GetBracketEndIndex(string str, int idx, char start, char end)
        {
            int level = 0;
            for (int i = idx; i < str.Length; i++)
            {
                if (str[i] == start)
                {
                    level++;
                }
                if (str[i] == end)
                {
                    level--;
                    if (level == 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        
        /// <summary>
        /// Returns true if the expression is a list, or false if not
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private bool MatchesList(string expression)
        {
            return IsWrappedBy(expression, LIST_START_WRAPPER, LIST_END_WRAPPER);
        }

        private bool MatchesSimpleLambda(string expression)
        {
            return IsWrappedBy(expression, SIMPLE_LAMBDA_START_WRAPPER, SIMPLE_LAMBDA_END_WRAPPER);
        }

        // Returns index of the '='
        private int TryMatchAssignment(string expression)
        {
            // Check that there is an '=' that is at the bottom level
            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];
                if (c == GENERIC_START_WRAPPER)
                {
                    int newI = GetBracketEndIndex(expression, i, GENERIC_START_WRAPPER, GENERIC_END_WRAPPER);
                    if (newI < 0)
                    {
                        return -1;
                    }
                    i = newI;
                }
                if (c == SIMPLE_LAMBDA_START_WRAPPER)
                {
                    int newI = GetBracketEndIndex(expression, i, SIMPLE_LAMBDA_START_WRAPPER, 
                        SIMPLE_LAMBDA_END_WRAPPER);
                    if (newI < 0)
                    {
                        return -1;
                    }
                    i = newI;
                }
                if (c == LIST_START_WRAPPER)
                {
                    int newI = GetBracketEndIndex(expression, i, LIST_START_WRAPPER, LIST_END_WRAPPER);
                    if (newI < 0)
                    {
                        return -1;
                    }
                    i = newI;
                }
                if (c == STRING_START_WRAPPER)
                {
                    int newI = GetBracketEndIndex(expression, i, STRING_START_WRAPPER, STRING_END_WRAPPER);
                    if (newI < 0)
                    {
                        return -1;
                    }
                    i = newI;
                }
                if (c == ASSIGNMENT_TOKEN)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Attempts to match this expression to a "call", returning if the match succeeded, and the
        /// caller/arguments
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private CallMatch MatchCall(string expression)
        {
            if (expression.Length < 2)
            {
                return CallMatch.Failure;
            }

            // What makes a call a call?
            // Well, it's simply some "thing" followed by parentheses, which may have arguments in them
            // It's important to note that the "thing" must have balanced brackets: that is, balanced curly braces
            // and balanced parentheses. That way, '((x)' doesn't count as a call, because it appears to really be
            // the start of a lambda ex. '((x)=>{_add(x,7)})
            // So, lets start from the back. The last character should be a parenthesis. Then, we simply find its
            // matching balanced one. As long as the match is not the first character in the string, then
            // there is something before the pair, and we've found what should be interpreted as a call!
            if (expression[expression.Length - 1] != CALL_END_WRAPPER)
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
                if (expression[i] == CALL_END_WRAPPER)
                {
                    wrapperCounter++;
                }
                else if (expression[i] == CALL_START_WRAPPER)
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

            // Everything before the start index is what was called (the callee/caller, terminology is not consistent here)
            string calledPart = expression.Substring(0, startWrapperIndex);

            string argsString = expression.Substring(startWrapperIndex + 1, expression.Length - (startWrapperIndex + 1) - 1);

            return new CallMatch(true, calledPart, argsString);
        }
        private struct CallMatch
        {
            public bool IsMatch;
            public string Caller;
            public string Args;

            public CallMatch(bool isMatch, string caller, string args)
            {
                IsMatch = isMatch;
                Caller = caller;
                Args = args;
            }

            public static readonly CallMatch Failure = new CallMatch(false, null, null);
        }
    }
}
