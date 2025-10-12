using System.Collections;
using System.Collections.Generic;

namespace STROOP.Utilities
{
    public class SortedList<T> : IEnumerable<T>
    {
        class Node
        {
            public readonly T content;
            public Node next, previous;

            public Node(T content)
            {
                this.content = content;
            }
        }

        public delegate int CompareValues(T a, T b);

        Node start, end;
        int count = 0;

        public int Count => count;
        public bool Empty => start == null;

        CompareValues compareValues;

        public SortedList(CompareValues compareValues)
        {
            this.compareValues = compareValues;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            Node current = start;
            while (current != null)
            {
                yield return current.content;
                current = current.next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

        public void Add(T value)
        {
            if (Empty)
                start = end = new Node(value);
            else
            {
                var current = start;
                Node lag = null;
                var newNode = new Node(value);
                while (current != null)
                {
                    if (compareValues(value, current.content) < 0)
                    {
                        current.previous = newNode;
                        newNode.next = current;
                        break;
                    }

                    lag = current;
                    current = current.next;
                }

                if (current == null)
                    lag.next = end = newNode;
                if (lag != null)
                    lag.next = newNode;
                else
                    start = newNode;
            }

            count++;
        }

        public void Remove(T value, bool all = false)
        {
            var current = start;
            Node lag = null;
            while (current != null)
            {
                if (value.Equals(current.content))
                {
                    if (lag != null)
                    {
                        lag.next = current.next;
                        if (current.next != null)
                            current.next.previous = lag;
                        else
                            end = current.previous;
                    }
                    else
                        start = current.next;

                    count--;
                    if (!all)
                        return;
                }

                lag = current;
                current = current.next;
            }
        }

        public void Clear()
        {
            start = end = null;
            count = 0;
        }
    }
}
