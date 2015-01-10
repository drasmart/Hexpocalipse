using UnityEngine;
using System.Collections;

public class WorldChunkLoader : MonoBehaviour {

	public HexPosition hexPosition;

	void OnTriggerEnter (Collider other) {
		WorldGen.mainWorldGen().LoadChunk (hexPosition);
		GameObject.DestroyObject (transform.parent.gameObject);
	}
}
