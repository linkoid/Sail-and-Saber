using PirateGame.Weather;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;
using static UnityEngine.InputManagerEntry;

namespace PirateGame.Sea
{
	public class BuoyancyEffector : MonoBehaviour
	{
		[SerializeField] public MeshRenderer WaterMesh;

		[SerializeField]
		public WaveParams Waves0 = new WaveParams()
		{
			Amplitude = 0.5f,
			Distance = 5,
			Speed = 5,
			Direction = new Vector3(1, 1).normalized,
		};

		[Tooltip("Density of the fluid in kg/m³")]
		[SerializeField] private float m_FluidDensity = 1000f;

		[SerializeField] private float m_DefaultSubmersionDepth = 1f;
		[SerializeField] private float m_MinimumDrag = 0.01f;

		[SerializeField, ReadOnly] private int m_ColliderID;

		[SerializeField] private Vector3 m_BoundsSize;

		[Header("Cached Values")]
		WaterForceJob.FrameInput m_FrameInput;

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
		public void UpdateWaveParameters()
		{
			Shader.SetGlobalFloat("WaveAmplitude", Waves0.Amplitude);
			Shader.SetGlobalFloat("WaveDistance", Waves0.Distance);
			Shader.SetGlobalFloat("WaveSpeed", Waves0.Speed);
			Shader.SetGlobalVector("WaveDirection", Waves0.Direction);
			Shader.SetGlobalFloat("WavePhaseOffset", Waves0.PhaseOffset);
		}
		
		void UpdateFrameInput()
		{
			m_FrameInput.gravity = Physics.gravity;
			m_FrameInput.gravityMagnitude = Physics.gravity.magnitude;
			m_FrameInput.gravityNormalized = Physics.gravity.normalized;
			m_FrameInput.fixedTime = Time.fixedTime;
			m_FrameInput.fixedDeltaTime = Time.fixedDeltaTime;
			m_FrameInput.Waves0 = Waves0;
			m_FrameInput.Waves0.Direction = Waves0.Direction.normalized;
			m_FrameInput.fluidDensity = m_FluidDensity;
			m_FrameInput.minimumDrag = m_MinimumDrag;
		}

		void FixedUpdate()
		{
			// Cache values
			UpdateFrameInput();


			// debug stuff
			m_RawContacts.Clear();
			m_Contacts.Clear();
			m_IgnoredPoints.Clear();
			m_Positions.Clear();
			m_OtherPositions.Clear();



			var pos = this.transform.position;
			pos.y = 0;
			var rot = this.transform.rotation;
			Collider[] colliders = Physics.OverlapBox(pos, m_BoundsSize, rot, GetIgnoreLayerCollisionMask());
			foreach (Collider collider in colliders)
			{
				if (collider.isTrigger) continue;
				if (collider.attachedRigidbody == null) continue;
				if (collider.attachedRigidbody.isKinematic) continue;
				if (collider.attachedRigidbody.isKinematic) continue;

				AddWaterForceAtCollider(collider.attachedRigidbody, collider);
			}
		}

		private int GetIgnoreLayerCollisionMask()
		{
			int layerMask = int.MaxValue;
			for (int i = 0; i < 32; i++)
			{
				if (Physics.GetIgnoreLayerCollision(this.gameObject.layer, i))
				{
					layerMask ^= 1 << i;
				}
			}
			return layerMask;
		}

		public struct VolumeTensor
		{
			public Vector3 point;
			public Vector3 normal;
			public float weight;
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

				Vector3 center0 = capsuleCollider.center + Vector3.up * centerOffset;
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
				float width = (corners[0].point - corners[2].point).magnitude;
				float height = (corners[0].point - corners[4].point).magnitude;
				volume = length * width * height;

				VolumeTensor[] faces = new VolumeTensor[6];
				for (int i = 0; i < faces.Length; i++)
				{
					Vector3 facePolarity = Vector3.zero;
					facePolarity[i / 2] = (i % 2 < 1) ? 1 : -1;

					faces[i].point = boxCollider.transform.TransformPoint(
						boxCollider.center + Vector3.Scale(boxCollider.size, facePolarity) * 0.5f);

					faces[i].normal = boxCollider.transform.TransformDirection(facePolarity);
				}

				tensors = new VolumeTensor[1 + 8 + 6];
				tensors[0].point = boxCollider.transform.TransformPoint(boxCollider.center);
				corners.CopyTo(tensors, 1);
				faces.CopyTo(tensors, 1 + 8);
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

			//Debug.Log("AddWaterForceAtCollider", collider);

			if (tensors.Length <= 0) return;

			float pointVolume = volume / tensors.Length;
			float pointAreaFactor = 1.0f / tensors.Length;
			float minHeight = tensors.Min((t) => Vector3.Dot(t.point, -m_FrameInput.gravityNormalized));
			float maxHeight = tensors.Max((t) => Vector3.Dot(t.point, -m_FrameInput.gravityNormalized));
			float submersionDepth = maxHeight - minHeight;

			if (submersionDepth <= 0)
			{
				Debug.LogWarning($"submersion depth is <= 0 for {collider}", collider);
				return;
			}

			for (int i = 0; i < tensors.Length; i++)
			{
				float depth = (maxHeight - tensors[i].point.y) / submersionDepth;
				tensors[i].weight = depth <= 0.5f ? 1 : depth * 2;
			}

			WaterForceJob.RigidbodyInput rigidbodyInput = new WaterForceJob.RigidbodyInput(rigidbody);

			WaterForceJob.ColliderInput colliderInput = new WaterForceJob.ColliderInput()
			{
				rigidbodyInput = rigidbodyInput,
			};


			List<WaterForceJob> jobs = new List<WaterForceJob>();
			List<JobHandle> jobHandles = new List<JobHandle>();

			var pointsInput = new NativeArray<WaterForceJob.PointInput>(tensors.Length, Allocator.TempJob);
			var pointsOutput = new NativeArray<WaterForceJob.PointOutput>(tensors.Length, Allocator.TempJob);
			var waterPointsInput = new NativeArray<Vector3>(0, Allocator.TempJob);
			var waterPointsOutput = new NativeArray<Vector3>(0, Allocator.TempJob);

			float weightsSum = tensors.Sum((t) => t.weight);
			for (int i = 0; i < tensors.Length; i++)
			{
				var tensor = tensors[i];
				float pointVolumeFactor = tensor.weight / weightsSum;
				WaterForceJob.PointInput pointInput = new WaterForceJob.PointInput
				{
					velocity = rigidbody.GetPointVelocity(tensor.point),
					tensor = tensor,
					pointVolume = volume * pointVolumeFactor,
					pointAreaFactor = pointAreaFactor,
					submersionDepth = submersionDepth,
				};
				pointsInput[i] = pointInput;
			}

			WaterForceJob job = new WaterForceJob()
			{
				frameInput = m_FrameInput,
				colliderInput = colliderInput,
				pointsInput = pointsInput,
				pointsOutput = pointsOutput,
				waterPointsInput = waterPointsInput,
				waterPointsOutput = waterPointsOutput,
				waterHeightOnly = false,
			};

			job.Schedule().Complete();

			job.ApplyForcesToRigidbody(rigidbody);
#if UNITY_EDITOR
			job.CopyToGizmos(m_Contacts, m_IgnoredPoints);
#endif

			job.pointsInput.Dispose();
			job.pointsOutput.Dispose();
			job.waterPointsInput.Dispose();
			job.waterPointsOutput.Dispose();
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

		[SerializeField, ReadOnly] private List<Ray> m_Contacts = new List<Ray>();
		[SerializeField, ReadOnly] private List<Ray> m_RawContacts = new List<Ray>();
		[SerializeField, ReadOnly] private List<Vector3> m_IgnoredPoints = new List<Vector3>();
		[SerializeField, ReadOnly] private List<Vector3> m_Positions = new List<Vector3>();
		[SerializeField, ReadOnly] private List<Vector3> m_OtherPositions = new List<Vector3>();



		float GetWaterHeight(Vector3 pos)
		{
			var frameInput = m_FrameInput;
			Vector2 dir = frameInput.Waves0.Direction;
			return Mathf.Sin((pos.x * dir.x + pos.z * dir.y + frameInput.fixedTime * frameInput.Waves0.Speed) / frameInput.Waves0.Distance) * frameInput.Waves0.Amplitude;
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
			Vector3 scale = m_BoundsSize / 10;
			int resolution = 50;
			int size = resolution * resolution;


			UpdateFrameInput();
			m_FrameInput.fixedTime = Time.time;

			WaterForceJob job = new WaterForceJob()
			{
				waterHeightOnly = true,
				frameInput = m_FrameInput,
				waterPointsInput = new NativeArray<Vector3>(size, Allocator.Persistent),
				waterPointsOutput = new NativeArray<Vector3>(size, Allocator.Persistent),
			};


			//var verts = new Vector3[size];
			for (int i = 0; i < size; i++)
			{
				float x = ((i % resolution) / (float)resolution - 0.5f) * scale.x;
				float z = ((i / resolution) / (float)resolution - 0.5f) * scale.z;
				Vector3 pos = new Vector3(x, 0, z);
				job.waterPointsInput[i] = pos;
				pos.y = GetWaterHeight(pos);
				//Gizmos.DrawWireSphere(pos, 0.003f * scale.magnitude);
			}

			//job.Schedule().Complete();


			Gizmos.color = Color.green;
			foreach (var point in job.waterPointsOutput)
			{
				Gizmos.DrawWireSphere(point, 0.0031f * scale.magnitude);
			}

			job.waterPointsInput.Dispose();
			job.waterPointsOutput.Dispose();
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(this.transform.position, m_BoundsSize);
			//Gizmos.DrawWireCube(this.transform.position, Vector3.Scale(m_BoundsSize, Vector3.up));
			//Gizmos.DrawWireCube(this.transform.position, Vector3.Scale(m_BoundsSize, Vector3.right));
			//Gizmos.DrawWireCube(this.transform.position, Vector3.Scale(m_BoundsSize, Vector3.forward));
			Gizmos.DrawWireCube(this.transform.position, Vector3.Scale(m_BoundsSize, Vector3.up + Vector3.right));
			Gizmos.DrawWireCube(this.transform.position, Vector3.Scale(m_BoundsSize, Vector3.right + Vector3.forward));
			Gizmos.DrawWireCube(this.transform.position, Vector3.Scale(m_BoundsSize, Vector3.forward + Vector3.up));
		}




		[BurstCompile]
		public struct WaterForceJob : IJob
		{
			public struct FrameInput
			{
				public WaveParams Waves0;
				public Vector3 gravity;
				public Vector3 gravityNormalized;
				public float gravityMagnitude;
				public float fixedTime;
				public float fixedDeltaTime;
				public float fluidDensity;
				public float minimumDrag;

				[BurstDiscard]
				public override string ToString()
				{
					return $"Waves0 = {Waves0}\n"
						 + $"gravity = {gravity}\n"
						 + $"gravityNormalized = {gravityNormalized}\n"
						 + $"gravityMagnitude = {gravityMagnitude}\n"
						 + $"fixedTime = {fixedTime}\n"
						 + $"fixedDeltaTime = {fixedDeltaTime}\n"
						 + $"fluidDensity = {fluidDensity}\n"
						 + $"minimumDrag = {minimumDrag}\n";
				}
			}

			public struct RigidbodyInput
			{
				public int instanceID;
				public Matrix4x4 transform;
				public Matrix4x4 inverseTransform;
				public Quaternion inertiaTensorRotation;
				public Vector3 inertiaTensor;
				public float mass;
				public Vector3 centerOfMass;
				public float drag;
				public float angularDrag;

				public RigidbodyInput(Rigidbody r)
				{
					instanceID = r.GetInstanceID();
					transform = r.transform.localToWorldMatrix;
					inverseTransform = r.transform.worldToLocalMatrix;
					inertiaTensorRotation = r.inertiaTensorRotation;
					inertiaTensor = r.inertiaTensor;
					mass = r.mass;
					centerOfMass = r.centerOfMass;
					drag = r.drag;
					angularDrag = r.angularDrag;
				}

				[BurstDiscard]
				public override string ToString()
				{
					return $"instanceID = {instanceID}\n"
					     + $"transform = {transform}\n"
					     + $"inverseTransform = {inverseTransform}\n"
					     + $"inertiaTensorRotation = {inertiaTensorRotation}\n"
					     + $"inertiaTensor = {inertiaTensor}\n"
					     + $"mass = {mass}\n"
					     + $"centerOfMass = {centerOfMass}\n"
					     + $"drag = {drag}\n"
					     + $"angularDrag = {angularDrag}\n";
				}
			}

			public struct ColliderInput
			{
				public RigidbodyInput rigidbodyInput;
			}

			public struct PointInput
			{
				public Vector3 velocity;
				public VolumeTensor tensor;
				public float pointVolume;
				public float pointAreaFactor;
				public float submersionDepth;

				[BurstDiscard]
				public override string ToString()
				{
					return $"velocity = {velocity}\n"
						 + $"tensor = {tensor}\n"
						 + $"pointVolume = {pointVolume}\n"
						 + $"pointAreaFactor = {pointAreaFactor}\n"
						 + $"submersionDepth = {submersionDepth}\n";
				}
			}

			public struct PointOutput
			{
				public Vector3 position;
				public Vector3 buoyancy;
				public Vector3 drag    ;

				public readonly static PointOutput zero = new PointOutput
				{
					position = Vector3.zero,
					buoyancy = Vector3.zero,
					drag     = Vector3.zero,
				};

				public static bool operator ==(PointOutput a, PointOutput b)
				{
					return a.position == b.position
						&& a.buoyancy == b.buoyancy 
					    && a.drag     == b.drag    ;
				}
				public static bool operator !=(PointOutput a, PointOutput b)
				{
					return a.position != b.position
					    || a.buoyancy != b.buoyancy 
					    || a.drag     != b.drag    ;
				}
			}

			[Unity.Collections.ReadOnly ] public FrameInput frameInput;
			[Unity.Collections.ReadOnly ] public ColliderInput colliderInput;
			[Unity.Collections.ReadOnly ] public NativeArray<PointInput> pointsInput;
			[Unity.Collections.WriteOnly] public NativeArray<PointOutput> pointsOutput;


			[Unity.Collections.ReadOnly ] public bool waterHeightOnly;
			[Unity.Collections.ReadOnly ] public NativeArray<Vector3> waterPointsInput;
			[Unity.Collections.WriteOnly] public NativeArray<Vector3> waterPointsOutput;

			public void Execute()
			{
				if (waterHeightOnly)
				{
					for (int i=0; i<waterPointsInput.Length; i++)
					{
						var point = waterPointsInput[i];
						float height = GetWaterHeight(point);
						waterPointsOutput[i] = new Vector3(point.x, height, point.z);
					}
				}
				else
				{
					for (int i=0; i < pointsInput.Length; i++)
					{
						pointsOutput[i] = GetWaterForceAtPoint(i);
					}
				}
			}

			PointOutput GetWaterForceAtPoint(int i)
			{
				Vector3 point = pointsInput[i].tensor.point;
				float waterHeight = GetWaterHeight(point);
				if (waterHeight < point.y)
				{
					return PointOutput.zero;
				}

				float submersion = waterHeight - point.y;
				float submersionFactor = Mathf.Clamp01(submersion / pointsInput[i].submersionDepth);


				float displacedVolume = submersionFactor * pointsInput[i].pointVolume;

				Vector3 buoyancy = GetBuoyancyAtPoint(i, displacedVolume);
				Vector3 drag = GetDragAtPoint(i);

				return new PointOutput()
				{
					position = point   ,
					buoyancy = buoyancy,
					drag     = drag    ,
				};
			}

			Vector3 GetBuoyancyAtPoint(int i, float displacedVolume)
			{
				var tensor = pointsInput[i].tensor;
				Vector3 waveNormal = GetWaterNormal(tensor.point);

				// Buoyancy B = ρ_f * V_disp * -g
				float adjust = frameInput.gravityMagnitude - 1;
				Vector3 buoyancy = frameInput.fluidDensity * displacedVolume * (-frameInput.gravity * adjust + waveNormal);
				Vector3 buoyantForce = Vector3.zero;
				if (tensor.normal.sqrMagnitude == 0)
				{
					buoyantForce = buoyancy;
				}
				else if (Vector3.Dot(buoyancy.normalized, -tensor.normal.normalized) > 0)
				{
					buoyantForce = Vector3.Project(buoyancy, -tensor.normal);
				}
				return buoyantForce;
			}

			const float k_AirDensity = 1.204f; // kg/m³
			Vector3 GetDragAtPoint(int i)
			{
				var rigidbodyInput = colliderInput.rigidbodyInput;
				var point = pointsInput[i].tensor.point;
				var pointAreaFactor = pointsInput[i].pointAreaFactor;

				// Drag D = C_d * ρ_fluid * A * 0.5 * v²
				// Drag (N/s²) = (1) * (kg/m³) * (m²) * 0.5 * (m²/s²)

				Vector3 centerOffset = rigidbodyInput.inverseTransform.MultiplyPoint(point) - rigidbodyInput.centerOfMass;
				Vector3 tensorSpaceOffset = Quaternion.Inverse(rigidbodyInput.inertiaTensorRotation) * centerOffset;
				float momentArm = Vector3.Scale(tensorSpaceOffset.normalized, rigidbodyInput.inertiaTensor).magnitude / rigidbodyInput.mass;
				float momentFactor = tensorSpaceOffset.sqrMagnitude / momentArm;

				float dragCo = Mathf.Lerp(rigidbodyInput.drag, rigidbodyInput.angularDrag, momentFactor);
				dragCo = Mathf.Max(dragCo, frameInput.minimumDrag);

				Vector3 velocity = pointsInput[i].velocity;

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


				float unityDragFactor = Mathf.Clamp01(dragCo * frameInput.fixedDeltaTime);
				float waterDragFactor = Mathf.Clamp01(dragCo * Mathf.Sqrt(frameInput.fluidDensity / k_AirDensity) * frameInput.fixedDeltaTime);
				float newDragFactor = Mathf.Clamp01(waterDragFactor);// - unityDragFactor);
				Vector3 dragDeltaV = newDragFactor * pointAreaFactor * -velocity;

				return dragDeltaV;
			}

			float GetWaterHeight(Vector3 pos)
			{
				return frameInput.Waves0.Position(frameInput.fixedTime, pos).y;
			}

			Vector3 GetWaterNormal(Vector3 pos)
			{
				float phase = frameInput.Waves0.Phase(frameInput.fixedTime, pos);
				Vector3 xTangent = new Vector3(1, Mathf.Cos(phase) * frameInput.Waves0.Amplitude, 0);
				Vector3 zTangent = new Vector3(0, Mathf.Cos(phase) * frameInput.Waves0.Amplitude, 1);
				return Vector3.Cross(zTangent.normalized, xTangent.normalized);
			}

			[BurstDiscard]
			public void ApplyForcesToRigidbody(Rigidbody rigidbody)
			{
				if (rigidbody.GetInstanceID() != colliderInput.rigidbodyInput.instanceID)
				{
					throw new System.InvalidOperationException("Rigidbody instance ID does not match");
				}

				// Making a managed copy of the native array before loopinhg over it greatly improves performance
				PointOutput[] pointsOutput = new PointOutput[this.pointsOutput.Length];
				this.pointsOutput.CopyTo(pointsOutput);

				foreach (var pointOutput in pointsOutput)
				{
					//var pointOutput = pointsOutput[i];
					rigidbody.AddForceAtPosition(pointOutput.buoyancy, pointOutput.position, ForceMode.Force         );
					rigidbody.AddForceAtPosition(pointOutput.drag    , pointOutput.position, ForceMode.VelocityChange);
				}
			}

			[BurstDiscard]
			public void CopyToGizmos(List<Ray> contacts, List<Vector3> ignored)
			{
				// Making a managed copy of the native array before loopinhg over it greatly improves performance
				PointOutput[] pointsOutput = new PointOutput[this.pointsOutput.Length];
				this.pointsOutput.CopyTo(pointsOutput);

				foreach (var pointOutput in pointsOutput)
				{
					//var pointOutput = pointsOutput[i];
					if (pointOutput == PointOutput.zero)
					{
						ignored.Add(pointOutput.position);
					}
					else
					{
						contacts.Add(new Ray(pointOutput.position, pointOutput.drag + pointOutput.buoyancy));
					}
				}
			}
		}
	}
}

