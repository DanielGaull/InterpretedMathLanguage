using MathCommandLine.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.CoreDataTypes
{
    public struct MList
    {
        private List<MValue> list;

        private MList(List<MValue> list)
        {
            this.list = list;
        }

        public static MList Empty = new MList();

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
            for (int i = 0; i < list.Count; i++)
            {
                builder.Append(list[i].ToString());
                if (i + 1 < list.Count)
                {
                    builder.Append(", ");
                }
            }
            builder.Append(" }");
            return builder.ToString();
        }
    }
}
