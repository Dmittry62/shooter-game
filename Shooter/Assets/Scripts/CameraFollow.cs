using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
	public float speed = 5f;

	Vector3 mOffset;
	Camera  mCamera;


	void Start ()
	{
		mCamera = Camera.main;
		mOffset = mCamera.transform.position - transform.position;
	}

	void Update ()
	{
		Vector3 targetPos = transform.position + mOffset;
		//Vector3 dir = targetPos - mCamera.transform.position;
		//
		//float dist = dir.magnitude;
		//
		//float d = speed * Time.deltaTime;
		//
		//if (dist <= d)
		//	mCamera.transform.position = targetPos;
		//else
		//	mCamera.transform.position += dir * d / dist;

		mCamera.transform.position = Vector3.Lerp (mCamera.transform.position, targetPos, speed * Time.deltaTime);
	}
}
