using IML.CoreDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.Parsing.AST
{
    public class AstParameter
    {
        public AstType Type { get; private set; }
        public string Name { get; private set; }

        public AstParameter(string name, AstType type)
        {
            Name = name;
            Type = type;
        }
    }
}
