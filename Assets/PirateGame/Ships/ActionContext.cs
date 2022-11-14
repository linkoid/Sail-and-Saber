using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace PirateGame
{
	[RequireComponent(typeof(Collider))]
	public class ActionContext : MonoBehaviour
	{
		public HumanoidController ActivePlayer { get => _acivePlayer; private set => _acivePlayer = value; }
		[SerializeField, ReadOnly] private HumanoidController _acivePlayer;

		public UnityEvent<ActionContext, HumanoidController> OnEnter;
		public UnityEvent<ActionContext, HumanoidController> OnExit;

		public bool Enter(HumanoidController player)
		{
			if (ActivePlayer != null) return false;
			ActivePlayer = player;

			// ...

			OnEnter.Invoke(this, ActivePlayer);

			return true;
		}

		public bool Exit(HumanoidController player)
		{
			if (player != ActivePlayer) return false;

			// ...

			OnExit.Invoke(this, ActivePlayer);

			ActivePlayer = null;
			return true;
		}
	}
}



