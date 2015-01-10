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
		prisms = WorldGen.mainWorldGen ().MakePrisms (hexPosition, prismContainer.transform);
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
}
