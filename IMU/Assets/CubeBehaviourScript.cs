using UnityEngine;
using System.Collections;

public class CubeBehaviourScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameObject cube = gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.Q)) {
			TransformCube(new Vector3(-1, 0, 0));
		}
		if (Input.GetKey(KeyCode.W)) {
			TransformCube(new Vector3(0, -1, 0));
		}
		if (Input.GetKey(KeyCode.E)) {
			TransformCube(new Vector3(0, 0, -1));
		}
		if (Input.GetKey(KeyCode.A)) {
			TransformCube(new Vector3(1, 0, 0));
		}
		if (Input.GetKey(KeyCode.S)) {
			TransformCube(new Vector3(0, 1, 0));
		}
		if (Input.GetKey(KeyCode.D)) {
			TransformCube(new Vector3(0, 0, 1));
		}

		TransformCube (new Vector3 (Input.acceleration.x, 0, Input.acceleration.z));
	}

	void ReadFromSerial() {
		
	}

	void TransformCube(Vector3 eulerAngles) {
		gameObject.transform.Rotate(eulerAngles);
	}
}
