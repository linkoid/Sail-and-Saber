using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.ParticleSystem;

namespace PirateGame.Ships
{
	public class CannonGroup : MonoBehaviour, IReadOnlyList<Cannon>
	{
		public bool UseSharedSettings = true;

		public Vector2 SharedAngle = new Vector2(90, 30);
		public float SharedRange = 50;


		[SerializeField, ReadOnly] private List<Cannon> m_CannonList = new List<Cannon>();
		
		[ExecuteAlways]
		void Awake()
		{
			FindCannonChildren();
		}

		[ExecuteAlways]
		void OnTransformChildrenChanged()
		{
			FindCannonChildren();
		}

		void OnValidate()
		{
			FindCannonChildren();
		}

		private void FindCannonChildren()
		{
			m_CannonList.Clear();
			m_CannonList.AddRange(this.GetComponentsInChildren<Cannon>());
			UpdateChildren();
		}
		
		private void UpdateChildren()
		{
			if (!UseSharedSettings) return;

			foreach (var cannon in m_CannonList)
			{
				cannon.Angle = SharedAngle;
				cannon.Range = SharedRange;
			}
		}

		public IEnumerable<Cannon> GetAllInRange(Vector3 target)
		{
			foreach (var cannon in m_CannonList)
			{
				if (cannon.CheckInRange(target))
				{
					yield return cannon;
				}
			}
		}

		public Cannon GetFirstInRange(Vector3 target)
		{
			foreach (var cannon in GetAllInRange(target))
			{
				return cannon;
			}
			return null;
		}


		#region IReadOnlyList

		public Cannon this[int index] => m_CannonList[index];
		public int Count => m_CannonList.Count;
		public IEnumerator<Cannon> GetEnumerator() => m_CannonList.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_CannonList).GetEnumerator();
		
		#endregion // IReadOnlyList
	}
}

