using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PirateGame.Crew
{
	public class CrewDirector : MonoBehaviour, IReadOnlyCollection<Crewmate>, ICollection<Crewmate>
	{
		public PrefabList CrewmateVariants 
		{
			get => (_crewmateVariants != null) ? _crewmateVariants : Resources.Load<PrefabList>("CrewmateVariants");
			set => _crewmateVariants = value;
		}

		public Ships.Ship Ship { get => _ship; private set => _ship = value; }

		public int Count { get => m_Crewmates.Count; set => SetCount(value); }

		/// <summary>
		/// A read-only list of crewmates currently participating in a raid
		/// </summary>
		public IReadOnlyCollection<Crewmate> CrewRaid => m_CrewRaid.AsReadOnly();

		[SerializeField] private Ships.Ship _ship;
		[SerializeField] private PrefabList _crewmateVariants;
		//TEST
		//public Object objectRemove;
		//public int i;

		[SerializeField] List<Crewmate> m_Crewmates = new List<Crewmate>();
		[SerializeField] List<Crewmate> m_CrewRaid = new List<Crewmate>();

		/// <summary>
		/// Raid the specified ship
		/// </summary>
		public void Raid(Ships.Ship ship)
		{
            if(Ship.Crew == null || Ship.Crew.Count == 0)
            {
                throw new System.Exception("Player crew is NULL");
            }

            if(ship.Crew == null || ship.Crew.Count == 0)
            {
                throw new System.Exception("ENEMY crew is NULL");
            }

			var iter = m_Crewmates.Zip(ship.Crew, (a, b) => new { crewmate = a, enemy = b });
			m_CrewRaid.Clear();
            ship.Crew.m_CrewRaid.Clear();

            foreach (var pair in iter)
			{
				pair.crewmate.Raid(ship, pair.enemy);
				pair.enemy.Defend(ship, pair.crewmate);
				m_CrewRaid.Add(pair.crewmate);
                ship.Crew.m_CrewRaid.Add(pair.enemy);
			}
		}

		/// <summary>
		/// Attack the targeted enemy human
		/// </summary>
		public void Attack(int damage)
		{
			var toRemove = new List<Crewmate>();
			foreach (var crewmate in m_CrewRaid)
			{
				crewmate.TakeDamage(damage);
				if (crewmate.Health <= 0)
				{
					toRemove.Add(crewmate);
				}
			}

			foreach (var crewmate in toRemove)
			{
				Remove(crewmate, 4);
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
		


		/// <summary>
		/// Adds a random crewmate variant to the crew
		/// </summary>
		public void Add()
		{
			var prefab = CrewmateVariants[Random.Range(0, CrewmateVariants.Count)];
			var newCrewmate = Object.Instantiate(prefab, this.transform).GetComponent<Crewmate>();
			this.Add(newCrewmate);
		}

		/// <summary>
		/// Removes the most recent crewmate from the crew
		/// </summary>
		public void Remove()
		{
			RemoveAt(m_Crewmates.Count - 1);
		}


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



		#region ICollection<Crewmate> Implementation

		bool ICollection<Crewmate>.IsReadOnly => false;
		
		public bool Contains(Crewmate crewmate) => m_Crewmates.Contains(crewmate);
		public void CopyTo(Crewmate[] array, int arrayIndex) => m_Crewmates.CopyTo(array, arrayIndex);
		public IEnumerator<Crewmate> GetEnumerator() => ((IEnumerable<Crewmate>)m_Crewmates).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_Crewmates).GetEnumerator();

		public void Add(Crewmate crewmate)
		{
			if (m_Crewmates.Contains(crewmate))
			{
				Debug.LogWarning($"{this} already contains crewmate {crewmate}", this);
				return;
			}

			m_Crewmates.Add(crewmate);

			if (Ship != null)
			{
				crewmate.Board(Ship);
			}
		}

		/// <summary>
		/// Remove the crewmate at the specified index from the crew
		/// </summary>
		public void RemoveAt(int index)
		{
			var toRemove = m_Crewmates[index];
			Remove(toRemove);
		}

		/// <summary>
		/// Remove the specified crewmate from the crew
		/// </summary>
		/// <returns><c>true</c> if the crewmate was successfully removed from the crew</returns>
		public bool Remove(Crewmate crewmate)
		{
			return Remove(crewmate, 0);
		}

		/// <inheritdoc cref="Remove(Crewmate)"/>
		/// <param name="delay">An optional delay for destroying the gameObject</param>
		public bool Remove(Crewmate crewmate, float delay)
		{
			Object.Destroy(crewmate.gameObject, delay);
			if (m_CrewRaid.Contains(crewmate)) 
			{
				m_CrewRaid.Remove(crewmate);
			}
			return m_Crewmates.Remove(crewmate);
		}

		public void Clear()
		{
			while (m_Crewmates.Count > 0)
			{
				Remove();
			}
		}

		#endregion // ICollection<Crewmate> Implementation
	}
}
