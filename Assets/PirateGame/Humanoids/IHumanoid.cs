using UnityEngine;
using UnityEngine.CustomUtils;

public interface IHumanoid : IBehavior
{
	bool IsJumping { get; }
	bool IsGrounded { get; }
	Vector3 Movement { get; }
	Vector3 Velocity { get; }
	Rigidbody Rigidbody { get; }
}
