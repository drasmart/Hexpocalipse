using UnityEngine;
using System.Collections;

public class WorldChunk : WorldGenPrismData {
	private WorldPrism[,] prisms;

	private GameObject prismContainer;

	private GameObject showerTrigger;
	private GameObject hiderTrigger;
	private GameObject unloadTrigger;

	// Lifecycle

	private void Awake() {
		prismContainer = new GameObject ();
		showerTrigger  = new GameObject ();
		hiderTrigger   = new GameObject ();
		unloadTrigger  = new GameObject ();
		
		prismContainer.transform.parent = transform;
		 showerTrigger.transform.parent = transform;
		  hiderTrigger.transform.parent = transform;
		 unloadTrigger.transform.parent = transform;
	}

	private void Start() {
		prisms = WorldGen.mainWorldGen ().MakePrisms (hexPosition, ref prismContainer);
	}

	private void OnDestroy() {
		UnloadChunk ();
	}

	// Interface

	public void HidePrisms() {
		prismContainer.SetActive (false);
	}

	public void ShowPrisms() {
		prismContainer.SetActive (true);
	}

	public void UnloadChunk() {
		// TODO
	}

	public float heightAt(HexPosition position) {
		return prisms[position.x - hexPosition.x, position.v - hexPosition.v].transform.localPosition.y;
	}
}
