using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator), typeof(IHumanoid))]
public class PlayerAnimator : MonoBehaviour
{
	Animator Animator => GetComponent<Animator>();
	IHumanoid Humanoid => GetComponent<IHumanoid>();


	// Start is called before the first frame update
	void Start()
    {

    }

	public void OnAttack(InputValue input)
	{
		Animator.SetTrigger("Attack");
		Animator.SetBool("IsAttacking", true);
	}
	public void OnDie(InputValue input)
	{
		Animator.SetTrigger("Die");
		Animator.SetBool("IsDead", true);
	}

	// Update is called once per frame
	void Update()
    {
		Animator.SetBool("IsJumping", Humanoid.IsJumping);
		Animator.SetBool("IsFalling", !Humanoid.IsGrounded && Humanoid.Rigidbody.velocity.y < 0);
		Animator.SetBool("IsWalking", Humanoid.IsGrounded && Humanoid.Movement.sqrMagnitude > 0 && Humanoid.Rigidbody.velocity.sqrMagnitude > 0.1);

		if (Humanoid.Movement.sqrMagnitude > 0)
		{
			Humanoid.Rigidbody.MoveRotation(Quaternion.LookRotation(Humanoid.Movement, -Physics.gravity.normalized));
		}
		
	}
}
