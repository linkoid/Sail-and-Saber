using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.CustomUtils
{
	public interface IObject
	{
		/// <summary>
		/// The name of the object.
		/// </summary>
		public string name { get; set; }
		/// <summary>
		/// Returns the instance id of the object.
		/// </summary>
		public int GetInstanceID();
	}

	public interface IComponent : IObject
	{
		/// <summary>
		/// The Transform attached to this GameObject.
		/// </summary>
		public Transform transform { get; }
		/// <summary>
		/// The game object this component is attached to. A component is always attached
		//  to a game object.
		/// </summary>
		public GameObject gameObject { get; }
		/// <summary>
		/// The tag of this game object.
		/// </summary>
		public string tag { get; set; }
		public bool CompareTag(string tag);
		public T GetComponent<T>();
		public T GetComponentInChildren<T>(bool includeInactive);
		public T GetComponentInChildren<T>();
		public T GetComponentInParent<T>();
		public void GetComponents<T>(List<T> results);
		public T[] GetComponents<T>();
		public void GetComponentsInChildren<T>(List<T> results);
		public T[] GetComponentsInChildren<T>();
		public void GetComponentsInChildren<T>(bool includeInactive, List<T> result);
		public T[] GetComponentsInChildren<T>(bool includeInactive);
		public T[] GetComponentsInParent<T>(bool includeInactive);
		public T[] GetComponentsInParent<T>();
		public void GetComponentsInParent<T>(bool includeInactive, List<T> results);
		public bool TryGetComponent<T>(out T component);
		public bool TryGetComponent(System.Type type, out Component component);
	}

	public interface IBehavior : IComponent
	{
		/// <summary>
		/// Enabled Behaviours are Updated, disabled Behaviours are not.
		/// </summary>
		public bool enabled { get; set; }
		/// <summary>
		/// Has the Behaviour had active and enabled called?
		/// </summary>
		public bool isActiveAndEnabled { get; }
	}

	public interface IMonoBehavior : IBehavior
	{
		/// <summary>
		/// Is any invoke pending on this MonoBehaviour?
		/// </summary>
		public bool IsInvoking();

		/// <summary>
		/// Cancels all Invoke calls on this MonoBehaviour.
		/// </summary>
		public void CancelInvoke();

		/// <summary>
		/// Invokes the method methodName in time seconds.
		/// </summary>
		public void Invoke(string methodName, float time);

		/// <summary>
		/// Invokes the method methodName in time seconds, then repeatedly every repeatRate seconds.
		/// </summary>
		public void InvokeRepeating(string methodName, float time, float repeatRate);

		/// <summary>
		/// Cancels all Invoke calls with name methodName on this behaviour.
		/// </summary>
		public void CancelInvoke(string methodName);

		/// <summary>
		/// Is any invoke on methodName pending?
		/// </summary>
		public bool IsInvoking(string methodName);

		/// <summary>
		/// Starts a coroutine named methodName.
		/// </summary>
		public Coroutine StartCoroutine(string methodName, object value = null);

		/// <inheritdoc cref="StartCoroutine"/>
		public Coroutine StartCoroutine(IEnumerator routine);

		/// <summary>
		/// Stops the first coroutine named methodName, or the coroutine stored in routine
		/// running on this behaviour.
		/// </summary>
		/// <param name="methodName">Name of coroutine.</param>
		/// <param name="routine">Name of the function in code, including coroutines.</param>
		public void StopCoroutine(IEnumerator routine);

		/// <inheritdoc cref="StopCoroutine"/>
		public void StopCoroutine(Coroutine routine);

		/// <inheritdoc cref="StopCoroutine"/>
		public void StopCoroutine(string methodName);


		/// <summary>
		/// Stops all coroutines running on this behaviour.
		/// </summary>
		public void StopAllCoroutines();
	}

}




