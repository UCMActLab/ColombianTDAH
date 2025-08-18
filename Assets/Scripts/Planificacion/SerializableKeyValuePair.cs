using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class SerializableKeyValuePair<K, V>
{
    public K Key;
    public V Value;

    public SerializableKeyValuePair(K key, V value)
    {
        Key = key;
        Value = value;
    }
}

[Serializable]
public class SerializableDictionary<K, V>
{
    public List<SerializableKeyValuePair<K, V>> list = new List<SerializableKeyValuePair<K, V>>();

    public void FromDictionary(Dictionary<K, V> dict)
    {
        list.Clear();
        foreach (var kv in dict)
        {
            list.Add(new SerializableKeyValuePair<K, V>(kv.Key, kv.Value));
        }
    }

    public Dictionary<K, V> ToDictionary()
    {
        Dictionary<K, V> dict = new Dictionary<K, V>();
        foreach (var kv in list)
        {
            dict[kv.Key] = kv.Value;
        }
        return dict;
    }
}