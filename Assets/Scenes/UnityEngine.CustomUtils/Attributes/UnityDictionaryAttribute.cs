using UnityEngine;

public class UnityDictionaryAttribute : PropertyAttribute
{
	public string key;
	public string value;
	public UnityDictionaryAttribute()
	{
		key   = "Key"  ;
		value = "Value";
	}

	public UnityDictionaryAttribute(string key, string value)
	{
		this.key   = key  ;
		this.value = value;
	}
}
