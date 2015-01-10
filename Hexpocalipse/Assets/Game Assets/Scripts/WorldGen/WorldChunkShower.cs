using UnityEngine;
using System.Collections;

public class WorldChunkShower : MonoBehaviour {
	
	public HexPosition hexPosition;
	
	void OnTriggerEnter (Collider other) {
		GetComponentInParent<WorldChunk> ().ShowPrisms ();
	}
}
