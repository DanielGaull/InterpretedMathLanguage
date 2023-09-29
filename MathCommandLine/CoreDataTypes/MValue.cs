using MathCommandLine.CoreDataTypes;
using MathCommandLine.Environments;
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
        public MClosure ClosureValue; // For the lambda type
        public decimal BigDecimalValue; // For the big_decimal type
        public long BigIntValue; // For the big_int type
        public MDataType TypeValue; // For the type type (that represents an actual data type)
        public MBoxedValue RefValue; // For the reference value
        public bool BoolValue; // For the boolean type
        public Dictionary<string, MField> DataValues; // The Data Values for composite types (maps name => value)
        public MDataType DataType;

        public MValue(MDataType dataType, double numberValue, MList listValue, MClosure closureValue, decimal bigDecimalValue, 
            long bigIntValue, MDataType typeValue, MBoxedValue refValue, bool boolValue, Dictionary<string, MField> dataValues)
        {
            DataType = dataType;
            NumberValue = numberValue;
            ListValue = listValue;
            ClosureValue = closureValue;
            BigDecimalValue = bigDecimalValue;
            BigIntValue = bigIntValue;
            TypeValue = typeValue;
            DataValues = dataValues;
            RefValue = refValue;
            BoolValue = boolValue;
        }

        public static readonly MValue Empty = new MValue(MDataType.Empty, 0, MList.Empty, MClosure.Empty, 0, 0, 
            MDataType.Empty, null, false, null);

        public static MValue Number(double numberValue)
        {
            return new MValue(MDataType.Number, numberValue, MList.Empty, MClosure.Empty, 0, 0, MDataType.Empty, null, 
                false, null);
        }
        public static MValue List(MList list)
        {
            return new MValue(MDataType.List, 0, list, MClosure.Empty, 0, 0, MDataType.Empty, null, false, null);
        }
        public static MValue Closure(MClosure closure)
        {
            return new MValue(MDataType.Closure, 0, MList.Empty, closure, 0, 0, MDataType.Empty, null, false, null);
        }
        public static MValue BigDecimal(decimal bigDecimal)
        {
            return new MValue(MDataType.BigDecimal, 0, MList.Empty, MClosure.Empty, bigDecimal, 0, MDataType.Empty, null, 
                false, null);
        }
        public static MValue BigInt(long bigInt)
        {
            return new MValue(MDataType.BigInt, 0, MList.Empty, MClosure.Empty, 0, bigInt, MDataType.Empty, null, 
                false, null);
        }
        public static MValue Type(MDataType type)
        {
            return new MValue(MDataType.Type, 0, MList.Empty, MClosure.Empty, 0, 0, type, null, false, null);
        }
        public static MValue Reference(MBoxedValue refValue)
        {
            return new MValue(MDataType.Reference, 0, MList.Empty, MClosure.Empty, 0, 0, MDataType.Empty, refValue, 
                false, null);
        }
        public static MValue Bool(bool boolValue)
        {
            return new MValue(MDataType.Boolean, 0, MList.Empty, MClosure.Empty, 0, 0, MDataType.Empty, null,
                boolValue, null);
        }
        public static MValue Void()
        {
            return new MValue(MDataType.Void, 0, MList.Empty, MClosure.Empty, 0, 0, MDataType.Empty, null, 
                false, null);
        }
        public static MValue Null()
        {
            return new MValue(MDataType.Null, 0, MList.Empty, MClosure.Empty, 0, 0, MDataType.Empty, null,
                false, null);
        }
        public static MValue Composite(MDataType type, Dictionary<string, MField> values)
        {
            return new MValue(type, 0, MList.Empty, MClosure.Empty, 0, 0, MDataType.Empty, null, false, values);
        }

        /// <summary>
        /// If this is a string, return the string value. Otherwise, return null
        /// </summary>
        /// <returns></returns>
        public string GetStringValue()
        {
            if (DataType == MDataType.String)
            {
                return Utilities.MListToString(GetValueByName("chars", true).ListValue);
            }
            return null;
        }

        // Errors are a core composite type, so they are not primitive but still exist in core code
        public static MValue Error(ErrorCodes code, string message, MList data)
        {
            Dictionary<string, MField> values = new Dictionary<string, MField>()
            {
                { "code", new MField(Number((int) code), MField.PUBLIC, MField.PRIVATE) },
                { "message", new MField(String(message), MField.PUBLIC, MField.PRIVATE) },
                { "data", new MField(List(data), MField.PUBLIC, MField.PRIVATE) }
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
            Dictionary<string, MField> values = new Dictionary<string, MField>()
            {
                { "chars", new MField(List(value), MField.PUBLIC, MField.PRIVATE) }
            };
            return Composite(MDataType.String, values);
        }

        public MValue GetValueByName(string name, bool isSelfAccessing)
        {
            if (DataValues != null)
            {
                if (DataValues.ContainsKey(name))
                {
                    MField field = DataValues[name];
                    if (!isSelfAccessing && field.ReadPermission != MField.PUBLIC)
                    {
                        return Error(ErrorCodes.KEY_DOES_NOT_EXIST,
                            "The key \"" + name + "\" does not exist in this data value, or the field is not accessible.",
                            MList.Empty);
                    }
                    return field.Value;
                }
                return Error(ErrorCodes.KEY_DOES_NOT_EXIST, 
                    "The key \"" + name + "\" does not exist in this data value, or the field is not accessible.", 
                    MList.Empty);
            }
            return Error(ErrorCodes.NOT_COMPOSITE);
        }
        public MValue SetValueByName(string name, MValue value, bool isSelfAccessing)
        {
            if (DataValues != null)
            {
                if (DataValues.ContainsKey(name))
                {
                    MField field = DataValues[name];
                    if (!isSelfAccessing && field.WritePermission != MField.PUBLIC)
                    {
                        return Error(ErrorCodes.KEY_DOES_NOT_EXIST,
                            "The key \"" + name + "\" does not exist in this data value, or the field is not accessible.",
                            MList.Empty);
                    }
                    field.SetValue(value);
                    return value;
                }
                return Error(ErrorCodes.KEY_DOES_NOT_EXIST,
                    "The key \"" + name + "\" does not exist in this data value, or the field is not accessible.",
                    MList.Empty);
            }
            return Error(ErrorCodes.NOT_COMPOSITE);
        }

        public bool IsTruthy()
        {
            return !(DataType == MDataType.Boolean && !BoolValue) &&
                DataType != MDataType.Null &&
                DataType != MDataType.Void;
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
            else if (DataType == MDataType.Closure)
            {
                return ClosureValue.ToString();
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
                return "<ref -> " + RefValue.ToString() + ">";
            }
            else if (DataType == MDataType.String)
            {
                StringBuilder builder = new StringBuilder("\"");
                builder.Append(Utilities.MListToString(GetValueByName("chars", true).ListValue));
                builder.Append("\"");
                return builder.ToString();
            }
            else if (DataType == MDataType.Void)
            {
                return "void";
            }
            else if (DataType == MDataType.Boolean)
            {
                if (BoolValue)
                {
                    return "TRUE";
                }
                else
                {
                    return "FALSE";
                }
            }
            else if (DataType == MDataType.Null)
            {
                return "null";
            }
            else if (DataType == MDataType.Error)
            {
                StringBuilder builder = new StringBuilder("Error: #");
                MValue codeValue = GetValueByName("code", true);
                builder.Append(codeValue.ToShortString());
                builder.Append(" (").Append(((ErrorCodes)codeValue.NumberValue).ToString()).Append(")");
                builder.Append(" '").Append(GetValueByName("message", true).GetStringValue()).Append("' Data: ");
                builder.Append(GetValueByName("data", true).ListValue.ToString());
                return builder.ToString();
            }
            else
            {
                // Some sort of composite type
                StringBuilder builder = new StringBuilder("( ");
                bool first = true;
                foreach (KeyValuePair<string, MField> kv in DataValues)
                {
                    if (kv.Value.ReadPermission != MField.PUBLIC)
                    {
                        continue;
                    }
                    if (!first)
                    {
                        builder.Append(", ");
                    }
                    first = false;
                    builder.Append(kv.Key).Append(": ");
                    builder.Append(kv.Value.Value.ToString());
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
            else if (dt == MDataType.Closure)
            {
                return v1.ClosureValue == v2.ClosureValue;
            }
            else if (dt == MDataType.Type)
            {
                return v1.TypeValue == v2.TypeValue;
            }
            else if (dt == MDataType.Boolean)
            {
                return v1.BoolValue == v2.BoolValue;
            }
            else if (dt == MDataType.Reference)
            {
                return v1.RefValue == v2.RefValue;
            }
            else
            {
                // Composite type
                // Check if fields are equal
                if (v1.DataValues.Count != v2.DataValues.Count)
                {
                    return false;
                }
                foreach (KeyValuePair<string, MField> kv in v1.DataValues)
                {
                    if (kv.Value.Value != v2.DataValues[kv.Key].Value)
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
