using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PirateGame.Crew
{
	[System.Serializable]
	public abstract class Command : MonoBehaviour
	{
		public abstract string DisplayName { get; }

		public bool IsActive { get => _isActive; private set => _isActive = value; }
		[SerializeField, ReadOnly] private bool _isActive = false;

        public Button ButtonObject;

        public Commander Commander
		{
			get
			{
				var commander = this.GetComponentInParent<Commander>();
				if (commander == null)
				{
					try
					{
						throw new MissingComponentException($"Could not find component {typeof(Commander)} in parents");
					}
					catch (MissingComponentException e)
					{
						Debug.LogException(e, this);
					}
				}
				return commander;
			}
		}

		protected virtual void Awake()
		{
			IsActive = false;
			ButtonObject = this.GetComponentInChildren<Button>();
			if (ButtonObject == null)
			{
				throw new MissingComponentException($"Could not find component {typeof(Button)} in children");
			}
			ButtonObject.onClick.AddListener(Execute);
		}

		/// <summary>
		/// Check if the command can be executed in the current context.
		/// </summary>
		/// <remarks>
		/// Run our check here with like a switch statement or something for various commands.
		/// Is a command only a valid target when targetting an enemy, self, or another object?
		/// </remarks>
		public abstract bool Poll();

		public virtual void Execute()
		{
			// XXX Consider moving this to Poll(). Some commands may be executable, even if already executing.
			if (IsActive) return; // Don't execute if already executing. 

			if (!Poll())
			{
				try
				{
					throw new System.InvalidOperationException($"Could not execute command {this.GetType().Name}. The context is incorrect; Poll() returned false.");
				}
				catch (System.Exception e) 
				{
					Debug.LogException(e, Commander);
				}
				return;
			}

			IsActive = true;
			OnExecute();
		}

		/// <summary>
		/// Stops the command
		/// </summary>
		public void Cancel()
		{
			IsActive = false;
			OnCancel();
		}

		/// <summary>
		/// What to do when the command is executed.
		/// </summary>
		protected abstract void OnExecute();

		/// <summary>
		/// What to do when the command is canceled.
		/// </summary>
		protected abstract void OnCancel();

		/// <summary>
		/// Run on every frame
		/// </summary>
		protected virtual void Update() 
		{
			UpdateButtonInteractable();
		}

		protected virtual void UpdateButtonInteractable()
		{
			ButtonObject.interactable = Poll();
		}
	}
}