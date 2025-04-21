using UnityEngine;
using World;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using CielaSpike;
using DeveloperConsole;

public class TerrainMaker : MonoBehaviour {

    private delegate void ChunkGeneratedHandler(GameObject chunk);

	private GameObject _activePrisms;

    //[SerializeField]
	private HexDataStorage _generator;

	public GameObject prefab;
	public GameObject defaultCamera;
	public GameObject controller;
    public Material triMaterial;
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
			Console.AddCommand("gen", GenPrisms);
			Console.AddCommand("setGen", SetGen);
			Console.AddCommand("clr", RemovePrisms);
			Console.AddCommand("swap", Swap);
			Console.AddCommand("tp", Teleport);
			Console.AddCommand("help2", Help);
            Console.AddCommand("defGen", DefGen);
            Console.AddCommand("gen3", Gen3);
            Console.AddCommand("gen6", Gen6);
            Console.AddCommand("fill3", Fill3);
            Console.AddCommand("fill6", Fill6);
            Console.Print("You can use 'help'.\n" +
                          "Try: 'defGen' -> 'swap' -> 'tp 0 300 0'");
        }
        if(!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        RemovePrisms();
        LoadGenParams();
        //Gen6("0", "0", "0", "8");
    }

	void GenPrisms(params string[] args) {
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
                float height = _generator.ValueAt(coords);
				GameObject prism = Instantiate(prefab) as GameObject;
				prism.transform.parent = _activePrisms.transform;
				prism.transform.localPosition = new Vector3(((float)i - (float)j / 2) * sq15 * sPos, height, (float)j * 0.75f * sPos);
				prism.transform.localScale = correctScale;
                prism.transform.name = "Prism " + coords.ToString() + scaleSuffix;
            }
		}
        World.Logger.Flush();
		Console.Print("Generation Finished.");
	}

	void RemovePrisms(params string[] args) {
		if(_activePrisms != null) {
			GameObject.Destroy(_activePrisms);
		}
		_activePrisms = new GameObject ();
		_activePrisms.transform.parent = transform;
		_activePrisms.transform.localScale = Vector3.one;
        Console.Print("Prisms removed.");
	}

	void Teleport(params string[] args) {
		float x = float.Parse (args [0]);
		float y = float.Parse (args [1]);
		float z = float.Parse (args [2]);
		controller.transform.position = new Vector3 (x, y, z);
        Console.Print("Teleported");
	}

	void Swap(params string[] args) {
		defaultCamera.SetActive (!defaultCamera.activeSelf);
		controller.SetActive (!controller.activeSelf);
		Console.Print("Swapped");
	}

    void SetGen(params string[] args)
    {
        SetGenParams(args);
        SaveGenParams(args);
        Console.Print("Generation parameters changed.");
    }

    void SetGenParams(string[] args)
    {
        float delta0 = float.Parse(args[0]);
        float lambda = float.Parse(args[1]);
        int cDepth = int.Parse(args[2]);
        int fDepth = (args.Length > 3) ? int.Parse(args[3]) : cDepth;
        _generator = new LockedStorage(new HexGridDriver(cDepth, fDepth, delta0, lambda, savePath));
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
        Debug.Log(args);
    }

	void Save(params string[] args) {
		string name = args [0];
		string path = Application.persistentDataPath;
		string fullName = path + name;
		Console.Print("Saving to '" + fullName + "'...");
	}

	void Help(params string[] args) {
        Console.Print("Available commands:\n" + 
                      "01. setGen <delta0> <lambda> <chunkDepth> [ <fractalDepth> ]\n" + 
                      "02. gen <relative_u0> <relative_v0> <scaleDepth> <size>\n" + 
                      "03. clr\n" + 
                      "04. swap\n" +
                      "05. tp <x> <y> <z>" +
                      "06. defGen [ <lambda> <chunkDepth> [ <fractalDepth> ] ]\n" +
                      "07. gen3 <relative_u0> <relative_v0> <scaleDepth> <size>\n" +
                      "08. gen6 <relative_u0> <relative_v0> <scaleDepth> <size>\n" +
                      "09. fill3 <relative_u0> <relative_v0> <relative_du> <relative_dv>\n" +
                      "10. fill6 <relative_u0> <relative_v0> <relative_du> <relative_dv>\n");
	}

    void DefGen(params string[] args)
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
        SetGen(s1);
        string sz = Mathf.Min(128, 4 << cDepth).ToString();
        string[] s2 = { "0", "0", "0", sz };
        GenPrisms(s2);
    }

    Mesh GenMesh3(float[] h, long size, Vector3 center)
    {
        long size1 = size + 1;
        long size2 = size * size;
        long size12 = size1 * size1;

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
                uv[idx0] = new HexCoords(u, v).toVector2;
                if (u >= size || v >= size)
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

        return genMesh;
    }

    IEnumerator GenChunk3(HexCoords absStart, int depth, long size, ChunkGeneratedHandler finishHandler)
    {
        // Data Part

        float hMin = float.PositiveInfinity;
        float hMax = float.NegativeInfinity;

        long size1 = size + 1;
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

        yield return Ninja.JumpToUnity;
        // Unity Part

        GameObject result = new GameObject();
        result.AddComponent<MeshRenderer>();
        result.AddComponent<MeshCollider>();
        result.AddComponent<MeshFilter>();
        MeshFilter filter = result.GetComponent<MeshFilter>();
        MeshCollider collider = result.GetComponent<MeshCollider>();
        MeshRenderer renderer = result.GetComponent<MeshRenderer>();
        renderer.material = triMaterial;

        Vector3 center = (new HexCoords(size1, size1).toVector3(hMax + hMin) / 2.0f);

        Mesh genMesh = GenMesh3(h, size, center);

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

        //return result;
        finishHandler(result);
        yield break;
    }

    public enum PrismGenMode
    {
        NegU = 1 << 0,
        NegV = 1 << 1,
        NegW = 1 << 2,
        PosU = 1 << 3,
        PosV = 1 << 4,
        PosW = 1 << 5,
        Collider = 1 << 6,
        AllSides = (NegU | NegV | NegW | PosU | PosV | PosW),
    }

    Mesh GenMesh6(float [] h, long size, Vector3 center, PrismGenMode mode)
    {
        float t = 1 / 3.0f;
        float r = Mathf.Sqrt(t);
        Vector2 vR = new Vector2(0.5f, -0.5f);
        Vector2 vS = new Vector2(0.5f, 0.5f);
        Vector2 vT = new Vector2(0.0f, 1.0f);
        Vector2[] hexS = { -vS, -vT, vR, vS, vT, -vR };
        Vector3[] hexP = new Vector3[6];
        Vector2[] hexT = new Vector2[6];
        for (int k = 0; k < 6; k++)
        {
            Vector2 v = hexS[k];
            hexP[k] = new Vector3(v.x, 0, r * v.y);
            hexT[k] = new Vector2(v.x, t * v.y);
        }
        int[] hexTriOrder = { 0, 5, 1, 1, 5, 2, 2, 5, 4, 2, 4, 3 };

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();

        long size2 = size + 2;
        long size02 = size * size;

        for (int u = 0; u < size; u++)
        {
            for (int v = 0; v < size; v++)
            {
                Vector3 pos = new HexCoords(u, v).toVector3(h[(u + 1) * size2 + v + 1]) - center;
                for(int k = 0; k < 6; k++)
                {
                    vertices.Add(pos + hexP[k]);
                    float q = (v - 1) / 2.0f;
                    uv.Add(new Vector2(u - q, q) + hexT[k]);
                }
                int idxV = (int)(6 * (u * size + v));
                for (int j = 0; j < 12; j++)
                {
                    triangles.Add(idxV + hexTriOrder[j]);
                }
            }
        }

        PrismGenMode[] sideTests = { PrismGenMode.PosW, PrismGenMode.PosV, PrismGenMode.NegU, PrismGenMode.NegW, PrismGenMode.NegV, PrismGenMode.PosU };
        HexCoords[] cellTests = { new HexCoords(-1, -1), new HexCoords(0, -1), new HexCoords(1, 0), new HexCoords(1, 1), new HexCoords(0, 1), new HexCoords(-1, 0) };
        int addedV = 0;
        bool colliderTest = (mode | PrismGenMode.Collider) == PrismGenMode.Collider;
        for (int i = 0; i < 6; i++)
        {
            if (!colliderTest && ((mode & sideTests[i]) == 0))
            {
                continue;
            }
            long du = cellTests[i].u;
            long dv = cellTests[i].v;
            int dvTL = i;
            int dvTR = (i + 1) % 6;
            int dvBL = (i + 3) % 6;
            int dvBR = (i + 4) % 6;
            for (long u = 0; u < size; u++)
            {
                for (long v = 0; v < size; v++)
                {
                    int idxHc = (int)((u + 1) * size2 + (v + 1));
                    int idxHx = (int)((u + 1 + du) * size2 + (v + 1 + dv));
                    if(h[idxHx] >= h[idxHc])
                    {
                        continue;
                    }
                    int idxVc = (int)(6 * (u * size + v));
                    int idxVx = (int)(6 * ((u + du) * size + (v + dv)));
                    int vTL = idxVc + dvTL;
                    int vTR = idxVc + dvTR;
                    int vBL = idxVx + dvBL;
                    int vBR = idxVx + dvBR;
                    if (!colliderTest)
                    {
                        vTL = (int)size02 * 6 + addedV;
                        vTR = vTL + 1;
                        addedV += 2;

                        Vector3 pos = new HexCoords(u, v).toVector3(h[idxHc]) - center;
                        vertices.Add(pos + hexP[dvTL]);
                        vertices.Add(pos + hexP[dvTR]);
                        float q = (v - 1) / 2.0f;
                        uv.Add(new Vector2(u - q, q) + hexT[dvTL]);
                        uv.Add(new Vector2(u - q, q) + hexT[dvTR]);
                    }
                    if (!colliderTest || u + du < 0 || u + du >= size || v + dv < 0 || v + dv >= size)
                    {
                        vBL = (int)size02 * 6 + addedV;
                        vBR = vBL + 1;
                        addedV += 2;

                        Vector3 pos = new HexCoords(u + du, v + dv).toVector3(h[idxHx]) - center;
                        vertices.Add(pos + hexP[dvBL]);
                        vertices.Add(pos + hexP[dvBR]);
                        float q = (v + dv - 1) / 2.0f;
                        uv.Add(new Vector2(u + du - q, q) + hexT[dvBL]);
                        uv.Add(new Vector2(u + du - q, q) + hexT[dvBR]);
                    }
                    triangles.Add(vBL);
                    triangles.Add(vTL);
                    triangles.Add(vTR);
                    triangles.Add(vTL);
                    triangles.Add(vBL);
                    triangles.Add(vBR);
                }
            }
        }

        Mesh genMesh = new Mesh();
        genMesh.vertices = vertices.ToArray();
        genMesh.uv = uv.ToArray();
        genMesh.triangles = triangles.ToArray();
        genMesh.RecalculateNormals();

        return genMesh;
    }

    IEnumerator GenChunk6(HexCoords absStart, int depth, long size, PrismGenMode genMode, ChunkGeneratedHandler finishHandler)
    {
        // Unity Part

        float hMin = float.PositiveInfinity;
        float hMax = float.NegativeInfinity;
        
        long size2 = size + 2;
        long size22 = size2 * size2;

        float[] h = new float[size22];
        for (long u = 0; u < size2; u++)
        {
            for (long v = 0; v < size2; v++)
            {
                HexCoords pos = absStart + (new HexCoords(u - 1, v - 1) << depth);
                float t = _generator.ValueAt(pos);
                h[u * size2 + v] = t;
                if(u == 0 || u == size2 || v == 0 || v == size2)
                {
                    continue;
                }
                if (t < hMin)
                {
                    hMin = t;
                }
                if (t > hMax)
                {
                    hMax = t;
                }
            }
        }

        yield return Ninja.JumpToUnity;
        // Data Part

        GameObject result = new GameObject();
        result.AddComponent<MeshRenderer>();
        if ((genMode & PrismGenMode.Collider) == PrismGenMode.Collider)
        {
            result.AddComponent<MeshCollider>();
        }
        result.AddComponent<MeshFilter>();
        MeshFilter filter = result.GetComponent<MeshFilter>();
        MeshCollider collider = result.GetComponent<MeshCollider>();
        MeshRenderer renderer = result.GetComponent<MeshRenderer>();
        renderer.material = hexMaterial;

        Vector3 center = (new HexCoords(size - 1, size - 1).toVector3(hMax + hMin) / 2.0f);

        if (collider != null)
        {
            collider.sharedMesh = GenMesh6(h, size, center, genMode);
        }
        filter.mesh = GenMesh6(h, size, center, genMode & ~PrismGenMode.Collider);
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
        result.transform.name = "6-Chunk " + absStart.ToString() + scaleSuffix + " s" + size.ToString();

        //return result;
        finishHandler(result);
        yield break;
    }

    void Gen3(params string[] args)
    {
        long u0 = long.Parse(args[0]);
        long v0 = long.Parse(args[1]);
        int sk = int.Parse(args[2]);
        long sd = long.Parse(args[3]);
        this.StartCoroutineAsync(GenChunk3(new HexCoords(u0, v0) << sk, sk, sd, OnChunkGenerated));
        Console.Print("Generation started...");
    }

    void Gen6(params string[] args)
    {
        long u0 = long.Parse(args[0]);
        long v0 = long.Parse(args[1]);
        int sk = int.Parse(args[2]);
        long sd = long.Parse(args[3]);
        this.StartCoroutineAsync(GenChunk6(new HexCoords(u0, v0) << sk, sk, sd, PrismGenMode.AllSides | PrismGenMode.Collider, OnChunkGenerated));
        Console.Print("Generation started...");
    }

    void Fill3(params string[] args)
    {
        long u0 = long.Parse(args[0]);
        long v0 = long.Parse(args[1]);
        long du = long.Parse(args[2]);
        long dv = long.Parse(args[3]);
        const int d = 7;
        HexCoords c0 = new HexCoords(u0, v0) >> d;
        HexCoords c1 = new HexCoords(u0 + du, v0 + dv) >> d;
        string[] pms = { "u", "v", "0", (1 << d).ToString() };
        for (long i = c0.u; i <= c1.u; i++)
        {
            pms[0] = (i << d).ToString();
            for (long j = c0.v; j <= c1.v; j++)
            {
                pms[1] = (j << d).ToString();
                Gen3(pms);
            }
        }
        Console.Print("Generation started...");
    }

    void Fill6(params string[] args)
    {
        long u0 = long.Parse(args[0]);
        long v0 = long.Parse(args[1]);
        long du = long.Parse(args[2]);
        long dv = long.Parse(args[3]);
        const int d = 5;
        HexCoords c0 = new HexCoords(u0, v0) >> d;
        HexCoords c1 = new HexCoords(u0 + du, v0 + dv) >> d;
        string[] pms = { "u", "v", "0", (1<<d).ToString() };
        for (long i = c0.u; i <= c1.u; i++)
        {
            pms[0] = (i<<d).ToString();
            for(long j = c0.v; j <= c1.v; j++)
            {
                pms[1] = (j<<d).ToString();
                Gen6(pms);
            }
        }
        Console.Print("Generation started...");
    }

    void OnChunkGenerated(GameObject chunk)
    {
        Console.Print(chunk.transform.name + " generated.");
    }
}
