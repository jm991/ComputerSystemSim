using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ComputerSystemSim
{
    /// <summary>
    /// SortedCollection can be used
    /// in WPF applications as the source of the binding.
    /// </summary>
    public class SortedObservableCollection<T> : ObservableCollection<T>
    {
        private readonly Func<T, double> func;

        public SortedObservableCollection(Func<T, double> func)
        {
            this.func = func;
        }

        public SortedObservableCollection(Func<T, double> func, IEnumerable<T> collection)
            : base(collection)
        {
            this.func = func;
        }

        public SortedObservableCollection(Func<T, double> func, List<T> list)
            : base(list)
        {
            this.func = func;
        }

        protected override void InsertItem(int index, T item)
        {
            bool added = false;
            for (int idx = 0; idx < Count; idx++)
            {
                if (func(item) < func(Items[idx]))
                {
                    base.InsertItem(idx, item);
                    added = true;
                    break;
                }
            }

            if (!added)
            {
                base.InsertItem(index, item);
            }
        }
    }
}