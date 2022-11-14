using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Crew
{
	public class CrewDirector : MonoBehaviour
	{
		public Ships.Ship Ship { get => _ship; private set => _ship = value; }
		[SerializeField] private Ships.Ship _ship;


		[SerializeField] List<Crewmate> m_Crewmates;

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
				throw new System.NotImplementedException();
			}
		}
	}
}
