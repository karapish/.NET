using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prep
{
    public sealed class Element<T>
    {
        private Element<T> next;
        private Element<T> previous;

        public static int num_elements;

        public T Value {get;set;}

        public Element<T> Next
        {
            get
            {
                return this.next;
            }

            set
            {
                this.next = value;
            }
        }

        public Element<T> Previous
        {
            get
            {
                return this.previous;
            }

            set
            {
                this.previous = value;
            }
        }
    }

    public class Queue<T> :
        IEnumerable<T>
    {
        Element<T> back = null; // last added element
        Element<T> front = null; // next element to dequeue

        public Queue<T> Enqueue(T value)
        {
            if(this.back == null)
            {
                // there are no elements in the queue
                // 
                this.back = new Element<T>()
                {
                    Next = null,
                    Value = value,
                    Previous = null,
                };

                this.front = this.back;
            }
            else
            {
                // there are 1+ elements
                // create new element and update the references in next
                var element = new Element<T>
                {
                    Next = this.back,
                    Previous = null,
                    Value = value,
                };

                this.back.Previous = element;
                this.back = element;
            }

            return this;
        } // end Enqueue
       

        // Return front element
        // 
        public T Dequeue()
        {
            if(this.front == null)
            {
                throw new ArgumentNullException("Queue is empty");
            }

            T returnValue = this.front.Value;
            this.front = this.front.Previous;
            return returnValue;
        }

        public Element<T> Find(T value)
        {
            var current = this.back;
            while(current != null)
            {
                if (current.Value.Equals(value))
                    return current;

                current = current.Next;
            }

            return null;
        }

        public T Peek()
        {
            return this.front.Equals(default(T)) ? default(T) : this.front.Value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var current = this.back;
            while (current != null)
            {
                yield return current.Value;
                current = current.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
