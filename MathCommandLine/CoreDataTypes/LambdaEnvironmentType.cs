using System;
using System.Collections.Generic;
using System.Text;

namespace IML.CoreDataTypes
{
    public enum LambdaEnvironmentType
    {
        ForceEnvironment, // Ex. ()!=>{}
        ForceNoEnvironment, // Ex. ()!~>{}
        AllowAny, // Ex. ()=>{}
    }
}
