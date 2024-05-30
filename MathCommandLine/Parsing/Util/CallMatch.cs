using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Parsing.Util
{
    public struct CallMatch
    {
        public bool IsMatch;
        public string Caller;
        public string Generics;
        public string Args;

        public CallMatch(bool isMatch, string caller, string generics, string args)
        {
            IsMatch = isMatch;
            Caller = caller;
            Generics = generics;
            Args = args;
        }

        public static readonly CallMatch Failure = new CallMatch(false, null, null, null);
    }
}
