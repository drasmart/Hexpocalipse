using UnityEngine;
using System.Collections;

public class WorldGen : MonoBehaviour {

	// Instance Variables
	
	public int fractalDepth     =  6;
	public int gridSideInChunks = 10;

	public float coreDelta0 = 50.0f;
	public float coreLambda =  0.2f;

	public float surfDelta0 =  5.0f;
	public float surfLambda =  0.5f;

	public float defPrismHeight = 0.5f;
	public GameObject defaultPrism;
	public float horScale = 2.0f;

	private GameObject chunks;
	private GameObject chunkLoaders;

	public long currentGeneration = 0;

	private float[,] tempHeightMap;
	private HexPosition tempHexPos;

	private float sq15 = Mathf.Sqrt (3) / 2;
	
	// Static Singleton
	
	private static WorldGen _worldGen = null;
	
	public static WorldGen mainWorldGen() {
		return _worldGen;
	}
	
	// Lifecycle
	
	private void Awake() {
		if (_worldGen) {
			GameObject.DestroyObject(gameObject);
		} else {
			_worldGen = this;
			chunks = new GameObject();
			chunkLoaders = new GameObject();
			chunks.transform.parent = chunkLoaders.transform.parent = transform;
			chunks.transform.localPosition = Vector3.zero;
			chunkLoaders.transform.localPosition = Vector3.zero;
		}
	}

	private void Start() {
		HexPosition tempPos = new HexPosition ();
		tempPos.x = 0;
		tempPos.v = 0;
		LoadChunk(tempPos);
		return;/*
		for (int i = 0; i < gridSideInChunks; i++) {
			tempPos.x = i;
			for (int j = 0; j < gridSideInChunks; j++) {
				tempPos.v = j;
				LoadChunk(tempPos);
			}
		}*/
	}

	// Interface

	public void LoadChunk(HexPosition position) {
		GameObject node = new GameObject ();
		node.transform.parent = chunks.transform;
		node.AddComponent("WorldChunk");
		WorldChunk chunk = node.GetComponent<WorldChunk>();
		chunk.hexPosition = position;
		chunk.transform.localPosition = Hex2Cartesian (position);
	}

	public WorldPrism[,] MakePrisms(HexPosition position, ref GameObject parentObject) {
		WorldPrism[,] result = new WorldPrism[1<<fractalDepth,1<<fractalDepth];
		parentObject.transform.localPosition = Hex2Cartesian(position);
		int side = 1 << fractalDepth;
		tempHeightMap = new float[side,side];
		for (int i = 0; i < side; i++) {
			for (int j = 0; j < side; j++) {
				tempHeightMap[i,j] = -1;
			}
		}
		tempHexPos = Hex2Chunk(position);
		for (int i = 0; i < side; i++) {
			for (int j = 0; j < side; j++) {
				float h = GetHeight(position.x + i, position.v + j, 0);
				GameObject node = Object.Instantiate(defaultPrism) as GameObject;
				node.transform.parent = parentObject.transform;
				HexPosition pos = new HexPosition();
				pos.x = i;
				pos.v = j;
				node.transform.localPosition = Hex2Cartesian(pos);
				node.transform.localScale = new Vector3(horScale, horScale, -10.0f/defPrismHeight);
				node.AddComponent("WorldPrism");
				WorldPrism prism = node.GetComponent<WorldPrism>();
				prism.hexPosition = pos;
				prism.coreHeight = h;
				prism.transform.localPosition = new Vector3(prism.transform.localPosition.x,
				                                            h,
				                                            prism.transform.localPosition.z);
				result[i,j] = prism;
			}
		}

		return result;
	}

	// Child Seekers

	private WorldChunk LookForChunk(HexPosition position) {
		/*WorldChunk[] allLoadedChunks = chunks.GetComponents<WorldChunk> ();
		foreach (WorldChunk chunk in allLoadedChunks) {
			if (chunk.hexPosition == position) {
				return chunk;
			}
		}*/
		return null;
	}

	// Calculations

	private Vector3 Hex2Cartesian(HexPosition position) {
		Vector3 result = new Vector3();
		result.x = position.x - 0.5f * position.v;
		result.y = 0;
		result.z = sq15 * position.v;
		return result * sq15 * horScale;
	}

	private float GenHeight(HexPosition position, int iteration = 0) {
		int depth = fractalDepth - iteration;
		if (depth < 0) {
			return Random.Range(0, surfDelta0);
		}
		long delta = 1 << iteration;
		long lx = position.x % delta;
		long lv = position.v % delta;
		if(lx != 0 || lv != 0) {
			float dh = Random.Range(0, SurfDelta(depth)); 
			long cx = position.x / delta;
			long cv = position.v / delta;
			return (GetHeight(cx, cv, iteration + 1) 
			        + GetHeight(cx + ((lx != 0) ? delta : 0), cv+ ((lv != 0) ? delta : 0), iteration + 1)
			        ) / 2 + dh;
		} else {
			return GenHeight(position, iteration + 1);
		}
	}

	private float GetHeight(long x, long v, int iteration = 0) {
		HexPosition pos = new HexPosition ();
		pos.x = x;
		pos.v = v;
		HexPosition chunkPos = Hex2Chunk (pos);
		WorldChunk chunk = LookForChunk (chunkPos);
		if (chunk != null && chunk.generation == currentGeneration) {
			//Debug.Log (x.ToString() + ";" + v.ToString() + " -> chunked");
			return chunk.heightAt(pos);
		}
		if (chunkPos.x == tempHexPos.x && chunkPos.v == tempHexPos.v) {
			float tempValue = tempHeightMap[x - chunkPos.x, v - chunkPos.v];
			if(tempValue >= 0) {
				//Debug.Log (x.ToString() + ";" + v.ToString() + " -> mapped" + tempValue.ToString());
				return tempValue;
			} else {
				tempValue = GenHeight (pos, 0);
				tempHeightMap[x - chunkPos.x, v - chunkPos.v] = tempValue;
				//Debug.Log (x.ToString() + ";" + v.ToString() + " -> genned" + tempValue.ToString());
				return tempValue;
			}
		}
		Debug.Log (x.ToString() + ";" + v.ToString() + " -> not in chunk");
		return GenHeight (pos, iteration);
	}

	private float CoreDelta(int iteration) {
		return coreDelta0 * Mathf.Pow (coreLambda, iteration);
	}

	private float SurfDelta(int iteration) {
		return surfDelta0 * Mathf.Pow (surfLambda, iteration);
	}

	private HexPosition Hex2Chunk(HexPosition position) {
		HexPosition result = new HexPosition();
		result.x = (position.x >> fractalDepth) << fractalDepth;
		result.v = (position.v >> fractalDepth) << fractalDepth;
		return result;
	}
}
