using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PirateGame.Crew
{
	public class CrewDirector : MonoBehaviour, IReadOnlyCollection<Crewmate>
	{
		public PrefabList CrewmateVariants 
		{
			get => (_crewmateVariants != null) ? _crewmateVariants : Resources.Load<PrefabList>("CrewmateVariants");
			set => _crewmateVariants = value;
		}

		public Ships.Ship Ship { get => _ship; private set => _ship = value; }

		public int Count { get => m_Crewmates.Count; set => SetCount(value); }

		[SerializeField] private Ships.Ship _ship;
		[SerializeField] private PrefabList _crewmateVariants;


		[SerializeField] List<Crewmate> m_Crewmates = new List<Crewmate>();

		/// <summary>
		/// Raid the specified ship
		/// </summary>
		public void Raid(Ships.Ship ship)
		{
			var iter = m_Crewmates.Zip(ship.Crew, (a, b) => new { crewmate = a, enemy = b });
			foreach (var pair in iter)
			{
				pair.crewmate.Raid(ship, pair.enemy);
				pair.enemy.Defend(ship, pair.crewmate);
			}
		}

		/// <summary>
		/// Support the ship assigned to this crew
		/// </summary>
		public void Support()
		{
			// TODO : Find better support objects
			Component supportObject = Ship;

			foreach (var crewmate in m_Crewmates)
			{
				crewmate.Support(Ship, supportObject);
			}
		}

		/// <summary>
		/// Have each crewmate man a cannon in the enumerable.
		/// </summary>
		public void ManCannons(IEnumerable<Ships.Cannon> cannons)
		{
			// TODO : Find better support objects
			GameObject supportObject = Ship.gameObject;

			var iter = m_Crewmates.Zip(cannons, (a, b) => new { crewmate = a, cannon = b });
			foreach (var pair in iter)
			{
				pair.crewmate.Support(Ship, pair.cannon);
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
