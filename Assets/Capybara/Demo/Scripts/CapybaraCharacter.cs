using UnityEngine;
using System.Collections;

public class CapybaraCharacter : MonoBehaviour {
	Animator capybaraAnimator;
	public bool jumpUp=false;
	public float groundCheckDistance = 0.1f;
	public float groundCheckOffset=0.01f;
	public GameObject leftFoot;
	public bool leftFootIsGrounded;
	public float jumpSpeed=1f;
	Rigidbody capybaraRigid;

	void Start () {
		capybaraAnimator = GetComponent<Animator> ();
		capybaraRigid=GetComponent<Rigidbody>();
	}

	void FixedUpdate(){
		CheckGroundStatus ();
	}

	public void Attack(){
		capybaraAnimator.SetTrigger("Attack");
	}
	
	public void Hit(){
		capybaraAnimator.SetTrigger("Hit");
	}
	
	public void Eat(){
		capybaraAnimator.SetTrigger("Eat");
	}
	
	public void Death(){
		capybaraAnimator.SetTrigger("Death");
	}
	
	public void Rebirth(){
		capybaraAnimator.SetTrigger("Rebirth");
	}

	public void SitDown(){
		capybaraAnimator.SetTrigger("SitDown");
	}

	public void LieDown(){
		capybaraAnimator.SetTrigger("LieDown");
	}

	public void Sleep(){
		capybaraAnimator.SetTrigger("Sleep");
	}

	public void StandUp(){
		capybaraAnimator.SetTrigger("StandUp");
	}

	public void WakeUp(){
		capybaraAnimator.SetTrigger("WakeUp");
	}
	
	public void Jump(){
		if (leftFootIsGrounded) {
			capybaraAnimator.SetTrigger ("Jump");
			jumpUp = true;
		}
	}

	void CheckGroundStatus()
	{
		RaycastHit hitInfo;

		if (Physics.Raycast(leftFoot.transform.position + (Vector3.up * groundCheckOffset), Vector3.down, out hitInfo, groundCheckDistance))
		{
			if(!jumpUp){
				leftFootIsGrounded = true;
				capybaraAnimator.applyRootMotion = true;
				capybaraAnimator.SetBool("IsGrounded",true);
			}
		}
		else
		{
			leftFootIsGrounded = false;
			capybaraAnimator.applyRootMotion = false;
			capybaraAnimator.SetBool("IsGrounded",false);
			if(jumpUp){
				jumpUp=false;
				capybaraRigid.AddForce(transform.up*jumpSpeed+transform.forward*capybaraRigid.velocity.sqrMagnitude,ForceMode.Impulse);
			}
		}
	}
	
	public void Move(float v,float h){
		capybaraAnimator.SetFloat ("Forward", v);
		capybaraAnimator.SetFloat ("Turn", h);
	}
}
