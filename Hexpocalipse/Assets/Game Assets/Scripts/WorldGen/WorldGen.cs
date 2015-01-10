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

	private GameObject chunks;
	private GameObject chunkLoaders;

	public long currentGeneration = 0;
	
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
		}
	}

	// Interface

	public void LoadChunk(HexPosition position) {
		GameObject node = new GameObject ();
		node.transform.parent = chunks.transform;


		WorldChunk chunk = new WorldChunk ();
		chunk.hexPosition = position;
	}

	public WorldPrism[,] MakePrisms(HexPosition position, Transform parent) {
		// TODO: implement loading or generating
		return null;
	}

	// Child Seekers

	private WorldChunk LookForChunk(HexPosition position) {
		WorldChunk[] allLoadedChunks = chunks.GetComponentsInChildren<WorldChunk> ();
		foreach (WorldChunk chunk in allLoadedChunks) {
			if (chunk.hexPosition == position) {
				return chunk;
			}
		}
		return null;
	}

	// Calculations

	private Vector3 Hex2Cartesian(HexPosition position) {
		Vector3 result = new Vector3();
		result.x = position.x - 0.5f * position.v;
		result.y = 0;
		result.z = Mathf.Sqrt (3) / 2 * position.v;
		return result;
	}

	private float GenHeight(HexPosition position, int iteration = 0) {
		bool genCore = iteration >= fractalDepth;
		int depth = (genCore ? 2 : 1) * fractalDepth - iteration;
		if (depth < 0) {
			return Random.Range(0, coreDelta0);
		}
		long delta = 1 << iteration;
		long lx = position.x % delta;
		long lv = position.v % delta;
		if(lx != 0 || lv != 0) {
			float dh = Random.Range(0, (depth >= fractalDepth) ? CoreDelta(depth) : SurfDelta(depth - fractalDepth)); 
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
		return GenHeight (pos, iteration);
	}

	private float CoreDelta(int iteration) {
		return coreDelta0 * Mathf.Pow (coreLambda, iteration);
	}

	private float SurfDelta(int iteration) {
		return surfDelta0 * Mathf.Pow (surfLambda, iteration);
	}
}
