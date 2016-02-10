using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
	public Transform muzzle;
	public Projectile projectile;
	public float timeBetweenShots = 0.1f;
	public float muzzleSpeed = 35f;

	float nextShotTime;

	//void Start ()
	//{
	//	nextShotTime = Time.time;
	//}

	public void Shoot ()
	{
		if (nextShotTime <= Time.time)
		{
			Projectile newProjectile = Instantiate (projectile, muzzle.position, muzzle.rotation) as Projectile;
			newProjectile.SetSpeed (muzzleSpeed);

			nextShotTime = Time.time + timeBetweenShots;
		}
	}
}
