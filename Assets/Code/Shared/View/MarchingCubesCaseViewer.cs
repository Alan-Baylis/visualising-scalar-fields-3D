using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Haa.MarchingCubes.Model;

namespace Haa.MarchingCubes
{
	public class MarchingCubesCaseViewer : MonoBehaviour
	{
		[SerializeField] private Isosurface Mesh;
		[SerializeField] private InputField SpecifiedCase;
		[SerializeField] private Button SpecifiedCaseButton;
		[SerializeField] private Button NextCaseButton;
		[SerializeField] private int currentCase = 1;

		private GridCell gridCell;
		private Vector3[] vertices;
		private int[] triangles;
		private int triangleCount = 0;
		private float squareSize = 5;
		private bool initialised;

		void Start ()
		{
			gridCell = new GridCell (0, 0, 0, 5);
			triangles = new int[36];
			vertices = new Vector3[12];
			SpecifiedCase.text = "0";

			gridCell.edgePoints[0] = new Vector3(0.5f, 0, 1);
			gridCell.edgePoints[1] = new Vector3(1, 0, 0.5f);
			gridCell.edgePoints[2] = new Vector3(0.5f, 0, 0);
			gridCell.edgePoints[3] = new Vector3(0, 0, 0.5f);

			gridCell.edgePoints[4] = new Vector3(0.5f, 1, 1);
			gridCell.edgePoints[5] = new Vector3(1, 1, 0.5f);
			gridCell.edgePoints[6] = new Vector3(0.5f, 1, 0);
			gridCell.edgePoints[7] = new Vector3(0, 1, 0.5f);

			gridCell.edgePoints[8] = new Vector3(0, 0.5f, 1);
			gridCell.edgePoints[9] = new Vector3(1, 0.5f, 1);
			gridCell.edgePoints[10] = new Vector3(1, 0.5f, 0);
			gridCell.edgePoints[11] = new Vector3(0, 0.5f, 0);
			Mesh.Initialise ();
			initialised = true;
			ShowCase ();
		}

		public void ShowNextCase ()
		{
			SpecifiedCaseButton.enabled = false;
			NextCaseButton.enabled = false;
			currentCase++;
			ShowCase();
			SpecifiedCaseButton.enabled = true;
			NextCaseButton.enabled = true;
		}

		public void ShowSpecifiedCase ()
		{
			SpecifiedCaseButton.enabled = false;
			NextCaseButton.enabled = false;
			if(!string.IsNullOrEmpty(SpecifiedCase.text))
			{
				currentCase = System.Convert.ToInt32(SpecifiedCase.text);
				ShowCase();
			}
			SpecifiedCaseButton.enabled = true;
			NextCaseButton.enabled = true;
		}

		private void ShowCase ()
		{
			triangleCount = 0;
			ConfigureGridCell ();
			System.Array.Clear (triangles, triangleCount, triangles.Length - triangleCount);
			Mesh.DrawMesh (vertices, triangles);
		
		}

		private int ConfigureGridCell ()
		{
			if (MarchingCubesLookupTables.edgeTable [currentCase] == 0)
				return(0);

			if ((MarchingCubesLookupTables.edgeTable [currentCase] & 1) == 1)
			{
				vertices [0] = gridCell.edgePoints[0]*squareSize;
			}
			if ((MarchingCubesLookupTables.edgeTable [currentCase] & 2) == 2)
			{
				vertices [1] = gridCell.edgePoints[1]*squareSize;
			}
			if ((MarchingCubesLookupTables.edgeTable [currentCase] & 4) == 4)
			{
				vertices [2] = gridCell.edgePoints[2]*squareSize;
			}
			if ((MarchingCubesLookupTables.edgeTable [currentCase] & 8) == 8)
			{
				vertices [3] = gridCell.edgePoints[3]*squareSize;
			}
			if ((MarchingCubesLookupTables.edgeTable [currentCase] & 16) == 16)
			{
				vertices [4] = gridCell.edgePoints[4]*squareSize;
			}
			if ((MarchingCubesLookupTables.edgeTable [currentCase] & 32) == 32)
			{
				vertices [5] = gridCell.edgePoints[5]*squareSize;
			}
			if ((MarchingCubesLookupTables.edgeTable [currentCase] & 64) == 64)
			{
				vertices [6] = gridCell.edgePoints[6]*squareSize;
			}
			if ((MarchingCubesLookupTables.edgeTable [currentCase] & 128) == 128)
			{
				vertices [7] = gridCell.edgePoints[7]*squareSize;
			}
			if ((MarchingCubesLookupTables.edgeTable [currentCase] & 256) == 256)
			{
				vertices [8] = gridCell.edgePoints[8]*squareSize;
			}
			if ((MarchingCubesLookupTables.edgeTable [currentCase] & 512) == 512)
			{
				vertices [9] = gridCell.edgePoints[9]*squareSize;
			}
			if ((MarchingCubesLookupTables.edgeTable [currentCase] & 1024) == 1024)
			{
				vertices [10] = gridCell.edgePoints[10]*squareSize;
			}
			if ((MarchingCubesLookupTables.edgeTable [currentCase] & 2048) == 2048)
			{
				vertices [11] = gridCell.edgePoints[11]*squareSize;
			}


			for (int i = 0; MarchingCubesLookupTables.triTable [currentCase, i] != -1; i += 3)
			{

				triangles [triangleCount] = MarchingCubesLookupTables.triTable [currentCase, i];
				triangles [triangleCount + 1] = MarchingCubesLookupTables.triTable [currentCase, i+1];
				triangles [triangleCount + 2] = MarchingCubesLookupTables.triTable [currentCase, i+2];
				triangleCount += 3;

			}
			return(triangleCount);
		}

		Vector3 VertexInterp (float isolevel, Vector3 p1, Vector3 p2, float valp1, float valp2)
		{
			float mu;
			Vector3 p;

			if (Mathf.Abs (isolevel - valp1) < 0.00001)
				return(p1);
			if (Mathf.Abs (isolevel - valp2) < 0.00001)
				return(p2);
			if (Mathf.Abs (valp1 - valp2) < 0.00001)
				return(p1);
			mu = (isolevel - valp1) / (valp2 - valp1);
			p.x = p1.x + mu * (p2.x - p1.x);
			p.y = p1.y + mu * (p2.y - p1.y);
			p.z = p1.z + mu * (p2.z - p1.z);

			return(p);
		}

		#region Debug

		#if UNITY_EDITOR

		public bool DrawDebugLines;

		void OnDrawGizmos ()
		{
			if(DrawDebugLines && initialised)
			{
				Debug.DrawLine (gridCell.VertexPosition[0], gridCell.VertexPosition[1], Color.yellow, 0.1f);
				Debug.DrawLine (gridCell.VertexPosition[3], gridCell.VertexPosition[2], Color.yellow, 0.1f);

				Debug.DrawLine (gridCell.VertexPosition[0], gridCell.VertexPosition[3], Color.magenta, 0.1f);
				Debug.DrawLine (gridCell.VertexPosition[1], gridCell.VertexPosition[2], Color.magenta, 0.1f);

				Debug.DrawLine (gridCell.VertexPosition[4], gridCell.VertexPosition[5], Color.yellow, 0.1f);
				Debug.DrawLine (gridCell.VertexPosition[7], gridCell.VertexPosition[6], Color.yellow, 0.1f);

				Debug.DrawLine (gridCell.VertexPosition[4], gridCell.VertexPosition[7], Color.magenta, 0.1f);
				Debug.DrawLine (gridCell.VertexPosition[5], gridCell.VertexPosition[6], Color.magenta, 0.1f);

				Debug.DrawLine (gridCell.VertexPosition[0], gridCell.VertexPosition[4], Color.cyan, 0.1f);
				Debug.DrawLine (gridCell.VertexPosition[1], gridCell.VertexPosition[5], Color.cyan, 0.1f);

				Debug.DrawLine (gridCell.VertexPosition[2], gridCell.VertexPosition[6], Color.cyan, 0.1f);
				Debug.DrawLine (gridCell.VertexPosition[3], gridCell.VertexPosition[7], Color.cyan, 0.1f);
			}

		}
		#endif
		#endregion
	}


}