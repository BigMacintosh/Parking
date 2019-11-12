using UnityEngine;

public class CameraFollowController : MonoBehaviour {
	
	[SerializeField] private Transform objectToFollow;
	[SerializeField] private Vector3 offset;
	[SerializeField] private float followSpeed = 10;
	[SerializeField] private float lookSpeed = 10;
	
	private void LookAtTarget()
	{
		var _lookDirection = objectToFollow.position - transform.position;
		var _rot = Quaternion.LookRotation(_lookDirection, Vector3.up);
		transform.rotation = Quaternion.Lerp(transform.rotation, _rot, lookSpeed * Time.deltaTime);
	}

	private void MoveToTarget()
	{
		var _targetPos = objectToFollow.position + 
						 objectToFollow.forward * offset.z + 
						 objectToFollow.right * offset.x + 
						 objectToFollow.up * offset.y;
		transform.position = Vector3.Lerp(transform.position, _targetPos, followSpeed * Time.deltaTime);
	}

	private void FixedUpdate()
	{
		LookAtTarget();
		MoveToTarget();
	}
}
