using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class UnityDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	[System.Serializable]
	public sealed class _KeyValuePair : UnityKeyValuePair<TKey, TValue>
	{
		public _KeyValuePair(TKey key, TValue value) : base(key, value) { }
		public _KeyValuePair(KeyValuePair<TKey, TValue> keyValuePair) : base(keyValuePair) { }
	}

	[SerializeField]
	private List<_KeyValuePair> _serializedKeyValuePairs;

	private Dictionary<TKey, TValue> _dictionary => this;

	public void OnAfterDeserialize()
	{
		_dictionary.Clear();
		foreach (_KeyValuePair pair in _serializedKeyValuePairs)
		{
			try
			{
				_dictionary.Add(pair.Key, pair.Value);
			}
			catch (System.ArgumentNullException e)
			{
				Debug.LogError(e);
			}
			catch (System.ArgumentException e)
			{
				Debug.LogError(e);
			}
			catch (System.Exception e)
			{
				throw new System.Exception("Unexpected Exception", e);
				//Debug.LogException(new System.Exception("Unexpected Exception", e));
			}
			
		}
		_serializedKeyValuePairs.Clear();
	}

	public void OnBeforeSerialize()
	{
		_serializedKeyValuePairs = new List<_KeyValuePair>(_dictionary.Count);
		foreach (KeyValuePair<TKey, TValue> pair in _dictionary)
		{
			_serializedKeyValuePairs.Add(new _KeyValuePair(pair));
		}
	}


	// IDictionary Implementation
	/*
	public object this[object key] { get => ((IDictionary)_dictionary)[key]; set => ((IDictionary)_dictionary)[key] = value; }
	public bool IsFixedSize => ((IDictionary)_dictionary).IsFixedSize;
	public bool IsReadOnly => ((IDictionary)_dictionary).IsReadOnly;
	public ICollection Keys => ((IDictionary)_dictionary).Keys;
	public ICollection Values => ((IDictionary)_dictionary).Values;
	public int Count => ((ICollection)_dictionary).Count;
	public bool IsSynchronized => ((ICollection)_dictionary).IsSynchronized;
	public object SyncRoot => ((ICollection)_dictionary).SyncRoot;
	public void Add(object key, object value)
	{
		((IDictionary)_dictionary).Add(key, value);
	}
	public void Clear()
	{
		((IDictionary)_dictionary).Clear();
	}
	public bool Contains(object key)
	{
		return ((IDictionary)_dictionary).Contains(key);
	}
	public IDictionaryEnumerator GetEnumerator()
	{
		return ((IDictionary)_dictionary).GetEnumerator();
	}
	public void Remove(object key)
	{
		((IDictionary)_dictionary).Remove(key);
	}
	public void CopyTo(System.Array array, int index)
	{
		((ICollection)_dictionary).CopyTo(array, index);
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_dictionary).GetEnumerator();
	}
	*/
}

public class UnityKeyValuePair<TKey, TValue>
{
	public TKey Key => _key;
	public TValue Value => _value;

	[SerializeField] private TKey _key;
	[SerializeField] private TValue _value;

	public UnityKeyValuePair(TKey key, TValue value)
	{
		_key = key;
		_value = value;
	}

	public UnityKeyValuePair(KeyValuePair<TKey, TValue> keyValuePair)
	{
		_key = keyValuePair.Key;
		_value = keyValuePair.Value;
	}

	public static implicit operator KeyValuePair<TKey, TValue>(UnityKeyValuePair<TKey, TValue> unityKeyValuePair)
	{
		return new KeyValuePair<TKey, TValue>(unityKeyValuePair.Key, unityKeyValuePair.Value);
	}

	public static implicit operator UnityKeyValuePair<TKey, TValue>(KeyValuePair<TKey, TValue> keyValuePair)
	{
		return new UnityKeyValuePair<TKey, TValue>(keyValuePair);
	}

	public override string ToString()
	{
		return ((KeyValuePair<TKey, TValue>)this).ToString();
	}
}