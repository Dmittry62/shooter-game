using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	CharacterController mCharacterController;
	//Vector3             mVelocity;

	public void Move (Vector3 velocity)
	{
		//mVelocity = velocity;
		mCharacterController.Move (velocity * Time.deltaTime);
	}

	public void LookAt (Vector3 point)
	{
		point.y = transform.position.y;
		transform.LookAt (point);
	}

	void Start ()
	{
		//mRigidBody = GetComponent<Rigidbody> ();
		//mRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		mCharacterController = GetComponent<CharacterController> ();
	}

	//void FixedUpdate ()
	//{
	//	mRigidBody.MovePosition (mRigidBody.position + mVelocity * Time.deltaTime);
	//}
}
