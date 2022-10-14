using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorParameterOverride : MonoBehaviour
{
    [System.Serializable]
    public class FloatDictionary : UnityDictionary<string, float> { }

    [System.Serializable]
    public class IntDictionary : UnityDictionary<string, int> { }

    [System.Serializable]
    public class BoolDictionary : UnityDictionary<string, bool> { }


    [SerializeField, UnityDictionary("Parameter", "Value")] private FloatDictionary _floatOverrides;
    [SerializeField, UnityDictionary("Parameter", "Value")] private IntDictionary   _intOverrides  ;
    [SerializeField, UnityDictionary("Parameter", "Value")] private BoolDictionary  _boolOverrides ;

    private Animator _animator;


    void Awake()
	{
		_animator = this.GetComponent<Animator>();
	}

	// Start is called before the first frame update
	void Start()
    {
        OnStartOrEnable();
    }

	void OnEnable()
    {
        OnStartOrEnable();
    }

	void OnStartOrEnable()
	{
        foreach (var pair in _floatOverrides)
        {
            _animator.SetFloat(pair.Key, pair.Value);
        }

        foreach (var pair in _intOverrides)
        {
            _animator.SetInteger(pair.Key, pair.Value);
        }

        foreach (var pair in _boolOverrides)
        {
            _animator.SetBool(pair.Key, pair.Value);
        }
    }
}
