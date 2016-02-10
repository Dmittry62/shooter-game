using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerController), typeof (GunController))]
public class Player : LivingEntity
{
	public float speed = 5f;
	public float turnSpeed = 180f;

	PlayerController mController;
	GunController    mGunController;
	BipedAnimation   mAnimation;
	Camera           mCamera;
	Plane            mFloorPlane;

	protected override void Start ()
	{
		base.Start ();

		mController    = GetComponent<PlayerController> ();
		mGunController = GetComponent<GunController> ();
		mAnimation     = GetComponent<BipedAnimation> ();
		mCamera        = Camera.main;
		mFloorPlane    = new Plane (Vector3.up, 0.8f);
	}

	void Update ()
	{
		// movement
		float x = Input.GetAxis ("Horizontal");
		float z = Input.GetAxis ("Vertical"  );

		float moveSpeed = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));
		Vector3 moveDirection = new Vector3 (x, 0f, z);

		if (moveSpeed != 0f)
		{
			moveSpeed *= speed;
			moveDirection.Normalize ();
			mController.Move (moveDirection * moveSpeed);

			//Quaternion targetRotation = Quaternion.LookRotation (moveDirection);
			//transform.rotation = Quaternion.RotateTowards (transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
		}

		if (mAnimation)
			mAnimation.Move (moveDirection, moveSpeed);

		// look
		Ray ray = mCamera.ScreenPointToRay (Input.mousePosition);

		float rayDist;
		if (mFloorPlane.Raycast (ray, out rayDist))
		{
			Vector3 point = ray.GetPoint (rayDist);
			mController.LookAt (point);
			//Debug.DrawLine (ray.origin, point, Color.red);
		}

		// weapon
		if (Input.GetButton ("Fire1"))
		{
			mGunController.Shoot ();
		}
	}
}
