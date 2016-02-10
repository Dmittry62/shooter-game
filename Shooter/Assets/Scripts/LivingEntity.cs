using UnityEngine;
using System.Collections;

public class LivingEntity : MonoBehaviour, IDamageable
{
	public float startingHealth;

	protected float health;
	protected bool dead;

	public event System.Action OnDeath;

	protected virtual void Start ()
	{
		health = startingHealth;
	}

	public void TakeHit (float damage, RaycastHit hit)
	{
		TakeDamage (damage);
	}

	public void TakeDamage (float damage)
	{
		health -= damage;

		if (health <= 0f && !dead)
			Die ();
	}

	[ContextMenu("Self destruct")]
	void Die ()
	{
		if (OnDeath != null)
			OnDeath ();
		
		dead = true;
		Destroy (gameObject);
	}
}
