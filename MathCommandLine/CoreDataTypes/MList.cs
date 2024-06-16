using IML.Environments;
using IML.Evaluation;
using IML.Exceptions;
using IML.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.CoreDataTypes
{
    public class MList
    {
        private List<MValue> iList;
        private MType type;
        public MType Type 
        { 
            get
            {
                return type;
            } 
        }

        public MList(MType of)
        {
            iList = new List<MValue>();
            type = of;
        }
        public MList(List<MValue> list, MType of)
        {
            iList = new List<MValue>(list);
            type = of;
        }

        public static MList Empty = new MList(MType.Any());

        public List<MValue> InternalList
        {
            get
            {
                return iList;
            }
        }

        public List<MValue> GetInternalList()
        {
            return iList;
        }

        public static MList FromArray(MValue[] values, MType type)
        {
            return new MList(values.ToList(), type);
        }
        public static MList FromOne(MValue value)
        {
            return FromArray(new MValue[] { value }, new MType(value.DataType));
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("{ ");
            for (int i = 0; i < iList.Count; i++)
            {
                builder.Append(iList[i].ToShortString());
                if (i + 1 < iList.Count)
                {
                    builder.Append(", ");
                }
            }
            builder.Append(" }");
            return builder.ToString();
        }

        public static int Length(MList list)
        {
            return list.iList.Count;
        }
        public static MValue Get(MList list, int index)
        {
            return list.iList[index];
        }
        public static int IndexOf(MList list, MValue value)
        {
            return list.iList.IndexOf(value);
        }
        public static int IndexOfCustom(MList list, MValue value, MFunction equalityEvaluator, IInterpreter evaluator,
            MEnvironment env)
        {
            for (int i = 0; i < list.iList.Count; i++)
            {
                MArguments args = new MArguments(
                    new MArgument(value), 
                    new MArgument(list.iList[i])
                );
                ValueOrReturn vr = evaluator.PerformCall(equalityEvaluator, args, env, new List<MType>());
                if (vr.IsReturn)
                {
                    throw new FatalRuntimeException("Should never get a return from value or return in List IndexOfCustom. " +
                        "Requires function that creates its own environment.");
                }
                MValue result = vr.Value;
                if (result.IsTruthy())
                {
                    // Result is true, so return index
                    return i;
                }
            }
            return -1;
        }
        public static MList Map(MList list, MFunction function, IInterpreter evaluator, MEnvironment env)
        {
            List<MValue> newList = new List<MValue>();
            for (int i = 0; i < list.iList.Count; i++)
            {
                ValueOrReturn vr = evaluator.PerformCall(function, 
                    new MArguments(new MArgument(list.iList[i])), env, new List<MType>());
                if (vr.IsReturn)
                {
                    throw new FatalRuntimeException("Should never get a return from value or return in List Map. " +
                        "Requires function that creates its own environment.");
                }
                newList.Add(vr.Value);
            }
            return new MList(newList, function.ReturnType);
        }
        public static MValue Reduce(MList list, MFunction function, MValue initial, IInterpreter evaluator,
            MEnvironment env)
        {
            MValue runningResult = initial;
            for (int i = 0; i < list.iList.Count; i++)
            {
                ValueOrReturn vr = evaluator.PerformCall(function,
                    new MArguments(new MArgument(runningResult), new MArgument(list.iList[i])), 
                    env, new List<MType>());
                if (vr.IsReturn)
                {
                    throw new FatalRuntimeException("Should never get a return from value or return in List Reduce. " +
                        "Requires function that creates its own environment.");
                }
                runningResult = vr.Value;
            }
            return runningResult;
        }
        public static MList CreateRange(int max)
        {
            if (max <= 0)
            {
                return Empty;
            }
            List<MValue> newList = new List<MValue>();
            for (int i = 0; i < max; i++)
            {
                newList.Add(MValue.Number(i));
            }
            return new MList(newList, new MType(MDataTypeEntry.Number));
        }
        public static MList Concat(MList list1, MList list2)
        {
            IEnumerable<MValue> result = list1.iList.Concat(list2.iList);
            
            return new MList(result.ToList(), list1.type.Union(list2.Type));
        }

        public static bool operator ==(MList l1, MList l2)
        {
            if (l1.InternalList.Count != l2.InternalList.Count)
            {
                return false;
            }
            for (int i = 0; i < l1.InternalList.Count; i++)
            {
                if (l1.InternalList[i] != l2.InternalList[i])
                {
                    return false;
                }
            }
            return true;
        }
        public static bool operator !=(MList l1, MList l2)
        {
            return !(l1 == l2);
        }
        public override bool Equals(object obj)
        {
            if (obj is MList)
            {
                MList list = (MList)obj;
                return list == this;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
