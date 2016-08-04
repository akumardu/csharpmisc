using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace csharpmisc.Misc
{
    public class MyEnumerable : IEnumerable<int>, IEnumerable
    {
        private List<int> _values;

        public MyEnumerable(int size)
        {
            _values = new List<int>();
            for(int i = 0; i < size; i++)
            {
                _values.Add(i);
            }
        }

        public IEnumerator<int> GetEnumerator()
        {
            return new MyEnumerator(this);
        }

        private IEnumerator GetEnumerator1()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }

        internal int Count
        {
            get
            {
                return (_values.Count);
            }
        }

        internal int this[int index]
        {
            get
            {
                return (_values[index]);
            }
            set
            {
                _values[index] = value;
            }
        }
    }

    public class MyEnumerator : IEnumerator<int>
    {
        private MyEnumerable _value;
        int index;

        int IEnumerator<int>.Current
        {
            get
            {
                Debug.WriteLine("Thread Id: {0}", Thread.CurrentThread.ManagedThreadId);
                return _value[index];
            }
        }

        public object Current
        {
            get
            {
                Debug.WriteLine("Current Thread Id: {0}", Thread.CurrentThread.ManagedThreadId);
                return _value[index];
            }
        }

        public bool MoveNext()
        {
            Debug.WriteLine("Move Next Thread Id: {0}", Thread.CurrentThread.ManagedThreadId);
            if (++index >= _value.Count)
                return (false);
            else
                return (true);
        }

        public void Reset()
        {
            index = -1;
        }

        public void Dispose()
        {
        }

        internal MyEnumerator(MyEnumerable value)
        {
            _value = value;
        }
    }
}
