using System.Collections.Generic;
using UnityEngine;
using PirateGame.Ships;

namespace PirateGame
{
	public class Fortress : Ship
	{
		public bool IsCaptured { get => _isCaptured; set => _isCaptured = value; }
		[SerializeField] private bool _isCaptured = false;

		public string fortName;
		private void Awake(){
			_isCaptured = PlayerPrefs.GetString(fortName) == "Captured" ? true : false;
			Debug.Log(_isCaptured);
		}

		public void Update(){
			if(IsCaptured){
				Capture();
			}
		}

		public void Capture(){
			PlayerPrefs.SetString(fortName,"Captured");
		}
	}
}
