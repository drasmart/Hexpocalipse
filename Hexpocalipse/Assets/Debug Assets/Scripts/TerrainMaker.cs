using UnityEngine;
using System.Collections;

public class TerrainMaker : MonoBehaviour {

	private GameObject _activePrisms;

	public GameObject prefab;
	public float scale = Mathf.Sqrt(3)/2;

	// Use this for initialization
	void Start () {
		if (ConsoleContainer.instance != null) {
			ConsoleCommandsRepository repo = ConsoleCommandsRepository.Instance;
			repo.RegisterCommand ("gen", GenPrisms);
			repo.RegisterCommand ("rem", RemovePrisms);
		}
	}

	string GenPrisms(params string[] args) {
		float delta0 = float.Parse (args [0]);
		float lambda = float.Parse (args [1]);
		int depth = int.Parse (args [2]);
		int size = int.Parse (args [3]);
		World.GridGen generator = new World.GridGen (delta0, lambda, depth);
		if(_activePrisms != null) {
			GameObject.Destroy(_activePrisms);
		}
		_activePrisms = new GameObject ();
		_activePrisms.transform.parent = transform;
		float sq15 = Mathf.Sqrt(3)/2;
		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {
				World.HexData data = generator[new World.HexCoords(i, j)];
				GameObject prism = Instantiate(prefab) as GameObject;
				prism.transform.parent = _activePrisms.transform;
				prism.transform.position = new Vector3(sq15 * i, data.height, (float)j * sq15 - (float)i / 2);
			}
		}
		return "Generation Finished.";
	}

	string RemovePrisms(params string[] args) {
		if(_activePrisms != null) {
			GameObject.Destroy(_activePrisms);
		}
		_activePrisms = new GameObject ();
		_activePrisms.transform.parent = transform;
		return "Prisms removed.";
	}
}
