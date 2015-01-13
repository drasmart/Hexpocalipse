using UnityEngine;
using System.Collections;

namespace World {

	public class WorldData : MonoBehaviour {

		// Singleton

		private static WorldData _data;

		public static WorldData data {
			get {
				if (_data != null) {
					return _data;
				} else {
					GameObject node = new GameObject();
					node.AddComponent<WorldData>();
					return _data = node.GetComponent<WorldData>();
				}
			}
		}

		// Lifecycle

		private void Awake() {
			if (_data == null) {
				_data = this;
				DontDestroyOnLoad(gameObject);
			} else if (_data != this) {
				GameObject.Destroy(gameObject);
			}
		}

		private void Start() {
			transform.parent = SingletonContainer.sharedContainer.transform;
			gameObject.name = "World Data";
		}

		private string _path;
	}

}