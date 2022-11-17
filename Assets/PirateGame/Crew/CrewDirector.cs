using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace PirateGame.Crew
{
	public class CrewDirector : MonoBehaviour, IReadOnlyCollection<Crewmate>
	{
		private static PrefabList CrewmateVariants => Resources.Load<PrefabList>("CrewmateVariants");

		public Ships.Ship Ship { get => _ship; private set => _ship = value; }

		public int Count { get => m_Crewmates.Count; set => SetCount(value); }

		[SerializeField] private Ships.Ship _ship;


		[SerializeField] List<Crewmate> m_Crewmates = new List<Crewmate>();

		/// <summary>
		/// Raid the specified ship
		/// </summary>
		public void Raid(Ships.Ship ship)
		{
			foreach (var crewmate in m_Crewmates)
			{
				throw new System.NotImplementedException();
			}
		}

		/// <summary>
		/// Support the ship assigned to this crew
		/// </summary>
		public void Support()
		{
			// TODO : Find better support objects
			GameObject supportObject = Ship.gameObject;

			foreach (var crewmate in m_Crewmates)
			{
				crewmate.Support(Ship, supportObject);
			}
		}

		/// <summary>
		/// Defend the ship assigned to this crew from the specified enemyCrew
		/// </summary>
		public void Defend(CrewDirector enemyCrew)
		{
			foreach (var crewmate in m_Crewmates)
			{
				throw new System.NotImplementedException();
			}
		}

		/// <summary>
		/// Board the specified ship. 
		/// Also assigns the CrewDirector's Ship.
		/// </summary>
		public void Board(Ships.Ship ship)
		{
			Ship = ship;

			foreach (var crewmate in m_Crewmates)
			{
				// XXX : could find better standby objects
				crewmate.Board(Ship);
			}
		}

		public IEnumerator<Crewmate> GetEnumerator() => ((IEnumerable<Crewmate>)m_Crewmates).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_Crewmates).GetEnumerator();

		private void SetCount(int newCount)
		{
			while (m_Crewmates.Count < newCount)
			{
				Add();
			}
			while (m_Crewmates.Count > newCount)
			{
				Remove();
			}
		}

		/// <summary>
		/// Adds a random crewmate variant to the crew
		/// </summary>
		public void Add()
		{
			var prefab = CrewmateVariants[Random.Range(0, CrewmateVariants.Count)];
			var newCrewmate = Object.Instantiate(prefab, this.transform).GetComponent<Crewmate>();
			m_Crewmates.Add(newCrewmate);

			if (Ship != null)
			{
				newCrewmate.Board(Ship);
			}
		}

		/// <summary>
		/// Removes the most recent crewmate from the crew
		/// </summary>
		public void Remove()
		{
			RemoveAt(m_Crewmates.Count - 1);
		}

		public void RemoveAt(int index)
		{
			var toRemove = m_Crewmates[index];
			m_Crewmates.Remove(toRemove);
			Object.Destroy(toRemove.gameObject);
		}
	}
}
