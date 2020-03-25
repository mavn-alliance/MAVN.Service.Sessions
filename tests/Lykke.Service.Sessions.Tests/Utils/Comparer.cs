using System;
using System.Collections.Generic;

namespace Lykke.Service.Sessions.Tests.Utils
{
    internal class Comparer
    {
        public static Comparer<U> Get<U>(Func<U, U, bool> func)
        {
            return new Comparer<U>(func);
        }
    }

    internal class Comparer<T> : Comparer, IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _comparisonFunc;

        public Comparer(Func<T, T, bool> func)
        {
            _comparisonFunc = func;
        }

        public bool Equals(T x, T y)
        {
            return _comparisonFunc(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}