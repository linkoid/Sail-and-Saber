using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SpanMinAttribute : PropertyAttribute
{
	public float MinLimit;

	public SpanMinAttribute(float minLimit)
	{
		MinLimit = minLimit;
	}
}

public sealed class SpanMaxAttribute : PropertyAttribute
{
	public float MaxLimit;

	public SpanMaxAttribute(float maxLimit)
	{
		MaxLimit = maxLimit;
	}
}


public sealed class SpanRangeAttribute : PropertyAttribute
{
	public float MinLimit;
	public float MaxLimit;

	public SpanRangeAttribute(float minLimit, float maxLimit)
	{
		MinLimit = minLimit;
		MaxLimit = maxLimit;
	}


}

public sealed class SpanIntAttribute : PropertyAttribute
{
	public SpanIntAttribute() { }
}