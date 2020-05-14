using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace MVsDotNetAMSIClient.Logging
{
    internal class HorizonedBuffer : IEnumerable<int>
    {
        readonly int[] buffer;
        readonly int capacity;

        int writeCursor;

        internal HorizonedBuffer(int capacity, int initialSize)
        {
            buffer = new int[capacity];
            this.capacity = capacity;
            buffer[0] = initialSize;
        }

        internal void Increase()
            => buffer[writeCursor % capacity]++;
        
        internal void Move()
        {
            unchecked
            {
                buffer[++writeCursor % capacity] = 0;
            }
        }
        internal void Move(int steps)
        {
            for (var i = 0; i < steps; i++)
                Move();
        }

        public IEnumerator<int> GetEnumerator()
            => Enumerable
               .Range(0, capacity)
                .Select(i => buffer[(writeCursor + capacity - i) % capacity])
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
