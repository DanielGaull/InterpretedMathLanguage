using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Evaluation
{
    public class Ast
    {
        public AstTypes Type { get; private set; }
        // Used for Literal types
        public MValue ValueArg { get; private set; }
        // The array of arguments. Used for Function types
        // Because everything breaks down to functions, literals, and variables, functions are the only way a tree is generated
        public Ast[] ArgumentArray { get; private set; }
        // Used for both Function and Variable types
        public string Name { get; private set; }

        private Ast(AstTypes type, MValue valueArg, Ast[] argumentArray, string name)
        {
            Type = type;
            ValueArg = valueArg;
            ArgumentArray = argumentArray;
            Name = name;
        }

        public static Ast Literal(MValue value)
        {
            return new Ast(AstTypes.LiteralValue, value, null, null);
        }
        public static Ast Function(string name, params Ast[] args)
        {
            return new Ast(AstTypes.Function, MValue.Empty, args, name);
        }
        public static Ast Variable(string name)
        {
            return new Ast(AstTypes.Variable, MValue.Empty, null, name);
        }
    }

    public enum AstTypes
    {
        LiteralValue,
        Function,
        Variable,
    }
}
