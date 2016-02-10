using UnityEngine;
using System.Collections;

//[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
	public LayerMask collisionMask;
	public float lifeTime = 3f;
	public float skinWidth = 0.1f;
	float speed = 10f;
	float damage = 1f;

	public void SetSpeed (float newSpeed)
	{
		speed = newSpeed;
	}

	void Start ()
	{
		Destroy (gameObject, lifeTime);

		Collider[] colliders = Physics.OverlapSphere (transform.position, 0.1f, collisionMask);

		if (colliders.Length > 0)
		{
			IDamageable damageable = colliders[0].GetComponent<IDamageable> ();
			if (damageable != null)
				damageable.TakeDamage (damage);
			
			GameObject.Destroy (gameObject);
		}
	}

	void Update ()
	{
		CheckCollisions (speed * Time.deltaTime);
		transform.Translate(Vector3.forward * speed * Time.deltaTime);
	}

	void CheckCollisions (float moveDistance)
	{
		Ray ray = new Ray (transform.position, transform.forward);

		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))
		{
			IDamageable damageable = hit.transform.GetComponent<IDamageable> ();
			if (damageable != null)
			{
				damageable.TakeHit(damage, hit);
			}

			GameObject.Destroy (gameObject);
		}
	}
}
