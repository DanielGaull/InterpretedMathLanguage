using MathCommandLine.CoreDataTypes;
using MathCommandLine.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Structure
{
    // Class for a math data value
    // Can represent a primitive data type or a composite data type
    public struct MValue
    {
        public double NumberValue; // For the number type, a 64-bit floating-pt number
        public MList ListValue; // For the list type
        public MLambda LambdaValue; // For the lambda type
        public decimal BigDecimalValue; // For the big_decimal type
        public long BigIntValue; // For the big_int type
        public MDataType TypeValue; // For the type type (that represents an actual data type)
        public string NameValue; // For the reference type
        public Dictionary<string, MValue> DataValues; // The Data Values for composite types (maps name => value)
        public MDataType DataType;

        public MValue(MDataType dataType, double numberValue, MList listValue, MLambda lambdaValue, decimal bigDecimalValue, 
            long bigIntValue, MDataType typeValue, string nameValue, Dictionary<string, MValue> dataValues)
        {
            DataType = dataType;
            NumberValue = numberValue;
            ListValue = listValue;
            LambdaValue = lambdaValue;
            BigDecimalValue = bigDecimalValue;
            BigIntValue = bigIntValue;
            TypeValue = typeValue;
            DataValues = dataValues;
            NameValue = nameValue;
        }

        public static readonly MValue Empty = new MValue(MDataType.Empty, 0, MList.Empty, MLambda.Empty, 0, 0, 
            MDataType.Empty, null, null);

        public static MValue Number(double numberValue)
        {
            return new MValue(MDataType.Number, numberValue, MList.Empty, MLambda.Empty, 0, 0, MDataType.Empty, null, null);
        }
        public static MValue List(MList list)
        {
            return new MValue(MDataType.List, 0, list, MLambda.Empty, 0, 0, MDataType.Empty, null, null);
        }
        public static MValue Lambda(MLambda lambda)
        {
            return new MValue(MDataType.Lambda, 0, MList.Empty, lambda, 0, 0, MDataType.Empty, null, null);
        }
        public static MValue BigDecimal(decimal bigDecimal)
        {
            return new MValue(MDataType.BigDecimal, 0, MList.Empty, MLambda.Empty, bigDecimal, 0, MDataType.Empty, null, null);
        }
        public static MValue BigInt(long bigInt)
        {
            return new MValue(MDataType.BigInt, 0, MList.Empty, MLambda.Empty, 0, bigInt, MDataType.Empty, null, null);
        }
        public static MValue Type(MDataType type)
        {
            return new MValue(MDataType.Type, 0, MList.Empty, MLambda.Empty, 0, 0, type, null, null);
        }
        public static MValue Reference(string referenceName)
        {
            return new MValue(MDataType.Reference, 0, MList.Empty, MLambda.Empty, 0, 0, MDataType.Empty, referenceName, null);
        }
        public static MValue Void()
        {
            return new MValue(MDataType.Void, 0, MList.Empty, MLambda.Empty, 0, 0, MDataType.Empty, null, null);
        }
        public static MValue Composite(MDataType type, Dictionary<string, MValue> values)
        {
            return new MValue(type, 0, MList.Empty, MLambda.Empty, 0, 0, MDataType.Empty, null, values);
        }

        /// <summary>
        /// If this is a string, return the string value. Otherwise, return null
        /// </summary>
        /// <returns></returns>
        public string GetStringValue()
        {
            if (DataType == MDataType.String)
            {
                return Utilities.MListToString(GetValueByName("chars").ListValue);
            }
            return null;
        }

        // Errors are a core composite type, so they are not primitive but still exist in core code
        public static MValue Error(ErrorCodes code, string message, MList data)
        {
            Dictionary<string, MValue> values = new Dictionary<string, MValue>()
            {
                { "code", Number((int) code) },
                { "message", String(message) },
                { "data", List(data) }
            };
            return Composite(MDataType.Error, values);
        }
        public static MValue Error(ErrorCodes code, string message)
        {
            return Error(code, message, MList.Empty);
        }
        public static MValue Error(ErrorCodes code)
        {
            return Error(code, "", MList.Empty);
        }
        public static MValue String(string value)
        {
            return String(Utilities.StringToMList(value));
        }
        public static MValue String(MList value)
        {
            Dictionary<string, MValue> values = new Dictionary<string, MValue>()
            {
                { "chars", List(value) }
            };
            return Composite(MDataType.String, values);
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

        public override string ToString()
        {
            return ToLongString();
        }
        public string ToShortString()
        {
            if (DataType == MDataType.Number)
            {
                return NumberValue.ToString();
            }
            else if (DataType == MDataType.List)
            {
                return ListValue.ToString();
            }
            else if (DataType == MDataType.Lambda)
            {
                return LambdaValue.ToString();
            }
            else if (DataType == MDataType.BigDecimal)
            {
                return BigDecimalValue.ToString();
            }
            else if (DataType == MDataType.BigInt)
            {
                return BigIntValue.ToString();
            }
            else if (DataType == MDataType.Type)
            {
                return TypeValue.ToString();
            }
            else if (DataType == MDataType.Reference)
            {
                return "&" + NameValue;
            }
            else if (DataType == MDataType.String)
            {
                StringBuilder builder = new StringBuilder("\"");
                builder.Append(Utilities.MListToString(GetValueByName("chars").ListValue));
                builder.Append("\"");
                return builder.ToString();
            }
            else if (DataType == MDataType.Void)
            {
                return "void";
            }
            else if (DataType == MDataType.Error)
            {
                StringBuilder builder = new StringBuilder("Error: #");
                MValue codeValue = GetValueByName("code");
                builder.Append(codeValue.ToShortString());
                builder.Append(" (").Append(((ErrorCodes)codeValue.NumberValue).ToString()).Append(")");
                builder.Append(" '").Append(GetValueByName("message").GetStringValue()).Append("' Data: ");
                builder.Append(GetValueByName("data").ListValue.ToString());
                return builder.ToString();
            }
            else
            {
                // Some sort of composite type
                StringBuilder builder = new StringBuilder("( ");
                bool first = true;
                foreach (KeyValuePair<string, MValue> kv in DataValues)
                {
                    if (!first)
                    {
                        builder.Append(", ");
                        first = false;
                    }
                    builder.Append(kv.Key).Append(": ");
                    builder.Append(kv.Value.ToString());
                }
                builder.Append(" )");
                return builder.ToString();
            }
        }
        public string ToLongString()
        {
            return "(" + DataType.Name + ") " + ToShortString();
        }

        public static bool operator ==(MValue v1, MValue v2)
        {
            // TODO: Handle other number types
            // A big_int(0) should be equal to a number(0)
            if (v1.DataType != v2.DataType)
            {
                return false;
            }
            MDataType dt = v1.DataType;
            if (dt == MDataType.Number)
            {
                return v1.NumberValue == v2.NumberValue;
            }
            else if (dt == MDataType.List)
            {
                return v1.ListValue == v2.ListValue;
            }
            else if (dt == MDataType.Lambda)
            {
                return v1.LambdaValue == v2.LambdaValue;
            }
            else if (dt == MDataType.Type)
            {
                return v1.TypeValue == v2.TypeValue;
            }
            else
            {
                // Composite type
                // Check if fields are equal
                if (v1.DataValues.Count != v2.DataValues.Count)
                {
                    return false;
                }
                foreach (KeyValuePair<string, MValue> kv in v1.DataValues)
                {
                    if (kv.Value != v2.DataValues[kv.Key])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public static bool operator !=(MValue v1, MValue v2)
        {
            return !(v1 == v2);
        }
        public override bool Equals(object obj)
        {
            if (obj is MValue)
            {
                MValue value = (MValue)obj;
                return value == this;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
