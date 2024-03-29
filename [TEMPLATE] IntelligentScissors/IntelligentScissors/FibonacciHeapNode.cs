﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelligentScissors
{
    internal class FibonacciHeapNode<T> : IComparable where T : IComparable
    {
        internal T Value { get; set; }

        internal int Degree;
        internal FibonacciHeapNode<T> ChildrenHead { get; set; }

        internal FibonacciHeapNode<T> Parent { get; set; }
        internal bool LostChild { get; set; }

        internal FibonacciHeapNode<T> Previous;
        internal FibonacciHeapNode<T> Next;

        internal FibonacciHeapNode(T value)
        {
            Value = value;
        }

        public int CompareTo(object obj)
        {
            return Value.CompareTo(((FibonacciHeapNode<T>)obj).Value);
        }
    }
}
