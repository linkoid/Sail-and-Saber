using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace PirateGame
{
	[RequireComponent(typeof(Collider))]
	public class ActionContext : MonoBehaviour
	{
		public PlayerController ActivePlayer { get => _acivePlayer; private set => _acivePlayer = value; }
		[SerializeField, ReadOnly] private PlayerController _acivePlayer;

		public UnityEvent<ActionContext, PlayerController> OnEnter;
		public UnityEvent<ActionContext, PlayerController> OnExit;

		public bool Enter(PlayerController player)
		{
			if (ActivePlayer != null) return false;
			ActivePlayer = player;

			// ...

			OnEnter.Invoke(this, ActivePlayer);

			return true;
		}

		public bool Exit(PlayerController player)
		{
			if (player != ActivePlayer) return false;

			// ...

			OnExit.Invoke(this, ActivePlayer);

			ActivePlayer = null;
			return true;
		}
	}
}



