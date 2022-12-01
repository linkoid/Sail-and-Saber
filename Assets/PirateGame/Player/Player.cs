using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using PirateGame.Crew;
using PirateGame.Ships;
using PirateGame.Sea;
using UnityEngine.Animations;
using Unity.VisualScripting;

namespace PirateGame
{
	public class Player : MonoBehaviour
	{
		[Header("Required")]
		[SerializeField] private Camera m_Camera;
		
		public long Gold      { get => m_Stats.Gold  ; set => FieldChangedCheck(ref m_Stats.Gold     , value, OnGoldChanged     ); }
		public int  CrewCount { get => GetCrewCount(); set => FieldChangedCheck(ref m_Stats.CrewCount, value, OnCrewCountChanged); }
		
		public Ship Ship   { get => m_Ship  ; set => FieldChangedCheck(ref m_Ship  , value, OnShipChanged  ); }
		public Ship Target { get => m_Target; set => FieldChangedCheck(ref m_Target, value, OnTargetChanged); }

		public CrewDirector Crew { get => (m_Crew != null) ? m_Crew : CreateCrew(); }
		public Commander Commander { get => m_Commander; private set => m_Commander = value; }

		[Header("Dynamic")]
		[SerializeField] private PlayerStats m_Stats;

		[SerializeField] private Ship m_Ship ;
		[SerializeField] private Ship m_Target;

		[SerializeField] private CrewDirector m_Crew;
		[SerializeField] private Commander m_Commander;

		[SerializeField] private SoundEffect m_GoldSound;

		[SerializeField, ReadOnly, Tooltip("Available in Unity 2023.1")]
		protected bool didStart = false;

		// Start is called before the first frame update
		void Start()
		{
			// Handle initial values
			Initialize();

			didStart = true;
		}
		

		// Update is called once per frame
		void Update()
		{
			if (m_DoInitialize)
			{
				Initialize();
				m_DoInitialize = false;
			}
			Target = Ship.Internal.Combat.TargetNearestShip();
		}

		private bool m_DoInitialize = false;
		void OnValidate()
		{
			if (Application.isPlaying)
			{
				m_DoInitialize = true;
			}
		}

		/// <summary>
		/// Calls OnChanged() for values assigned via inspector
		/// </summary>
		private void Initialize()
		{
			if (m_Stats.Gold      != 0) OnGoldChanged     (0);
			if (m_Stats.CrewCount != 0) OnCrewCountChanged(0);

			if (Ship   != null) OnShipChanged  (null);
			if (Target != null) OnTargetChanged(null);
		}

		private CrewDirector CreateCrew()
		{
			m_Crew = new GameObject("Crew", typeof(CrewDirector)).GetComponent<CrewDirector>();
			m_Crew.transform.parent = this.transform;
			m_Crew.Count = m_Stats.CrewCount;
			return m_Crew;
		}
		private int GetCrewCount()
		{
			int crewCount = 0;
			if (m_Crew != null)
			{
				// If a crew exists, redirect the value
				crewCount = m_Crew.Count;
			}
			else
			{
				// Otherwise, just use the value in m_Stats
				crewCount = m_Stats.CrewCount;
			}
			// Always ensure the value in stats is synced with m_Crew.Count
			m_Stats.CrewCount = crewCount;
			return crewCount;
		}

		/// <summary>
		/// If the value is different from field, assigns value to field and invokes onChanged.
		/// </summary>
		/// <returns>true if the field was changed</returns>
		private bool FieldChangedCheck<T>(ref T field, T value, System.Action<T> onChanged)
		{
			if(field == null && value == null) return false;
			if (field != null && field.Equals(value)) return false;

			T oldValue = field;
			field = value;

			if (!didStart) return true;

			try
			{
				onChanged.Invoke(oldValue);
			}
			catch (System.Exception e)
			{
				Debug.LogException(e, this);
			}

			return true;
		}

		private void OnGoldChanged(long oldGold)
		{
			if (oldGold < Gold)
			{
				IEnumerator RepeatGoldSound()
				{
					var newGold = Gold;
					for (long i = oldGold; i < newGold; i++)
					{
						m_GoldSound.Play();
						yield return new WaitForSecondsRealtime(0.05f);
					}
				}
				StartCoroutine(RepeatGoldSound());
			}
		}
		private void OnCrewCountChanged(int oldCrewCount)
		{
			if (m_Crew == null) return;
			if (m_Stats.CrewCount == m_Crew.Count) return;
			m_Crew.Count = m_Stats.CrewCount;
		}
		private void OnShipChanged(Ship oldShip)
		{
			Ship.AssignCrew(Crew);
			if (Crew.Ship != Ship)
			{
				Crew.Board(Ship);
			}
			ConstrainSeaToShip();
		}
		private void OnTargetChanged(Ship oldTarget)
		{
			Ship.Internal.Combat.Target = Target;
		}
		
		public void ConstrainSeaToShip()
		{
			var sea = Object.FindObjectOfType<BuoyancyEffector>();
			if (sea != null)
			{
				var constraint = sea.WaterMesh.GetComponent<PositionConstraint>();
				if (constraint == null)
				{
					constraint = sea.WaterMesh.gameObject.AddComponent<PositionConstraint>();
				}
				constraint.constraintActive = true;
				constraint.translationAxis = Axis.X | Axis.Z;
				constraint.weight = 1;
				constraint.SetSources(new List<ConstraintSource> 
				{ 
					new ConstraintSource 
					{ 
						sourceTransform = Ship.transform,
						weight = 0.5f,
					},
					new ConstraintSource
					{
						sourceTransform = m_Camera.transform,
						weight = 0.5f,
					},
				});
			}
		}
	}

	[System.Serializable]
	public struct PlayerStats
	{
		public long Gold;
		public int CrewCount;
	}
}