using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.CustomUtils
{
	[RequireComponent(typeof(Animator))]
	public class AnimatorParameterForward : MonoBehaviour
	{
		public string Parameter;

		private Animator Animator => GetComponent<Animator>();


		[SerializeField, ReadOnly] private bool m_CachedBool = default;
		[SerializeField, ReadOnly] private int m_CachedInteger = default;
		[SerializeField, ReadOnly] private float m_CachedFloat = default;

		// Prevents overriding with default if some value has already been forwarded.
		private bool m_Forwarded = false;

		protected void Awake()
		{
			ForwardCachedValue();
		}

		protected void OnEnable()
		{
			ForwardCachedValue();
		}

		private void Update()
		{
			ForwardCachedValue();
		}


		public void ForwardCachedValue()
		{
			if (m_CachedBool != default)
			{
				SetBool(m_CachedBool);
			}

			if (m_CachedInteger != default)
			{
				SetInteger(m_CachedInteger);
			}

			if (m_CachedFloat != default)
			{
				SetFloat(m_CachedFloat);
			}
		}


		// Unity Event Callbacks
		public void SetBool(bool value)
		{
			m_CachedBool = value;
			Animator.SetBool(Parameter, value);
			m_Forwarded = true;
		}
		public void SetInteger(int value)
		{
			m_CachedInteger = value;
			Animator.SetInteger(Parameter, value);
			m_Forwarded = true;
		}
		public void SetFloat(float value)
		{
			m_CachedFloat = value;
			Animator.SetFloat(Parameter, value);
			m_Forwarded = true;
		}
	}
}