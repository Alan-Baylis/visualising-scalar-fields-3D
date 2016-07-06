#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HopeAndAnchor.MarchingCubes.Shared.Model;
using HopeAndAnchor.MarchingCubes.Shared.View;

using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;

namespace HopeAndAnchor.MarchingCubes.Terrain.View
{
	public class TerrainApp : MonoBehaviour
	{
		[SerializeField] private Isosurface Mesh;

		[SerializeField] private float Isolevel = 1f;
		[SerializeField] private float SquareSize = 2f;
		[SerializeField] private int GridX = 80;
		[SerializeField] private int GridY = 80;
		[SerializeField] private int GridZ = 1;
		[SerializeField] private Gradient DesertColouring;
		[SerializeField] private Gradient GrassLandColouring;
		[SerializeField] private int Seed = 100;

		[SerializeField] private Player Player;
		[SerializeField] private Vector3 gridOffset;

		[SerializeField] private ColourSelection SelectedColour;
		[SerializeField] private double Displacement = 4;
		[SerializeField] private double Frequency = 2;
		[SerializeField] private NoiseType Noise = NoiseType.Mix;
		[SerializeField] private float Turbulence = 0f;
		[SerializeField] private int PerlinOctaves = 6;

		private Vector3[] vertices;
		private int[] triangles;
		private Color[] colors;
		private int vertexCount = 0;
		private int triangleCount = 0;

		private float gridPositionX;
		private float gridPositionZ;
		private GridCell[] grid;
		private Gradient Colour;
		private ModuleBase moduleBase;
		private Noise2D noiseMap = null;

		private bool initialised;

		void Start ()
		{
			SelectTerrainColour();
			GenerateGrid ();
			GenerateMesh ();
			GenerateScalarField ();
			Player.Initialise();
			AdjustTerrainOffsetsForPlayerPosition();
			GenerateTerrain (); 
			initialised = true;
		}

		private void SelectTerrainColour()
		{
			switch (SelectedColour)
			{
			case ColourSelection.Desert:
				Colour = DesertColouring;
				break;
			default :
				Colour = GrassLandColouring;
				break;
			}
		}

		private void GenerateGrid ()
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
						if (x > 0)
							cell.neighbours [0] = grid [squareIndex - 1];
						if (z > 0)
							cell.neighbours [1] = grid [squareIndex - GridX];
						if (y > 0)
							cell.neighbours [2] = grid [squareIndex - (GridX * GridZ)];
						grid [squareIndex] = cell;
					}
				}
			}
		}

		public enum NoiseType
		{
			Perlin,
			Billow,
			RidgedMultifractal,
			Voronoi,
			Mix,
			Practice}

		;

		public enum ColourSelection
		{
			GrassLands,
			Desert
		};

		void Update ()
		{
			if(Player.HasMoved)
			{
				AdjustTerrainOffsetsForPlayerPosition();
				GenerateTerrain (); 
			}
		}

		private void AdjustTerrainOffsetsForPlayerPosition()
		{
			gridPositionX = Mathf.FloorToInt(Player.PlayerPosition.x/SquareSize) * SquareSize;
			gridPositionZ = Mathf.FloorToInt(Player.PlayerPosition.z/SquareSize) * SquareSize;

			gridOffset.x = Player.PlayerPosition.x - (gridPositionX * SquareSize);
			gridOffset.z = Player.PlayerPosition.z - (gridPositionZ * SquareSize);
		}

		private void GenerateScalarField ()
		{

			switch (Noise)
			{
			case NoiseType.Billow:	
				moduleBase = new Billow ();
				break;

			case NoiseType.RidgedMultifractal:	
				moduleBase = new RidgedMultifractal ();
				break;   

			case NoiseType.Voronoi:	
				moduleBase = new Voronoi (Frequency, Displacement, Seed, false);
				break;             	         	

			case NoiseType.Mix:            	
				Perlin perlin = new Perlin (Frequency, 2.0, 0.5, PerlinOctaves, Seed, QualityMode.Low);
				var rigged = new RidgedMultifractal ();
				moduleBase = new Add (perlin, rigged);
				break;

			case NoiseType.Practice:
				var bill = new Billow ();
				bill.Frequency = Frequency;
				moduleBase = new Turbulence (Turbulence / 10, bill);
				break;
			default:
				var defPerlin = new Perlin ();
				defPerlin.OctaveCount = PerlinOctaves;
				moduleBase = defPerlin;
				break;
			}
		
			this.noiseMap = new Noise2D (GridX*2, GridZ*2, moduleBase);

		}

		void SetCornerValues (GridCell cell, int x, int z)
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

			SetVertexValue (x, z, cell, 5);

			if (cell.neighbours [0] == null)
			{
				SetVertexValue (x, z, cell, 4);
				if (cell.neighbours [1] == null)
				{
					SetVertexValue (x, z, cell, 7);

					if (cell.neighbours [2] == null)
					{
						SetVertexValue (x, z, cell, 3);
					}
				}
				if (cell.neighbours [2] == null)
				{
					SetVertexValue (x, z, cell, 0);
				}
			}

			if (cell.neighbours [1] == null)
			{
				SetVertexValue (x, z, cell, 6);
				if (cell.neighbours [2] == null)
				{
					SetVertexValue (x, z, cell, 2);
				}
			}

			if (cell.neighbours [2] == null)
			{
				SetVertexValue (x, z, cell, 1);
			}	
		}

		private void SetVertexValue (int x, int z, GridCell cell, int vertexIndex)
		{
			if(vertexIndex == 0 || vertexIndex == 1 || vertexIndex == 2 || vertexIndex == 3)
				z += 1;
			if(vertexIndex == 1 || vertexIndex == 2 || vertexIndex == 5 || vertexIndex == 6)
				x += 1;
			float sample = GetValueForPoint (x, z);
			sample = Mathf.Max (sample, 0);
			cell.VertexValue [vertexIndex] = cell.VertexPosition [vertexIndex].y / (((sample + 1) / 2) * GridY);
		}
			
		float GetValueForPoint (int x, int z)
		{
			return this.noiseMap [x, z];
		}

		void GenerateTerrain ()
		{
			vertexCount = 0;
			triangleCount = 0;
			noiseMap.GeneratePlanar (
				Player.PlayerPosition.x,
				Player.PlayerPosition.x + SquareSize,
				Player.PlayerPosition.z,
				Player.PlayerPosition.z + SquareSize
			);
			for (int y = 0; y < GridY; y++)
			{
				for (int z = 0; z < GridZ; z++)
				{
					for (int x = 0; x < GridX; x++)
					{
						int squareIndex = (y * GridX * GridZ) + (GridX * z) + x;
						GridCell cell = grid [squareIndex];
						SetCornerValues (cell, x, z);
						ConfigureGridCell (cell, Isolevel, ref triangles);
					}
				}
			}
			System.Array.Clear (vertices, vertexCount, vertices.Length - vertexCount);
			System.Array.Clear (triangles, triangleCount, triangles.Length - triangleCount);
			Mesh.DrawMesh (vertices, triangles, colors);

		}

		void GenerateMesh ()
		{
			triangles = new int[GridX * GridY * GridZ * 6];
			vertices = new Vector3[GridX * GridY * GridZ * 4];
			colors = new Color[vertices.Length];
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
			colors [vertexCount] = Colour.Evaluate (yPos / GridY);
		}

		#region Debug

		#if UNITY_EDITOR
		public bool DrawDebugLines;

		void OnDrawGizmos ()
		{
			if (DrawDebugLines && initialised)
			{
				int y = 5;
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
