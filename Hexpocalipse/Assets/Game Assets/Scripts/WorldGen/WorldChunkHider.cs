using UnityEngine;
using System.Collections;

public class WorldChunkHider : MonoBehaviour {
	
	public HexPosition hexPosition;
	
	void OnTriggerEnter (Collider other) {
		GetComponentInParent<WorldChunk> ().HidePrisms ();
	}
}
