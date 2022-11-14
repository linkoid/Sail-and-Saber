using UnityEngine;

public interface IHumanoid
{
	bool IsJumping { get; }
	bool IsGrounded { get; }
	Vector3 Movement { get; }
	Rigidbody Rigidbody { get; }
}
