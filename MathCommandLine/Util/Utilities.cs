using MathCommandLine.CoreDataTypes;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine
{
    public static class Utilities
    {
        public static MList StringToMList(string str)
        {
            return new MList(str.Select((c) => MValue.Number(c)).ToList());
        }
        public static string MListToString(MList list)
        {
            return new string(list.InternalList.Select((v) => (char)v.NumberValue).ToArray());
        }
    }
}
