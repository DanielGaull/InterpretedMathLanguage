using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Parsing.Util
{
    public struct LambdaMatch
    {
        public bool IsMatch;
        public string Generics;
        public string Params;
        public string ReturnType;
        public string ArrowBit;
        public string Body;

        public LambdaMatch(bool isMatch, string generics, string @params, 
            string returnType, string arrowBit, string body)
        {
            IsMatch = isMatch;
            Generics = generics;
            Params = @params;
            ReturnType = returnType;
            ArrowBit = arrowBit;
            Body = body;
        }

        public static LambdaMatch Failure = new LambdaMatch(false, null, null, null, null, null);
    }
}
