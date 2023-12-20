using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Util
{
    public class Set<T>
    {
        List<T> set;

        public Set()
        {
            set = new List<T>();
        }

        public void Add(T item)
        {
            if (!set.Contains(item))
            {
                set.Add(item);
            }
        }
        public bool Contains(T item)
        {
            return set.Contains(item);
        }
        public void Remove(T item)
        {
            set.Remove(item);
        }
        public List<T> ToList()
        {
            return set;
        }
    }
}
