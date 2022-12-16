using MathCommandLine.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Syntax
{
    // Represents a syntax parameter, which is a normal parameter with extensions for additional
    // options that syntax provides
    public class SyntaxParameter
    {
        public MParameter BaseParam { get; private set; }
        public string Name
        {
            get
            {
                return BaseParam.Name;
            }
        }

        // Used only for string parameters
        // If true, should match a symbol (literal code) and put it into a string, rather than trying to match a string
        public bool IsStringSymbol { get; private set; }
        // Used only for lambda parameters
        // If true, should match a section of code (i.e. any expression) and put it into a lambda
        public bool IsWrappingLambda { get; private set; }

        public SyntaxParameter(MParameter param)
            : this(param, false, false)
        {}
        public SyntaxParameter(MParameter param, bool isStringSymbol, bool isWrappingLambda)
        {
            BaseParam = param;
            IsStringSymbol = isStringSymbol;
            IsWrappingLambda = isWrappingLambda;
        }
    }
}
