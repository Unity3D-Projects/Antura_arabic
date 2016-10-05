﻿using UnityEngine;
using System.Collections;
//using ArabicSupport;
using TMPro;

namespace EA4S.DancingDots
{
	public class DancingDotsDraggableDot : MonoBehaviour {

		private Vector3 screenPoint;
		private Vector3 offset;

		public bool isDot;
		[Range (0, 3)] public int dots;

		public Diacritic diacritic;

		public Vector3 fingerOffset;
		public GameObject splatPrefab;
		public TextMeshPro draggableText;

		public bool isNeeded = false;

		bool isDragging = false;
		bool overDestinationMarker = false;
		bool overPlayermarker = false;

		void OnMouseDown()
		{
			isDragging = true;
			screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

			offset = gameObject.transform.position - 
				Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

		}

		void OnMouseDrag()
		{
			Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

			Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
			transform.position = new Vector3 (curPosition.x + fingerOffset.x, curPosition.y + fingerOffset.y, -10);

		}

		void OnMouseUp()
		{
			if (overDestinationMarker)
			{
				if (isDot)
				{
					DancingDotsGameManager.instance.CorrectDot(); 
				}
				else
				{
					DancingDotsGameManager.instance.CorrectDiacritic();
				}
				gameObject.SetActive(false);
			}
			else
			{
				if (overPlayermarker && !isNeeded)
				{
					Instantiate(splatPrefab,new Vector3(transform.position.x,transform.position.y,-20), Quaternion.identity);
					isDragging = false;
					gameObject.SetActive(false);
				}
				else
				{
					isDragging = false;

//					StartCoroutine(GoToStartPosition3());
				}
			}

			overPlayermarker = false;
			overDestinationMarker = false;
		}

		float startX;
		float startY;
		float startZ;

		void Start()
		{

			startX = transform.position.x;
			startY = transform.position.y;
			startZ = transform.position.z;
			Reset();
		}

		public void Reset()
		{
			transform.position = new Vector3(startX, startY, startZ);
			isDragging = false;
			gameObject.SetActive(true);
		}
			
		void Update() {

			if (!isDragging) Dance();

		}

		void Dance()
		{
			transform.position = new Vector3(
				startX + Mathf.PerlinNoise(Time.time, startX) * 3 + 1, 
				startY + Mathf.PerlinNoise(Time.time, startY) * 3 + 1, 
				startZ + Mathf.PerlinNoise(Time.time, startZ) * 3 + 1);
		}

		void Setmarker(Collider other, bool markerStatus)
		{
			if (other.tag == "Player") overPlayermarker = markerStatus;


			if (isDot)
			{
				if (other.tag == "DotZone")
				{
					if (other.GetComponent<DancingDotsDropZone>().letters.Contains(DancingDotsGameManager.instance.currentLetter) 
						&& DancingDotsGameManager.instance.dotsCount == dots)
					{
						overDestinationMarker = markerStatus;
					}
				}
			}
			else
			{
				if (other.tag == "Diacritic")
				{
					if (DancingDotsGameManager.instance.activeDiacritic.diacritic == diacritic)
					{
						overDestinationMarker = markerStatus;
					}
				}
			}
		}

		void OnTriggerEnter(Collider other)
		{
			Setmarker(other, true);
		}

		void OnTriggerStay(Collider other)
		{
			Setmarker(other, true);
		}

		void OnTriggerExit(Collider other)
		{
			Setmarker(other, false);
		}


	}

}
