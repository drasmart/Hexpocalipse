using UnityEngine;
using System.Collections;

public class WorldChunkUnloader : MonoBehaviour {
	
	public HexPosition hexPosition;
	
	void OnTriggerEnter (Collider other) {
		GameObject.DestroyObject (transform.parent.gameObject);
	}
}
