using UnityEngine;
using System.Collections;

public class CapybaraUserController : MonoBehaviour {
	CapybaraCharacter capybaraCharacter;
	
	void Start () {
		capybaraCharacter = GetComponent < CapybaraCharacter> ();
	}
	
	void Update () {	
		if (Input.GetButtonDown ("Fire1")) {
			capybaraCharacter.Attack();
		}
		if (Input.GetButtonDown ("Jump")) {
			capybaraCharacter.Jump();
		}
		if (Input.GetKeyDown (KeyCode.H)) {
			capybaraCharacter.Hit();
		}
		if (Input.GetKeyDown (KeyCode.E)) {
			capybaraCharacter.Eat();
		}
		if (Input.GetKeyDown (KeyCode.K)) {
			capybaraCharacter.Death();
		}
		if (Input.GetKeyDown (KeyCode.R)) {
			capybaraCharacter.Rebirth();
		}		
		if (Input.GetKeyDown (KeyCode.N)) {
			capybaraCharacter.SitDown();
		}		
		if (Input.GetKeyDown (KeyCode.B)) {
			capybaraCharacter.LieDown();
		}		

		if (Input.GetKeyDown (KeyCode.M)) {
			capybaraCharacter.Sleep();
		}		
		if (Input.GetKeyDown (KeyCode.R)) {
			capybaraCharacter.Rebirth();
		}		
		if (Input.GetKeyDown (KeyCode.I)) {
			capybaraCharacter.WakeUp();
		}		
		if (Input.GetKeyDown (KeyCode.U)) {
			capybaraCharacter.StandUp();
		}		
	}

	private void FixedUpdate()
	{
		float h = Input.GetAxis ("Horizontal");
		float v = Input.GetAxis ("Vertical");
		if (Input.GetKey(KeyCode.LeftShift)) v *= 0.5f;
		capybaraCharacter.Move (v,h);
	}
}
