#pragma strict

public var force: float = 20;

function Update () {
	if (Input.GetKeyDown(KeyCode.F)) {
		rigidbody.AddForce(transform.up * force);
	}
}