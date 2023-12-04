using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.Evaluation
{
    public class AstParameter
    {
        public AstParameterTypeEntry[] TypeEntries { get; private set; }
        public string Name { get; private set; }

        public AstParameter(string name, params AstParameterTypeEntry[] typeEntries)
        {
            Name = name;
            TypeEntries = typeEntries;
        }
        public AstParameter(string name, params string[] dataTypeNames)
            : this(name, dataTypeNames.Select(x => new AstParameterTypeEntry(x)).ToArray())
        {

        }
    }
    public class AstParameterTypeEntry
    {
        public string DataTypeName { get; private set; }

        public AstParameterTypeEntry(string dataTypeName)
        {
            DataTypeName = dataTypeName;
        }
    }
}
