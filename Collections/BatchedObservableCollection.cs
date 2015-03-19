using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Linq;

namespace Sqor.Utils.Collections
{
    public class BatchedObservableCollection<T> : ObservableCollection<T>
    {
        bool isNotificationSuspended = false;

        public BatchedObservableCollection ()
        {
        }
        public BatchedObservableCollection (IEnumerable<T> collection) : base (collection)
        {
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!isNotificationSuspended)
                base.OnCollectionChanged(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!isNotificationSuspended)
                base.OnPropertyChanged(e);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            PauseChangeEvents ();

            try
            {
                IList<T> items = base.Items;

                if (items != null)
                {
                    using (IEnumerator<T> enumerator = collection.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            items.Add(enumerator.Current);
                        }
                    }
                }
            }
            finally
            {
                ResumeChangedEvents ();
            }
        }

        public void RemoveRange(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            PauseChangeEvents ();

            try
            {
                IList<T> items = base.Items;
                if (items != null)
                {
                    using (IEnumerator<T> enumerator = collection.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            items.Remove(enumerator.Current);
                        }
                    }
                }
            }
            finally
            {
                ResumeChangedEvents ();
            }
        }

        public void PauseChangeEvents()
        {
            isNotificationSuspended = true;
        }

        public void ResumeChangedEvents(NotifyCollectionChangedAction typeOfChange = NotifyCollectionChangedAction.Reset)
        {
            isNotificationSuspended = false;
            OnPropertyChanged (new PropertyChangedEventArgs ("Count"));
            OnPropertyChanged (new PropertyChangedEventArgs ("Item[]"));
            OnCollectionChanged (new NotifyCollectionChangedEventArgs (typeOfChange));
        }

        public List<T> GetRange(int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((this.Count - index) < count)
            {
                throw new ArgumentException("Invalid Offset Length");
            }

            return new List<T>(this.Items.Skip(index).Take(count));
        }

    }
}

