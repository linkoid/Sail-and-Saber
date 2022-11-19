using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using PirateGame.Crew;
using PirateGame.Ships;


namespace PirateGame
{
	public class Player : MonoBehaviour
	{
		public long Gold      { get => m_Stats.Gold  ; set => FieldChangedCheck(ref m_Stats.Gold     , value, OnGoldChanged     ); }
		public int  CrewCount { get => GetCrewCount(); set => FieldChangedCheck(ref m_Stats.CrewCount, value, OnCrewCountChanged); }
		
		public Ship Ship   { get => m_Ship  ; set => FieldChangedCheck(ref m_Ship  , value, OnShipChanged  ); }
		public Ship Target { get => m_Target; set => FieldChangedCheck(ref m_Target, value, OnTargetChanged); }

		public CrewDirector Crew { get => (m_Crew != null) ? m_Crew : CreateCrew(); }
		public Commander Commander { get => m_Commander; private set => m_Commander = value; }

		[SerializeField] private PlayerStats m_Stats;

		[SerializeField] private Ship m_Ship  ;
		[SerializeField] private Ship m_Target;

		[SerializeField] private CrewDirector m_Crew;
		[SerializeField] private Commander m_Commander;

		[SerializeField, ReadOnly]
		// Available in Unity 2023.1
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

		}

		void OnValidate()
		{
			if (Application.isPlaying)
			{
				Initialize();
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
			if (field.Equals(value)) return false;

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
		}
		private void OnTargetChanged(Ship oldTarget)
		{
		}
	}

	[System.Serializable]
	public struct PlayerStats
	{
		public long Gold;
		public int CrewCount;
	}
}