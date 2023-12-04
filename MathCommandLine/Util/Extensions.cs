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
    }
}
