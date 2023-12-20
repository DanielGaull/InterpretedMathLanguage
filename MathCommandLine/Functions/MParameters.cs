using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.Functions
{
    public class MParameters
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
        public MParameters(List<MParameter> parameters)
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
    }
}
