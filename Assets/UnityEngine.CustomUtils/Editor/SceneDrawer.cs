// source : https://stewmcc.com/post/unity-property-drawers-scene-enum/
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.CustomUtils;

/// <summary>
/// Scene Drawer, strings tagged [Scene] now show Scene selection instead of strings.
/// <para/>
/// Use [SerializeField, Scene] to display theScene selection window instead of a string input box.
/// This property drawer will also check if the selected scene has already been added to 'Scenes in the Build'
/// if not it will warn the user.
/// Set strings to "" for the property drawer to correctly select 'none' without display error logs.
/// </summary>
[CustomPropertyDrawer(typeof(SceneAttribute))]
public class SceneDrawer : PropertyDrawer
{

	/// <summary>
	/// Overrides Unity display of scenes tagged with [Scene].
	/// </summary>
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{

		// Only override rendering on properties with type of string (otherwise it doesn't work).
		if (property.propertyType == SerializedPropertyType.String)
		{
			// Get currently selected object (generated from previous string value).
			SceneAsset sceneObject = GetSceneObject(property.stringValue, out string _);
			// Render a SceneAsset object field, with the previous string value, shown as a SceneAsset.
			Object scene = EditorGUI.ObjectField(position, label, sceneObject, typeof(SceneAsset), true);

			if (scene == null)
			{
				// We get nulls if the scene asset isn't found so wipe the string value.
				property.stringValue = "";

				// If the scene name doesn't match the property they have changed it
				// either by drag n drop or the object picker.
			}
			else if (scene.name != property.stringValue)
			{
				GetSceneObject(scene.name, out string scenePath);
				// If its a valid scene asset we use it, otherwise assume select invalid and ignore.
				if (scenePath != null)
				{
					property.stringValue = scenePath;
				}
			}
		}
		else
		{
			EditorGUI.LabelField(position, label.text, "Use [Scene] with strings.");
		}
	}

	/// <summary>
	/// Retrieve the scene string from the editor build settings, returns null if not found.
	/// 
	/// </summary>
	/// <param name="sceneName"> The asset name path of the scene object. </param>
	/// <returns> The SceneAsset or null if not found. </returns>
	protected SceneAsset GetSceneObject(string sceneName, out string scenePath)
	{
		scenePath = null;
		if (string.IsNullOrEmpty(sceneName))
		{
			// Early exit as know it will return null when string null or empty.
			return null;
		}
		// Iterate over the scenes in the build settings
		foreach (EditorBuildSettingsScene editorScene in EditorBuildSettings.scenes)
		{
			// We found the scene object's name in the editor scene's path.
			// This assumes that a scene will not be named exactly the same as a parent folder & a duplicate of another scene.
			if (editorScene.path.IndexOf(sceneName) != -1)
			{
				scenePath = editorScene.path;
				return AssetDatabase.LoadAssetAtPath(editorScene.path, typeof(SceneAsset)) as SceneAsset;
			}
		}
		// Scene not found, assume it isn't in the build settings.
		Debug.LogWarning("Scene [" + sceneName + "] cannot be used. Add this scene to the 'Scenes in the Build' in build settings.");
		return null;
	}
}