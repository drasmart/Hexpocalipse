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

	void Awake () {
		if (_sharedContainer == null) {
			_sharedContainer = this;
			DontDestroyOnLoad(gameObject);
		} else if (_sharedContainer != this) {
			GameObject.Destroy(gameObject);
		}
	}

	void Start() {
		transform.parent = null;
		gameObject.name = "Singletons";
	}
}
