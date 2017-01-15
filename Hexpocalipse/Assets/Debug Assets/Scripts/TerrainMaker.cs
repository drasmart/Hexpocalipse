    using UnityEngine;
using System.Collections;

public class TerrainMaker : MonoBehaviour {

	private GameObject _activePrisms;

    [SerializeField]
	private World.HexGridDriver _generator;

	public GameObject prefab;
	public GameObject defaultCamera;
	public GameObject controller;

	// Use this for initialization
	void Start () {
		if (ConsoleContainer.instance != null) {
			ConsoleCommandsRepository repo = ConsoleCommandsRepository.Instance;
			repo.RegisterCommand ("gen", GenPrisms);
			repo.RegisterCommand ("setGen", SetGen);
			repo.RegisterCommand ("clr", RemovePrisms);
			repo.RegisterCommand ("swap", Swap);
			repo.RegisterCommand ("tp", Teleport);
			repo.RegisterCommand ("help", Help);
            repo.RegisterCommand ("defGen", DefGen);
            ConsoleLog.Instance.Log ("You can use 'help'.\n" + 
			                         "Try: 'setGen 400 0.5 10' -> 'gen 0 0 128 128' -> 'swap' -> 'tp 0 300 0'");
		}
        RemovePrisms();
    }

	string GenPrisms(params string[] args) {
		long u0 = long.Parse (args [0]);
		long v0 = long.Parse (args [1]);
		long u1 = long.Parse (args [2]);
		long v1 = long.Parse (args [3]);
		float sq15 = Mathf.Sqrt(3)/2;
		for (long i = u0; i < u1; i++) {
			for (long j = v0; j < v1; j++) {
				float height = _generator[new World.HexCoords(i, j)];
				GameObject prism = Instantiate(prefab) as GameObject;
				prism.transform.parent = _activePrisms.transform;
				prism.transform.localPosition = new Vector3(((float)i - (float)j / 2) * sq15, height, (float)j * 0.75f);
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
		_activePrisms.transform.localScale = Vector3.one;
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

	string SetGen(params string[] args) {
		float delta0 = float.Parse (args [0]);
		float lambda = float.Parse (args [1]);
		int depth = int.Parse (args [2]);
		_generator = new World.HexGridDriver (depth, delta0, lambda);
		return "Generation parameters changed.";
	}

	string Save(params string[] args) {
		string name = args [0];
		string path = Application.persistentDataPath;
		string fullName = path + name;
		return "Saving to '" + fullName + "'...";
	}

	string Help(params string[] args) {
		return ("Available commands:\n" + 
		        "1. setGen <delta0> <lambda> <depth>\n" + 
		        "2. gen <u0> <v0> <u1> <v1>\n" + 
		        "3. clr\n" + 
		        "4. swap\n" + 
		        "5. tp <x> <y> <z>");
	}

    string DefGen(params string[] args)
    {
        float lambda = 0.5f;
        int   depth  = 6;
        if (args.Length == 2)
        {
            lambda = float.Parse(args[0]);
            depth = int.Parse(args[1]);
        }
        string[] s1 = { (0.39f * (1 << depth)).ToString(), lambda.ToString(), depth.ToString() };
        string o1 = SetGen(s1);
        string[] s2 = { "0", "0", "128", "128" };
        string o2 = GenPrisms(s2);
        return o1 + "\n" + o2;
    }
}
