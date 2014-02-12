using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Sqor.Utils.Ios;

namespace Sqor.Utils.Models
{
    public class ModelCollection<T> : ObservableCollection<T>
        where T : Model<T>
    {
        public event Action<T> ItemChanged;
        public event Action<T> ItemSaving;
        
        private IComparer<T> sort;
    
        public ModelCollection(IEnumerable<T> sequence, IComparer<T> sort = null) : base(sequence)
        {
            this.sort = sort;
        }
            
        public ModelCollection(IComparer<T> sort = null)
        {
            this.sort = sort;
        }
        
        public void AddRange(IEnumerable<T> sequence)
        {
            foreach (var o in sequence)
                Add(o);
        }

        protected override void InsertItem(int index, T item)
        {
            if (sort != null)
            {
                var currentItemAtPrecedence = this.FirstOrDefault(x => sort.Compare(x, item) > 0);
                index = currentItemAtPrecedence != null ? IndexOf(currentItemAtPrecedence) : Count;
            }
            base.InsertItem(index, item);
            
            item.PropertyChanged += OnPropertyChanged;
            item.Saving += OnItemSaving;
        }
        
        protected void OnPropertyChanged(T model, IProperty property, object oldValue, object newValue)
        {
            base.OnPropertyChanged(new PropertyChangedEventArgs(property.Name));
        }
        
        protected override void RemoveItem(int index)
        {
            var item = this[index];
            base.RemoveItem(index);
            
            item.PropertyChanged -= OnPropertyChanged;
            item.Saving -= OnItemSaving;
        }

        protected override void ClearItems()
        {
            while (Count > 0)
                RemoveAt(0);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            throw new NotImplementedException();
        }
        
        protected virtual void OnItemChanged(T item)
        {
            ItemChanged.Fire(x => x(item));
        }
        
        protected virtual void OnPropertyChanged(T obj, string propertyName, object oldValue, object newValue)
        {
            OnItemChanged(obj);
        }
        
        protected virtual void OnItemSaving(T obj)
        {
            ItemSaving.Fire(x => x(obj));
        }
    }
}

