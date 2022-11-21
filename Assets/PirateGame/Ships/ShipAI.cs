using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PirateGame.Crew;

namespace PirateGame.Ships
{
	public class ShipAI : Ship.ShipBehaviour
	{
		[SerializeField]
		private int m_CrewCount = 5;

		[SerializeField]
		private CrewDirector m_Crew;


		// Start is called before the first frame update
		void Start()
		{
			CreateCrew();
			this.Ship.AssignCrew(m_Crew);
			m_Crew.Board(this.Ship);
		}

		private CrewDirector CreateCrew()
		{
			if (m_Crew == null)
			{
				m_Crew = new GameObject("EnemyCrew", typeof(CrewDirector)).GetComponent<CrewDirector>();
				m_Crew.transform.parent = this.transform;
			}
			m_Crew.Count = m_CrewCount;
			this.Ship.AssignCrew(m_Crew);
			m_Crew.Board(this.Ship);
			return m_Crew;
		}

		// Update is called once per frame
		void Update()
		{
			
		}

		protected override void OnRaided()
		{
			base.OnRaided();
			CreateCrew();
		}

		protected override void OnSink()
		{
			if (m_Crew != null)
			{
				Object.Destroy(m_Crew.gameObject);
			}
		}
	}
}

