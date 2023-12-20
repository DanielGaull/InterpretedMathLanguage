using IML.CoreDataTypes;
using IML.Environments;
using IML.Functions;
using IML.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Structure
{
    // Class for a math data value
    // Can represent a primitive data type or a composite data type
    public class MValue
    {
        public double NumberValue; // For the number type, a 64-bit floating-pt number
        public MList ListValue; // For the list type
        public MClosure ClosureValue; // For the lambda type
        public decimal BigDecimalValue; // For the big_decimal type
        public long BigIntValue; // For the big_int type
        public MType TypeValue; // For the type type (that represents an actual data type)
        public MBoxedValue RefValue; // For the reference value
        public bool BoolValue; // For the boolean type
        public Dictionary<string, MField> DataValues; // The Data Values for composite types (maps name => value)
        public MConcreteDataTypeEntry DataType;

        public MValue(MConcreteDataTypeEntry dataType, double numberValue, MList listValue, MClosure closureValue, decimal bigDecimalValue, 
            long bigIntValue, MType typeValue, MBoxedValue refValue, bool boolValue, Dictionary<string, MField> dataValues)
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

        public static MValue Number(double numberValue)
        {
            return new MValue(MDataTypeEntry.Number, numberValue, MList.Empty, MClosure.Empty, 0, 0, null, null, 
                false, new Dictionary<string, MField>());
        }
        public static MValue List(MList list)
        {
            return new MValue(MDataTypeEntry.List(list.Type), 0, list, MClosure.Empty, 0, 0, null, null, false,
                ListProperties(list));
        }
        public static MValue Closure(MClosure closure)
        {
            return new MValue(MDataTypeEntry.Function, 0, MList.Empty, closure, 0, 0, null, null, false,
                new Dictionary<string, MField>());
        }
        public static MValue Type(MType type)
        {
            return new MValue(MDataTypeEntry.Type, 0, MList.Empty, MClosure.Empty, 0, 0, type, null, false,
                new Dictionary<string, MField>());
        }
        public static MValue Reference(MBoxedValue refValue)
        {
            MValue v = refValue.GetValue();
            MType type = new MType(v.DataType);
            return new MValue(MDataTypeEntry.Reference(type), 0, MList.Empty, MClosure.Empty, 0, 0, null, refValue, 
                false, new Dictionary<string, MField>());
        }
        public static MValue Bool(bool boolValue)
        {
            return new MValue(MDataTypeEntry.Boolean, 0, MList.Empty, MClosure.Empty, 0, 0, null, null,
                boolValue, new Dictionary<string, MField>());
        }
        public static MValue Void()
        {
            return new MValue(MDataTypeEntry.Void, 0, MList.Empty, MClosure.Empty, 0, 0, null, null, 
                false, null);
        }
        public static MValue Null()
        {
            return new MValue(MDataTypeEntry.Null, 0, MList.Empty, MClosure.Empty, 0, 0, null, null,
                false, null);
        }
        public static MValue Composite(MConcreteDataTypeEntry type, Dictionary<string, MField> values)
        {
            return new MValue(type, 0, MList.Empty, MClosure.Empty, 0, 0, null, null, false, values);
        }

        /// <summary>
        /// If this is a string, return the string value. Otherwise, return null
        /// </summary>
        /// <returns></returns>
        public string GetStringValue()
        {
            if (DataType.DataType.MatchesTypeExactly(MDataType.String))
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
            return Composite(MDataTypeEntry.Error, values);
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
            return Composite(MDataTypeEntry.String, values);
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
                        if (DataType == MDataTypeEntry.Error)
                        {
                            return this;
                        }
                        return Error(ErrorCodes.KEY_DOES_NOT_EXIST,
                            "The field \"" + name + "\" is not accessible on this data value.",
                            MList.Empty);
                    }
                    return field.Value;
                }
                if (DataType == MDataTypeEntry.Error)
                {
                    return this;
                }
                return Error(ErrorCodes.KEY_DOES_NOT_EXIST, 
                    "The key \"" + name + "\" does not exist in this data value.", 
                    MList.Empty);
            }
            return Error(ErrorCodes.NO_PROPERTIES, $"Cannot read properties of \"{ToShortString()}\"");
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
                        if (DataType == MDataTypeEntry.Error)
                        {
                            return this;
                        }
                        return Error(ErrorCodes.KEY_DOES_NOT_EXIST,
                            "The field \"" + name + "\" is not modifiable on this data value.",
                            MList.Empty);
                    }
                    field.SetValue(value);
                    return value;
                }
                if (DataType == MDataTypeEntry.Error)
                {
                    return this;
                }
                return Error(ErrorCodes.KEY_DOES_NOT_EXIST,
                    "The key \"" + name + "\" does not exist in this data value.",
                    MList.Empty);
            }
            return Error(ErrorCodes.NO_PROPERTIES, $"Cannot read properties of \"{ToShortString()}\"");
        }

        public bool IsTruthy()
        {
            return !(DataType == MDataTypeEntry.Boolean && !BoolValue) &&
                DataType != MDataTypeEntry.Null &&
                DataType != MDataTypeEntry.Void;
        }

        public override string ToString()
        {
            return ToLongString();
        }
        public string ToShortString()
        {
            if (DataType.DataType.MatchesTypeExactly(MDataType.Number))
            {
                return NumberValue.ToString();
            }
            else if (DataType.DataType.MatchesTypeExactly(MDataType.List))
            {
                return ListValue.ToString();
            }
            else if (DataType.DataType.MatchesTypeExactly(MDataType.Function))
            {
                return ClosureValue.ToString();
            }
            else if (DataType.DataType.MatchesTypeExactly(MDataType.Type))
            {
                return TypeValue.ToString();
            }
            else if (DataType.DataType.MatchesTypeExactly(MDataType.Reference))
            {
                return "<ref -> " + RefValue.ToString() + ">";
            }
            else if (DataType.DataType.MatchesTypeExactly(MDataType.String))
            {
                StringBuilder builder = new StringBuilder("\"");
                builder.Append(Utilities.MListToString(GetValueByName("chars", true).ListValue));
                builder.Append("\"");
                return builder.ToString();
            }
            else if (DataType.DataType.MatchesTypeExactly(MDataType.Void))
            {
                return "void";
            }
            else if (DataType.DataType.MatchesTypeExactly(MDataType.Boolean))
            {
                if (BoolValue)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            else if (DataType.DataType.MatchesTypeExactly(MDataType.Null))
            {
                return "null";
            }
            else if (DataType.DataType.MatchesTypeExactly(MDataType.Error))
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
            return "(" + DataType.DataType.Name + ") " + ToShortString();
        }

        public static bool operator ==(MValue v1, MValue v2)
        {
            // A big_int(0) should be equal to a number(0)
            if (v1.DataType != v2.DataType)
            {
                return false;
            }
            MDataType dt = v1.DataType.DataType;
            if (dt.MatchesTypeExactly(MDataType.Number))
            {
                return v1.NumberValue == v2.NumberValue;
            }
            else if (dt.MatchesTypeExactly(MDataType.List))
            {
                return v1.ListValue == v2.ListValue;
            }
            else if (dt.MatchesTypeExactly(MDataType.Function))
            {
                return v1.ClosureValue == v2.ClosureValue;
            }
            else if (dt.MatchesTypeExactly(MDataType.Type))
            {
                return v1.TypeValue == v2.TypeValue;
            }
            else if (dt.MatchesTypeExactly(MDataType.Boolean))
            {
                return v1.BoolValue == v2.BoolValue;
            }
            else if (dt.MatchesTypeExactly(MDataType.Reference))
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

        private static Dictionary<string, MField> ListProperties(MList list)
        {
            return new Dictionary<string, MField>()
            {
                {
                    "get",
                    new MField(Closure(new MClosure(
                        MDataTypeEntry.Function(list.Type, new List<MType>()
                        {
                            MType.Number
                        }, true, false), 
                        new List<string>(){"index"},
                        MEnvironment.Empty,
                        (args, env, interpreter) => {
                            int index = (int) args[0].Value.NumberValue;
                            int len = MList.Length(list);
                            if (index >= len || index < 0)
                            {
                                return Error(ErrorCodes.I_OUT_OF_RANGE, $"Index {index} out of range for list of length {len}.");
                            }
                            return MList.Get(list, index);
                        })), 1, 0)
                },
                {
                    "index",
                    new MField(Closure(new MClosure(
                        MDataTypeEntry.Function(MType.Number, new List<MType>()
                        {
                            list.Type
                        }, true, false),
                        new List<string>(){"value"},
                        MEnvironment.Empty,
                        (args, env, interpreter) => {
                            MValue value = args[0].Value;
                            int index = MList.IndexOf(list, value);
                            return Number(index);
                        })), 1, 0)
                },
                {
                    "indexc",
                    new MField(Closure(new MClosure(
                        MDataTypeEntry.Function(MType.Number, new List<MType>()
                        {
                            list.Type,
                            MType.Function(MType.Any, list.Type, list.Type)
                        }, true, false),
                        new List<string>(){"element", "equality_evaluator"},
                        MEnvironment.Empty,
                        (args, env, interpreter) => {
                            return Number(MList.IndexOfCustom(list, args.Get(0).Value,
                                args.Get(1).Value.ClosureValue, interpreter, env));
                        })), 1, 0)
                },
                {
                    "length",
                    new MField(Closure(new MClosure(
                        MDataTypeEntry.Function(MType.Number, new List<MType>(), true, false),
                        new List<string>(),
                        MEnvironment.Empty,
                        (args, env, interpreter) => {
                            int len = MList.Length(list);
                            return Number(len);
                        })), 1, 0)
                },
                {
                    "map", // We need a generic here for the return type of the mapper
                    new MField(Closure(new MClosure(
                        MDataTypeEntry.Function(MType.List(new MType(new MGenericDataTypeEntry("T"))),
                            new List<MType>()
                            {
                                MType.Function(new MType(new MGenericDataTypeEntry("T")), list.Type)
                            }, 
                            new List<string>()
                            {
                                "T"
                            },
                            true, false),
                        new List<string>(){"mapper_func"},
                        MEnvironment.Empty,
                        (args, env, interpreter) =>
                        {
                            return List(MList.Map(list,
                                args.Get(0).Value.ClosureValue, interpreter, env));
                        })), 1, 0)
                },
                {
                    "reduce", // Need a generic here for return type of the reducer
                    new MField(Closure(new MClosure(
                        MDataTypeEntry.Function(new MType(new MGenericDataTypeEntry("T")),
                            new List<MType>()
                            {
                                // Returns a T, takes in a T & list value
                                MType.Function(new MType(new MGenericDataTypeEntry("T")), 
                                    new MType(new MGenericDataTypeEntry("T")), list.Type),
                                new MType(new MGenericDataTypeEntry("T"))
                            },
                            new List<string>() // Generics
                            {
                                "T"
                            },
                            true, false),
                        new List<string>(){"reducer_func", "init_value"},
                        MEnvironment.Empty,
                        (args, env, interpreter) =>
                        {
                            return MList.Reduce(list, args.Get(0).Value.ClosureValue,
                                args.Get(1).Value, interpreter, env);
                        })), 1, 0)
                },
                {
                    "add",
                    new MField(Closure(new MClosure(
                        MDataTypeEntry.Function(MType.Void,
                            new List<MType>()
                            {
                                list.Type
                            },
                            true, false),
                        new List<string>(){"item"},
                        MEnvironment.Empty,
                        (args, env, interpreter) =>
                        {
                            list.InternalList.Add(args[0].Value);
                            return Void();
                        })), 1, 0)
                },
                {
                    "insert",
                    new MField(Closure(new MClosure(
                        MDataTypeEntry.Function(MType.Void,
                            new List<MType>()
                            {
                                list.Type,
                                MType.Number
                            },
                            true, false),
                        new List<string>(){"item", "index"},
                        MEnvironment.Empty,
                        (args, env, interpreter) =>
                        {
                            int index = (int) args[1].Value.NumberValue;
                            int len = MList.Length(list);
                            if (index >= len || index < 0)
                            {
                                return Error(ErrorCodes.I_OUT_OF_RANGE, $"Index {index} out of range for list of length {len}.");
                            }
                            list.InternalList.Insert(index, args[0].Value);
                            return Void();
                        })), 1, 0)
                },
                {
                    "removeAt",
                    new MField(Closure(new MClosure(
                         MDataTypeEntry.Function(MType.Void,
                            new List<MType>()
                            {
                                MType.Number
                            },
                            true, false),
                        new List<string>(){"index"},
                        MEnvironment.Empty,
                        (args, env, interpreter) =>
                        {
                            int index = (int) args[0].Value.NumberValue;
                            int len = MList.Length(list);
                            if (index >= len || index < 0)
                            {
                                return Error(ErrorCodes.I_OUT_OF_RANGE, $"Index {index} out of range for list of length {len}.");
                            }
                            list.InternalList.RemoveAt(index);
                            return Void();
                        })), 1, 0)
                },
                {
                    "remove",
                    new MField(Closure(new MClosure(
                         MDataTypeEntry.Function(MType.Boolean,
                            new List<MType>()
                            {
                                list.Type
                            },
                            true, false),
                        new List<string>(){"item"},
                        MEnvironment.Empty,
                        (args, env, interpreter) =>
                        {
                            int index = MList.IndexOf(list, args[0].Value);
                            if (index < 0)
                            {
                                return Bool(false);
                            }
                            list.InternalList.RemoveAt(index);
                            return Bool(true);
                        })), 1, 0)
                },
                {
                    "removec",
                    new MField(Closure(new MClosure(
                         MDataTypeEntry.Function(MType.Boolean,
                            new List<MType>()
                            {
                                list.Type,
                                MType.Function(MType.Any, list.Type, list.Type)
                            },
                            true, false),
                        new List<string>(){"item", "equality_evaluator"},
                        MEnvironment.Empty,
                        (args, env, interpreter) =>
                        {
                            int index = MList.IndexOfCustom(list, args.Get(0).Value,
                                args.Get(1).Value.ClosureValue, interpreter, env);
                            if (index < 0)
                            {
                                return Bool(false);
                            }
                            list.InternalList.RemoveAt(index);
                            return Bool(true);
                        })), 1, 0)
                },
                {
                    "addAll",
                    new MField(Closure(new MClosure(
                        MDataTypeEntry.Function(MType.Void,
                            new List<MType>()
                            {
                                MType.List(list.Type)
                            },
                            true, false),
                        new List<string>(){"other"},
                        MEnvironment.Empty,
                        (args, env, interpreter) =>
                        {
                            MList other = args[0].Value.ListValue;
                            list.InternalList.AddRange(other.InternalList);
                            return Void();
                        })), 1, 0)
                }
            };
        }
    }
}
