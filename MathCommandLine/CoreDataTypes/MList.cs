using MathCommandLine.Evaluation;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.CoreDataTypes
{
    public class MList
    {
        private List<MValue> iList;

        public MList()
        {
            iList = new List<MValue>();
        }
        public MList(List<MValue> list)
        {
            iList = new List<MValue>(list);
        }

        public static MList Empty = new MList();

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

        public static MList FromArray(MValue[] values)
        {
            return new MList(values.ToList());
        }
        public static MList FromOne(MValue value)
        {
            return FromArray(new MValue[] { value });
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
        public static MList Insert(MList list, int index, MValue value)
        {
            List<MValue> newList = new List<MValue>(list.iList);
            newList.Insert(index, value);
            return new MList(newList);
        }
        public static MList Remove(MList list, int index)
        {
            List<MValue> newList = new List<MValue>(list.iList);
            newList.RemoveAt(index);
            return new MList(newList);
        }
        public static int IndexOf(MList list, MValue value)
        {
            return list.iList.IndexOf(value);
        }
        public static int IndexOfCustom(MList list, MValue value, MLambda equalityEvaluator, IInterpreter evaluator)
        {
            for (int i = 0; i < list.iList.Count; i++)
            {
                MArguments args = new MArguments(
                    new MArgument(value), 
                    new MArgument(list.iList[i])
                );
                MValue result = equalityEvaluator.Evaluate(args, evaluator);
                if (result != MValue.Number(0))
                {
                    // Result is true, so return index
                    return i;
                }
            }
            return -1;
        }
        public static MList Map(MList list, MLambda lambda, IInterpreter evaluator)
        {
            List<MValue> newList = new List<MValue>();
            for (int i = 0; i < list.iList.Count; i++)
            {
                MValue result = lambda.Evaluate(
                    new MArguments(
                        new MArgument(list.iList[i]), 
                        new MArgument(MValue.Number(i))
                    ),
                    evaluator
                );
                newList.Add(result);
            }
            return new MList(newList);
        }
        public static MValue Reduce(MList list, MLambda lambda, MValue initial, IInterpreter evaluator)
        {
            MValue runningResult = initial;
            for (int i = 0; i < list.iList.Count; i++)
            {
                runningResult = lambda.Evaluate(
                    new MArguments(
                        new MArgument(runningResult),
                        new MArgument(list.iList[i])
                    ),
                    evaluator
                );
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
            return new MList(newList);
        }
        public static MList Join(MList list1, MList list2)
        {
            IEnumerable<MValue> result = list1.iList.Concat(list2.iList);
            return new MList(result.ToList());
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
