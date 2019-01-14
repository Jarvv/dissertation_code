using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.IO;
using UnityEngine.SceneManagement;

public class Results : MonoBehaviour {

	public GameObject fragment01, fragment02, combinedFragments, resultsUI;

	private Server server;
	private ResultSet res;
	private Matrix4x4 trs;
	private string rms, noiseRms;

	void Start(){
		server = GameObject.Find ("Connections").GetComponent<Server> ();
	}

	void Update(){
		if (server.resultsRecieved) {
			server.resultsRecieved = false;
			res = ReadFromResultFile ();
			rms = res.rmse;
			trs = res.trs;
			noiseRms = res.nrmse;
			resultsUI.GetComponent<UserInterfaceManager>().LoadUI (rms, trs, noiseRms);
		}
	}

	// A set of results containing the RMSE value, the TRS matrix and the actual RMSE value that the
	// fragment was created with (only applicable to generated fragments).
	private class ResultSet{
		public string rmse { get; set; }
		public Matrix4x4 trs { get; set; }
		public string nrmse { get; set; }
	}
		
	// Reads from results file, get the rmse value and create the trs matrix to be used in transformation.
	ResultSet ReadFromResultFile(){
		StreamReader results = new StreamReader ("result.txt");
		Matrix4x4 trs = new Matrix4x4 ();
		string line = "";

		rms = results.ReadLine ();

		// Testing the new percentage error.
		float percentageError = 0;
		if(server.message == "fragment01")
			percentageError = (float.Parse(rms) / fragment01.GetComponent<MeshCollider> ().bounds.extents.sqrMagnitude) * 100;
		else if(server.message == "fragment02")
			percentageError = (float.Parse(rms) / fragment02.GetComponent<MeshCollider> ().bounds.extents.sqrMagnitude) * 100;
		Debug.Log (percentageError);

		// Get the values for the trs matrix from the results file.
		while (!results.EndOfStream) {
			for (int i = 0; i < 4; i++) {
				for (int j = 0; j < 4; j++) {
					line = results.ReadLine ();
					trs [i, j] = float.Parse (line);
				}
			}
		}
		results.Close ();

		noiseRms = "0";
		string meshName = fragment01.GetComponent<MeshFilter> ().mesh.name;
		// Read the fracturedXrmse.txt file to get the actual rmse value created in Blender.
		if (meshName.Length == 22) {
			string cuts = meshName.Substring (12, 1);
			StreamReader rmsNoise = new StreamReader ("fractured" + cuts + "rmse.txt");
			noiseRms = rmsNoise.ReadLine ();
		}
			
		return new ResultSet { rmse = rms, trs = trs, nrmse = noiseRms };
	}

	// With the TRS matrix, apply the transformation based on the message recieved from MATLAB,
	// the 'Moving' point cloud in the ICP algorithm.
	public void ApplyTransformation(){
		if(server.message == "fragment01")
			TransformWithTrs (fragment01, trs);
		else if(server.message == "fragment02")
			TransformWithTrs (fragment02, trs);

		// Combine the two fragments so that users can visually see the results.
		CombineFragments (fragment02, fragment01, combinedFragments);
	}

	//Transforms the given gameobject the with transformation matrix.
	void TransformWithTrs(GameObject obj, Matrix4x4 trs){
		Vector3[] vertices = obj.GetComponent<MeshFilter> ().mesh.vertices;
		Vector3 pos = obj.transform.position;
		pos = trs.MultiplyPoint3x4 (pos);
		pos = obj.transform.InverseTransformPoint (pos);
		for (int i=0; i < vertices.Length ; i++) {
			vertices[i] = obj.transform.TransformPoint (vertices[i]);
			vertices[i] = trs.MultiplyPoint3x4 (vertices[i]);
			vertices[i] = obj.transform.InverseTransformPoint (vertices[i]);
		}

		obj.GetComponent<MeshFilter> ().mesh.vertices = vertices;
	}
		
	// Using the meshes from both fragment gameobjects, combine then into a single mesh in
	// a different gameobject. Set the new fragment to be active within the scene and the
	// other two inactive. Alternatively set the meshes to a parent and create a collider
	// around, simulating a single mesh.
	void CombineFragments(GameObject f1, GameObject f2, GameObject newMesh){
		Destroy (f1.GetComponent<MeshCollider> ());
		Destroy (f2.GetComponent<MeshCollider> ());

		f1.transform.SetParent (newMesh.transform);
		f2.transform.SetParent (newMesh.transform);

		bool hasBounds = false;
		Bounds bounds = new Bounds (Vector3.zero, Vector3.zero);
		for (int i = 0; i < newMesh.transform.childCount; i++) {
			Renderer renderer = newMesh.transform.GetChild (i).GetComponent<Renderer>();
			if (renderer != null) {
				if (hasBounds) {
					bounds.Encapsulate(renderer.bounds);
				}
				else {
					bounds = renderer.bounds;
					hasBounds = true;
				}
			}
		}

		BoxCollider collider = newMesh.AddComponent<BoxCollider> ();
		collider.center = bounds.center - newMesh.transform.position;
		collider.size = bounds.size;

		//  Only works with Untiy's latest version removing mesh limit.
//		MeshFilter[] meshFilters = new MeshFilter[2];
//		meshFilters[0] = f1.GetComponent<MeshFilter> ();
//		meshFilters[1] = f2.GetComponent<MeshFilter> ();
//
//		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
//
//		for (int i = 0; i < meshFilters.Length; i++) {
//			combine [i].mesh = meshFilters[i].mesh;
//			combine [i].transform = meshFilters [i].transform.localToWorldMatrix;
//			meshFilters [i].gameObject.SetActive (false);
//		}
//
//		// Set the mesh filter and collider of newMesh to be a combination of the two fragment meshes
//		newMesh.transform.GetComponent<MeshFilter>().mesh = new Mesh ();
//		// Do not want to merge submeshes as separate colours are needed for each mesh
//		newMesh.transform.GetComponent<MeshFilter>().mesh.CombineMeshes (combine, false);
//		newMesh.GetComponent<MeshCollider> ().sharedMesh = newMesh.transform.GetComponent<MeshFilter> ().mesh;
//
//		newMesh.transform.gameObject.SetActive (true);


	}

	// Reset the scene by reloading the scene to its initial state.
	public void ResetFragments(){		
		Scene scene = SceneManager.GetActiveScene ();
		SceneManager.LoadScene (scene.buildIndex);
	}
		
}
