using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCommandLine.Functions
{
    public struct MParameters
    {
        private List<MParameter> parameters;

        public int Length
        {
            get
            {
                if (parameters != null)
                {
                    return parameters.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        public MParameters(params MParameter[] parameters)
        {
            this.parameters = new List<MParameter>(parameters);
        }

        public static MParameters Empty = new MParameters(new MParameter[0]);

        public MParameter Get(int index)
        {
            return parameters[index];
        }
        public MParameter Get(string name)
        {
            return parameters.Where((arg) => arg.Name == name).FirstOrDefault();
        }

        public MParameter this[int index]
        {
            get
            {
                return Get(index);
            }
        }
        public MParameter this[string key]
        {
            get
            {
                return Get(key);
            }
        }

        //public static bool operator ==(MParameters p1, MParameters p2)
        //{
        //    if (p1.parameters.Count != p2.parameters.Count)
        //    {
        //        return false;
        //    }
        //    for (int i = 0; i < p1.parameters.Count; i++)
        //    {
        //        if (p1.parameters[i] != p2.parameters[i])
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}
        //public static bool operator !=(MParameters p1, MParameters p2)
        //{
        //    return !(p1 == p2);
        //}
        //public override bool Equals(object obj)
        //{
        //    if (obj is MParameters)
        //    {
        //        MParameters value = (MParameters)obj;
        //        return value == this;
        //    }
        //    return false;
        //}
        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}

        public override string ToString()
        {
            if (parameters == null)
            {
                return "";
            }
            return string.Join(',', parameters);
        }
    }
}
