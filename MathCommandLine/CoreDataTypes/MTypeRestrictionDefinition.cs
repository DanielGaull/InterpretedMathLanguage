using IML.Environments;
using IML.Evaluation;
using IML.Exceptions;
using IML.Functions;
using IML.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.CoreDataTypes
{
    public class MTypeRestrictionDefinition
    {
        // Has a name and args
        // For example, could have name > and args of a number,
        // which represents the >(20) value
        // Also, needs to store some method of determining if a type matches the restriction
        // One way to do that is to provide a function that returns a boolean

        public string Name { get; private set; }
        public MClosure VerificationFunction { get; private set; }
        public List<Parameter> Parameters { get; private set; }
        
        public MTypeRestrictionDefinition(string name, MClosure verificationFunc)
        {
            Name = name;
            VerificationFunction = verificationFunc;

            if (!verificationFunc.CreatesEnv)
            {
                // Must create an environment. Cannot use the parent environment here
                throw new MException("Verification functions for restrictions must create environments");
            }

            // Derive parameters from the verification function
            MParameters givenParams = verificationFunc.Parameters;
            Parameters = new List<Parameter>();
            // Start at 1 to skip the first parameter, which is the value being tested
            for (int i = 1; i < givenParams.Length; i++)
            {
                MParameter param = givenParams[i];
                Parameter convertedParam = new Parameter(param.Name, TypeToRestrictionType(param.Type));
                Parameters.Add(convertedParam);
            }
        }

        public bool IsValid(MValue value, MTypeRestriction instance, IInterpreter interpreter, MEnvironment env)
        {
            // Convert the instance's values to arguments to call the function with
            // We will also pass in the value as the very first argument
            List<MArgument> args = new List<MArgument>();
            args.Add(new MArgument(value));
            for (int i = 0; i < instance.ArgumentValues.Count; i++)
            {
                args.Add(new MArgument(ArgumentToValue(instance.ArgumentValues[i])));
            }
            // Call the function
            MValue result = interpreter.PerformCall(VerificationFunction, new MArguments(args), env);
            // Return the truthy-ness value of the result
            return result.IsTruthy();
        }

        private RestrictionArgumentType TypeToRestrictionType(MType type)
        {
            // The MType can only have a single entry; multiple entries results in an error
            if (type.Entries.Count != 1)
            {
                throw new MException("Type restriction verification function parameters can only have a single type");
            }
            // We cannot restrict this value at all, either, it must be a "pure" type
            MDataTypeRestrictionEntry entry = type.Entries[0];
            if (entry.TypeRestrictions.Count != 0)
            {
                throw new MException("Type restriction verification function parameters cannot have type restrictions");
            }
            // Finally, it must be one of the valid types, which are number, string, and type
            if (entry.TypeDefinition.MatchesTypeExactly(MDataType.Number))
            {
                return RestrictionArgumentType.Number;
            }
            if (entry.TypeDefinition.MatchesTypeExactly(MDataType.String))
            {
                return RestrictionArgumentType.String;
            }
            if (entry.TypeDefinition.MatchesTypeExactly(MDataType.Type))
            {
                return RestrictionArgumentType.Type;
            }
            throw new MException("Type restriction verification function parameters must be of type number, string, or type.");
        }

        private MValue ArgumentToValue(MTypeRestriction.Argument arg)
        {
            switch (arg.ArgumentType)
            {
                case RestrictionArgumentType.Number:
                    return MValue.Number(arg.NumberValue);
                case RestrictionArgumentType.String:
                    return MValue.String(arg.StringValue);
                case RestrictionArgumentType.Type:
                    return MValue.Type(arg.TypeValue);
            }
            return null;
        }

        public class Parameter
        {
            public string Name { get; private set; }
            public RestrictionArgumentType Type { get; private set; }

            public Parameter(string name, RestrictionArgumentType type)
            {
                Name = name;
                Type = type;
            }
        }
    }
}
