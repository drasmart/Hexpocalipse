using UnityEngine;
using System.Collections;

public class TerrainMaker : MonoBehaviour {

	private GameObject _activePrisms;

	public GameObject prefab;
	public GameObject defaultCamera;
	public GameObject controller;

	// Use this for initialization
	void Start () {
		if (ConsoleContainer.instance != null) {
			ConsoleCommandsRepository repo = ConsoleCommandsRepository.Instance;
			repo.RegisterCommand ("gen", GenPrisms);
			repo.RegisterCommand ("clr", RemovePrisms);
			repo.RegisterCommand ("swap", Swap);
			repo.RegisterCommand ("tp", Teleport);
			repo.RegisterCommand ("help", Help);
			ConsoleLog.Instance.Log ("You can use 'help'.\n" + 
			                         "Try: 'gen 400 0.5 10 128' -> 'swap' -> 'tp 0 300 0'");
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
		_activePrisms.transform.localScale = Vector3.one;
		float sq15 = Mathf.Sqrt(3)/2;
		for (int i = 0; i < size; i++) {
			for (int j = 0; j < size; j++) {
				World.HexData data = generator[new World.HexCoords(i, j)];
				GameObject prism = Instantiate(prefab) as GameObject;
				prism.transform.parent = _activePrisms.transform;
				prism.transform.localPosition = new Vector3(((float)i - (float)j / 2) * sq15, data.height, (float)j * 0.75f);
				prism.transform.localScale = prefab.transform.localScale;
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

	string Teleport(params string[] args) {
		float x = float.Parse (args [0]);
		float y = float.Parse (args [1]);
		float z = float.Parse (args [2]);
		controller.transform.position = new Vector3 (x, y, z);
		return "Teleported";
	}

	string Swap(params string[] args) {
		defaultCamera.SetActive (!defaultCamera.activeSelf);
		controller.SetActive (!controller.activeSelf);
		return "Swapped";
	}

	string Help(params string[] args) {
		return ("Available commands:\n" + 
		        "1. gen <delta0> <lambda> <depth> <side>\n" + 
		        "2. clr\n" + 
		        "3. swap\n" + 
		        "4. tp <x> <y> <z>");
	}
}
