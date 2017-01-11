using UnityEngine;

namespace Haa.MarchingCubes
{
	public class Player : MonoBehaviour 
	{
		[SerializeField] private float Speed = 8;
		public Vector3 PlayerPosition;
		public bool HasMoved;

		public void Initialise()
		{
			PlayerPosition = transform.position;
		}

		void Update () {
		
			if(Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
			{
				HasMoved = false;
			}
			else
			{
				HasMoved = true;
				PlayerPosition.x += Speed * Time.deltaTime * Input.GetAxis("Horizontal");
				PlayerPosition.z += Speed * Time.deltaTime * Input.GetAxis("Vertical");
			}
		}
	}
}
