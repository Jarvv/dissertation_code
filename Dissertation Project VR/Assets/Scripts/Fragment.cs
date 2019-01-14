using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

public class Fragment : MonoBehaviour {

	private Color32[] colors32;
	private Mesh mesh;
	private Mesh colliderMesh;
	private Vector3[] vertices;
	private int[] triangles;
	private Vector3[] normals;

	private List<Vector3> selectedVertices = new List<Vector3>();
	private List<Vector3> selectedNormals = new List<Vector3> ();
	private HashSet<Vector3> hashSelectedVertices = new HashSet<Vector3> ();

	void Start(){
		mesh = GetComponent<MeshFilter>().mesh;
		colliderMesh = GetComponent<MeshCollider>().sharedMesh;
		vertices = mesh.vertices;
		colors32 = new Color32[vertices.Length];
		triangles = colliderMesh.triangles;
		normals = mesh.normals;
		InitPaintedVertices ();
	}

	// Paint the specified vertex red to indicate it has been selected
	private void PaintVertex(int vertIndex){
		colors32 [vertIndex] = Color32.Lerp (Color.red, Color.red, 1f);
		// Apply the new colour
		mesh.colors32 = colors32;
	}

	// Initialises the fragment colour by painting all vertices.
	private void InitPaintedVertices(){
		Color fragCol = new Color ();
		if (this.gameObject.name == "Fragment01")
			fragCol = Color.gray;
		else if (this.gameObject.name == "Fragment02")
			fragCol = new Color (0.8f, 0.8f, 0.8f, 1f);

		for (int i = 0; i < vertices.Length; i++) {
			colors32 [i] = Color32.Lerp (fragCol, fragCol, 0.1f);
		}
		mesh.colors32 = colors32;
	}

	// Called when the collider is hit by a ray from VertexSelection 
	public void SelectVertcies(RaycastHit hit, Transform tr){
		Vector3 triangleVertex = new Vector3 ();
		// For each vertex in the triangle
		for (int i = 0; i < 3; i++) {
			int vertIndex = triangles [hit.triangleIndex * 3 + i];
			triangleVertex = vertices [vertIndex];

			// Paint the vertices
			PaintVertex (vertIndex);

			// Add the vertex if it's unique
			if (hashSelectedVertices.Add (triangleVertex))
				// Add the normal to the selected vertex normals
				if (i == 0) selectedNormals.Add (normals [vertIndex]);
		}

	}
		
	// Finds the fracture region normal by getting the average normal from the list of selected normals.
	public Vector3 FindFractureRegionNormal(){
		Vector3 averageNormal = new Vector3 (0, 0, 0);
		if (selectedNormals.Count != 0) {
			averageNormal = this.transform.TransformDirection (selectedNormals [0]);
			for (int i = 1; i < selectedNormals.Count; i++) {
				averageNormal = ((this.transform.TransformDirection (selectedNormals [i])) + averageNormal).normalized;
			}
		} else {
			for (int i = 0; i < normals.Length; i++) {
				averageNormal = ((this.transform.TransformDirection (normals [i])) + averageNormal);
			}
		}
		// Return the normalised fracture region normal
		return averageNormal.normalized;
	}
		
	// Rotates the fragment to in the direction of the cross product of the two vectors.
	public void RotateToVector(Vector3 directionVector, Vector3 normal){
		Quaternion rotation = Quaternion.LookRotation (Vector3.Cross(directionVector, normal));
		transform.rotation = rotation;
		transform.position = new Vector3 (0, 0, 0);
	}
		
	// Returns the selected vertices list for this particular fragment.
	public List<Vector3> ReturnSelectedVertices(){
		Transform tr = this.transform;

		// Add the world space coordiantes of the vertices
		if (hashSelectedVertices.Count != 0) {
			foreach (Vector3 vertex in hashSelectedVertices) {
				selectedVertices.Add (tr.TransformPoint(vertex));

			}
		}
				
		// If no vertices are selected, use the whole of the mesh instead
		if (selectedVertices.Count == 0) {
//			foreach (Vector3 vertex in vertices) {
//				if(!selectedVertices.Contains(vertex)){
//					selectedVertices.Add (tr.TransformPoint(vertex));
//				}
//			}
		}
		return selectedVertices;
	}

	public bool IsPartiallySelected(){
		return (hashSelectedVertices.Count != 0);
	}

	public void ClearVertexList(){
		selectedVertices.Clear ();
	}

}