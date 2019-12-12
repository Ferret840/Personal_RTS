using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainCamera : MonoBehaviour
{
    Camera cam;

    LayerMask view;

    public LayerMask dim1;
    public LayerMask dim2;
    public LayerMask dim3;

    // Use this for initialization
    void Start ()
    {
        cam = GetComponent<Camera>();

        view = dim3;
	}
	
	// Update is called once per frame
	void Update ()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            view = dim1;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            view = dim2;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            view = dim3;

        cam.cullingMask = view;
    }
}
