using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace UnityEngine.CustomUtils
{
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


	public static class CollisionExtensions
	{

		/// <summary>
		/// This function is needed because impulse can be flipped depending
		/// on the order objects were added to the scene.
		/// </summary>
		public static Vector3 GetImpulse(this Collision collision)
		{
			if (collision.contactCount <= 0) return Vector3.zero;

			Vector3 avgNormal = Vector3.zero;

			var contactPoints = GetContactPointsInSharedArray(collision);
			for (int i = 0; i < collision.contactCount; i++)
			{
				var contact = contactPoints[i];
				avgNormal += contact.normal;
			}
			avgNormal *= 1 / collision.contactCount;

			if (Vector3.Dot(collision.impulse, avgNormal) < 0)
			{
				return -collision.impulse;
			}
			else
			{
				return collision.impulse;
			}
		}


		private static ContactPoint[] _contactPoints = new ContactPoint[0];
		private static ContactPoint[] GetContactPointsInSharedArray(Collision collision)
		{
			if (_contactPoints.Length < collision.contactCount)
			{
				_contactPoints = new ContactPoint[collision.contactCount];
			}

			collision.GetContacts(_contactPoints);
			return _contactPoints;
		}
	}

}
