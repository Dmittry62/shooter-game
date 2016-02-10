using UnityEngine;
using System.Collections;

[System.Serializable]
public class Foot
{
	public Transform end;
	Transform handle;
	TwoBoneIK ik;
	Vector3 localPosition;

	//swing
	public Vector3 swingTarget;
	public Vector3 swingVector;
	public float swingProgress;
	public bool isMoving;

	public void UpdateSwing (Vector3 target, float maxFootOffsetSq)
	{
		if ((target - swingTarget).sqrMagnitude <= maxFootOffsetSq)
			return;

		swingTarget = target;
		swingVector = swingTarget - handle.position;

		if (isMoving)
		{
			swingVector.y = 0f;
			swingVector /= 1f - swingProgress;
		}
		else
		{
			isMoving = true;
		}
	}

	public void Swing (float swingSpeed, float strideHeight)
	{
		swingProgress += swingSpeed * Time.deltaTime;
		if (swingProgress >= 1f)
		{
			handle.position = swingTarget;
			swingProgress = 0f;
			isMoving = false;
		}
		else
		{
			handle.position = swingTarget - swingVector * (1f - swingProgress) +
			Vector3.up * strideHeight * 4f * (swingProgress - swingProgress * swingProgress);
		}
	}

	public Vector3 position
	{
		get { return handle.position;  }
		set { handle.position = value; }
	}

	public void Initialize (Transform T)
	{
		Vector3 pos = end.position;
		pos.y = 0f;
		localPosition = T.InverseTransformPoint(pos);

		handle = new GameObject (end.name + ".handle").transform;
		handle.position = pos;
		ik = new TwoBoneIK (end);
	}

	public void UpdateIK ()
	{
		ik.Move (handle.position);
	}

	public Vector3 GetTargetPosition (Transform T)
	{
		return T.TransformPoint(localPosition);
	}
}

public class BipedAnimation : MonoBehaviour
{
	[HideInInspector] public Foot foot0;
	[HideInInspector] public Foot foot1;

	public float runSpeed = 7f;
	public float strideHeight = 0.5f;
	[HideInInspector] public float swingSpeed = 2f;
	[HideInInspector] public float maxFootOffsetSq = 0.04f;
	public float strideLengthFactor = 0.07f;
	public float swingSpeedFactor = 0.5f;

	float currentSwingSpeed;

	void Start ()
	{
		foot0.Initialize (transform);
		foot1.Initialize (transform);
	}

	public void Move (Vector3 dir, float speed)
	{
		Vector3 increase = dir * speed * strideLengthFactor;
		Vector3 target0 = foot0.GetTargetPosition (transform) + increase;
		currentSwingSpeed = swingSpeed + speed * swingSpeedFactor;


		bool isRunning = (speed >= runSpeed) && (foot0.swingProgress - foot1.swingProgress >= 0.75f);
		
		if (isRunning)
		{
			Vector3 target1 = foot1.GetTargetPosition (transform) + increase;
		
			foot0.UpdateSwing (target0, maxFootOffsetSq);
			foot1.UpdateSwing (target1, maxFootOffsetSq);
		}
		else if (foot0.isMoving)
			foot0.UpdateSwing (target0, maxFootOffsetSq);
		else
		{
			// choose foot to move

			Vector3 target1 = foot1.GetTargetPosition (transform) + increase;

			float offsetSq0 = (target0 - foot0.position).sqrMagnitude;
			float offsetSq1 = (target1 - foot0.position).sqrMagnitude;

			//swingProgress = 0f;

			if (offsetSq0 + maxFootOffsetSq < offsetSq1)
			{
				foot1.UpdateSwing (target1, maxFootOffsetSq);
				NextFoot ();
			}
			else
				foot0.UpdateSwing (target0, maxFootOffsetSq);
		}

		// move

		if (foot0.isMoving)
		{
			foot0.Swing (currentSwingSpeed, strideHeight);
			//Debug.DrawRay (foot0.swingTarget, Vector3.up, Color.red);
		}

		if (foot1.isMoving)
		{
			foot1.Swing (currentSwingSpeed, strideHeight);
			//Debug.DrawRay (foot1.swingTarget, Vector3.up, Color.blue);
		}

		//if (isRunning)
		//	Debug.Log ("run");

		if (!foot0.isMoving && foot1.isMoving)
			NextFoot ();

		//Debug.DrawRay (foot0.swingTarget, Vector3.up, Color.red);

		foot0.UpdateIK ();
		foot1.UpdateIK ();
	}

	//void Swing ()
	//{
	//	foot0.swingProgress += currentSwingSpeed * Time.deltaTime;
	//	if (foot0.swingProgress >= 1f)
	//	{
	//		foot0.position = foot0.swingTarget;
	//		NextFoot ();
	//		isMoving = false;
	//	}
	//	else
	//	{
	//		//foot0.position = foot0.swingTarget - foot0.swingVector * (1f - foot0.swingProgress) +
	//		//	Vector3.up * strideHeight * foot0.swingHeight;
	//	}
	//}

	void NextFoot ()
	{
		Foot foot = foot0;
		foot0 = foot1;
		foot1 = foot;

		//isMoving = false;
	}
}