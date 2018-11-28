using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void Action();
public delegate void Action2();
public delegate void Action3();

public class ClickDetect : MonoBehaviour {

	public Action onMouseClickAction;
	public Action2 onMouseClickRightAction;
	public Action3 onMouseDragAction;

	bool exit = true;

	void OnMouseDrag(){
		if( onMouseDragAction != null & !exit) {
			onMouseDragAction();
		}
	}

	void OnMouseExit(){
		exit = true;
	}

	void OnMouseOver(){
		if( onMouseClickRightAction != null) {
			if(Input.GetMouseButtonDown(1))onMouseClickRightAction();
		}

		if( onMouseClickAction != null) {
			if(Input.GetMouseButtonDown(0))onMouseClickAction();
		}
		exit = false;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
