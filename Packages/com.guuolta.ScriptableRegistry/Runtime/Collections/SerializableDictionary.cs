using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableRegistry.Collections
{
    /// <summary>
    /// Serializable dictionary-like container for Unity
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
        : ISerializationCallbackReceiver, IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// Unity-serializable key/value pair (KeyValuePair is not serialized by Unity)
        /// </summary>
        [Serializable]
        private struct Pair
        {
            public TKey Key;
            public TValue Value;
        }

        // Data for Inspector/serialization
        [SerializeField]
        private List<Pair> _pairs = new();

        // Runtime dictionary (source of truth)
        private readonly Dictionary<TKey, TValue> _dict = new();

        /// <summary>
        /// Get/Set value by key
        /// </summary>
        public TValue this[TKey key]
        {
            get => _dict[key];
            set => Set(key, value);
        }

        public int Count => _dict.Count;
        public IEnumerable<TKey> Keys => _dict.Keys;
        public IEnumerable<TValue> Values => _dict.Values;

        public bool ContainsKey(TKey key) => _dict.ContainsKey(key);
        public bool TryGetValue(TKey key, out TValue value) => _dict.TryGetValue(key, out value);

        /// <summary>
        /// Enable foreach over key/value pairs
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Add or update a value and keep the serialized list in sync
        /// </summary>
        public void Set(TKey key, TValue value)
        {
            _dict[key] = value;

            int index = _pairs.FindIndex(p =>
                EqualityComparer<TKey>.Default.Equals(p.Key, key));

            if (index >= 0)
                _pairs[index] = new Pair { Key = key, Value = value };
            else
                _pairs.Add(new Pair { Key = key, Value = value });
        }

        /// <summary>
        /// Add a new key/value (throws if key exists)
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            _dict.Add(key, value);
            _pairs.Add(new Pair { Key = key, Value = value });
        }

        /// <summary>
        /// Remove a key/value and sync the list
        /// </summary>
        public bool Remove(TKey key)
        {
            if (!_dict.Remove(key)) return false;

            int index = _pairs.FindIndex(p =>
                EqualityComparer<TKey>.Default.Equals(p.Key, key));
            if (index >= 0) _pairs.RemoveAt(index);

            return true;
        }

        /// <summary>
        /// Clear all entries
        /// </summary>
        public void Clear()
        {
            _dict.Clear();
            _pairs.Clear();
        }

        /// <summary>
        /// Return a normal Dictionary copy
        /// </summary>
        public Dictionary<TKey, TValue> ToDictionary() => new Dictionary<TKey, TValue>(_dict);

        public void OnBeforeSerialize() { }

        /// <summary>
        /// After loading: rebuild runtime dict from list
        /// </summary>
        public void OnAfterDeserialize()
        {
            _dict.Clear();
            foreach (var p in _pairs)
            {
                _dict[p.Key] = p.Value;
            }
        }
    }
}
