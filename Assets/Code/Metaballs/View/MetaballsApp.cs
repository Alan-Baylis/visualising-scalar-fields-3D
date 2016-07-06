#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HopeAndAnchor.MarchingCubes.Shared.Model;
using HopeAndAnchor.MarchingCubes.Shared.View;
using HopeAndAnchor.MarchingCubes.Metaballs.Model;

namespace HopeAndAnchor.MarchingCubes.Metaballs.View
{
	public class MetaballsApp : MonoBehaviour
	{
		[SerializeField] private Isosurface Mesh;

		[SerializeField] private int NumberOfBalls;
		[SerializeField] private int minVelocity = -2;
		[SerializeField] private int maxVelocity = 2;
		[SerializeField] private float minRadius = 1.5f;
		[SerializeField] private float maxRadius = 0.5f;

		[SerializeField] private float Isolevel = 1f;
		[SerializeField] private float SquareSize = 0.1f;
		[SerializeField] private int GridX = 40;
		[SerializeField] private int GridY = 25;
		[SerializeField] private int GridZ = 25;

		private Vector3[] vertices;
		private int[] triangles;
		private int vertexCount = 0;
		private int triangleCount = 0;
		private GridCell[] grid;
		private Ball[] balls;

		private bool initialised;

		void Start ()
		{
			GenerateGrid();
			GenerateMesh ();
			GenerateBalls();
			initialised = true;
		}

		private void GenerateGrid()
		{
			grid = new GridCell[GridX * GridY * GridZ];
			for (int y = 0; y < GridY; y++)
			{
				for (int z = 0; z < GridZ; z++)
				{
					for (int x = 0; x < GridX; x++)
					{
						int squareIndex = (y * GridX * GridZ) + (GridX * z) + x;
						GridCell cell = new GridCell ((x * SquareSize), (y * SquareSize), (z * SquareSize), SquareSize);
						if(x > 0)
							cell.neighbours[0] = grid[squareIndex - 1];
						if(z > 0)
							cell.neighbours[1] = grid[squareIndex - GridX];
						if(y > 0)
							cell.neighbours[2] = grid[squareIndex - (GridX*GridZ)];
						grid [squareIndex] = cell;
					}
				}
			}
		}

		private void GenerateBalls ()
		{
			balls = new Ball[NumberOfBalls];
			for (int i = 0; i < NumberOfBalls; i++)
			{
				Ball ball = new Ball ();
				ball.radius = Random.Range (minRadius, maxRadius);
				Isolevel = ball.radius;
				ball.position = new Vector3 (Random.Range (0, GridX*SquareSize), Random.Range (0, GridY*SquareSize), GridZ/2);
				ball.velocity = new Vector3 (Random.Range (minVelocity, maxVelocity), Random.Range (minVelocity, maxVelocity), Random.Range (minVelocity, maxVelocity));
				balls [i] = ball;
			}
		}

		void SetCornerValues (GridCell cell)
		{
			
			cell.VertexValue[0] = 0;
			cell.VertexValue[1] = 0;
			cell.VertexValue[2] = 0;
			cell.VertexValue[3] = 0;
			cell.VertexValue[4] = 0;
			cell.VertexValue[5] = 0;
			cell.VertexValue[6] = 0;
			cell.VertexValue[7] = 0;

			if(cell.neighbours[0] != null)
			{
				cell.VertexValue[0] = cell.neighbours[0].VertexValue[1];
				cell.VertexValue[3] = cell.neighbours[0].VertexValue[2];
				cell.VertexValue[4] = cell.neighbours[0].VertexValue[5];
				cell.VertexValue[7] = cell.neighbours[0].VertexValue[6];
			}
			if(cell.neighbours[1] != null)
			{
				if(cell.neighbours[0] == null) cell.VertexValue[3] = cell.neighbours[1].VertexValue[0];
				if(cell.neighbours[0] == null) cell.VertexValue[7] = cell.neighbours[1].VertexValue[4];
				cell.VertexValue[6] = cell.neighbours[1].VertexValue[5];
				cell.VertexValue[2] = cell.neighbours[1].VertexValue[1];
			}
			if(cell.neighbours[2] != null)
			{
				if(cell.neighbours[0] == null) cell.VertexValue[0] = cell.neighbours[2].VertexValue[4];
				if(cell.neighbours[1] == null) cell.VertexValue[2] = cell.neighbours[2].VertexValue[6];
				if(cell.neighbours[0] == null && cell.neighbours[1] == null) cell.VertexValue[3] = cell.neighbours[2].VertexValue[7];
				cell.VertexValue[1] = cell.neighbours[2].VertexValue[5];
			}

			foreach (Ball ball in balls)
			{
				cell.VertexValue[5] += GetValueForPoint (ball, cell.VertexPosition [5]);

				if(cell.neighbours[0] == null)
				{
					cell.VertexValue[4] += GetValueForPoint (ball, cell.VertexPosition [4]);
					if(cell.neighbours[1] == null)
					{
						cell.VertexValue[7] += GetValueForPoint (ball, cell.VertexPosition [7]);

						if(cell.neighbours[2] == null)
						{
							cell.VertexValue[3] += GetValueForPoint (ball, cell.VertexPosition [3]);
						}
					}
					if(cell.neighbours[2] == null)
					{
						cell.VertexValue[0] += GetValueForPoint (ball, cell.VertexPosition [0]);
					}
				}

				if(cell.neighbours[1] == null)
				{
					cell.VertexValue[6] += GetValueForPoint (ball, cell.VertexPosition [6]);
					if(cell.neighbours[2] == null)
					{
						cell.VertexValue[2] += GetValueForPoint (ball, cell.VertexPosition [2]);
					}
				}

				if(cell.neighbours[2] == null)
				{
					cell.VertexValue[1] += GetValueForPoint (ball, cell.VertexPosition [1]);
				}

			}
		}

		float GetValueForPoint (Ball ball, Vector3 point)
		{
			float diffX = (point.x - ball.position.x)*(point.x - ball.position.x);
			float diffY = (point.y - ball.position.y)*(point.y - ball.position.y);
			float diffZ = (point.z - ball.position.z)*(point.z - ball.position.z);
			return (ball.radius*ball.radius)/(diffX + diffY + diffZ);
		}

		void Update()
		{
			if(initialised)
			{
				vertexCount = 0;
				triangleCount = 0;
				MoveBalls();
				for (int y = 0; y < GridY; y++)
				{
					for (int z = 0; z < GridZ; z++)
					{
						for (int x = 0; x < GridX; x++)
						{
							int squareIndex = (y * GridX * GridZ) + (GridX * z) + x;
							GridCell cell = grid[squareIndex];
							SetCornerValues (cell);
							ConfigureGridCell (cell, Isolevel, ref triangles);

						}
					}
				}
				System.Array.Clear(vertices, vertexCount, vertices.Length - vertexCount);
				System.Array.Clear(triangles, triangleCount, triangles.Length - triangleCount);
				Mesh.DrawMesh (vertices, triangles);
			}
		}

		private void MoveBalls ()
		{
			for (int i = 0; i < NumberOfBalls; i++)
			{
				Ball ball = balls [i];
				ball.position += ball.velocity * Time.deltaTime;

				if ((ball.position.x > (GridX*SquareSize) && ball.velocity.x > 0) || (ball.position.x < 0 && ball.velocity.x < 0))
					ball.velocity.x = -ball.velocity.x;
				if ((ball.position.y > (GridY*SquareSize) && ball.velocity.y > 0) || (ball.position.y < 0 && ball.velocity.y < 0))
					ball.velocity.y = -ball.velocity.y;
				if ((ball.position.z > (GridZ*SquareSize) && ball.velocity.z > 0) || (ball.position.z < 0 && ball.velocity.z < 0))
					ball.velocity.z = -ball.velocity.z;
			}
		}

		void GenerateMesh ()
		{
			triangles = new int[GridX * GridY * GridZ * 6];
			vertices = new Vector3[3000];
			Mesh.Initialise ();
		}

		private int ConfigureGridCell (GridCell gridCell, float isolevel, ref int[] triangles)
		{
			int i;
			int caseIndex = 0;

			if (gridCell.VertexValue [0] > isolevel)
				caseIndex |= 1;
			if (gridCell.VertexValue [1] > isolevel)
				caseIndex |= 2;
			if (gridCell.VertexValue [2] > isolevel)
				caseIndex |= 4;
			if (gridCell.VertexValue [3] > isolevel)
				caseIndex |= 8;
			if (gridCell.VertexValue [4] > isolevel)
				caseIndex |= 16;
			if (gridCell.VertexValue [5] > isolevel)
				caseIndex |= 32;
			if (gridCell.VertexValue [6] > isolevel)
				caseIndex |= 64;
			if (gridCell.VertexValue [7] > isolevel)
				caseIndex |= 128;
			
			if (MarchingCubesLookupTables.edgeTable [caseIndex] == 0)
				return(0);
			
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 1) == 1 && gridCell.neighbours [2] == null)
			{
				gridCell.SetEdgePoint(0, 0, 1, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [0];
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 2) == 2 && gridCell.neighbours [2] == null)
			{
				gridCell.SetEdgePoint(1, 2, 1, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [1];
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 4) == 4 && gridCell.neighbours [1] == null && gridCell.neighbours [2] == null)
			{
				gridCell.SetEdgePoint(2, 2, 3, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [2];
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 8) == 8 && gridCell.neighbours [0] == null && gridCell.neighbours [2] == null)
			{
				gridCell.SetEdgePoint(3, 3, 0, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [3];
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 16) == 16)
			{
				gridCell.SetEdgePoint(4, 4, 5, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [4];
				vertexCount++;

			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 32) == 32)
			{
				gridCell.SetEdgePoint(5, 5, 6, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [5];
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 64) == 64 && gridCell.neighbours [1] == null)
			{
				gridCell.SetEdgePoint(6, 6, 7, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [6];
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 128) == 128 && gridCell.neighbours [0] == null)
			{
				gridCell.SetEdgePoint(7, 7, 4, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [7];
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 256) == 256 && gridCell.neighbours [0] == null)
			{
				gridCell.SetEdgePoint(8, 0, 4, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [8];
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 512) == 512)
			{
				gridCell.SetEdgePoint(9, 1, 5, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [9];
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 1024) == 1024 && gridCell.neighbours [1] == null)
			{
				gridCell.SetEdgePoint(10, 2, 6, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [10];
				vertexCount++;
			}
			if ((MarchingCubesLookupTables.edgeTable [caseIndex] & 2048) == 2048 && gridCell.neighbours [0] == null && gridCell.neighbours [1] == null)
			{
				gridCell.SetEdgePoint(11, 3, 7, vertexCount, isolevel);
				vertices [vertexCount] = gridCell.edgePoints [11];
				vertexCount++;
			}

			for (i = 0; MarchingCubesLookupTables.triTable [caseIndex, i] != -1; i += 3)
			{
				
				triangles [triangleCount + 2] = GetVertexIndex(gridCell, MarchingCubesLookupTables.triTable [caseIndex, i]);
				triangles [triangleCount + 1] = GetVertexIndex(gridCell, MarchingCubesLookupTables.triTable [caseIndex, i + 1]);
				triangles [triangleCount] = GetVertexIndex(gridCell, MarchingCubesLookupTables.triTable [caseIndex, i + 2]);
				triangleCount+=3;
			}
			return(triangleCount);
		}

		private int GetVertexIndex(GridCell cell, int localIndex)
		{
			switch(localIndex)
			{
			case 0 : case 1 :
				if(cell.neighbours[2] != null)
					cell.edgePointIndex[localIndex] = cell.neighbours[2].edgePointIndex[localIndex + 4]; 
				break;
			case 2 :
				if(cell.neighbours[1] != null)
					cell.edgePointIndex[localIndex] = cell.neighbours[1].edgePointIndex[0]; 
				else if(cell.neighbours[2] != null)
					cell.edgePointIndex[localIndex] = cell.neighbours[2].edgePointIndex[6]; 				
				break;
			case 3 : 
				
				if(cell.neighbours[0] != null)
					cell.edgePointIndex[localIndex] = cell.neighbours[0].edgePointIndex[1];
				else if(cell.neighbours[2] != null)
					cell.edgePointIndex[localIndex] = cell.neighbours[2].edgePointIndex[7];
				
				break;
			case 6 : 
				if(cell.neighbours[1] != null)
					cell.edgePointIndex[localIndex] = cell.neighbours[1].edgePointIndex[4]; 
				break;	 
			case 7 : 
				if(cell.neighbours[0] != null)
					cell.edgePointIndex[localIndex] = cell.neighbours[0].edgePointIndex[5]; 
				break;
			case 8 : 
				if(cell.neighbours[0] != null)
					cell.edgePointIndex[localIndex] = cell.neighbours[0].edgePointIndex[9]; 
				break;
			case 10 : 
				if(cell.neighbours[1] != null)
					cell.edgePointIndex[localIndex] = cell.neighbours[1].edgePointIndex[9]; 
				break;
			case 11 : 
				if(cell.neighbours[0] != null)
					cell.edgePointIndex[localIndex] = cell.neighbours[0].edgePointIndex[10]; 
				else if(cell.neighbours[1] != null)
					cell.edgePointIndex[localIndex] = cell.neighbours[1].edgePointIndex[8]; 
				
				break;
			default :
				
				break;
			}

			return cell.edgePointIndex[localIndex];
		}
	}
}