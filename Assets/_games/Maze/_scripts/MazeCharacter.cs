﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace EA4S.Maze
{
	
	public class MazeCharacter: MonoBehaviour {

		//for internal use:
		private delegate void VoidDelegate();


		public List<Vector3> characterWayPoints;

		public GameObject collider;

		public List<GameObject> Fruits;


		public bool characterIsMoving;

		public MazeDot dot = null;

		public Transform nextPosition;


		int currentCharacterWayPoint;


		Vector3 initialPosition;
		Quaternion initialRotation;
		Vector3 targetPos;
		Quaternion targetRotation;
		int currentWayPoint;


		List<GameObject> _fruits;
		int currentFruitList = 0;


		int currentFruitIndex;

		private bool startCheckingForCollision = false;
		private bool donotHandleBorderCollision = false;


		void Start()
		{
			
			characterIsMoving = false;
			characterWayPoints = new List<Vector3>();
			currentCharacterWayPoint = 0;



			currentWayPoint = 0;
			GetComponent<BoxCollider> ().enabled = false;

			collider.GetComponent<MeshRenderer> ().enabled = false;
			collider.SetActive(false);


			//foreach (GameObject fruitList in Fruits)
			//	fruitList.SetActive (false);


			

		}

		public void initialize()
		{
			initialPosition = transform.position;
			targetPos = initialPosition;

			initialRotation = transform.rotation;
			targetRotation = initialRotation;


			characterWayPoints.Add(initialPosition);
			setFruitsList ();

			var dir = transform.position - _fruits[0].transform.position;
			var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
			transform.rotation =  Quaternion.AngleAxis(angle, Vector3.forward);


		}


		void setFruitsList()
		{
			//fruits to collect
			_fruits = new List<GameObject> ();
			int i = 0;
			if (Fruits.Count == 0)
				return;
			
			//Fruits [currentFruitList].SetActive (true);
			foreach (Transform child in Fruits[currentFruitList].transform) {
				//child.gameObject.SetActive (i==0||i==1? true:false);
				child.gameObject.name = "fruit_" + (i++);
				child.gameObject.GetComponent<BoxCollider> ().enabled = false;
				_fruits.Add (child.gameObject);

			}
			currentFruitIndex = 0;
		}
		

		void OnTriggerEnter(Collider other)
		{
			if (donotHandleBorderCollision || !characterIsMoving)
				return;
			
			print ("Colliding with: " + other.gameObject.name);

			if (other.gameObject.name == "BorderCollider") {

				//if this is the 1st hit ignore it:
				if (!startCheckingForCollision) {
					startCheckingForCollision = true;
					return;
				}

				wasHit ();
			}

			if (other.gameObject.name.IndexOf ("fruit_") == 0) {
				//we hit a fruit make sure it is in order:
				int index = int.Parse( other.gameObject.name.Substring(6));

				if (index == currentFruitIndex) {
					_fruits [currentFruitIndex].GetComponent<BoxCollider> ().enabled = false;


					//_fruits [currentFruitIndex].SetActive (false);
					currentFruitIndex++;



				} else if(index > currentFruitIndex){
					//lose?
					waitAndRestartScene();
				}
			}
		}


		void OnTriggerExit(Collider other)
		{
			print ("trigger exit " + other.gameObject.name);

			if (other.gameObject.name == "MazeLetter") {
				//if the character completely exits the maze letter:
				//stop for a second and restart the level:
				waitAndRestartScene();
			}
		
		}

		void wasHit()
		{
			

			MazeGameManager.Instance.wasHit ();




			if (MazeGameManager.Instance.health == 0) {

				waitAndRestartScene ();

				return;
			}

		
			//stop checking for border collision for half a second
			StartCoroutine(waitAndPerformCallback(0.5f,()=>{
				donotHandleBorderCollision = true;
			},
				()=>{
					donotHandleBorderCollision = false;
				}));

			//stop moving the character for a second
			StartCoroutine(waitAndPerformCallback(1,()=>{
				characterIsMoving = false;
			},
				()=>{
					characterIsMoving = true;
				}));
			

		}



		void waitAndRestartScene()
		{
			//stop for a second and restart the level:
			StartCoroutine(waitAndPerformCallback(1,()=>{
				donotHandleBorderCollision = true;
				characterIsMoving = false;
			},
				()=>{
					MazeGameManager.Instance.lostCurrentLetter();//SceneManager.LoadScene(SceneManager.GetActiveScene().name);
				}));


		}

		//corutine to handle pausing a bit then resuming
		IEnumerator waitAndPerformCallback(float seconds,VoidDelegate init, VoidDelegate callback)
		{
			init ();

			yield return new WaitForSeconds (seconds);

			callback ();
		}

		public bool isComplete()
		{
			if (currentFruitList == Fruits.Count - 1) {
				if (dot == null)
					return true;
				else
					return dot.isClicked;
			} else
				return false;
			
		}

		public void setClickedDot()
		{
			MazeGameManager.Instance.moveToNext (true);
		}

		public void nextPath()
		{
			if (currentFruitList == Fruits.Count - 1)
				return;


			currentFruitList++;




			setFruitsList ();
			transform.position = nextPosition.position;


			initialPosition = transform.position;
			targetPos = initialPosition;

			initialRotation = transform.rotation;
			targetRotation = initialRotation;

			currentCharacterWayPoint = 0;
			characterWayPoints = new List<Vector3> ();
			characterWayPoints.Add (initialPosition);


			var dir = transform.position - _fruits[0].transform.position;
			var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
			transform.rotation =  Quaternion.AngleAxis(angle, Vector3.forward);

	
		}

		public bool canMouseBeDown()
		{
			if (_fruits.Count == 0)
				return false;
			
			float distance = transform.position.z - Camera.main.transform.position.z;
			Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
			pos = Camera.main.ScreenToWorldPoint(pos);

			//check distance to first fruit:
			pos.z = _fruits[0].transform.position.z;

			return ((pos - _fruits [0].transform.position).sqrMagnitude) <= 1;


		}

		public void initMovement()
		{
			
			characterIsMoving = true;
			GetComponent<BoxCollider> ().enabled = true;

			foreach (GameObject fruit in _fruits) {
				fruit.GetComponent<BoxCollider> ().enabled = true;
			}
		}

		public void calculateMovementAndRotation()
		{
			//if(victory) return;

			Vector3 previousPosition = targetPos;
			float distance = transform.position.z - Camera.main.transform.position.z;
			targetPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance);
			targetPos = Camera.main.ScreenToWorldPoint(targetPos);

			if (previousPosition != initialPosition && previousPosition != targetPos) {
				//MazeGameManager.Instance.DrawLine (previousPosition, targetPos, Color.red);
				MazeGameManager.Instance.appendToLine(targetPos);
			}





			var dir = transform.position - characterWayPoints[currentCharacterWayPoint];
			var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
			targetRotation =  Quaternion.AngleAxis(angle, Vector3.forward) * initialRotation;


			if(previousPosition != targetPos)
			{
				targetPos.z = -0.1f;
				characterWayPoints.Add(targetPos);

			}

			//print ((_fruits [_fruits.Count - 1].transform.position - targetPos).sqrMagnitude);
			if ((_fruits [_fruits.Count - 1].transform.position - targetPos).sqrMagnitude < 0.5f) {
				initMovement ();
				MazeGameManager.Instance.timer.StopTimer ();
			}

		}



		void Update()
		{
			

			if (characterIsMoving) {
				transform.position = Vector3.MoveTowards (transform.position, characterWayPoints[currentCharacterWayPoint], Time.deltaTime*5);
				var dir = transform.position - characterWayPoints[currentCharacterWayPoint+1];
				var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
				targetRotation =  Quaternion.AngleAxis(angle, Vector3.forward) * initialRotation;

				transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 5);
				//transform.LookAt(characterWayPoints[currentCharacterWayPoint+1]);

				if((transform.position - characterWayPoints[currentCharacterWayPoint]).magnitude == 0 && currentCharacterWayPoint < characterWayPoints.Count-1){

					currentCharacterWayPoint++;

					//reached the end:
					if (currentCharacterWayPoint == characterWayPoints.Count-1) {
						//arrived!
						transform.rotation = initialRotation;
						if (currentFruitIndex == _fruits.Count) {
							print ("Won");
							GetComponent<BoxCollider> ().enabled = false;
							characterIsMoving = false;
							MazeGameManager.Instance.moveToNext (true);

							if (currentFruitList == Fruits.Count - 1) {
								if (dot != null)
									dot.GetComponent<BoxCollider> ().enabled = true;
							}
						} else
							waitAndRestartScene ();
					}

					//enable collider when we reach the second waypoint
					if (currentCharacterWayPoint == 1)
						collider.SetActive (true);
				}
			}
		}
	}
}