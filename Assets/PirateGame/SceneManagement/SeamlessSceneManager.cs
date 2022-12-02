using PirateGame.Weather;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.CustomUtils;
using UnityEngine.SceneManagement;

namespace PirateGame
{
	public class SeamlessSceneManager : GlobalSingleton<SeamlessSceneManager>
	{
		[SerializeField]
		private Scene m_LastScene;

		[SerializeField]
		private WeatherScene m_LastWeatherScene;

		[SerializeField]
		private bool m_IsLoadingScene = false;




		/// <summary>
		/// Load the given scene seamlessly
		/// </summary>
		/// <remarks>
		/// Example: <see cref="SeamlessSceneManager"/>.Instance.LoadSceneSeamless( <see cref="SceneManager"/>.GetSceneByBuildIndex(1) )
		/// </remarks>
		public void LoadSceneSeamless(int sceneIndex, GameObject[] keepObjects = null)
		{
			if (m_IsLoadingScene) return;

			var activeScene = SceneManager.GetActiveScene();
			if (m_LastScene.buildIndex == sceneIndex) return;

			// Get current scene info
			m_LastScene = activeScene;
			m_LastWeatherScene = Object.FindObjectOfType<WeatherScene>();

			// Start loading the next scene
			AsyncOperation loading = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
			loading.allowSceneActivation = true;
			loading.completed += (asyncOperation) => { OnSceneLoaded(sceneIndex, asyncOperation, keepObjects); };

			// and more stuff...
		}

		public void LoadSceneSeamless(string scenePath, GameObject[] keepObjects = null)
		{
			LoadSceneSeamless(SceneUtility.GetBuildIndexByScenePath(scenePath), keepObjects);
		}

		private void OnSceneLoaded(int sceneIndex, AsyncOperation loading, GameObject[] keepObjects = null)
		{
			Scene scene = SceneManager.GetSceneByBuildIndex(sceneIndex);

			// Activate the scene
			RemoveExistingPlayer(scene);
			SceneManager.SetActiveScene(scene);

			// Move objects over to new scene
			if (keepObjects != null)
			{
				foreach (GameObject go in keepObjects)
				{
					SceneManager.MoveGameObjectToScene(go, scene);
				}
			}

			// Transition weather
			if (m_LastWeatherScene != null)
			{
				m_LastWeatherScene.enabled = false;
			}
			
			var player = Object.FindObjectOfType<Player>();

			WeatherScene newWeatherScene = FindObjectOfTypeInScene<WeatherScene>(scene);
			WeatherManager.Instance.TransitionWeather(newWeatherScene.Parameters, player.Ship.Rigidbody.position);

			// Unload old scene
			SceneManager.UnloadSceneAsync(m_LastScene);

			player.ConstrainSeaToShip();

			m_IsLoadingScene = false;
		}

		private void RemoveExistingPlayer(Scene scene)
		{
			Player player = FindObjectOfTypeInScene<Player>(scene);
			if (player != null)
			{
				if (player.Ship != null)
				{
					Object.Destroy(player.Ship.gameObject);
				}
				Object.Destroy(player.gameObject);
			}
		}

		public T FindObjectOfTypeInScene<T>(Scene scene) 
			where T : Object
		{
			foreach (var gameObject in scene.GetRootGameObjects())
			{
				if (gameObject.TryGetComponent(out T obj))
				{
					return obj;
				}
			}

			foreach (var gameObject in scene.GetRootGameObjects())
			{
				T obj = gameObject.GetComponentInChildren<T>();
				if (obj != null)
				{
					return obj;
				}
			}

			return null;
		}
	}
}
