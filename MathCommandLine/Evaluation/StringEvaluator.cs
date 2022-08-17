using MathCommandLine.CoreDataTypes;
using MathCommandLine.Exceptions;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MathCommandLine.Evaluation
{
    public class StringEvaluator : IEvaluator
    {
        private IEvaluator superEvaluator;

        private FunctionDict funcDict;

        private const string NUMBER_REGEX_STR = @"[+-]?[0-9]+(\.[0-9]*)?";
        private const string LIST_REGEX_STR = @"\{.*\}";
        private const string LAMBDA_REGEX_STR = @"\(.*\)=>\{.*\}";
        private const string TYPE_REGEX_STR = @"#[^\s]*";
        private const string FUNCTION_REGEX_STR = @"[a-zA-Z_][a-zA-Z0-9_]*\(.*\)";

        // Regexes for matching language symbols
        private readonly Regex NUMBER_REGEX = new Regex(NUMBER_REGEX_STR);
        private readonly Regex LIST_REGEX = new Regex(LIST_REGEX_STR);
        private readonly Regex LAMBDA_REGEX = new Regex(LAMBDA_REGEX_STR);
        private readonly Regex TYPE_REGEX = new Regex(TYPE_REGEX_STR);
        private readonly Regex FUNCTION_REGEX = new Regex(FUNCTION_REGEX_STR);
        // Regexes for matching language symbols, where they are the ONLY thing in the string (i.e. find if the whole string is a list) 
        private readonly Regex NUMBER_ONLY_REGEX = new Regex("^" + NUMBER_REGEX_STR + "$");
        private readonly Regex LIST_ONLY_REGEX = new Regex("^" + LIST_REGEX_STR + "$");
        private readonly Regex LAMBDA_ONLY_REGEX = new Regex("^" + LAMBDA_REGEX_STR + "$");
        private readonly Regex TYPE_ONLY_REGEX = new Regex("^" + TYPE_REGEX_STR + "$");
        private readonly Regex FUNCTION_ONLY_REGEX = new Regex("^" + FUNCTION_REGEX_STR + "$");

        // Other useful regexes
        // List helpers
        private readonly Regex LIST_DELIMITER_REGEX = new Regex(@",");
        private readonly Regex LIST_EXTRACTOR_REGEX = new Regex(@"\{([^}]*)\}");
        // Other helpers
        private readonly Regex WHITESPACE_REGEX = new Regex(@"\s+");

        public StringEvaluator(IEvaluator superEvaluator, FunctionDict funcDict)
        {
            this.superEvaluator = superEvaluator;
            this.funcDict = funcDict;
        }

        public MValue Evaluate(MExpression expression, MArguments variables)
        {
            if (!expression.IsNativeExpression)
            {
                // TODO: Evaluate the string expression
            }
            throw new NotImplementedException();
        }

        // For the "final stage" in evaluation, when the expression has been whittled down to only
        // functions, variables (i.e. arguments), and literal core values
        private MValue FinalStageEvaluate(string expression, MArguments variables)
        {
            Ast tree = ParseExpression(expression);
            return EvaluateAst(tree, variables);
        }

        private Ast ParseExpression(string expression)
        {
            // TODO: Add support for big_decimal and big_int (require D or L at the end of the number literal, i.e. 750L or 0.642D)
            // 'expression' is either a function or literal
            if (FUNCTION_ONLY_REGEX.IsMatch(expression))
            {

            }
            else if (NUMBER_ONLY_REGEX.IsMatch(expression))
            {
                // Parse the double and throw it into the AST
                double number = double.Parse(expression);
                return Ast.NumberLiteral(number);
            }
            else if (LIST_ONLY_REGEX.IsMatch(expression))
            {
                // Extract the elements of the list
                string elements = LIST_EXTRACTOR_REGEX.Match(expression).Groups[1].Value;
                // Separate by the list delimiter
                string[] elementStrings = LIST_DELIMITER_REGEX.Split(elements);
                List<Ast> elementAsts = new List<Ast>();

            }
            else if (LAMBDA_ONLY_REGEX.IsMatch(expression))
            {

            }
            else if (TYPE_ONLY_REGEX.IsMatch(expression))
            {
                // TODO: Extract type from string
            }
            throw new InvalidParseException(expression);
        }
        private MValue EvaluateAst(Ast ast, MArguments variables)
        {
            switch (ast.Type)
            {
                case AstTypes.Function:
                    List<MArgument> argsList = new List<MArgument>();
                    for (int i = 0; i < ast.AstCollectionArg.Length; i++)
                    {
                        argsList.Add(new MArgument(EvaluateAst(ast.AstCollectionArg[i], variables)));
                    }
                    MArguments args = new MArguments(argsList);
                    MFunction function = funcDict.GetFunction(ast.Name);
                    return function.Evaluate(args, superEvaluator);
                case AstTypes.Variable:
                    // Return the value of the variable with this name (in arguments)
                    return variables[ast.Name].Value;

                case AstTypes.NumberLiteral:
                    return MValue.Number(ast.NumberArg);
                case AstTypes.ListLiteral:
                    // Need to evaluate each element of the list
                    List<MValue> elements = new List<MValue>();
                    foreach (Ast elem in ast.AstCollectionArg)
                    {
                        elements.Add(EvaluateAst(elem, variables));
                    }
                    return MValue.List(new MList(elements));
                case AstTypes.LambdaLiteral:
                    // TODO
                    break;
                case AstTypes.TypeLiteral:
                    // TODO
                    break;
            }
            return MValue.Empty;
        }
    }
}
