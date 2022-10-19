using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DensityHelper : MonoBehaviour
{
    [SerializeField] public float Density = 1000;
	[SerializeField, ReadOnly] public float m_Mass = 1000;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        UpdateDensity();
	}

    void OnValidate()
	{
		UpdateDensity();
	}

    public void UpdateDensity()
    {
		if (this.TryGetComponent(out Rigidbody rigidbody))
		{
			rigidbody.SetDensity(Density);
            m_Mass = rigidbody.mass;
			rigidbody.mass = rigidbody.mass;
		}
	}
}
