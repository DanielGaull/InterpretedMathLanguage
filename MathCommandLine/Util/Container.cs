using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace IML.Util
{
    public class Container<T>
    {
        private T item;
        
        public Container(T item)
        {
            this.item = item;
        }

        public T Get()
        {
            return item;
        }

        public void Set(T item)
        {
            this.item = item;
        }
    }
}
