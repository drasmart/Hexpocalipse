using UnityEngine;
using System.Collections;

public class SingletonContainer : MonoBehaviour {

	private static SingletonContainer _sharedContainer;

	public static SingletonContainer sharedContainer {
		get {
			if (_sharedContainer != null) {
				return _sharedContainer;
			} else {
				GameObject node = new GameObject();
				node.AddComponent<SingletonContainer>();
				return _sharedContainer = node.GetComponent<SingletonContainer>();
			}
		}
	}

	private void Awake () {
		if (_sharedContainer == null) {
			_sharedContainer = this;
			DontDestroyOnLoad(gameObject);
		} else if (_sharedContainer != this) {
			GameObject.Destroy(gameObject);
		}
	}

	private void Start() {
		// Setting up self
		transform.parent = null;
		gameObject.name = "Singletons";
		// Load Console
		if (ConsoleContainer.instance) {
			ConsoleLog.Instance.Log("Console Loaded");
		} else {
			Debug.Log("Console not loaded.");
		};
	}
}
