using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.UI.GridLayoutGroup;

namespace PirateGame.Sea
{
	public class BuoyancyEffector : MonoBehaviour
	{
		[SerializeField] private float WaveAmplitude = 1;
		[SerializeField] private float WaveDistance  = 10;
		[SerializeField] private float WaveSpeed     = 5;

		[Tooltip("Density of the fluid in kg/m³")]
		[SerializeField] private float m_FluidDensity = 1000f;

		[SerializeField] private float m_DefaultSubmersionDepth = 1f;
		[SerializeField] private float m_MinimumDrag = 0.01f;

		[SerializeField, ReadOnly] private int m_ColliderID;
		[SerializeField, ReadOnly] private float m_FixedTime;

		public void OnEnable()
		{
			UpdateWaveParameters();
		}

		public void OnDisable()
		{
		}

		void OnValidate()
		{
			UpdateWaveParameters();
		}

		/// <summary>
		/// Updates parameters in shader
		/// </summary>
		void UpdateWaveParameters()
		{
			Shader.SetGlobalFloat("WaveAmplitude", WaveAmplitude);
			Shader.SetGlobalFloat("WaveDistance" , WaveDistance );
			Shader.SetGlobalFloat("WaveSpeed"    , WaveSpeed    );
		}


		void FixedUpdate()
		{
			// debug stuff
			m_RawContacts.Clear();
			m_Contacts.Clear();
			m_IgnoredPoints.Clear();
			m_Positions.Clear();
			m_OtherPositions.Clear();


			m_FixedTime = Time.fixedTime;

			var pos = this.transform.position;
			pos.y = 0;
			var rot = this.transform.rotation;
			Collider[] colliders = Physics.OverlapBox(pos, new Vector3(1000, 10, 1000), rot, GetIgnoreLayerCollisionMask());
			foreach (Collider collider in colliders)
			{
				if (collider.attachedRigidbody == null) continue;
				if (collider.attachedRigidbody.isKinematic) continue;
				if (collider.attachedRigidbody.isKinematic) continue;

				AddWaterForceAtCollider(collider.attachedRigidbody, collider);
			}
		}

		private int GetIgnoreLayerCollisionMask()
		{
			int layerMask = int.MaxValue;
			for (int i=0; i < 32; i++)
			{
				if (Physics.GetIgnoreLayerCollision(this.gameObject.layer, i))
				{
					layerMask ^= 1 << i;
				}
			}
			return layerMask;
		}

		struct VolumeTensor
		{
			public Vector3 point;
			public Vector3 normal;
			public float   weight;
		}
		
		private void AddWaterForceAtCollider(Rigidbody rigidbody, Collider collider)
		{
			VolumeTensor[] tensors = new VolumeTensor[0];
			float volume = collider.transform.lossyScale.x * collider.transform.lossyScale.y * collider.transform.lossyScale.z;
			//var submersionDepth = m_DefaultSubmersionDepth;
			if (collider is SphereCollider sphereCollider)
			{
				volume = GetSphereVolume(sphereCollider.transform, sphereCollider.center, sphereCollider.radius, out tensors);
			}
			else if (collider is CapsuleCollider capsuleCollider)
			{
				float centerOffset = Mathf.Max(0, capsuleCollider.height - capsuleCollider.radius) / 2;

				Vector3 center0 = capsuleCollider.center + Vector3.up   * centerOffset;
				Vector3 center1 = capsuleCollider.center + Vector3.down * centerOffset;

				float sphereVolume = GetSphereVolume(capsuleCollider.transform, center0, capsuleCollider.radius, out VolumeTensor[] points0);
				                     GetSphereVolume(capsuleCollider.transform, center1, capsuleCollider.radius, out VolumeTensor[] points1);

				tensors = new VolumeTensor[points0.Length + points1.Length];
				points0.CopyTo(tensors, 0);
				points1.CopyTo(tensors, points0.Length);

				float cylinderVolume = 0; // TODO find cylinder volume
				volume = sphereVolume + cylinderVolume;
			}
			else if (collider is BoxCollider boxCollider)
			{
				VolumeTensor[] corners = new VolumeTensor[8];
				for (int i = 0; i < corners.Length; i++)
				{
					Vector3 cornerPolarity = Vector3.one;
					cornerPolarity.x = (i % 8 < 4) ? 1 : -1;
					cornerPolarity.y = (i % 4 < 2) ? 1 : -1;
					cornerPolarity.z = (i % 2 < 1) ? 1 : -1;

					corners[i].point = boxCollider.transform.TransformPoint(
						boxCollider.center + Vector3.Scale(boxCollider.size, cornerPolarity) * 0.5f);

					corners[i].normal = boxCollider.transform.TransformDirection(cornerPolarity);
				}
				float length = (corners[0].point - corners[1].point).magnitude;
				float width  = (corners[0].point - corners[2].point).magnitude;
				float height = (corners[0].point - corners[4].point).magnitude;
				volume = length * width * height;

				VolumeTensor[] faces = new VolumeTensor[6];
				for (int i = 0; i < faces.Length; i++)
				{
					Vector3 facePolarity = Vector3.zero;
					facePolarity[i/2] = (i % 2 < 1) ? 1 : -1;

					faces[i].point = boxCollider.transform.TransformPoint(
						boxCollider.center + Vector3.Scale(boxCollider.size, facePolarity) * 0.5f);

					faces[i].normal = boxCollider.transform.TransformDirection(facePolarity);
				}

				tensors = new VolumeTensor[1+8+6];
				tensors[0].point = boxCollider.transform.TransformPoint(boxCollider.center);
				corners.CopyTo(tensors, 1);
				faces  .CopyTo(tensors, 1+8);
			}
			else if (collider is MeshCollider meshCollider)
			{
				if (!meshCollider.convex) return;

				var mesh = meshCollider.sharedMesh;

				var vertices = mesh.vertices;
				var normals = mesh.normals;
				if (vertices.Length != normals.Length)
					Debug.LogError(($"vert[{vertices.Length}] != norm[{normals.Length}]"));
				tensors = new VolumeTensor[mesh.vertices.Length];
				for (int i = 0; i < tensors.Length; i++)
				{
					tensors[i].point = meshCollider.transform.TransformPoint(vertices[i]);
					tensors[i].normal = meshCollider.transform.TransformDirection(normals[i]);
					//Debug.Log(points[i]);
				}

				if (meshCollider.TryGetComponent(out MeshVolume meshVolume))
				{
					volume = meshVolume.Volume;
				}
			}

			if (tensors.Length <= 0) return;

			float pointVolume = volume / tensors.Length;
			float pointAreaFactor = 1.0f / tensors.Length;
			float minHeight = tensors.Min((t) => Vector3.Dot(t.point, -Physics.gravity.normalized));
			float maxHeight = tensors.Max((t) => Vector3.Dot(t.point, -Physics.gravity.normalized));
			float submersionDepth = maxHeight - minHeight;
			
			if (submersionDepth <= 0)
			{
				Debug.LogWarning($"submersion depth is <= 0 for {collider}", collider);
				return;
			}

			for (int i=0; i < tensors.Length; i++)
			{
				float depth = (maxHeight - tensors[i].point.y) / submersionDepth;
				tensors[i].weight = depth <= 0.5f ? 1 : depth * 2;
			}

			float weightsSum = tensors.Sum((t) => t.weight);
			foreach (var tensor in tensors)
			{
				float pointVolumeFactor = tensor.weight / weightsSum;
				AddWaterForceAtPoint(rigidbody, tensor, volume * pointVolumeFactor, pointAreaFactor, submersionDepth);
			}
		}

		private float GetSphereVolume(Transform localTransform, Vector3 localCenter, float localRadius, out VolumeTensor[] contacts)
		{
			contacts = new VolumeTensor[3];
			var point = localTransform.TransformPoint(localCenter);
			contacts[0].point = point;
			contacts[0].normal = Vector3.zero;

			var lossyScale = localTransform.lossyScale;
			var radius = localRadius * Mathf.Max(lossyScale.x, lossyScale.y, lossyScale.z);
			point.y -= Mathf.Abs(radius);
			contacts[1].point = point;
			contacts[1].normal = Vector3.zero;

			point.y += Mathf.Abs(radius) * 2;
			contacts[2].point = point;
			contacts[2].normal = Vector3.zero;

			// V = (4/3) * π * r³ 
			float volume = (4 / 3.0f) * Mathf.PI * Mathf.Pow(radius, 3);
			return volume;
		}


		void AddWaterForceAtPoint(Rigidbody rigidbody, VolumeTensor tensor, float pointVolume, float pointAreaFactor, float submersionDepth)
		{
			Vector3 point = tensor.point;
			float waterHeight = GetWaterHeight(point);
			if (waterHeight < point.y)
			{
				m_IgnoredPoints.Add(point); // for debug
				return;
			}

			float submersion = waterHeight - point.y;
			float submersionFactor = Mathf.Clamp01(submersion / submersionDepth);

			float displacedVolume = submersionFactor * pointVolume;
			Vector3 buoyancy = AddBuoyancyAtPoint(rigidbody, tensor, displacedVolume);
			Vector3 drag = AddDragAtPoint(rigidbody, point, pointAreaFactor);

			m_Contacts.Add(new Ray(point, buoyancy));
		}

		Vector3 AddBuoyancyAtPoint(Rigidbody rigidbody, VolumeTensor tensor, float displacedVolume)
		{
			Vector3 waveNormal = GetWaterNormal(tensor.point);

			// Buoyancy B = ρ_f * V_disp * -g
			float adjust = Physics.gravity.magnitude - 1;
			Vector3 buoyancy = m_FluidDensity * displacedVolume * (-Physics.gravity * adjust + waveNormal);
			Vector3 buoyantForce = Vector3.zero;
			if (tensor.normal.sqrMagnitude == 0)
			{
				buoyantForce = buoyancy;
			}
			else if (Vector3.Dot(buoyancy.normalized, -tensor.normal.normalized) > 0)
			{
				buoyantForce = Vector3.Project(buoyancy, -tensor.normal);
			}
			rigidbody.AddForceAtPosition(buoyantForce, tensor.point, ForceMode.Force);
			return buoyantForce;
		}

		const float k_AirDensity = 1.204f; // kg/m³
		Vector3 AddDragAtPoint(Rigidbody rigidbody, Vector3 point, float pointAreaFactor)
		{
			// Drag D = C_d * ρ_fluid * A * 0.5 * v²
			// Drag (N/s²) = (1) * (kg/m³) * (m²) * 0.5 * (m²/s²)

			Vector3 centerOffset = rigidbody.transform.InverseTransformPoint(point) - rigidbody.centerOfMass;
			Vector3 tensorSpaceOffset = Quaternion.Inverse(rigidbody.inertiaTensorRotation) * centerOffset;
			float momentArm = Vector3.Scale(tensorSpaceOffset.normalized, rigidbody.inertiaTensor).magnitude / rigidbody.mass;
			float momentFactor = tensorSpaceOffset.sqrMagnitude / momentArm;

			float dragCo = Mathf.Lerp(rigidbody.drag, rigidbody.angularDrag, momentFactor);
			dragCo = Mathf.Max(dragCo, m_MinimumDrag);

			Vector3 velocity = rigidbody.GetPointVelocity(point);

			// The drag applied by Unity (so we don't re-apply it)
			// Δv_unity = -v * C_dUnity * Δt
			// (m/s) = (m/s) * (1/s) * (s)
			// v' = v - v * C_dUnity * (t' - t) 
			// v = e^[t - C_dUnity * (-0.5t² + t) + v_0]

			// Δv_unity = -v * C_dUnity * ρ_air / 1 kg * Δt
			// (m/s) = (m/s) * (1/s) * (kg/m³) (s)

			// f = ma
			// a = f/m
			// Δv = at
			// Δv = at 

			// rigidbody.drag = C_d * ρ_air * A * 0.5
			// dragFactor = C_d * A * 0.5


			float unityDragFactor = Mathf.Clamp01(dragCo * Time.fixedDeltaTime);
			float waterDragFactor = Mathf.Clamp01(dragCo * Mathf.Sqrt(m_FluidDensity / k_AirDensity) * Time.fixedDeltaTime);
			float newDragFactor = Mathf.Clamp01(waterDragFactor);// - unityDragFactor);
			Vector3 dragDeltaV = newDragFactor * pointAreaFactor * -velocity;

			rigidbody.AddForceAtPosition(dragDeltaV, point, ForceMode.VelocityChange);
			return dragDeltaV;
		}

		[SerializeField, ReadOnly] private List<Ray> m_Contacts  = new List<Ray>();
		[SerializeField, ReadOnly] private List<Ray> m_RawContacts = new List<Ray>();
		[SerializeField, ReadOnly] private List<Vector3> m_IgnoredPoints  = new List<Vector3>();
		[SerializeField, ReadOnly] private List<Vector3> m_Positions = new List<Vector3>();
		[SerializeField, ReadOnly] private List<Vector3> m_OtherPositions = new List<Vector3>();

		float GetWaterHeight(Vector3 pos)
		{
			return Mathf.Sin((pos.x + pos.z + m_FixedTime * WaveSpeed) / WaveDistance) * WaveAmplitude;
		}

		Vector3 GetWaterNormal(Vector3 pos)
		{
			Vector3 xTangent = new Vector3(1, Mathf.Cos((pos.x + pos.z + m_FixedTime * WaveSpeed) / WaveDistance) * WaveAmplitude, 0);
			Vector3 zTangent = new Vector3(0, Mathf.Cos((pos.x + pos.z + m_FixedTime * WaveSpeed) / WaveDistance) * WaveAmplitude, 1);
			return Vector3.Cross(zTangent.normalized, xTangent.normalized);
		}



		private static Mesh s_GizmoMesh;
		void OnDrawGizmos()
		{
			Gizmos.color = Color.black;
			foreach (var contact in m_RawContacts)
			{
				Gizmos.DrawWireSphere(contact.origin, 0.1f);
				Gizmos.DrawRay(contact);
			}
			Gizmos.color = Color.red;
			foreach (var contact in m_Contacts)
			{
				Gizmos.DrawWireSphere(contact.origin, 0.1f);
				Gizmos.DrawRay(contact);
			}
			Gizmos.color = Color.yellow;
			foreach (var point in m_IgnoredPoints)
			{
				Gizmos.DrawWireSphere(point, 0.1f);
			}
			Gizmos.color = Color.green;
			foreach (var point in m_Positions)
			{
				Gizmos.DrawWireSphere(point, 0.2f);
			}
			Gizmos.color = Color.cyan;
			foreach (var point in m_OtherPositions)
			{
				Gizmos.DrawWireSphere(point, 0.2f);
			}

			if (s_GizmoMesh == null)
			{
				s_GizmoMesh = new Mesh();
			}

			Gizmos.color = Color.blue;
			//var collider = m_Collider != null ? m_Collider : this.GetComponent<Collider>();
			Vector3 scale = this.transform.lossyScale * 10;
			int resolution = 20;
			int size = resolution * resolution;
			//var verts = new Vector3[size];
			for (int i = 0; i < size; i++)
			{
				float x = ((i % resolution) / (float)resolution - 0.5f) * scale.x;
				float z = ((i / resolution) / (float)resolution - 0.5f) * scale.z;
				Vector3 pos = new Vector3(x, 0, z);
				pos.y = GetWaterHeight(pos);
				Gizmos.DrawWireSphere(pos, 0.1f * this.transform.lossyScale.magnitude);
			}
		}
	}
}

