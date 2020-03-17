using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils {
    /// <summary>
    /// Models a buffer that holds a 'history' of the past n items which supports indexing. Useful if the items in a
    /// buffer are constantly shifting.
    /// </summary>
    /// <typeparam name="T">Buffer element type.</typeparam>
    public class HistoryBuffer<T> : IEnumerable<T> {
        public int Length   { get; private set; }
        public int Capacity => buffer.Length;

        private readonly T[] buffer;
        private          int head;

        public HistoryBuffer(int capacity) {
            buffer = new T[capacity];
            head   = -1;
            Length = 0;
        }

        /// <summary>
        /// Inserts an item at the current position and 'shifts' all other items over by 1.
        /// </summary>
        /// <param name="item">Item to insert.</param>
        public void Put(T item) {
            head         = (head + 1) % Capacity;
            buffer[head] = item;
            Length       = Math.Min(Length + 1, Capacity);
        }
        
        public void Put(params T[] items) {
            foreach (var i in items) {
                Put(i);
            }
        }
        
        /// <summary>
        /// Gets an item at index i relative to the current position.
        /// </summary>
        /// <param name="i">Index offset.</param>
        public T this[int i] {
            get {
                if (!(0 <= i && i < Length)) {
                    throw new IndexOutOfRangeException(
                        $"Index was outside the bounds of the buffer. Got {i}, expected 0 <= i < {Length}."
                    );
                }

                return buffer[((head - i) + Capacity) % Capacity];
            }
        }

        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < Length; i++) {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public override string ToString() {
            return $"[{string.Join(", ", buffer.Take(Length))}]";
        }
    }
}