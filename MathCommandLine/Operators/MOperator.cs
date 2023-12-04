using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Operators
{
    // Class that represents an operator
    // This is without behavior defined; just that the operator exists
    public class MOperator
    {
        // Note: for ternary, splits the string in half to get the 'before' and 'after'
        public string CodeString { get; private set; }
        public string Name { get; private set; }
        // 1 = unary, 2 = binary, 3 = ternary
        public int Tier { get; private set; }

        private MOperator(string code, string name, int tier)
        { 
            CodeString = code;
            Name = name;
            Tier = tier;
        }

        public static List<MOperator> GetOperators()
        {
            return new List<MOperator>()
            {
                new MOperator("+", "add", 2),
                new MOperator("-", "subtract", 2),
                new MOperator("*", "multiply", 2),
                new MOperator("/", "divide", 2),
                new MOperator("%", "remainder", 2),

                new MOperator("&", "bit_and", 2),
                new MOperator("|", "bit_or", 2),
                new MOperator("~", "bit_not", 1),

                new MOperator("&&", "and", 2),
                new MOperator("||", "or", 2),
                new MOperator("!", "not", 1),

                new MOperator("<<", "bitshift_left", 2),
                new MOperator(">>", "bitshift_right", 2),

                new MOperator("==", "equals", 2),
                new MOperator("!=", "not_equals", 2),
                new MOperator("<", "less", 2),
                new MOperator(">", "greater", 2),
                new MOperator("<=", "less_equal", 2),
                new MOperator(">=", "greater_equal", 2),

                new MOperator("?:", "conditional", 3),
            };
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MOperator))
            {
                return false;
            }
            MOperator mop = (MOperator)obj;
            return mop.Name == Name && mop.Tier == Tier && mop.CodeString == CodeString;
        }
        public static bool operator ==(MOperator m1, MOperator m2)
        {
            return m1.Equals(m2);
        }
        public static bool operator !=(MOperator m1, MOperator m2)
        {
            return !(m1 == m2);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
