using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using System.Linq;
using System.Collections.Generic;

public class ArduinoBehaviourScript : MonoBehaviour
{

	public static SerialPort serialPort = new SerialPort ("/dev/cu.usbmodem1d11231", 115200);
	public static string inputString;
	GameObject upperArmRight;
	GameObject lowerArmRight;
	GameObject head;

	// Use this for initialization
	void Start () {
		
		OpenConnection ();
		upperArmRight = GameObject.Find("upperarm_r");
		lowerArmRight = GameObject.Find ("lowerarm_r");
		head = GameObject.Find ("head");
	}
	
	// Update is called once per frame
	void Update () {

		try {
			inputString = serialPort.ReadLine ();
			print (inputString);
			string[] input = inputString.Split (':');

			if (input.Length == 16) {
				byte[] param = new byte[16];
				for (int i = 0; i < 16; i++) {
					param[i] = byte.Parse (input [i], System.Globalization.NumberStyles.HexNumber);
				}

				float w = ByteArrayToFloat (param, 0);
				float x = ByteArrayToFloat (param, 4);
				float y = ByteArrayToFloat (param, 8);
				float z = ByteArrayToFloat (param, 12);
				TransformCube (w, x, y, z);
			}
		} catch (TimeoutException) {
			print ("TIMEOUT");
		}
		// Discard buffer in case we are not keeping up to speed with the IMUs
		serialPort.DiscardInBuffer();
	}

	float ByteArrayToFloat(byte[] array, int index) {
		return System.BitConverter.ToSingle (array, index);
	}

	void TransformCube (float w, float x, float y, float z) {
		Quaternion quat = Quaternion.identity;
		quat.w = w;
		quat.x = y;
		quat.y = z;
		quat.z = x;
		//upperArmRight.transform.rotation = quat;
		//lowerArmRight.transform.rotation = quat;
		head.transform.rotation = quat;
	}

	void OpenConnection() {
		if (serialPort != null) {
			if (serialPort.IsOpen) {
				serialPort.Close ();
				print ("Closing port because it was already open!");
			} else {
				serialPort.Open ();
				serialPort.ReadTimeout = 40; // 25Hz
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

