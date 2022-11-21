using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.CustomUtils;

[CustomPropertyDrawer(typeof(Span))]
public class SpanDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = base.GetPropertyHeight(property, label);
		return height;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty min = property.FindPropertyRelative("Min");
		SerializedProperty max = property.FindPropertyRelative("Max");

		Span oldSpan = new Span(min.floatValue, max.floatValue);
		Span newSpan = SpanField(position, label, oldSpan);

		ValidateSpan(oldSpan, ref newSpan);

		min.floatValue = newSpan.Min;
		max.floatValue = newSpan.Max;
	}

	public virtual Span SpanField(Rect position, in Span span)
	{
		GUIContent[] subLabels = { new GUIContent("Min"), new GUIContent("Max") };
		float[] values = { span.Min, span.Max };
		EditorGUI.MultiFloatField(position, subLabels, values);
		return new Span(values[0], values[1]);
	}
	
	public virtual Span SpanField(Rect position, GUIContent label, in Span span)
	{
		Rect controlPosition = EditorGUI.PrefixLabel(position, label);
		return SpanField(controlPosition, span);
	}

	public virtual Span SpanField(Rect position, string label, in Span span)
	{
		GUIContent newLabel = new GUIContent(label);
		return SpanField(position, newLabel, span);
	}

	public virtual void ValidateSpan(in Span oldSpan, ref Span newSpan) { 
		if (oldSpan.Min != newSpan.Min)
		{
			newSpan.Truncate(true);
		}
		else if (oldSpan.Max != newSpan.Max)
		{
			newSpan.Truncate(false);
		}
	}
}



[CustomPropertyDrawer(typeof(SpanMinAttribute))]
public class SpanMinDrawer : SpanDrawer
{
	public override void ValidateSpan(in Span oldSpan, ref Span newSpan) 
	{
		var limiter = attribute as SpanMinAttribute;
		if (newSpan.Min < limiter.MinLimit)
			newSpan.Min = limiter.MinLimit;
		if (newSpan.Max < limiter.MinLimit)
			newSpan.Max = limiter.MinLimit;
		base.ValidateSpan(oldSpan, ref newSpan);
	}
}



[CustomPropertyDrawer(typeof(SpanMaxAttribute))]
public class SpanMaxDrawer : SpanDrawer
{
	public override void ValidateSpan(in Span oldSpan, ref Span newSpan)
	{
		var limiter = attribute as SpanMaxAttribute;
		if (newSpan.Min > limiter.MaxLimit)
			newSpan.Min = limiter.MaxLimit;
		if (newSpan.Max > limiter.MaxLimit)
			newSpan.Max = limiter.MaxLimit;
		base.ValidateSpan(oldSpan, ref newSpan);
	}
}



[CustomPropertyDrawer(typeof(SpanRangeAttribute))]
public class SpanRangeDrawer : SpanDrawer
{
	public override Span SpanField(Rect position, in Span span)
	{
		float margin = 5;

		Rect leftPos = new Rect(position);
		leftPos.width = 50;

		Rect midPos = new Rect(position);
		midPos.width -= (leftPos.width + margin) * 2;
		midPos.x = leftPos.x + leftPos.width + margin;
		
		Rect rightPos = new Rect(leftPos);
		rightPos.x = midPos.x + midPos.width + margin;

		var limiter = attribute as SpanRangeAttribute;
		Span newSpan = new Span(span);

		float newMin = EditorGUI.DelayedFloatField(leftPos, newSpan.Min);
		EditorGUI.MinMaxSlider(midPos, ref newSpan.Min, ref newSpan.Max, limiter.MinLimit, limiter.MaxLimit);
		float newMax = EditorGUI.DelayedFloatField(rightPos, newSpan.Max);

		if (span.Min != newMin) 
			newSpan.Min = newMin;
		if (span.Max != newMax)
			newSpan.Max = newMax;

		return newSpan;
	}

	public override void ValidateSpan(in Span oldSpan, ref Span newSpan)
	{
		var limiter = attribute as SpanRangeAttribute;
		newSpan.Clamp(limiter.MinLimit, limiter.MaxLimit);
		base.ValidateSpan(oldSpan, ref newSpan);
	}
}