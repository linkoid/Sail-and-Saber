using PirateGame.Humanoids;
using PirateGame.Ships;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class HumanoidAnimator : MonoBehaviour
{
	public Animator Animator => GetComponent<Animator>();
	IHumanoid Humanoid => GetComponent<IHumanoid>();


	// Start is called before the first frame update
	void Start()
	{
		Animator.stabilizeFeet = false;
		Animator.feetPivotActive = 1;
	}

	public void OnAttack(bool value)
	{
		if (value)
			Animator.SetTrigger("Attack");
		Animator.SetBool("IsAttacking", value);
	}
	public void OnAttack(InputValue input)
	{
		OnAttack(input.isPressed);
	}
	public void OnDie(InputValue input)
	{
		OnDie(input.isPressed);
	}

	public void OnDie(bool value)
	{
		if (value)
			Animator.SetTrigger("Die");
		Animator.SetBool("IsDead", value);
	}

	// Update is called once per frame
	void Update()
    {
		if (Humanoid == null) return;


		Animator.SetBool("IsJumping", Humanoid.IsJumping);
		Animator.SetBool("IsFalling", !Humanoid.IsGrounded && Humanoid.Velocity.y < 0);
		Animator.SetBool("IsWalking", Humanoid.IsGrounded && Humanoid.Movement.sqrMagnitude > 0 && Humanoid.Velocity.sqrMagnitude > 0.1);

		if (Humanoid.Movement.sqrMagnitude > 0)
		{
			var lookDirection = Quaternion.LookRotation(Humanoid.Movement, -Physics.gravity.normalized);
			Humanoid.Rigidbody.MoveRotation(lookDirection);
		}
	}

	// Callback for setting up animation IK
	void OnAnimatorIK(int layerIndex)
	{
		if (TryGetTarget(out Component target))
		{
			LookAt(target);
		}
		else
		{
			Animator.SetLookAtWeight(0);
		}
	}

	private bool TryGetTarget(out Component target)
	{
		target = null;
		if (Humanoid is AIHumanoid ai)
		{
			target = ai.Goal;
		}
		else
		{
			
		}

		return target != null;
	}

	private void LookAt(Component target)
	{
		Vector3 targetPosition = target.transform.position;
		if (target is Transform transform)
		{
			// Don't look at pure transform objects
			Animator.SetLookAtWeight(0);
		}
		else if (target.TryGetComponent(out Animator animator) && animator.isHuman)
		{
			Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
			if (head != null)
			{
				targetPosition = head.position;
				Animator.SetLookAtWeight(1, 0.5f, 0.75f, 1.0f, 0.5f);
			}
			else // If no head
			{
				targetPosition = animator.bodyPosition;
				Animator.SetLookAtWeight(0.25f);
			}
		}
		else if (target is Cannon cannon)
		{
			targetPosition = cannon.LookTarget.position;
			Animator.SetLookAtWeight(1, 0.25f, 0.75f, 1.0f, 0.5f);
		}
		else // If not a recognized type
		{
			Animator.SetLookAtWeight(0.25f);
		}
		Animator.SetLookAtPosition(targetPosition);
	}
}
