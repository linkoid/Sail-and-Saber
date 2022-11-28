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

	public bool ManualCenter = false;
	public Vector3 Center = Vector3.zero;

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
			Volume = CalculateVolume();
		}
	}

	public float CalculateVolume()
	{
		return CalculateMeshVolume(MeshCollider.sharedMesh, this.transform.localToWorldMatrix, GetCenter());
	}

	public Vector3 GetCenter()
	{
		if (ManualCenter)
		{
			return this.transform.TransformPoint(Center);
		}

		return CalculateCenter(MeshCollider.sharedMesh, this.transform.localToWorldMatrix);
	}

	public static float CalculateMeshVolume(Mesh mesh, Matrix4x4 transform, Vector3 center)
    {
		float meshVolume = 0;

        Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		float3x3 tri = new float3x3();

		for (int i = 0; i < triangles.Length; i += 3)
		{
			tri[0] = transform.MultiplyPoint(vertices[triangles[i + 0]]) - center;
			tri[1] = transform.MultiplyPoint(vertices[triangles[i + 1]]) - center;
			tri[2] = transform.MultiplyPoint(vertices[triangles[i + 2]]) - center;
			meshVolume += CalculateTetraVolume(tri);
        }
        return meshVolume;
    }

	public static Vector3 CalculateCenter(Mesh mesh, Matrix4x4 transform)
	{
		Vector3[] vertices = mesh.vertices;

		Vector3 center = Vector3.zero;
		for (int i = 0; i < vertices.Length; i += 1)
		{
			center += transform.MultiplyPoint(vertices[i]);
		}
		center /= vertices.Length;
		return center;
	} 

	[BurstCompile]
    public static float CalculateTetraVolume(float3x3 tri)
    {
		return math.determinant(tri) / 6;
    }



	private void OnDrawGizmosSelected()
	{
		Mesh mesh = MeshCollider.sharedMesh;
		Matrix4x4 transform = this.transform.localToWorldMatrix;

		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		float3x3 tri = new float3x3();

		Vector3 center = GetCenter();

		Gizmos.color = Color.green;

		for (int i = 0; i < triangles.Length; i += 3)
		{
			tri[0] = transform.MultiplyPoint(vertices[triangles[i + 0]]);
			tri[1] = transform.MultiplyPoint(vertices[triangles[i + 1]]);
			tri[2] = transform.MultiplyPoint(vertices[triangles[i + 2]]);

			Gizmos.DrawLine(tri[0], tri[1]);
			Gizmos.DrawLine(tri[1], tri[2]);
			Gizmos.DrawLine(tri[2], tri[0]);
			Gizmos.DrawLine(tri[0], center);
			Gizmos.DrawLine(tri[1], center);
			Gizmos.DrawLine(tri[2], center);
		}
	}
}
