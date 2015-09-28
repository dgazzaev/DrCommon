﻿/*
  DrLinkedDictionary.cs -- linked dictionary 1.0.0, September 26, 2015
 
  Copyright (c) 2013-2015 Kudryashov Andrey aka Dr
                          Kirillov Vasiliy
 
  This software is provided 'as-is', without any express or implied
  warranty. In no event will the authors be held liable for any damages
  arising from the use of this software.
  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:
      1. The origin of this software must not be misrepresented; you must not
      claim that you wrote the original software. If you use this software
      in a product, an acknowledgment in the product documentation would be
      appreciated but is not required.
      2. Altered source versions must be plainly marked as such, and must not be
      misrepresented as being the original software.
      3. This notice may not be removed or altered from any source distribution.
 
      Kudryashov Andrey <kudryashov.andrey at gmail.com>
      Kirillov Vasiliy  <vskirillov.spb at gmail.com>
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DrLinkedDictionary
{
   
    public class DrLinkedDictonary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
    {
        [DebuggerDisplay("Previous = {Previous} Next = {Next} Key = {Key} Value = {Value}")]
        public class DrLinkedValue
        {

            public DrLinkedValue(TKey key, TValue value)
                : this(key, value, null, null)
            { }

            public DrLinkedValue(TKey key, TValue value, DrLinkedValue previous, DrLinkedValue next)
            {
                this.Key = key;
                this.Value = value;
                this.Previous = previous;
                this.Next = next;
            }
            public TValue Value { get; set; }
            public TKey Key { get; set; }
            public DrLinkedValue Previous { get; set; }
            public DrLinkedValue Next { get; set; }

            public bool IsFirst { get { return (this.Previous == null); } }
            public bool IsLast { get { return (this.Next == null); } }

            public static implicit operator KeyValuePair<TKey, TValue>(DrLinkedValue value)
            {
                return new KeyValuePair<TKey, TValue>(value.Key, value.Value);
            }

        }

        /// <summary>
        /// Rules of the enumeration items of the dictionary
        /// </summary>
         public sealed class DrEnumerationRules
         {
             /// <summary>
             /// Direction of the enumeration items of the dictionary
             /// </summary>
             public enum EDirection
             {
                 FORWARD,
                 BACKWARD
             }
             /// <summary>
             /// The key of item with which to begin enumeration
             /// </summary>
             private TKey startFromKey;
             /// <summary>
             /// The key of item with which to begin enumeration
             /// </summary>
             public TKey StartFromKey
             {
                 get { return this.startFromKey; }
                 set 
                 { 
                     this.IsFirstItemRelative = false; 
                     this.startFromKey = value; 
                 }
             }
             /// <summary>
             /// Return true if enumeration will be started for each item of the dictionary from the first or the last item depends from specified direction.
             /// Otherwise, returns false if enumeration will be started for each item of the dictionary from specified key of item <paramref name="StartFromKey"/> and direction.
             /// </summary>
             public bool IsFirstItemRelative { get; private set; }
             /// <summary>
             /// Direction of the enumeration items of the dictionary
             /// </summary>
             public EDirection Direction { get; set; }

             public DrEnumerationRules()
             {
                 this.Reset();
             }
             /// <summary>
             /// Reset rules to default. Set <paramref name="StartFromKey"/> to default <paramref name="TKey"/>, 
             /// set <paramref name="IsFirstItemRelative"/> to true and <paramref name="Direction"/> will be set to FORWARD 
             /// </summary>
             public void Reset()
             {
                 this.Direction = EDirection.FORWARD;
                 this.startFromKey = default(TKey);
                 this.IsFirstItemRelative = true;
             }
         }

        public DrLinkedDictonary()
        {
            dic = new Dictionary<TKey, DrLinkedValue>();
            first = null;
            last = null;
            EnumerationRules = new DrEnumerationRules();
        }

        private Dictionary<TKey, DrLinkedValue> dic;
        private DrLinkedValue first;
        private DrLinkedValue last;

        //public TKey StartFrom { get; set; }
        //public bool ReverseDirection { get; set; }
        public DrEnumerationRules EnumerationRules {get; private set;}

        #region Insert
        public void Add(TKey key, TValue value)
        {
            var item = new DrLinkedValue(key, value, last, null);
            dic.Add(key, item);

            {// the following code will be executed only if the item was successfully added to the dictionary
                if (last != null) last.Next = item;
                last = item;
                if (first == null) first = item;
            }
        }

        public void InsertAsLast(TKey key, TValue value)
        {
            Add(key, value);
        }

        public void InsertAsFirst(TKey key, TValue value)
        {
            var item = new DrLinkedValue(key, value, null, first); // first is null if dictionary does not have any items
            dic.Add(key, item);
            if (first != null) first.Previous = item;
            this.first = item; // the following code will be executed only if the item was successfully added to the dictionary
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="beforeKey">if value is null item will be inserted as first element</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void InsertBefore(TKey beforeKey, TKey key, TValue value)
        {
            if (beforeKey == null)
                InsertAsFirst(key, value);
            else
            {
                DrLinkedValue beforeValue;
                if (dic.TryGetValue(beforeKey, out beforeValue))
                {
                    var item = new DrLinkedValue(key, value, beforeValue.Previous, beforeValue);
                    dic.Add(key, item);
                    if (beforeValue.IsFirst)
                        this.first = item;
                    else
                        beforeValue.Previous.Next = item;
                    beforeValue.Previous = item;
                }
                else
                {
                    throw new ApplicationException(string.Format(Res.Msg.CANNOT_INSERT_BEFORE_KEY_NOT_FOUND, beforeKey.ToString()));
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="afterKey">if value is null item will be inserted as last element</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void InsertAfter(TKey afterKey, TKey key, TValue value)
        {
            if (afterKey == null)
                InsertAsLast(key, value);
            else
            {
                DrLinkedValue afterValue;
                if (dic.TryGetValue(afterKey, out afterValue))
                {
                    var item = new DrLinkedValue(key, value, afterValue, afterValue.Next);
                    dic.Add(key, item);
                    if (afterValue.IsLast)
                        this.last = item;
                    else
                        afterValue.Next.Previous = item;
                    afterValue.Next = item;
                }
                else
                {
                    throw new ApplicationException(string.Format(Res.Msg.CANNOT_INSERT_AFTER_KEY_NOT_FOUND, afterKey.ToString()));
                }
            }
        }
        #endregion Insert

        public TValue this[TKey key]
        {
            get { return dic[key].Value; }
            set { dic[key].Value = value; }
        }

        /// <summary>
        /// rebuild links for the item which will be removed
        /// </summary>
        /// <param name="value">value which will be removed</param>
        private void rebuildLinkBeforeRemoveItem(DrLinkedValue value)
        {
            try
            {
                if ((value.IsFirst) && (value.IsLast)) return;     // dictionary has only one item
                if (value.IsFirst) { value.Next.Previous = null; return; }       // the first element will be removed
                if (value.IsLast) { value.Previous.Next = null; return; }        // the last element will be removed
                value.Previous.Next = value.Next;
                value.Next.Previous = value.Previous;
            }
            catch (Exception e)
            {
                if (value == null) throw new ApplicationException(Res.Msg.CANNOT_REBUILD_LINKS_VALUE_IS_NULL, e);
                if (value.Key == null) throw new ApplicationException(Res.Msg.CANNOT_REBUILD_LINKS_KEY_IS_NULL, e);
                throw new ApplicationException(string.Format(Res.Msg.CANNOT_REBUILD_LINKS_FOR_ITEM, value.Key.ToString()), e);
            }

        }

        # region implement IDictionary<TKey, TValue>
        public bool ContainsKey(TKey key)
        {
            return dic.ContainsKey(key);
        }
        public bool ContainsValue(TValue value)
        {
            if (value == null)
            {
                foreach (var item in dic)
                {
                    if (item.Value.Value.GetHashCode() >= 0 && item.Value.Value == null) return true;
                }
            }
            else
            {
                var eComparer = EqualityComparer<TValue>.Default;
                foreach (var item in dic)
                {
                    if (item.Value.Value.GetHashCode() >= 0 && eComparer.Equals(item.Value.Value, value)) return true;
                }

            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            DrLinkedValue lValue;

            var result = dic.TryGetValue(key, out lValue);
            if (result)
                value = lValue.Value;
            else
                value = default(TValue);
            return result;
        }

        public bool Remove(TKey key)
        {
            DrLinkedValue value;
            if (dic.TryGetValue(key, out value))
            {
                rebuildLinkBeforeRemoveItem(value);
                if (value.Previous == null) this.first = value.Next; // if value is first
                if (value.Next == null) this.last = value.Previous; // if value is last
                return dic.Remove(key);
            }
            return false; // item doesn't exist
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            dic.Clear();
            this.first = null;
            this.last = null;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            DrLinkedValue value;
            if (dic.TryGetValue(item.Key, out value)) return EqualityComparer<TValue>.Default.Equals(value.Value, item.Value); // if key is exists compare value
            return false;
        }


        /// <summary>
        /// Copies the System.Collections.Generic.Dictionary<TKey,TValue>.KeyCollection elements to an existing one-dimensional System.Array, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied from System.Collections.Generic.Dictionary<TKey,TValue>.KeyCollection. The System.Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null) throw new NullReferenceException();
            if (array.Rank != 1) throw new ArgumentOutOfRangeException(Res.Msg.ARRAY_MUST_SINGLE_DIMENSIONAL);
            if (index < 0 || index > array.Length) throw new ArgumentOutOfRangeException(Res.Msg.INDEX_IS_INCORRECT);

            int i = index;
            foreach (var item in this) // ToDo maybe check for index out of range? Or exception here is fine?
            {
                array[i] = item;
                i++;
            }
        }


        public int Count
        {
            get { return dic.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (Contains(item)) return Remove(item.Key);
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion implement IDictionary<TKey, TValue>

        #region Enumerator

        public class LinkedEnumerator
        {
            protected DrLinkedDictonary<TKey, TValue> dic;
            protected DrLinkedValue nextLinkedValue;
            protected DrLinkedValue currentLinkedValue;
            protected DrLinkedValue startFrom;

            #region LinkedEnumerator
            public LinkedEnumerator(DrLinkedDictonary<TKey, TValue> linkedDictionary)
            {
                this.dic = linkedDictionary;
                if (linkedDictionary.EnumerationRules.IsFirstItemRelative)
                    this.startFrom = linkedDictionary.EnumerationRules.Direction==DrEnumerationRules.EDirection.FORWARD ? linkedDictionary.first: linkedDictionary.last;
                else
                    this.startFrom = linkedDictionary.dic[linkedDictionary.EnumerationRules.StartFromKey];
                //this.startFrom = (linkedDictionary.enumRules.StartFrom != null ? linkedDictionary.dic[linkedDictionary.enumRules.StartFrom] : linkedDictionary.first);
                nextLinkedValue = this.startFrom;
                currentLinkedValue = default(DrLinkedValue);
            }
            #endregion LinkedEnumerator

            public bool MoveNext()
            {
                if (nextLinkedValue == null) return false;
                currentLinkedValue = nextLinkedValue;
                if (this.dic.EnumerationRules.Direction==DrEnumerationRules.EDirection.FORWARD)
                    nextLinkedValue = nextLinkedValue.Next;
                else
                    nextLinkedValue = nextLinkedValue.Previous;
                return true;
            }

            public void Dispose() { }
        }

        public class Enumerator : LinkedEnumerator, IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            #region Enumerator
            public Enumerator(DrLinkedDictonary<TKey, TValue> linkedDictionary)
                : base(linkedDictionary)
            { }
            #endregion Enumerator

            object System.Collections.IEnumerator.Current
            {
                get { return currentLinkedValue; }
            }

            void System.Collections.IEnumerator.Reset()
            {
                dic.EnumerationRules.Reset();
            }

            public DictionaryEntry Entry
            {
                get
                {
                    return new DictionaryEntry(currentLinkedValue.Key, currentLinkedValue.Value);
                }
            }

            public object Key
            {
                get { return currentLinkedValue.Key; }
            }

            public object Value
            {
                get { return currentLinkedValue.Value; }
            }

            KeyValuePair<TKey, TValue> Current
            {
                get { return currentLinkedValue; }
            }

            KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current
            {
                get { return currentLinkedValue; }
            }
        }
        #endregion Enumerator

        #region ValueCollection
        public sealed class ValueCollection : ICollection<TValue>, ICollection
        {
            DrLinkedDictonary<TKey, TValue> dictionary;
            public ValueCollection(DrLinkedDictonary<TKey, TValue> dictionary)
            {
                if (dictionary == null) throw new ArgumentNullException("Dictonary cannot be null.");
                this.dictionary = dictionary;
            }

            public void Add(TValue item)
            {
                throw new NotSupportedException(Res.Msg.MUTATING_COLLECTION_IS_NOT_SUPPORTED);
            }
            public void Clear()
            {
                throw new NotSupportedException(Res.Msg.MUTATING_COLLECTION_IS_NOT_SUPPORTED);
            }
            public bool Remove(TValue item)
            {
                throw new NotSupportedException(Res.Msg.MUTATING_COLLECTION_IS_NOT_SUPPORTED);
            }

            public bool Contains(TValue item)
            {
                return this.dictionary.ContainsValue(item);
            }

            public void CopyTo(Array array, int index)
            {
                var values = (TValue[])array;
                if (values != null)
                {
                    CopyTo(values, index);
                }
                else
                {
                    var objects = (object[])array;
                    int i = index;
                    foreach (var item in this)
                    {
                        objects[i] = item;
                        i++;
                    }
                }
            }

            public void CopyTo(TValue[] array, int index)
            {
                if (array == null) throw new NullReferenceException(Res.Msg.ARRAY_IS_NULL);
                if (array.Rank != 1) throw new ArgumentOutOfRangeException(Res.Msg.ARRAY_MUST_SINGLE_DIMENSIONAL);
                if (index < 0 || index > array.Length) throw new ArgumentOutOfRangeException(Res.Msg.INDEX_IS_INCORRECT);

                int i = index;
                foreach (var item in this) // ToDo is indexOutOfRange exception fine here?
                {
                    array[i] = item;
                    i++;
                }
            }

            public int Count
            {
                get { return dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }
            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            Object ICollection.SyncRoot
            {
                get { return ((ICollection)dictionary).SyncRoot; }
            }
            public IEnumerator<TValue> GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public class Enumerator : LinkedEnumerator, IEnumerator<TValue>, IEnumerator
            {
                #region Enumerator
                public Enumerator(DrLinkedDictonary<TKey, TValue> linkedDictionary)
                    : base(linkedDictionary)
                { }
                #endregion Enumerator
                void System.Collections.IEnumerator.Reset()
                {
                    dic.EnumerationRules.Reset();
                }

                public TValue Current
                {
                    get { return currentLinkedValue.Value; }
                }

                object IEnumerator.Current
                {
                    get { return currentLinkedValue.Value; }
                }
            }
        }
        #endregion ValueCollection


        #region KeyCollection
        public sealed class KeyCollection : ICollection<TKey>, ICollection
        {
            DrLinkedDictonary<TKey, TValue> dictionary;
            public KeyCollection(DrLinkedDictonary<TKey, TValue> dictionary)
            {
                if (dictionary == null) throw new ArgumentNullException("Dictonary cannot be null.");
                this.dictionary = dictionary;
            }

            public void Add(TKey item)
            {
                throw new NotSupportedException(Res.Msg.MUTATING_COLLECTION_IS_NOT_SUPPORTED);
            }
            public void Clear()
            {
                throw new NotSupportedException(Res.Msg.MUTATING_COLLECTION_IS_NOT_SUPPORTED);
            }
            public bool Remove(TKey item)
            {
                throw new NotSupportedException(Res.Msg.MUTATING_COLLECTION_IS_NOT_SUPPORTED);
            }

            public bool Contains(TKey item)
            {
                return this.dictionary.ContainsKey(item);
            }

            public void CopyTo(Array array, int index)
            {
                var keys = (TKey[])array;
                if (keys != null)
                {
                    CopyTo(keys, index);
                }
                else
                {
                    var objects = (object[])array;
                    int i = index;
                    foreach (var item in this)
                    {
                        objects[i] = item;
                        i++;
                    }
                }
            }

            public void CopyTo(TKey[] array, int index)
            {
                if (array == null) throw new NullReferenceException(Res.Msg.ARRAY_IS_NULL);
                if (array.Rank != 1) throw new ArgumentOutOfRangeException(Res.Msg.ARRAY_MUST_SINGLE_DIMENSIONAL);
                if (index < 0 || index > array.Length) throw new ArgumentOutOfRangeException(Res.Msg.INDEX_IS_INCORRECT);

                int i = index;
                foreach (var item in this) // ToDo is indexOutOfRange exception fine here?
                {
                    array[i] = item;
                    i++;
                }
            }

            public int Count
            {
                get { return dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }
            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            Object ICollection.SyncRoot
            {
                get { return ((ICollection)dictionary).SyncRoot; }
            }
            public IEnumerator<TKey> GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public class Enumerator : LinkedEnumerator, IEnumerator<TKey>, IEnumerator
            {
                #region Enumerator
                public Enumerator(DrLinkedDictonary<TKey, TValue> linkedDictionary)
                    : base(linkedDictionary)
                { }
                #endregion Enumerator
                void System.Collections.IEnumerator.Reset()
                {
                    dic.EnumerationRules.Reset();
                }

                public TKey Current
                {
                    get { return currentLinkedValue.Key; }
                }

                object IEnumerator.Current
                {
                    get { return currentLinkedValue.Key; }
                }
            }
        }
        #endregion KeyCollection

        public void Add(object key, object value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object key)
        {
            throw new NotImplementedException();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        public void Remove(object key)
        {
            throw new NotImplementedException();
        }

        //public KeyCollection Keys
        //{
        //    get { return new KeyCollection(this); }
        //}
        public ICollection<TKey> Keys
        {
            get { return new KeyCollection(this); }
        }
        ICollection IDictionary.Keys
        {
            get { return new KeyCollection(this); }
        }
        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return new KeyCollection(this); }
        }

        ICollection IDictionary.Values
        {
            get { return new ValueCollection(this); }
        }
        public ValueCollection Values
        {
            get { return new ValueCollection(this); }
        }
        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get { return new ValueCollection(this); }
        }

        public object this[object key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }
    }
}
