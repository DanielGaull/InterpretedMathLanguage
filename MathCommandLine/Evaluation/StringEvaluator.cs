using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Evaluation
{
    public class StringEvaluator : IEvaluator
    {
        private IEvaluator superEvaluator;

        private FunctionDict funcDict;

        private const string NUMBER_REGEX = @"[+-]?[0-9]+(\.[0-9]*)?";
        private const string LIST_REGEX = @"\{.*\}";
        private const string LAMBDA_REGEX = @"\(.*\)=>\{.*\}";
        private const string FUNCTION_REGEX = @"^[a-zA-Z_][a-zA-Z0-9_]*\(.*\)$";

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

            throw new NotImplementedException();
        }

        private Ast BuildTree(string expression)
        {
            return null;
        }
        private MValue EvaluateAst(Ast ast, MArguments variables)
        {
            switch (ast.Type)
            {
                case AstTypes.Function:
                    List<MArgument> argsList = new List<MArgument>();
                    for (int i = 0; i < ast.ArgumentArray.Length; i++)
                    {
                        argsList.Add(new MArgument(EvaluateAst(ast.ArgumentArray[i], variables)));
                    }
                    MArguments args = new MArguments(argsList);
                    MFunction function = funcDict.GetFunction(ast.Name);
                    return function.Evaluate(args, superEvaluator);
                case AstTypes.LiteralValue:
                    // Very easy, can just literally return the value of the AST
                    return ast.ValueArg;
                case AstTypes.Variable:
                    // Return the value of the variable with this name (in arguments)
                    return variables[ast.Name].Value;
            }
            return MValue.Empty;
        }
    }
}
