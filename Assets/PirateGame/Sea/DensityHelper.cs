using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DensityHelper : MonoBehaviour
{

	[SerializeField] public Transform CenterOfMass;
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
        UpdateRigidbody();
	}

    void OnValidate()
	{
		UpdateRigidbody();
	}

    public void UpdateRigidbody()
	{
		if (!this.TryGetComponent(out Rigidbody rigidbody)) return;

		rigidbody.SetDensity(Density);
        m_Mass = rigidbody.mass;
		rigidbody.mass = m_Mass;
		
		if (CenterOfMass != null)
		{
			rigidbody.centerOfMass = rigidbody.transform.InverseTransformPoint(CenterOfMass.position);
		}
	}
}
