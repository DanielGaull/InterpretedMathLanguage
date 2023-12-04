using IML.Evaluation;
using IML.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Syntax
{
    // Represents a syntax parameter, which is a normal parameter with extensions for additional
    // options that syntax provides
    public class SyntaxParameter
    {
        public string Name { get; private set; }

        // Used only for string parameters
        // If true, should match a symbol (literal code) and put it into a string, rather than trying to match a string
        public bool IsStringSymbol { get; private set; }
        // Used only for lambda parameters
        // If true, should match a section of code (i.e. any expression) and put it into a lambda
        public bool IsWrappingLambda { get; private set; }

        public SyntaxParameter(string name)
            : this(name, false, false)
        {}
        public SyntaxParameter(string name, bool isStringSymbol, bool isWrappingLambda)
        {
            Name = name;
            IsStringSymbol = isStringSymbol;
            IsWrappingLambda = isWrappingLambda;
        }
    }
}
