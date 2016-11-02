using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

public class ArduinoBehaviourScript : MonoBehaviour
{
	int QUAT_RANGE = 1073741824; // 2^30
	Thread uartThread;
	public static SerialPort serialPort = new SerialPort ("/dev/cu.usbmodem1A131", 115200);
	public static string inputString;
	GameObject upperArmRight;
	GameObject lowerArmRight;
	GameObject head;
	Quaternion upperArmRightQuat = Quaternion.identity;
	Quaternion lowerArmRightQuat = Quaternion.identity;
	float[] quatInitial = new float[4];
	bool firstReading = true;

	// Use this for initialization
	void Start () {
		
		OpenConnection ();
		upperArmRight = GameObject.Find("upperarm_r");
		lowerArmRight = GameObject.Find ("lowerarm_r");
		upperArmRightQuat = upperArmRight.transform.rotation;
		lowerArmRightQuat = lowerArmRight.transform.rotation;
		head = GameObject.Find ("head");
		ThreadStart mThreadStart = new ThreadStart (UARTStreamer);
		uartThread = new Thread (mThreadStart);
		uartThread.Start ();
	}
	
	/*
	 * Called once per frame.
	 * The data from the UART is composed of 16 bytes on each line, expressed as a heximal value.
	 * These 16 bytes represents w, x, y, z of 4 bytes each
	 * The 4 bytes represents an 32-bit integer that is the value of the quaternion unit.
	 * 
	*/

	void Update() {
		upperArmRight.transform.rotation = upperArmRightQuat;
		lowerArmRight.transform.rotation = lowerArmRightQuat;

	}

	void UARTStreamer () {
		print ("UART stream running");
		while(true) {
			try {
				inputString = serialPort.ReadLine ();
				print (inputString);
				string[] input = inputString.Split (':');

				if (input.Length == 17) {
					byte id = byte.Parse(input[0], System.Globalization.NumberStyles.HexNumber);
					byte[] param = new byte[16];
					for (int i = 0; i < 16; i++) {
						param[i] = byte.Parse (input [i+1], System.Globalization.NumberStyles.HexNumber);
					}
					Array.Reverse(param);
					float w = Int32ToQuaternionFloat(ByteArrayToInt32(param, 0));
					float x = Int32ToQuaternionFloat(ByteArrayToInt32(param, 4));
					float y = Int32ToQuaternionFloat(ByteArrayToInt32(param, 8));
					float z = Int32ToQuaternionFloat(ByteArrayToInt32(param, 12));
					if (firstReading) {
						firstReading = false;
						quatInitial[0] = w;
						quatInitial[1] = x;
						quatInitial[2] = y;
						quatInitial[3] = z;
					}
					Quaternion quat = Quaternion.identity;
					quat.w = w - quatInitial[0];
					quat.x = x - quatInitial[1];
					quat.y = y - quatInitial[2];
					quat.z = z - quatInitial[3];
					lowerArmRightQuat = quat;
					/*
					switch (id) {
						case 0x00:
							upperArmRightQuat = quat;
							break;
						case 0x01:
							lowerArmRightQuat = quat;
							break;
					*/
				} 
			} catch (TimeoutException) {
				print ("TIMEOUT");
			} catch (FormatException) {
				print ("INCORRECT FORMAT");
			}
		}
	}

	float Int32ToQuaternionFloat(int quatInt32) {
		return (float)quatInt32/(float)QUAT_RANGE;
	}

	int ByteArrayToInt32(byte[] array, int index) {
		int i = BitConverter.ToInt32(array, index);
		return i;
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
		if (uartThread != null)
			uartThread.Abort ();
		if (serialPort != null)
			serialPort.Close ();
	}
}

