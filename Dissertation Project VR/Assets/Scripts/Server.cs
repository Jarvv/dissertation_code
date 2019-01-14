using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System;
using System.IO;
using System.Text;

public class Server : MonoBehaviour {

	// Indicates to any other scripts that the results file has been written to.
	public bool resultsRecieved = false;
	public string message = "";

	private TcpListener listener;
	private String msg;

	void Start () {
		listener = new TcpListener (55001);
		listener.Start ();
		print ("is listening");
	}

	// Called once per frame, waiting to recieve a message from MATLAB.
	void Update () {
		if (!listener.Pending ()) {
		}
		else 
		{
			print ("socket comes");
			TcpClient client = listener.AcceptTcpClient ();
			NetworkStream ns = client.GetStream ();
			StreamReader reader = new StreamReader (ns);
			msg = reader.ReadToEnd();
			message = msg;
			reader.Close ();
			resultsRecieved = true;
		}
	}

	// Listener must be stopped before a new one is opened.
	public void Reset(){
		listener.Stop ();
	}
		
}