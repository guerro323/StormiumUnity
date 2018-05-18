using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setvelocity : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update ()
	{
		GetComponent<Rigidbody>().velocity = new Vector3(0, -1000, 0);
	}
}
