using MathCommandLine.CoreDataTypes;
using MathCommandLine.Exceptions;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using MathCommandLine.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MathCommandLine.Evaluation
{
    public class StringEvaluator
    {
        private IInterpreter superEvaluator;
        private Parser parser;
        private DataTypeDict dtDict;
        private VariableManager varManager;

        private static readonly Regex WHITESPACE_REGEX = new Regex(@"\s+");

        public StringEvaluator(IInterpreter superEvaluator, Parser parser, DataTypeDict dtDict, 
            VariableManager varManager)
        {
            this.superEvaluator = superEvaluator;
            this.parser = parser;
            this.dtDict = dtDict;
            this.varManager = varManager;
        }

        public MValue Evaluate(MExpression mExpression, MArguments variables)
        {
            if (!mExpression.IsNativeExpression)
            {
                for (int i = 0; i < variables.Length; i++)
                {
                    varManager.AddVariable(variables[i].Name, variables[i].Value, false);
                }
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
                    MValue evaluatedCaller = EvaluateAst(ast.CalledAst, variables);
                    // If an error, return that error instead of attempting to call it
                    if (evaluatedCaller.DataType == MDataType.Error)
                    {
                        return evaluatedCaller;
                    }
                    if (evaluatedCaller.DataType != MDataType.Lambda)
                    {
                        // Not a callable object
                        return MValue.Error(Util.ErrorCodes.NOT_CALLABLE, 
                            "Cannot invoke because \"" + evaluatedCaller.DataType.Name + "\" is not a callable data type.", 
                            MList.Empty);
                    }
                    MLambda lambda = evaluatedCaller.LambdaValue;
                    List<MArgument> argsList = new List<MArgument>();
                    for (int i = 0; i < ast.AstCollectionArg.Length; i++)
                    {
                        argsList.Add(new MArgument(EvaluateAst(ast.AstCollectionArg[i], variables)));
                    }
                    MArguments args = new MArguments(argsList);
                    // We have a callable type!
                    return lambda.Evaluate(args, superEvaluator);
                case AstTypes.Variable:
                    // Return the value of the variable with this name
                    if (varManager.HasValue(ast.Name))
                    {
                        return varManager.GetValue(ast.Name);
                    }
                    else
                    {
                        return MValue.Error(Util.ErrorCodes.VAR_DOES_NOT_EXIST, 
                            $"Variable or argument \"{ast.Name}\" does not exist.", MList.Empty);
                    }
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
                    MParameters parameters = new MParameters(paramArray);
                    return MValue.Lambda(new MLambda(parameters, expression));
            }
            return MValue.Empty;
        }
    }
}
