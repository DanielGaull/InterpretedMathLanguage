using MathCommandLine.CoreDataTypes;
using MathCommandLine.Exceptions;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MathCommandLine.Evaluation
{
    public class StringEvaluator : IEvaluator
    {
        private IEvaluator superEvaluator;
        private Parser parser;
        //private FunctionDict funcDict;
        private DataTypeDict dtDict;

        private static readonly Regex WHITESPACE_REGEX = new Regex(@"\s+");

        public StringEvaluator(IEvaluator superEvaluator, Parser parser, FunctionDict funcDict, DataTypeDict dtDict)
        {
            this.superEvaluator = superEvaluator;
            this.parser = parser;
            //this.funcDict = funcDict;
            this.dtDict = dtDict;
        }

        public MValue Evaluate(MExpression mExpression, MArguments variables)
        {
            if (!mExpression.IsNativeExpression)
            {
                // TODO: Evaluate the string expression
                // TODO: Remove whitespace, handle variables, etc.
                string expr = mExpression.Expression;
                expr = parser.ConvertStringsToLists(expr);
                expr = CleanWhitespace(expr);
                return FinalStageEvaluate(expr, variables);
            }
            throw new NotImplementedException();
        }

        private string CleanWhitespace(string expression)
        {
            return WHITESPACE_REGEX.Replace(expression, "");
        }

        // For the "final stage" in evaluation, when the expression has been whittled down to only
        // functions, variables (i.e. arguments), and literal core values
        private MValue FinalStageEvaluate(string expression, MArguments variables)
        {
            Ast tree = parser.ParseExpression(expression);
            return EvaluateAst(tree, variables);
        }

        private MValue EvaluateAst(Ast ast, MArguments variables)
        {
            switch (ast.Type)
            {
                case AstTypes.Call:
                    List<MArgument> argsList = new List<MArgument>();
                    for (int i = 0; i < ast.AstCollectionArg.Length; i++)
                    {
                        argsList.Add(new MArgument(EvaluateAst(ast.AstCollectionArg[i], variables)));
                    }
                    MArguments args = new MArguments(argsList);
                    MValue evaluatedCaller = EvaluateAst(ast.CalledAst, variables);
                    if (evaluatedCaller.DataType != MDataType.Lambda)
                    {
                        // Not a callable object
                        return MValue.Error(Util.ErrorCodes.NOT_CALLABLE, 
                            "\"" + evaluatedCaller.DataType.Name + "\" is not a callable data type.", MList.Empty);
                    }
                    // We have a callable type!
                    return evaluatedCaller.LambdaValue.Evaluate(args, superEvaluator);
                case AstTypes.Variable:
                    // TODO: Handle if variable doesn't exist
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
                    MExpression expression = new MExpression(ast.Expression);
                    MParameter[] paramArray = ast.Parameters.Select((astParam) =>
                    {
                        string name = astParam.Name;
                        MTypeRestrictionsEntry[] entries = astParam.TypeEntries.Select((entry) =>
                        {
                            if (!dtDict.Contains(entry.DataTypeName))
                            {
                                return new MTypeRestrictionsEntry();
                            }
                            return new MTypeRestrictionsEntry(dtDict.GetType(entry.DataTypeName), entry.ValueRestrictions);
                        }).ToArray();
                        if (entries.Any((entry) => entry.IsEmpty))
                        {
                            return MParameter.Empty;
                        }
                        // If any type entries are empty, then return an error (type doesn't exist)
                        return new MParameter(name, entries);
                    }).ToArray();
                    if (paramArray.Any((param) => param.IsEmpty))
                    {
                        return MValue.Error(Util.ErrorCodes.TYPE_DOES_NOT_EXIST,
                            "Type \"" + ast.Name + "\" is not defined.", MList.Empty);
                    }
                    MParameters parameters = new MParameters();
                    return MValue.Lambda(new MLambda(parameters, expression));
                case AstTypes.TypeLiteral:
                    if (!dtDict.Contains(ast.Name))
                    {
                        return MValue.Error(Util.ErrorCodes.TYPE_DOES_NOT_EXIST,
                            "Type \"" + ast.Name + "\" is not defined.", MList.Empty);
                    }
                    return MValue.Type(dtDict.GetType(ast.Name));
            }
            return MValue.Empty;
        }
    }
}
