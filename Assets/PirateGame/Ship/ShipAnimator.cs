using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PirateGame.Ships
{
	public class ShipAnimator : MonoBehaviour
	{

		//Model and its animator
		[SerializeField] private GameObject m_Model;
		public Animator Animator => m_Model.GetComponent<Animator>();

		//Animator Parameters
		int rotationHash = Animator.StringToHash("rotation");

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			// Set Animator Parameter
			Animator.SetFloat(rotationHash, 1); //m_Steering);
		}
	}
}

