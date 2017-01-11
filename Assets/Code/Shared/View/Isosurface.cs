using UnityEngine;

namespace Haa.MarchingCubes
{
	[RequireComponent (typeof (MeshFilter))]
	[RequireComponent (typeof (MeshRenderer))]
	public class Isosurface : MonoBehaviour 
	{
		[SerializeField] private float Threshold = 1f;
		private const int MAX_VERTEX_VALUE = 1;

		private Mesh mesh;

		private int vertexCount;
		private int triangleCount;

		public void Initialise ()
		{
			mesh = new Mesh();
			mesh.MarkDynamic ();
			GetComponent<MeshFilter>().mesh = mesh;
		}

		public void DrawMesh(Vector3[] vertices, int[] triangles)
		{
			mesh.Clear();
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.RecalculateNormals();
		}

		public void DrawMesh(Vector3[] vertices, int[] triangles, Color[] colours)
		{
			mesh.Clear();
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.colors = colours;
			mesh.RecalculateNormals();
		}

		public float ThresholdValue
		{
			get
			{
				return Threshold;
			}
		}

	}
}

