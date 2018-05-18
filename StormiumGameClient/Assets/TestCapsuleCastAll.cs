using System.Collections;
using System.Collections.Generic;
using Packet.Guerro.Shared.Physic;
using UnityEngine;

public class TestCapsuleCastAll : MonoBehaviour
{

	public CapsuleCollider CapsuleCollider;
	public Vector3 Target;
	
	// Use this for initialization
	void Start ()
	{
		CapsuleCollider = GetComponent<CapsuleCollider>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		var min = CapsuleCollider.GetWorldBottom(transform.position, transform.rotation);
		var max = CapsuleCollider.GetWorldTop(transform.position, transform.rotation);
		var center = CapsuleCollider.GetWorldCenter(transform.position, transform.rotation);
		
		var direction = Target - transform.position;

		var hits = Physics.CapsuleCastAll(min, max, CapsuleCollider.radius, direction, 15);
		foreach (var hit in hits)
		{
			Debug.DrawLine(center, hit.point, Color.blue, 0.1f);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		Gizmos.DrawWireSphere(Target, 0.1f);
	}
}
