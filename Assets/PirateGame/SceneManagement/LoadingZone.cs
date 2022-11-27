using PirateGame.Sea;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.CustomUtils;
using UnityEngine.SceneManagement;

namespace PirateGame
{
	[RequireComponent(typeof(Collider))]
	public class LoadingZone : MonoBehaviour
	{
		[SerializeField, Scene] private string m_Scene;

		public void OnTriggerEnter(Collider other)
		{
			var rigidbody = other.attachedRigidbody;
			if (rigidbody == null) return;

			var ship = rigidbody.GetComponent<Ships.Ship>();
			if (ship == null) return;

			var player = FindPlayerOfShip(ship);
			if (player == null) return;

			LoadScene(player, ship);
		}

		public Player FindPlayerOfShip(Ships.Ship ship)
		{
			var players = Object.FindObjectsOfType<Player>();
			foreach (var player in players)
			{
				if (player.Ship == ship) return player;
			}
			return null;
		}

		private void LoadScene(Player player, Ships.Ship ship)
		{
			GameObject[] keepObjects = new GameObject[]
			{
				player.gameObject,
				ship.gameObject,
				Object.FindObjectOfType<BuoyancyEffector>().gameObject,
			};

			SeamlessSceneManager.Instance.LoadSceneSeamless(m_Scene, keepObjects);
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			foreach (var collider in this.GetComponents<Collider>())
			{
				if (!collider.enabled) continue;
				if (!collider.isTrigger) continue;
				Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
			}
			Handles.Label(this.transform.position + new Vector3(0, 2, 0), 
				m_Scene,
				new GUIStyle()
				{
					fontStyle = FontStyle.Bold,
					normal = new GUIStyleState()
					{
						textColor = Color.black,
						background = Texture2D.blackTexture,
					},
					alignment = TextAnchor.LowerLeft,
				});
			//Gizmos.DrawIcon(this.transform.position, "LoadingZone Gizmo.png", false);
			
			if (this.gameObject.layer != LayerMask.NameToLayer("Inter-Ship Collision"))
			{
				Handles.Label(this.transform.position + new Vector3(0, -1, 0),
					"gameObject.layer != 'Inter-Ship Collision'",
					new GUIStyle() 
					{ 
						fontStyle = FontStyle.Bold,
						normal = new GUIStyleState()
						{
							textColor = Color.red
						},
						alignment = TextAnchor.LowerLeft,
					});
			}
		}
	}
}
