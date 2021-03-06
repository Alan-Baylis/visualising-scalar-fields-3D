﻿using UnityEngine;
using System.Collections;

namespace Haa.MarchingCubes.Model
{
	public class GridCell
	{
		public Vector3[] VertexPosition = new Vector3[8];
		public float[] VertexValue = new float[8];
		//0 = left; 1 = back; 2 = bottom neighbour cells
		public GridCell[] neighbours = new GridCell[3];
		public Vector3[] edgePoints = new Vector3[12];
		//only those generated by this cell. Others are taken from neighbouring cells
		public int[] edgePointIndex = new int[12] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

		public GridCell (float bottomLeftCornerX, float bottomLeftCornerY, float bottomLeftCornerZ, float cellSize)
		{
			VertexPosition [0] = new Vector3 (bottomLeftCornerX, bottomLeftCornerY, bottomLeftCornerZ + cellSize);
			VertexPosition [1] = new Vector3 (bottomLeftCornerX + cellSize, bottomLeftCornerY, bottomLeftCornerZ + cellSize);
			VertexPosition [2] = new Vector3 (bottomLeftCornerX + cellSize, bottomLeftCornerY, bottomLeftCornerZ);
			VertexPosition [3] = new Vector3 (bottomLeftCornerX, bottomLeftCornerY, bottomLeftCornerZ);

			VertexPosition [4] = new Vector3 (bottomLeftCornerX, bottomLeftCornerY + cellSize, bottomLeftCornerZ + cellSize);
			VertexPosition [5] = new Vector3 (bottomLeftCornerX + cellSize, bottomLeftCornerY + cellSize, bottomLeftCornerZ + cellSize);
			VertexPosition [6] = new Vector3 (bottomLeftCornerX + cellSize, bottomLeftCornerY + cellSize, bottomLeftCornerZ);
			VertexPosition [7] = new Vector3 (bottomLeftCornerX, bottomLeftCornerY + cellSize, bottomLeftCornerZ);
		}

		public void SetEdgePoint(int edgeIndex, int firstCornerIndex, int secondCornerIndex, int vertexCount, float isolevel)
		{
			edgePointIndex [edgeIndex] = vertexCount;
			edgePoints [edgeIndex] =
				VertexInterp (isolevel, VertexPosition [firstCornerIndex], VertexPosition [secondCornerIndex], VertexValue [firstCornerIndex], VertexValue [secondCornerIndex]);
		}

		static public Vector3 VertexInterp (float isolevel, Vector3 p1, Vector3 p2, float valp1, float valp2)
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

		public int GetVertexIndex (int localIndex)
		{
			switch (localIndex)
			{
			case 0:
			case 1:
				if (neighbours [2] != null)
					edgePointIndex [localIndex] = neighbours [2].edgePointIndex [localIndex + 4]; 
				break;
			case 2:
				if (neighbours [1] != null)
					edgePointIndex [localIndex] = neighbours [1].edgePointIndex [0];
				else if (neighbours [2] != null)
					edgePointIndex [localIndex] = neighbours [2].edgePointIndex [6]; 				
				break;
			case 3: 

				if (neighbours [0] != null)
					edgePointIndex [localIndex] = neighbours [0].edgePointIndex [1];
				else if (neighbours [2] != null)
					edgePointIndex [localIndex] = neighbours [2].edgePointIndex [7];

				break;
			case 6: 
				if (neighbours [1] != null)
					edgePointIndex [localIndex] = neighbours [1].edgePointIndex [4]; 
				break;	 
			case 7: 
				if (neighbours [0] != null)
					edgePointIndex [localIndex] = neighbours [0].edgePointIndex [5]; 
				break;
			case 8: 
				if (neighbours [0] != null)
					edgePointIndex [localIndex] = neighbours [0].edgePointIndex [9]; 
				break;
			case 10: 
				if (neighbours [1] != null)
					edgePointIndex [localIndex] = neighbours [1].edgePointIndex [9]; 
				break;
			case 11: 
				if (neighbours [0] != null)
					edgePointIndex [localIndex] = neighbours [0].edgePointIndex [10];
				else if (neighbours [1] != null)
					edgePointIndex [localIndex] = neighbours [1].edgePointIndex [8]; 

				break;
			default :

				break;
			}

			return edgePointIndex [localIndex];
		}
	}
}