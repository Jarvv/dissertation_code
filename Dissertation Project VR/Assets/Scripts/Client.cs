using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using System.IO;
using System.Text;
using System.Threading;

public class Client : MonoBehaviour {

	public GameObject fragment01, fragment02;
	public bool confirmed = false;

	private List<Vector3> f1Vertices, f2Vertices;
	private Fragment f1, f2;
	private TcpClient client;
	private bool fileOneDone = false, fileTwoDone = false;

	// Called when the 'Confirm' button is pressed down, get the vertex list for each
	// fragment and set up the Unity client.
	public void GetVertexList(){
		f1 = fragment01.GetComponent<Fragment> ();
		f2 = fragment02.GetComponent<Fragment> ();

		// Find the fracture region normals to allow the fragments to rotate towards
		// eachother.
		Vector3 f1Normal = f1.FindFractureRegionNormal ();
		Vector3 f2Normal = f2.FindFractureRegionNormal ();
	
		if (Vector3.Dot (f1Normal, f2Normal) > -0.9) {
			//f2.RotateToVector(f1Normal, f2Normal);
			//f1.RotateToVector(f1Normal, f2Normal);
		}

		// Get the list of vertices selected on each fragment.
		f1Vertices = f1.ReturnSelectedVertices ();
		f2Vertices = f2.ReturnSelectedVertices ();
		WriteFiles ();
	}

	void Update(){
		// Checks if both files have been written to.
		if(fileOneDone && fileTwoDone){
			SetupSocket();
		}
	}

	// Spawn two threads to write to the text files simultaneously to avoid a lockup on the main thread.
	private void WriteFiles(){
		Thread threadOne = new Thread (WriteFileOne);
		Thread threadTwo = new Thread (WriteFileTwo);
		threadOne.Start ();
		threadTwo.Start ();
	}

	private void WriteFileOne(){
		WriteToFile ("pc1.txt", f1Vertices);
	}

	private void WriteFileTwo(){
		WriteToFile ("pc2.txt", f2Vertices);
	}

	private void SetupSocket() {
		fileOneDone = false;
		fileTwoDone = false;

		// Send a message to the MATLAB client indicating that the vertices
		// have been written to their files.
		try {
			String Host = "localhost";
			Int32 Port = 55000;
			client = new TcpClient(Host, Port);

			NetworkStream nStream = client.GetStream();
			// Send a different message depending on if the user has done any selection
			string message = "";
			if(f1.IsPartiallySelected() || f2.IsPartiallySelected())
				message = "0";
			else
				message = "1";
			
			byte[] byteArray = Encoding.UTF8.GetBytes(message);
			nStream.Write(byteArray, 0, byteArray.Length);
		}
		catch (Exception e) {
			Debug.Log("Socket error: " + e);
		}
	}
		
	//  Writes the fragments x,y,z positions of its mesh vertices to a file.
	void WriteToFile(string fileName, List<Vector3> vertList){
		StreamWriter sw = new StreamWriter (fileName);
		string xPosition = ""; 
		string yPosition = ""; 
		string zPosition = "";

		foreach (Vector3 vertex in vertList) {
			xPosition += vertex.x + " ";
			yPosition += vertex.y + " ";
			zPosition += vertex.z + " ";
		}
	
		sw.WriteLine (xPosition);
		sw.WriteLine (yPosition);
		sw.WriteLine (zPosition);
		sw.Dispose ();
		sw.Close();

		// Indicate the thread has completed writing to the text file.
		if (fileName == "pc1.txt")
			fileOneDone = true;	
		if (fileName == "pc2.txt")
			fileTwoDone = true;
	}

}