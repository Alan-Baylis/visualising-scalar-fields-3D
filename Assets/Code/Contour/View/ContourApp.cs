#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Haa.MarchingCubes.Model;
using Haa.MarchingCubes;

namespace Haa.MarchingCubes.Contour.View
{
	public class ContourApp : MonoBehaviour
	{
		[SerializeField] private Color Color1 = new Color ();
		[SerializeField] private Color Color2 = new Color ();
		[SerializeField] private Color Color3 = new Color ();
		[SerializeField] private Color Color4 = new Color ();
		[SerializeField] private Color Color5 = new Color ();
		private Color[] ColourMap;

		[SerializeField] private Vector3 GridSize = new Vector3 (80, 80, 1);
		[SerializeField] private Vector3 ConeCentre = new Vector3 (2, 0, 2);
		[SerializeField] private float ConeSize = 0.1f;
		[SerializeField] private Isosurface Mesh;
		[SerializeField] private float Isolevel = 1f;
		[SerializeField] private float SquareSize = 0.1f;
		[SerializeField] private int TotalVertices = 500;

		private Vector3[] vertices;
		private int[] triangles;
		private Color[] colours;
		private int triangleCount = 0;
		private int vertexCount = 0;
		private float TanAngle = Mathf.Tan (30 * Mathf.Deg2Rad);
		private int GridX = 80;
		private int GridY = 80;
		private int GridZ = 1;
		private GridCell[] grid;

		private bool initialised;

		void Start ()
		{
			ColourMap = new Color[]{ Color1, Color2, Color3, Color4, Color5 };
			GenerateMesh ();
			GridX = (int)GridSize.x;
			GridY = (int)GridSize.y;
			GridZ = (int)GridSize.z;

			vertices = new Vector3[TotalVertices];
			colours = new Color[TotalVertices];
			triangles = new int[TotalVertices * 6];
			grid = new GridCell[GridX * GridY * GridZ];

			for (int y = 0; y < GridY; y++)
			{
				for (int z = 0; z < GridZ; z++)
				{
					for (int x = 0; x < GridX; x++)
					{
						int squareIndex = (y * GridX * GridZ) + (GridX * z) + x;
						GridCell cell = new GridCell ((x * SquareSize), (y * SquareSize), (z * SquareSize), SquareSize);
						if (x > 0)
							cell.neighbours [0] = grid [squareIndex - 1];
						if (z > 0)
							cell.neighbours [1] = grid [squareIndex - GridX];
						if (y > 0)
							cell.neighbours [2] = grid [squareIndex - (GridX * GridZ)];
						
						grid [squareIndex] = cell;
						SetCornerValues (cell);
						ConfigureGridCell (cell, Isolevel, ref triangles);
					}
				}
			}
		
			initialised = true;
			Mesh.DrawMesh (vertices, triangles, colours);
		}

		void SetCornerValues (GridCell cell)
		{
			cell.VertexValue [0] = 0;
			cell.VertexValue [1] = 0;
			cell.VertexValue [2] = 0;
			cell.VertexValue [3] = 0;
			cell.VertexValue [4] = 0;
			cell.VertexValue [5] = 0;
			cell.VertexValue [6] = 0;
			cell.VertexValue [7] = 0;

			if (cell.neighbours [0] != null)
			{
				cell.VertexValue [0] = cell.neighbours [0].VertexValue [1];
				cell.VertexValue [3] = cell.neighbours [0].VertexValue [2];
				cell.VertexValue [4] = cell.neighbours [0].VertexValue [5];
				cell.VertexValue [7] = cell.neighbours [0].VertexValue [6];
			}
			if (cell.neighbours [1] != null)
			{
				if (cell.neighbours [0] == null)
					cell.VertexValue [3] = cell.neighbours [1].VertexValue [0];
				if (cell.neighbours [0] == null)
					cell.VertexValue [7] = cell.neighbours [1].VertexValue [4];
				cell.VertexValue [6] = cell.neighbours [1].VertexValue [5];
				cell.VertexValue [2] = cell.neighbours [1].VertexValue [1];
			}
			if (cell.neighbours [2] != null)
			{
				if (cell.neighbours [0] == null)
					cell.VertexValue [0] = cell.neighbours [2].VertexValue [4];
				if (cell.neighbours [1] == null)
					cell.VertexValue [2] = cell.neighbours [2].VertexValue [6];
				if (cell.neighbours [0] == null && cell.neighbours [1] == null)
					cell.VertexValue [3] = cell.neighbours [2].VertexValue [7];
				cell.VertexValue [1] = cell.neighbours [2].VertexValue [5];
			}

			cell.VertexValue [5] = GetValueForPoint (cell.VertexPosition [5]);

			if (cell.neighbours [0] == null)
			{
				cell.VertexValue [4] = GetValueForPoint (cell.VertexPosition [4]);
				if (cell.neighbours [1] == null)
				{
					cell.VertexValue [7] = GetValueForPoint (cell.VertexPosition [7]);

					if (cell.neighbours [2] == null)
					{
						cell.VertexValue [3] = GetValueForPoint (cell.VertexPosition [3]);
					}
				}
				if (cell.neighbours [2] == null)
				{
					cell.VertexValue [0] = GetValueForPoint (cell.VertexPosition [0]);
				}
			}
			if (cell.neighbours [1] == null)
			{
				cell.VertexValue [6] = GetValueForPoint (cell.VertexPosition [6]);
				if (cell.neighbours [2] == null)
				{
					cell.VertexValue [2] = GetValueForPoint (cell.VertexPosition [2]);
				}
			}
			if (cell.neighbours [2] == null)
			{
				cell.VertexValue [1] = GetValueForPoint (cell.VertexPosition [1]);
			}
		}

		private void SetVertexValue (Vector3 point, GridCell cell, int vertexIndex)
		{
			cell.VertexValue [vertexIndex] = GetValueForPoint (point);
		}

		private float GetValueForPoint (Vector3 point)
		{
			float distanceFromPeak = (ConeSize * SquareSize) - point.y;

			if (distanceFromPeak <= 0)
				return Isolevel;

			Vector3 corePosition = ConeCentre;
			corePosition.y = point.y;
			float distance = Vector3.Distance (point, corePosition);
			float radiusAtHeight = distanceFromPeak * TanAngle;

			return distance / radiusAtHeight;
		}

		private void GenerateMesh ()
		{
			Mesh.Initialise ();
		}

		private int ConfigureGridCell (GridCell gridCell, float isolevel, ref int[] triangles)
		{
			int i;
			int caseIndex = 0;

			if (gridCell.VertexValue [0] < isolevel)
				caseIndex |= 1;
			if (gridCell.VertexValue [1] < isolevel)
				caseIndex |= 2;
			if (gridCell.VertexValue [2] < isolevel)
				caseIndex |= 4;
			if (gridCell.VertexValue [3] < isolevel)
				caseIndex |= 8;
			if (gridCell.VertexValue [4] < isolevel)
				caseIndex |= 16;
			if (gridCell.VertexValue [5] < isolevel)
				caseIndex |= 32;
			if (gridCell.VertexValue [6] < isolevel)
				caseIndex |= 64;
			if (gridCell.VertexValue [7] < isolevel)
				caseIndex |= 128;


			if (MarchingCubesLookupTables.edgeTable [caseIndex] == 0)
				return(0);

			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 1) == 1 && gridCell.neighbours [2] == null)
			{
				gridCell.SetEdgePoint(0, 0, 1, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [0];
				SetColour (vertexCount, vertices [vertexCount].y);
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 2) == 2 && gridCell.neighbours [2] == null)
			{
				gridCell.SetEdgePoint(1, 2, 1, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [1];
				SetColour (vertexCount, vertices [vertexCount].y);
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 4) == 4 && gridCell.neighbours [1] == null && gridCell.neighbours [2] == null)
			{
				gridCell.SetEdgePoint(2, 2, 3, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [2];
				SetColour (vertexCount, vertices [vertexCount].y);
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 8) == 8 && gridCell.neighbours [0] == null && gridCell.neighbours [2] == null)
			{
				gridCell.SetEdgePoint(3, 3, 0, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [3];
				SetColour (vertexCount, vertices [vertexCount].y);
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 16) == 16)
			{
				gridCell.SetEdgePoint(4, 4, 5, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [4];
				SetColour (vertexCount, vertices [vertexCount].y);
				vertexCount++;

			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 32) == 32)
			{
				gridCell.SetEdgePoint(5, 5, 6, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [5];
				SetColour (vertexCount, vertices [vertexCount].y);
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 64) == 64 && gridCell.neighbours [1] == null)
			{
				gridCell.SetEdgePoint(6, 6, 7, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [6];
				SetColour (vertexCount, vertices [vertexCount].y);
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 128) == 128 && gridCell.neighbours [0] == null)
			{
				gridCell.SetEdgePoint(7, 7, 4, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [7];
				SetColour (vertexCount, vertices [vertexCount].y);
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 256) == 256 && gridCell.neighbours [0] == null)
			{
				gridCell.SetEdgePoint(8, 0, 4, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [8];
				SetColour (vertexCount, vertices [vertexCount].y);
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 512) == 512)
			{
				gridCell.SetEdgePoint(9, 1, 5, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [9];
				SetColour (vertexCount, vertices [vertexCount].y);
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 1024) == 1024 && gridCell.neighbours [1] == null)
			{
				gridCell.SetEdgePoint(10, 2, 6, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [10];
				SetColour (vertexCount, vertices [vertexCount].y);
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 2048) == 2048 && gridCell.neighbours [0] == null && gridCell.neighbours [1] == null)
			{
				gridCell.SetEdgePoint(11, 3, 7, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [11];
				SetColour (vertexCount, vertices [vertexCount].y);
				vertexCount++;
			}

			for (i = 0; MarchingCubesLookupTables.triTable [caseIndex, i] != -1; i += 3)
			{
				triangles [triangleCount + 2] = gridCell.GetVertexIndex (MarchingCubesLookupTables.triTable [caseIndex, i]);
				triangles [triangleCount + 1] = gridCell.GetVertexIndex (MarchingCubesLookupTables.triTable [caseIndex, i + 1]);
				triangles [triangleCount] = gridCell.GetVertexIndex (MarchingCubesLookupTables.triTable [caseIndex, i + 2]);
				triangleCount += 3;
			}
			return(triangleCount);
		}

		private void SetColour (int vertexCount, float yPos)
		{
			colours [vertexCount] = ColourMap [Mathf.FloorToInt ((yPos / (ConeSize * SquareSize)) * (ColourMap.Length - 1))];
		}

		private int GetVertexIndex (GridCell cell, int localIndex)
		{
			switch (localIndex)
			{
			case 0:
			case 1:
				if (cell.neighbours [2] != null)
					cell.edgePointIndex [localIndex] = cell.neighbours [2].edgePointIndex [localIndex + 4]; 
				break;
			case 2:
				if (cell.neighbours [1] != null)
					cell.edgePointIndex [localIndex] = cell.neighbours [1].edgePointIndex [0];
				else if (cell.neighbours [2] != null)
					cell.edgePointIndex [localIndex] = cell.neighbours [2].edgePointIndex [6]; 				
				break;
			case 3: 

				if (cell.neighbours [0] != null)
					cell.edgePointIndex [localIndex] = cell.neighbours [0].edgePointIndex [1];
				else if (cell.neighbours [2] != null)
					cell.edgePointIndex [localIndex] = cell.neighbours [2].edgePointIndex [7];

				break;
			case 6: 
				if (cell.neighbours [1] != null)
					cell.edgePointIndex [localIndex] = cell.neighbours [1].edgePointIndex [4]; 
				break;	 
			case 7: 
				if (cell.neighbours [0] != null)
					cell.edgePointIndex [localIndex] = cell.neighbours [0].edgePointIndex [5]; 
				break;
			case 8: 
				if (cell.neighbours [0] != null)
					cell.edgePointIndex [localIndex] = cell.neighbours [0].edgePointIndex [9]; 
				break;
			case 10: 
				if (cell.neighbours [1] != null)
					cell.edgePointIndex [localIndex] = cell.neighbours [1].edgePointIndex [9]; 
				break;
			case 11: 
				if (cell.neighbours [0] != null)
					cell.edgePointIndex [localIndex] = cell.neighbours [0].edgePointIndex [10];
				else if (cell.neighbours [1] != null)
					cell.edgePointIndex [localIndex] = cell.neighbours [1].edgePointIndex [8]; 

				break;
			default :

				break;
			}

			return cell.edgePointIndex [localIndex];
		}

		#region Debug

		#if UNITY_EDITOR
		public bool DrawDebugLines;

		void OnDrawGizmos ()
		{
			if (DrawDebugLines && initialised)
			{
				int y = 0;
				for (float z = 0; z < (GridZ + 1) * SquareSize; z += SquareSize)
				{

					for (float x = 0; x < (GridX + 1) * SquareSize; x += SquareSize)
					{

						Debug.DrawLine (new Vector3 (x, y, z), new Vector3 (x, y, z + SquareSize), Color.yellow, 0.1f);
						Debug.DrawLine (new Vector3 (x + SquareSize, y, z), new Vector3 (x + SquareSize, y, z + SquareSize), Color.yellow, 0.1f);

						Debug.DrawLine (new Vector3 (x, y, z), new Vector3 (x + SquareSize, y, z), Color.magenta, 0.1f);
						Debug.DrawLine (new Vector3 (x, y, z + SquareSize), new Vector3 (x + SquareSize, y, z + SquareSize), Color.magenta, 0.1f);

					}
				}
			}
		}
		#endif
		#endregion
	}
}