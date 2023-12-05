using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.Util
{
    public static class Extensions
    {
        public static int CountChar(this string str, char toFind)
        {
            return str.Count(x => x == toFind);
        }

        public static string SubstringBetween(this string str, int start, int end)
        {
            int length = end - start;
            return str.Substring(start, length);
        }
    }
}
