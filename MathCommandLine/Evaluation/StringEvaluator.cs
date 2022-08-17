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
        private Parser parser;
        private FunctionDict funcDict;
        private DataTypeDict dtDict;

        public StringEvaluator(IEvaluator superEvaluator, Parser parser, FunctionDict funcDict, DataTypeDict dtDict)
        {
            this.superEvaluator = superEvaluator;
            this.parser = parser;
            this.funcDict = funcDict;
            this.dtDict = dtDict;
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
            Ast tree = parser.ParseExpression(expression);
            return EvaluateAst(tree, variables);
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
                    return MValue.Type(dtDict.GetType(ast.Name));
            }
            return MValue.Empty;
        }
    }
}
