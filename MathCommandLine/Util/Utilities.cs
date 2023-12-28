using IML.CoreDataTypes;
using IML.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML
{
    public static class Utilities
    {
        public static MList StringToMList(string str)
        {
            return new MList(str.Select((c) => MValue.Number(c)).ToList(), MType.Number);
        }
        public static string MListToString(MList list)
        {
            return new string(list.InternalList.Select((v) => (char)v.NumberValue).ToArray());
        }
    }
}
