using UnityEngine;
using System.Collections;

enum Approachability
{
	reachable,
	unreachableNear,
	unreachableFar
}

public class TwoBoneIK
{
	Transform joint0;
	Transform joint1;
	Transform joint2;

	Quaternion rootTwist;
	//Quaternion twist1;
	Vector3 localUp;

	float maxDist;

	Vector3 localDir0;
	Vector3 localDir1;

	float distSq0;
	float distSq1;

	float dist0;
	float dist1;

	bool valid = false;

	// target
	Approachability approachability = Approachability.reachable;
	Vector3 targetDir;
	Vector3 targetNorm;
	float targetDist;
	float targetDistSq;

	public Transform Root()
	{
		if (valid)
			return joint0;
		else
			return joint2;
	}

	public Transform end
	{
		get
		{
			return joint2;
		}
	}

	public float maxDistance
	{
		get
		{
			return maxDist;
		}
	}

	public TwoBoneIK (Transform endTransform)
	{
		if (endTransform == null)
		{
			Debug.LogWarning ("missing joint2 joint");
			return;
		}

		joint2 = endTransform;

		if (joint2.parent == null)
		{
			Debug.LogWarning ("missing middle joint");
			return;
		}

		joint1 = joint2.parent;

		if (joint1.parent == null)
		{
			Debug.LogWarning ("missing base joint");
			return;
		}

		joint0 = joint1.parent;

		Vector3 dir0 = joint1.position - joint0.position;
		Vector3 dir1 = joint2.position - joint1.position;

		distSq0 = dir0.sqrMagnitude;
		distSq1 = dir1.sqrMagnitude;

		dist0 = Mathf.Sqrt (distSq0);
		dist1 = Mathf.Sqrt (distSq1);

		dir0 /= dist0;
		dir1 /= dist1;

		localDir0 = joint0.InverseTransformDirection (dir0);
		localDir1 = joint1.InverseTransformDirection (dir1);

		maxDist = dist0 + dist1;

		if (1f - Mathf.Abs (Vector3.Dot (dir0, dir1)) < GMath.epsilon)
		{
			Debug.LogWarning ("two bones in a line");
			return;
		}

		Vector3 dir = joint2.position - joint0.position;
		dir = joint0.InverseTransformDirection(dir);
		localUp = -(dir - localDir0 * Vector3.Dot(localDir0, dir)).normalized;

		rootTwist = GMath.Twist (joint0.localRotation, localDir0);
		//twist1 = IKMath.Twist (joint1.localRotation, localDir1);

		valid = true;
	}

	// true if adjust needed
	public bool CheckTarget (Vector3 target, out Vector3 offset, float stock = 0.0f)
	{
		offset = Vector3.zero;
		//targetNorm = Vector3.zero;

		if (!valid)
			return false;

		targetDir = target - joint0.position;
		targetDistSq = targetDir.sqrMagnitude;
		targetDist = Mathf.Sqrt (targetDistSq);

		if (targetDist - Mathf.Abs (dist0 - dist1) < GMath.epsilon)
		{
			approachability = Approachability.unreachableNear;
			return false;
		}

		targetNorm = targetDir / targetDist;
		//offset = -stock * targetNorm;

		if (targetDist >= maxDist)
		{
			approachability = Approachability.unreachableFar;
			offset = targetNorm * (targetDist - maxDist + stock);
			return true;
		}

		if (targetDist >= maxDist - stock)
		{
			approachability = Approachability.reachable;
			offset = targetNorm * (targetDist - maxDist + stock);
			return true;
		}

		approachability = Approachability.reachable;
		return false;
	}

	public void Move()
	{
		if (!valid)
			return;

		switch (approachability)
		{
		case Approachability.reachable:
			{
				float x = (distSq0 - distSq1 + targetDistSq) / (2f * targetDist);
				float y = Mathf.Sqrt(distSq0 - x * x);

				Vector3 dir0 = joint0.TransformDirection(localDir0);
				joint0.rotation = Quaternion.FromToRotation(dir0, targetNorm) * joint0.rotation;
				UnTwist ();


				Vector3 up = joint0.TransformDirection(localUp);

				//Vector3 prevDir0 = dir0;
				dir0 = x * targetNorm + y * up;
				joint0.rotation = Quaternion.FromToRotation(targetNorm, dir0) * joint0.rotation;

				//joint0.rotation = Quaternion.FromToRotation(prevDir0, dir0) * joint0.rotation;

				//newTwist = IKMath.Twist(joint0.localRotation, localDir0);
				//joint0.localRotation = joint0.localRotation * Quaternion.Inverse(newTwist) * rootTwist;

				Vector3 prevDir1 = joint1.TransformDirection(localDir1);//dir1;
				Vector3 dir1 = targetDir - dir0;

				joint1.rotation = Quaternion.FromToRotation(prevDir1, dir1) * joint1.rotation;
				//newTwist = IKMath.Twist(joint1.localRotation, localDir1);
				//joint1.rotation = joint1.rotation * Quaternion.Inverse(newTwist) * twist1;
			}
			break;

		case Approachability.unreachableFar:
			{
				Vector3 dir0 = joint0.TransformDirection (localDir0);
				joint0.rotation = Quaternion.FromToRotation (dir0, targetNorm) * joint0.rotation;
				UnTwist ();

				Vector3 dir1 = joint1.TransformDirection (localDir1);
				joint1.rotation = Quaternion.FromToRotation (dir1, targetNorm) * joint1.rotation;
			}
			break;
		}

		Debug.DrawLine (joint0.position, joint1.position, Color.green, 0f, false);
		Debug.DrawLine (joint1.position, joint2.position, Color.green, 0f, false);
		Vector3 n = joint0.TransformDirection(localUp);
		Debug.DrawLine (joint0.position, joint0.position + n * 0.3f, Color.blue, 0f, false);
	}

	public void Move(Vector3 target)
	{
		if (!valid)
			return;
	
		Vector3 dir = target - joint0.position;
		float distSq = dir.sqrMagnitude;
		float dist = Mathf.Sqrt (distSq);
	
		if (dist - Mathf.Abs(dist0 - dist1) < GMath.epsilon)
			return;
	
		Vector3 forward = dir / dist;
	
		if (dist >= maxDist)
		{
			Vector3 dir0 = joint0.TransformDirection(localDir0);
			joint0.rotation = Quaternion.FromToRotation(dir0, forward) * joint0.rotation;
			UnTwist ();
	
			Vector3 dir1 = joint1.TransformDirection(localDir1);
			joint1.rotation = Quaternion.FromToRotation(dir1, forward) * joint1.rotation;
		}
		else
		{
			float x = (distSq0 - distSq1 + distSq) / (2f * dist);
			float y = Mathf.Sqrt(distSq0 - x * x);
	
			Vector3 dir0 = joint0.TransformDirection(localDir0);
			joint0.rotation = Quaternion.FromToRotation(dir0, forward) * joint0.rotation;
			UnTwist ();
	
	
			Vector3 up = joint0.TransformDirection(localUp);
	
			//Vector3 prevDir0 = dir0;
			dir0 = x * forward + y * up;
			joint0.rotation = Quaternion.FromToRotation(forward, dir0) * joint0.rotation;
	
			//joint0.rotation = Quaternion.FromToRotation(prevDir0, dir0) * joint0.rotation;
	
			//newTwist = IKMath.Twist(joint0.localRotation, localDir0);
			//joint0.localRotation = joint0.localRotation * Quaternion.Inverse(newTwist) * rootTwist;
	
			Vector3 prevDir1 = joint1.TransformDirection(localDir1);//dir1;
			Vector3 dir1 = dir - dir0;
	
			joint1.rotation = Quaternion.FromToRotation(prevDir1, dir1) * joint1.rotation;
			//newTwist = IKMath.Twist(joint1.localRotation, localDir1);
			//joint1.rotation = joint1.rotation * Quaternion.Inverse(newTwist) * twist1;
		}
	
		Debug.DrawLine (joint0.position, joint1.position, Color.green, 0f, false);
		Debug.DrawLine (joint1.position, joint2.position, Color.green, 0f, false);
		Vector3 n = joint0.TransformDirection(localUp);
		Debug.DrawLine (joint0.position, joint0.position + n * 0.3f, Color.blue, 0f, false);
		//return distSq <= maxDistSq;
	}

	void UnTwist ()
	{
		Quaternion newTwist = GMath.Twist(joint0.localRotation, localDir0);
		joint0.localRotation = joint0.localRotation * Quaternion.Inverse(newTwist) * rootTwist;
	}
}