using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace LuCompiler
{
    public class Hashtable<T1,T2,V>
    {
        private Hashtable _hashtable;
        public Hashtable()
        {
            _hashtable = new Hashtable();
        }

        public V this[T1 key1,T2 key2]
        {
            get
            {
                object o = _hashtable[key1];
                if (o == null) return default(V);
                else { return (V)(o as Hashtable)[key2]; }
            }
            set
            {
                object o = _hashtable[key1];
                Hashtable ht;
                if (o == null)
                {
                    ht = new Hashtable();
                    ht[key2] = value;
                    _hashtable[key1] = ht;
                }
                else
                {
                    ht = o as Hashtable;
                    ht[key2] = value;
                }
            }
        }

        public Hashtable Table
        {
            get
            {
                return _hashtable;
            }
        }

        public bool ContainsKey(T1 key1,T2 key2)
        {
            if (!_hashtable.ContainsKey(key1)) return false;
            else return (_hashtable[key1] as Hashtable).ContainsKey(key2);            
        }

        public bool ContainsKey1(T1 key1)
        {
            return _hashtable.ContainsKey(key1);
        }
    }
}
