using UnityEngine;
using System.Collections;

public class ConsoleContainer : MonoBehaviour {

	private static ConsoleContainer _instance;

	public static ConsoleContainer instance {
		get {
			if (_instance) {
				return _instance;
			} else {
				GameObject prefab = Resources.Load<GameObject>("Prefabs/Console");
				GameObject console = Instantiate(prefab) as GameObject;
				return _instance = console.GetComponent<ConsoleContainer>();
			}
		}
	}
	
	// Lifecycle
	
	private void Awake() {
		if (_instance == null) {
			_instance = this;
			DontDestroyOnLoad(gameObject);
		} else if (_instance != this) {
			GameObject.Destroy(gameObject);
		}
	}
	
	private void Start() {
		transform.parent = SingletonContainer.sharedContainer.transform;
		gameObject.name = "Console";
	}
}
