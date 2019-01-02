using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace LuCompiler
{
    public class Hashtable<T, V>
    {
        public Hashtable()
        {
            _hashtable = new Hashtable();
        }

        private Hashtable _hashtable;

        public Hashtable Table
        {
            get
            {
                return _hashtable;
            }
            set
            {
                _hashtable = value;
            }
        }

        public V this[T key] { 
            get{
                return (V)_hashtable[key];
            }
            set
            {
                _hashtable[key] = value;
            }
        }

        public ICollection Keys
        {
            get { return _hashtable.Keys; }
        }
        public ICollection Values
        {
            get { return _hashtable.Values; }
        }
        public void Remove(T key)
        {
            _hashtable.Remove(key);            
        }
        public bool ContainsKey(T key)
        {            
            return _hashtable.ContainsKey(key);
        }
        public bool ContainsValue(V value)
        {
            return _hashtable.ContainsValue(value);
        }
    }
}
