using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;

[RequireComponent(typeof(MeshCollider))]
public class MeshVolume : MonoBehaviour
{
    public MeshCollider MeshCollider => this.GetComponent<MeshCollider>();
	public bool AutoInitialize = false;
	public float Volume = 0;

	void Start()
	{
		Init();
	}

	void OnValidate()
	{
		Init();
	}

	void Init()
	{
		if (AutoInitialize)
		{
			Volume = CalculateMeshVolume(MeshCollider.sharedMesh, this.transform.localToWorldMatrix);
		}
	}

	public static float CalculateMeshVolume(Mesh mesh, Matrix4x4 transform)
    {
		float meshVolume = 0;

        Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		float3x3 tri = new float3x3();

		for (int i = 0; i < triangles.Length; i += 3)
		{
			tri[0] = transform.MultiplyPoint(vertices[triangles[i + 0]]);
			tri[1] = transform.MultiplyPoint(vertices[triangles[i + 1]]);
			tri[2] = transform.MultiplyPoint(vertices[triangles[i + 2]]);
			meshVolume += CalculateTriVolume(tri);
        }
        return meshVolume;
    }

	[BurstCompile]
    public static float CalculateTriVolume(float3x3 tri)
    {
		return math.determinant(tri) * (1.0f / 6);
    }
}
