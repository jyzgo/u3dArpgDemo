using UnityEngine;

public class MoveController : MonoBehaviour
{
	public float speed = 5;

	private NavMeshAgent _navAgent;

	private Transform _transform;

	void Start()
	{
		_navAgent = this.GetComponent<NavMeshAgent>();

		_transform = transform;
	}

	void Update()
	{
		Vector3 destPoint = _transform.localPosition;

		if (Input.GetKey(KeyCode.A))
		{
			_navAgent.Move(-_transform.right * speed * Time.deltaTime);
		}
		else if (Input.GetKey(KeyCode.D))
		{
			_navAgent.Move(_transform.right * speed * Time.deltaTime);
		}
		else if (Input.GetKey(KeyCode.W))
		{
			_navAgent.Move(_transform.forward * speed * Time.deltaTime);
		}
		else if (Input.GetKey(KeyCode.S))
		{
			_navAgent.Move(-_transform.forward * speed * Time.deltaTime);
		}
	}
}
