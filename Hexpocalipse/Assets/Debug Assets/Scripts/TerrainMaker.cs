using UnityEngine;
using World;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public class TerrainMaker : MonoBehaviour {

	private GameObject _activePrisms;

    //[SerializeField]
	private World.HexGridDriver _generator;

	public GameObject prefab;
	public GameObject defaultCamera;
	public GameObject controller;
    public Material hexMaterial;

    string savePath = null;
    string paramsPath = null;

    void Awake()
    {
        savePath = Application.persistentDataPath + "/saveTest";
        paramsPath = savePath + "/genParams.dat";
    }

	// Use this for initialization
	void Start () {
		if (ConsoleContainer.instance != null) {
			ConsoleCommandsRepository repo = ConsoleCommandsRepository.Instance;
			repo.RegisterCommand("gen", GenPrisms);
			repo.RegisterCommand("setGen", SetGen);
			repo.RegisterCommand("clr", RemovePrisms);
			repo.RegisterCommand("swap", Swap);
			repo.RegisterCommand("tp", Teleport);
			repo.RegisterCommand("help", Help);
            repo.RegisterCommand("defGen", DefGen);
            repo.RegisterCommand("gen3", Gen3);
            ConsoleLog.Instance.Log ("You can use 'help'.\n" + 
			                         "Try: 'defGen' -> 'swap' -> 'tp 0 300 0'");
        }
        if(!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        RemovePrisms();
        LoadGenParams();
    }

	string GenPrisms(params string[] args) {
		long u0 = long.Parse (args [0]);
		long v0 = long.Parse (args [1]);
		int  sk = int .Parse (args [2]);
		long sd = long.Parse (args [3]);
		float sq15 = Mathf.Sqrt(3)/2;
        int sPos = (1 << sk);
        HexCoords start = new HexCoords(u0, v0);
        Vector3 correctScale = prefab.transform.localScale;
        correctScale.x *= sPos;
        correctScale.y *= sPos;
        string scaleSuffix = (sPos == 1) ? "" : (" x" + sPos.ToString());
        for (long i = 0; i < sd; i++) {
			for (long j = 0; j < sd; j++) {
                HexCoords coords = (start + new HexCoords(i, j)) << sk;
                float height = _generator[coords];
				GameObject prism = Instantiate(prefab) as GameObject;
				prism.transform.parent = _activePrisms.transform;
				prism.transform.localPosition = new Vector3(((float)i - (float)j / 2) * sq15 * sPos, height, (float)j * 0.75f * sPos);
				prism.transform.localScale = correctScale;
                prism.transform.name = "Prism " + coords.ToString() + scaleSuffix;
            }
		}
        World.Logger.Flush();
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

    string SetGen(params string[] args)
    {
        SetGenParams(args);
        SaveGenParams(args);
        return "Generation parameters changed.";
    }

    void SetGenParams(string[] args)
    {
        float delta0 = float.Parse(args[0]);
        float lambda = float.Parse(args[1]);
        int cDepth = int.Parse(args[2]);
        int fDepth = (args.Length > 3) ? int.Parse(args[3]) : cDepth;
        _generator = new World.HexGridDriver(cDepth, fDepth, delta0, lambda, savePath);
    }

    void SaveGenParams(string[] args)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = File.Create(paramsPath);
        bf.Serialize(fs, args);
        fs.Close();
    }

    void LoadGenParams()
    {
        if(!File.Exists(paramsPath))
        {
            return;
        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = File.Open(paramsPath, FileMode.Open);
        string[] args = (string[])bf.Deserialize(fs);
        fs.Close();

        SetGenParams(args);
    }

	string Save(params string[] args) {
		string name = args [0];
		string path = Application.persistentDataPath;
		string fullName = path + name;
		return "Saving to '" + fullName + "'...";
	}

	string Help(params string[] args) {
		return ("Available commands:\n" + 
		        "1. setGen <delta0> <lambda> <chunkDepth> [ <fractalDepth> ]\n" + 
		        "2. gen <relative_u0> <relative_v0> <scaleDepth> <size>\n" + 
		        "3. clr\n" + 
		        "4. swap\n" +
                "5. tp <x> <y> <z>" +
                "6. defGen [ <lambda> <chunkDepth> [ <fractalDepth> ] ]\n" +
                "7. gen3 <relative_u0> <relative_v0> <scaleDepth> <size>");
	}

    string DefGen(params string[] args)
    {
        float lambda = 0.5f;
        int   cDepth = 4;
        int   fDepth = 10;
        if (args.Length >= 2)
        {
            lambda = float.Parse(args[0]);
            cDepth = int.Parse(args[1]);
            fDepth = (args.Length > 3) ? int.Parse(args[3]) : cDepth;
        }
        string[] s1 = { (0.39f * (1 << fDepth)).ToString(), lambda.ToString(), cDepth.ToString(), fDepth.ToString() };
        string o1 = SetGen(s1);
        string sz = Mathf.Min(128, 4 << cDepth).ToString();
        string[] s2 = { "0", "0", "0", sz };
        string o2 = GenPrisms(s2);
        return o1 + "\n" + o2;
    }

    GameObject GenChunk3(HexCoords absStart, int depth, long size)
    {
        GameObject result = new GameObject();
        result.AddComponent<MeshRenderer>();
        result.AddComponent<MeshCollider>();
        result.AddComponent<MeshFilter>();
        MeshFilter filter = result.GetComponent<MeshFilter>();
        MeshCollider collider = result.GetComponent<MeshCollider>();
        MeshRenderer renderer = result.GetComponent<MeshRenderer>();
        renderer.material = hexMaterial;

        float hMin = float.PositiveInfinity;
        float hMax = float.NegativeInfinity;

        long size1 = size + 1;
        long size2 = size * size;
        long size12 = size1 * size1;
        float[] h = new float[size12];
        for (long u = 0; u < size1; u++)
        {
            for (long v = 0; v < size1; v++)
            {
                HexCoords pos = absStart + (new HexCoords(u, v) << depth);
                float t = _generator.ValueAt(pos);
                h[u * size1 + v] = t;
                if(t < hMin)
                {
                    hMin = t;
                }
                if(t > hMax)
                {
                    hMax = t;
                }
            }
        }
        
        Vector3 center = (new HexCoords(size1, size1).toVector3(hMax + hMin) / 2.0f);

        Vector3[] vertices = new Vector3[size12];
        Vector2[] uv = new Vector2[size12];
        int[] triangles = new int[6 * size2];

        for (long u = 0; u < size1; u++)
        {
            for (long v = 0; v < size1; v++)
            {
                int idx0 = (int)(u * size1 + v);
                int idx1 = (int)(idx0 + size1);
                vertices[idx0] = new HexCoords(u, v).toVector3(h[idx0]) - center;
                uv[idx0] = new Vector2(u, v);
                if(u >= size || v >= size)
                {
                    continue;
                }
                int idxT = (int)(u * size + v);
                triangles[6 * idxT] = idx0;
                triangles[6 * idxT + 1] = idx1 + 1;
                triangles[6 * idxT + 2] = idx1;
                triangles[6 * idxT + 3] = idx0;
                triangles[6 * idxT + 4] = idx0 + 1;
                triangles[6 * idxT + 5] = idx1 + 1;
            }
        }

        Mesh genMesh = new Mesh();
        genMesh.vertices = vertices;
        genMesh.uv = uv;
        genMesh.triangles = triangles;
        genMesh.RecalculateNormals();

        collider.sharedMesh = genMesh;
        filter.mesh = genMesh;
        int depthScale = 1 << depth;
        string scaleSuffix = (depthScale == 1) ? "" : (" x" + depthScale.ToString());

        result.transform.parent = _activePrisms.transform;
        Vector3 tPos = center;
        tPos.x *= depthScale;
        tPos.z *= depthScale;
        result.transform.localPosition = tPos + absStart.toVector3(0);
        Vector3 tScale = result.transform.localScale;
        tScale.x *= depthScale;
        tScale.z *= depthScale;
        result.transform.localScale = tScale;
        result.transform.name = "3-Chunk " + absStart.ToString() + scaleSuffix + " s" + size.ToString();

        return result;
    }

    string Gen3(params string[] args)
    {
        long u0 = long.Parse(args[0]);
        long v0 = long.Parse(args[1]);
        int sk = int.Parse(args[2]);
        long sd = long.Parse(args[3]);
        GameObject result = GenChunk3(new HexCoords(u0, v0) << sk, sk, sd);
        return result.transform.name + " generated.";
    }
}
