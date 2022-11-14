using System.Collections.Generic;
using UnityEngine;
using PirateGame.Ships;

namespace PirateGame.Crew
{
	/// <summary>
	/// Handles automated tasks, pathfinding, and animations for a crewmate.
	/// </summary>
	public class Crewmate : MonoBehaviour
	{
		/// <summary>
		/// Raid the specified ship, attacking the specified enemy
		/// </summary>
		public void Raid(Ship ship, Crewmate enemy)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Support the specified ship, at the specified supportObject.
		/// </summary>
		public void Support(Ship ship, GameObject supportObject)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Defend the specified ship, by attacking the specified enemy
		/// </summary>
		public void Defend(Ship ship, Crewmate enemy)
		{
			throw new System.NotImplementedException();
		}



		/// <summary>
		/// Board the specified ship, and wait at the specified standbyObject
		/// </summary>
		public void Board(Ship ship, GameObject standbyObject)
		{
			throw new System.NotImplementedException();
		}
	}
}
