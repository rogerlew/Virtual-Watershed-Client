using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class mouselistener : MonoBehaviour
{
    public bool cursorState;

    public Vector2 mousePos
    {
        get { return Input.mousePosition; }
    }

    // Use this for initialization
    void Start()
    {
        cursorState = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.Y))
        {
            if (cursorState == true)
            {
                cursorState = false;
            }
            else
            {
                cursorState = true;
            }
        }
        //Cursor.visible = cursorState;
    }
}