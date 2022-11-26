using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CustomUtils;
using UnityEngine.SceneManagement;

namespace PirateGame
{
	[RequireComponent(typeof(Collider))]
	public class LoadingZone : MonoBehaviour
	{

		public void OnTriggerEnter(Collider other)
		{

		}

		public void LoadSceneSeamless(Scene scene)
		{
			// some stuff...

			AsyncOperation loading = SceneManager.LoadSceneAsync(scene.buildIndex, LoadSceneMode.Additive);
			loading.allowSceneActivation = true;
			loading.completed += (asyncOperation) => { OnSceneLoaded(scene, asyncOperation); };

			// and more stuff...
		}

		private void OnSceneLoaded(Scene scene, AsyncOperation asyncOperation)
		{
			throw new System.NotImplementedException();
		}
	}
}
