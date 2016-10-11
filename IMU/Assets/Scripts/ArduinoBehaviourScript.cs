﻿using UnityEngine;
using System.Collections;
using System.IO.Ports;

public class ArduinoBehaviourScript : MonoBehaviour
{

	public static SerialPort serialPort = new SerialPort ("/dev/cu.usbmodem1A132", 9600);
	public static string inputString;

	// Use this for initialization
	void Start () {
		
		OpenConnection ();
	}
	
	// Update is called once per frame
	void Update () {
		
		inputString = serialPort.ReadLine ();
		print (inputString);
		string[] param = inputString.Split (';');

		if (param.Length == 4) {
			float w = (float.Parse (param [0]));
			float x = (float.Parse (param [1]));
			float y = (float.Parse (param [2]));
			float z = (float.Parse (param [3]));
			TransformCube (w, x, y, z);
		}


		if (param.Length == 3) {
			float yaw = (float.Parse (param [0]));// / 0.3f) * 360f;
			float pitch = (float.Parse (param [1]));// / 0.3f) * 360f;
			float roll = (float.Parse (param [2]));// / 0.3f) * 360f;
			TransformCube (yaw, pitch, roll);
		}


	}

	void TransformCube (float w, float x, float y, float z) {
		Quaternion quat = Quaternion.identity;
		quat.w = w;
		quat.x = y;
		quat.y = z;
		quat.z = x;
		gameObject.transform.rotation = quat;
	}
		

	void TransformCube(float yaw, float pitch, float roll) {
		print ("transforming cube...");
		//gameObject.transform.Rotate(0f, 0f, roll, Space.Self);
		//gameObject.transform.Rotate(pitch, 0f, 0f, Space.Self);
		//gameObject.transform.Rotate(0f, yaw, 0f, Space.Self);
		Quaternion AddRot = Quaternion.identity;
		AddRot.eulerAngles = new Vector3(pitch, yaw, -roll);
		gameObject.transform.rotation = AddRot;
		//gameObject.transform.rotation = Quaternion.Euler (pitch, -yaw, -roll);
	
	}

	void OpenConnection() {
		if (serialPort != null) {
			if (serialPort.IsOpen) {
				serialPort.Close ();
				print ("Closing port because it was already open!");
			} else {
				serialPort.Open ();
				serialPort.ReadTimeout = 500;
				print ("Port opened");
			}
		} else {
			if (serialPort.IsOpen) {
				print ("Port is already open!");
			} else {
				print ("Port is null!");
			}
		}
	}

	void OnApplicationQuit() {
		if (serialPort != null) serialPort.Close ();
	}
}

