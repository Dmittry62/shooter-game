using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
	enum State { Idle, Chasing, Attack }

	public float attackDistance = 0.5f;
	public float timeBetweenAttacks = 1f;
	public float damage = 1f;

	float nextAttackTime;

	State currentState;

	NavMeshAgent mPathFinder;
	Transform target;
	LivingEntity targetEntity;

	Material skinMaterial;

	float mCollisionRadius;
	float mTargetCollisionRadius;

	bool hasTarget;

	protected override void Start ()
	{
		base.Start ();

		//nextAttackTime = Time.time;
		currentState = State.Chasing;

		mPathFinder = GetComponent<NavMeshAgent> ();
		skinMaterial = GetComponent<Renderer> ().material;

		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		if (player)
		{
			hasTarget = true;

			target = player.transform;
			targetEntity = target.GetComponent<LivingEntity> ();
			targetEntity.OnDeath += OnTargetDeath;

			//mTargetCollisionRadius = target.GetComponent<CapsuleCollider> ().radius;
			mTargetCollisionRadius = target.GetComponent<CharacterController> ().radius;
			mCollisionRadius = GetComponent<CapsuleCollider> ().radius;

			StartCoroutine (UpdatePath ());
		}
	}

	void Update ()
	{
		if (!hasTarget)
			return;

		if (Time.time >= nextAttackTime)
		{
			float sqrDist = (target.position - transform.position).sqrMagnitude;
			float dist = attackDistance + mCollisionRadius + mTargetCollisionRadius;
			if (sqrDist <= dist * dist)
			{
				nextAttackTime = Time.time + timeBetweenAttacks;
				StartCoroutine (Attack ());
			}
		}
	}

	void OnTargetDeath ()
	{
		hasTarget = false;
		currentState = State.Idle;
	}

	IEnumerator Attack ()
	{
		currentState = State.Attack;
		mPathFinder.enabled = false;

		Color originalColor = skinMaterial.color;
		skinMaterial.color = Color.red;

		Vector3 originalPosition = transform.position;
		Vector3 targetDir = (target.position - transform.position).normalized;
		Vector3 attackPosition = target.position - targetDir * (mCollisionRadius + mTargetCollisionRadius);

		float attackSpeed = 3f;
		float x = 0f;

		bool hasAppliedDamage = false;

		while (x < 1f)
		{
			if (x >= 0.5f && !hasAppliedDamage)
			{
				hasAppliedDamage = true;
				targetEntity.TakeDamage (damage);
			}

			x += attackSpeed * Time.deltaTime;
			float interpolation = 4f * (x - x * x);
			transform.position = Vector3.Lerp (originalPosition, attackPosition, interpolation);

			yield return null;
		}

		skinMaterial.color = originalColor;

		currentState = State.Chasing;
		mPathFinder.enabled = true;
	}

	IEnumerator UpdatePath ()
	{
		const float refreshTime = 0.1f;

		while (target && !dead)
		{
			if (currentState == State.Chasing)
			{
				Vector3 targetDir = (target.position - transform.position).normalized;
				Vector3 targetPos = target.position - targetDir * (mCollisionRadius + mTargetCollisionRadius + attackDistance * 0.5f);

				mPathFinder.SetDestination (targetPos);
			}

			yield return new WaitForSeconds (refreshTime);
		}
	}
}
