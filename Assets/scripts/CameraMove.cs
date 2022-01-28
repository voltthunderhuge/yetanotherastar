using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
		if (Input.GetMouseButtonDown(1))
		{
			_downPos = Input.mousePosition;
		}

		if (Input.GetMouseButton(1))
		{
			transform.position +=
				transform.right * (Input.mousePosition.x - _downPos.x) * 0.0005f +
				transform.up * (Input.mousePosition.y - _downPos.y) * 0.0005f;

		}
    }

	private Vector3 _downPos;
}
