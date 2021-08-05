﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlServer.Dac
{
    public static class Misc
    {
        private static StringComparer _comparer = StringComparer.OrdinalIgnoreCase;
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }
        public static void TryRemove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary.Remove(key);
            }
        }
        public static IDictionary<K, T> AddRange<K, T>(this IDictionary<K, T> dic1, IDictionary<K, T> dic2)
        {
            foreach (var item in dic2)
            {
                dic1.AddOrUpdate(item.Key, item.Value);
            }

            return dic1;
        }
        public static void RemoveAll<K, V>(this IDictionary<K, V> dict, Func<K, V, bool> match)
        {
            foreach (var key in dict.Keys.ToArray().Where(key => match(key, dict[key])))
            {
                dict.Remove(key);
            }
        }
        public static void RemoveAll<T>(this IList<T> list, Func<T, bool> match)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];
                if (match(item))
                {
                    list.Remove(item);
                }
            }
        }
        public static bool StringEquals(this object value1, object value2)
        {
            if (value1 == null || value2 == null) { return false; }
            return _comparer.Equals(value1, value2);
        }

    }
}