using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Ships
{
	public class ShipAnimator : Ship.ShipBehaviour
	{
		// Model and its animator
		[SerializeField] private GameObject m_Model;
		public Animator Animator => m_Model.GetComponent<Animator>();

		// Animator Parameters
		static readonly int s_RotationHash = Animator.StringToHash("rotation");

		// Update is called once per frame
		void Update()
		{
			// Set Animator Parameter
			Animator.SetFloat(s_RotationHash, Ship.Internal.Physics.Steering);
		}
	}
}

