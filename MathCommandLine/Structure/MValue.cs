using MathCommandLine.CoreDataTypes;
using MathCommandLine.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Structure
{
    // Class for a math data value
    // Can represent a primitive data type or a composite data type
    struct MValue
    {
        public double NumberValue; // For the number type, a 64-bit floating-pt number
        public MList ListValue; // For the list type
        public MLambda LambdaValue; // For the lambda type
        public decimal BigDecimalValue; // For the big_decimal type
        public long BigIntValue; // For the big_int type
        public MDataType TypeValue; // For the type type (that represents an actual data type)
        public Dictionary<string, MValue> DataValues; // The Data Values for composite types (maps name => value)

        public MValue(double numberValue, MList listValue, MLambda lambdaValue, decimal bigDecimalValue, 
            long bigIntValue, MDataType typeValue, Dictionary<string, MValue> dataValues)
        {
            NumberValue = numberValue;
            ListValue = listValue;
            LambdaValue = lambdaValue;
            BigDecimalValue = bigDecimalValue;
            BigIntValue = bigIntValue;
            TypeValue = typeValue;
            DataValues = dataValues;
        }

        public static MValue Number(double numberValue)
        {
            return new MValue(numberValue, MList.Empty, MLambda.Empty, 0, 0, MDataType.Empty, null);
        }
        public static MValue List(MList list)
        {
            return new MValue(0, list, MLambda.Empty, 0, 0, MDataType.Empty, null);
        }
        public static MValue Lambda(MLambda lambda)
        {
            return new MValue(0, MList.Empty, lambda, 0, 0, MDataType.Empty, null);
        }
        public static MValue BigDecimal(decimal bigDecimal)
        {
            return new MValue(0, MList.Empty, MLambda.Empty, bigDecimal, 0, MDataType.Empty, null);
        }
        public static MValue BigInt(long bigInt)
        {
            return new MValue(0, MList.Empty, MLambda.Empty, 0, bigInt, MDataType.Empty, null);
        }
        public static MValue Type(MDataType type)
        {
            return new MValue(0, MList.Empty, MLambda.Empty, 0, 0, type, null);
        }
        public static MValue Composite(Dictionary<string, MValue> values)
        {
            return new MValue(0, MList.Empty, MLambda.Empty, 0, 0, MDataType.Empty, values);
        }

        // Errors are a core composite type, so they are not primitive but still exist in core code
        public static MValue Error(ErrorCodes code, string message, MList data)
        {
            Dictionary<string, MValue> values = new Dictionary<string, MValue>()
            {
                { "code", Number((int) code) },
                { "message", List(Utilities.StringToMList(message)) },
                { "data", List(data) }
            };
            return Composite(values);
        }
        public static MValue Error(ErrorCodes code)
        {
            Dictionary<string, MValue> values = new Dictionary<string, MValue>()
            {
                { "code", Number((int) code) },
                { "message", List(MList.Empty) },
                { "data", List(MList.Empty) }
            };
            return Composite(values);
        }

        public MValue GetValueByName(string name)
        {
            if (DataValues != null)
            {
                if (DataValues.ContainsKey(name))
                {
                    return DataValues[name];
                }
                return Error(ErrorCodes.KEY_DOES_NOT_EXIST, 
                    "The key \"" + name + "\" does not exist in this data value.", 
                    MList.Empty);
            }
            return Error(ErrorCodes.NOT_COMPOSITE);
        }
    }
}
