using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneExtensions
{
	public static string LogString(this Scene scene)
	{
		return $"<[{scene.buildIndex}] \"{scene.name}\" {scene}>";
	}
}

public static class AnimatorExtensions
{
	public static AnimatorControllerParameter RequireParamater(this Animator animator, string name)
	{
		if (!animator.TryGetParamater(name, out var parameter))
		{
			Debug.LogError($"Could not find required parameter \"{name}\" in {animator}", animator);
		}
		return parameter;
	}

	public static bool TryGetParamater(this Animator animator, string name, out AnimatorControllerParameter parameter)
	{
		foreach (var param in animator.parameters)
		{
			if (param.name == name)
			{
				parameter = param;
				return true;
			}
		}
		parameter = null;
		return false;
	}
}
