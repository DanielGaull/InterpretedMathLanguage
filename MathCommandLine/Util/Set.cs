using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Util
{
    public class Set<T> where T : IEquatable<T>
    {
        List<T> set;

        public Set()
        {
            set = new List<T>();
        }

        public void Add(T item)
        {
            if (!Contains(item))
            {
                set.Add(item);
            }
        }
        public bool Contains(T item)
        {
            foreach (T i in set)
            {
                IEquatable<T> ie = i;
                if (ie.Equals(item))
                {
                    return true;
                }
            }
            return false;// set.Contains(item);
        }
        public List<T> ToList()
        {
            return set;
        }
    }
}
