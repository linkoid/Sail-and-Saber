using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CustomUtils;
using UnityEngine.SceneManagement;

namespace PirateGame
{
	public class SeamlessSceneManager : GlobalSingleton<SeamlessSceneManager>
	{
		/// <summary>
		/// Load the given scene seamlessly
		/// </summary>
		/// <remarks>
		/// Example: <see cref="SeamlessSceneManager"/>.Instance.LoadSceneSeamless( <see cref="SceneManager"/>.GetSceneByBuildIndex(1) )
		/// </remarks>
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
