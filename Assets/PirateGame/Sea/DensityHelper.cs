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

		List<MeshCollider> wasEnabled = new List<MeshCollider>();
		foreach (var mc in this.GetComponentsInChildren<MeshCollider>(true))
		{
			if (mc.attachedRigidbody != rigidbody) continue;
			if (!mc.enabled) continue;
			if (!mc.gameObject.activeInHierarchy) continue;
			if (!mc.convex) continue;
			if (mc.isTrigger) continue;
			if (!mc.TryGetComponent(out MeshVolume _)) continue;

			mc.enabled = false;
			wasEnabled.Add(mc);
		}

		rigidbody.SetDensity(Density);
		m_Mass = rigidbody.mass;

		foreach (var mc in wasEnabled)
		{
			m_Mass += mc.GetComponent<MeshVolume>().Volume * Density;
			mc.enabled = true;
		}

		rigidbody.mass = m_Mass;

		if (CenterOfMass != null)
		{
			rigidbody.centerOfMass = rigidbody.transform.InverseTransformPoint(CenterOfMass.position);
		}
	}
}
