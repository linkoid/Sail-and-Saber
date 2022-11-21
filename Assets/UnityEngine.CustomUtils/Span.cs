using UnityEngine;

namespace UnityEngine.CustomUtils
{
	[System.Serializable]
	public struct Span
	{
		public float Min;
		public float Max;

		public float Size => Max - Min;



		public Span(in float min, in float max)
		{
			Min = min;
			Max = max;
		}

		public Span(in Span span)
		{
			Min = span.Min;
			Max = span.Max;
		}



		/// <summary>
		/// Flips the Min and Max of the span.
		/// </summary>
		public void Flip()
		{
			float newMin = Max;
			Max = Min;
			Min = newMin;
		}

		/// <summary>
		/// Returns a new span with the Min and Max of the span flipped.
		/// </summary>
		public Span Flipped()
		{
			Span newSpan = new Span(this);
			newSpan.Flip();
			return newSpan;
		}

		/// <summary>
		/// Ensures that the span is not inverted by swapping Min and Max if necessary.
		/// </summary>
		public void Sort()
		{
			if (Min < Max) return;
		}

		/// <summary>
		/// Ensures that the span is not inverted by swapping Min and Max if necessary.
		/// </summary>
		/// <returns>A new sorted Span.</returns>
		public Span Sorted()
		{
			Span newSpan = new Span(this);
			newSpan.Sort();
			return newSpan;
		}

		/// <summary>
		/// Truncates an inverted span in the specified direction.
		/// </summary>
		/// <param name="up">
		/// If <c>true</c> will move both Min and Max to the highest value.
		/// If <c>false</c> will move both Min and Max to the lowest value. (default behavior)
		/// </param>
		public void Truncate(bool up = false)
		{
			if (Min < Max) return;

			if (up)
				Max = Min;
			else
				Min = Max;
		}

		/// <summary>
		/// Truncates an inverted span in the specified direction.
		/// </summary>
		/// <param name="up">
		/// If <c>true</c> will move both Min and Max to the highest value.
		/// If <c>false</c> will move both Min and Max to the lowest value. (default behavior)
		/// </param>
		/// <returns>A new truncated Span.</returns>
		public Span Truncated(bool up = false)
		{
			Span newSpan = new Span(this);
			newSpan.Truncate(up);
			return newSpan;
		}

		/// <summary>
		/// Ensures the span's Min and Max are within the specified limits.
		/// </summary>
		/// <param name="limits">The span that specifies the limits.</param>
		public void Clamp(Span limits)
		{
			limits.Sort();
			if (Min < limits.Min)
				Min = limits.Min;
			if (Min > limits.Max)
				Min = limits.Max;
			if (Max < limits.Min)
				Max = limits.Min;
			if (Max > limits.Max)
				Max = limits.Max;
		}

		/// <summary>
		/// Ensures the span's Min and Max are within the specified limits.
		/// </summary>
		/// <param name="minLimit">The lower limit for the Span.</param>
		/// <param name="maxLimit">The upper limit for the Span.</param>
		public void Clamp(in float minLimit, in float maxLimit)
		{
			Clamp(new Span(minLimit, maxLimit));
		}

		/// <summary>
		/// Ensures the span's Min and Max are within the specified limits.
		/// </summary>
		/// <param name="limits">The span that specifies the limits.</param>
		/// <returns> A new clamped Span.</returns>
		public Span Clamped(Span limits)
		{
			Span newSpan = new Span(this);
			newSpan.Clamp(limits);
			return newSpan;
		}

		/// <summary>
		/// Ensures the span's Min and Max are within the specified limits.
		/// </summary>
		/// <param name="minLimit">The lower limit for the Span.</param>
		/// <param name="maxLimit">The upper limit for the Span.</param>
		/// <returns> A new clamped Span.</returns>
		public Span Clamped(in float minLimit, in float maxLimit)
		{
			Span newSpan = new Span(this);
			newSpan.Clamp(minLimit, maxLimit);
			return newSpan;
		}

		/// <summary>
		/// Determines if the specified point is contained by the span.
		/// </summary>
		/// <returns><c>true</c> if the specified point is within the span.</returns>
		public bool Contains(in float point)
		{
			return point >= Min && point <= Max;
		}

		/// <summary>
		/// Determines if the specified squared point is contained by the squared span.
		/// Used for avoiding square roots such as with sqrMagnitude.
		/// </summary>
		/// <returns><c>true</c> if the specified point is within the span.</returns>
		/// <remarks>
		/// Note that if the lower bound is a large negative number,
		/// smaller positive values will not qualify.
		/// </remarks>
		public bool SqrContains(in float point)
		{
			return point >= Min * Min && point <= Max * Max;
		}

		/// <summary>
		/// Transforms a relative point as a point inside the span denoted from Min and Max.
		/// </summary>
		/// <param name="relativePoint">The 1 dimensional relative point to transform.</param>
		/// <param name="extrapolate">If false, will clamp the return value between Min and Max.</param>
		/// <returns>If the <c>relativePoint == 0</c>, it returns Min. If <c>relativePoint == 1</c>, it returns Max.</returns>
		public float Map(float relativePoint, bool extrapolate = false)
		{
			if (Size == 0)
				return Max;

			if (!extrapolate)
				relativePoint = Mathf.Clamp01(relativePoint);
			return relativePoint * Size + Min;
		}

		/// <summary>
		/// Transforms an absolue point as a point inside the span denoted from 0 and 1.
		/// The inverse of Map.
		/// </summary>
		/// <remarks>
		/// If <c>Min == Max</c> it will return zero when less that Min/Max and 1 when greater than or equal to Min/Max.
		/// </remarks>
		/// <param name="absolutePoint">The 1 dimensional absolute point to transform.</param>
		/// <param name="extrapolate">If false, will clamp the return value between 0 and 1.</param>
		/// <returns>If the <c>absolutePoint == Min</c>, it returns 0. If <c>absolutePoint == Max</c>, it returns 1.</returns>
		public float InverseMap(float absolutePoint, bool extrapolate = false)
		{
			if (Size == 0)
				return absolutePoint < Min ? 0 : 1;

			float relativePoint = (absolutePoint - Min) / Size;
			if (!extrapolate)
				relativePoint = Mathf.Clamp01(relativePoint);
			return relativePoint;
		}

		/// <summary>
		/// Transforms an absolute point from this spans's local space the other span's local space.
		/// </summary>
		/// <param name="absolutePoint">The 1 dimensional absolute point to transform.</param>
		/// <param name="extrapolate">If false, will clamp the return value between toSpan.Min and toSpan.Max.</param>
		/// <returns>If the <c>absolutePoint == this.Min</c>, it returns toSpan.Min. If <c>absolutePoint == this.Max</c>, it returns tpSpan.Max.</returns>
		public float Remap(Span toSpan, float absolutePoint, bool extrapolate = false)
		{
			//return toSpan.Map( this.InverseMap(absolutePoint, extrapolate), extrapolate );

			if (toSpan.Size == 0)
				return toSpan.Max;

			float relativePoint;
			if (Size == 0)
				relativePoint = absolutePoint < Min ? 0 : 1;
			else
				relativePoint = (absolutePoint - Min) / Size;

			if (!extrapolate)
				relativePoint = Mathf.Clamp01(relativePoint);

			return relativePoint * toSpan.Size + toSpan.Min;
		}
	}

	public static class SpanExtensions
	{
		public static float RandomRange(this Span span)
		{
			return Random.RandomRange(span.Min, span.Max);
		}
	}
}