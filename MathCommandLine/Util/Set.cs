using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace IML.Util
{
    public class Set<T> : IEnumerable<T> where T : IEquatable<T>
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

        public IEnumerator<T> GetEnumerator()
        {
            return new SetIterator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SetIterator<T>(this);
        }

        public List<T> ToList()
        {
            return set;
        }
    }

    class SetIterator<T> : IEnumerator<T> where T : IEquatable<T>
    {
        private List<T> setList;
        private int index;

        public SetIterator(Set<T> set)
        {
            setList = set.ToList();
            index = 0;
        }

        public T Current
        {
            get
            {
                return setList[index];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return setList[index];
            }
        }

        public void Dispose()
        {
            // Don't need to dispose of the list, so nothing to do here
        }

        public bool MoveNext()
        {
            if (index + 1 < setList.Count)
            {
                index++;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            index = 0;
        }
    }
}
