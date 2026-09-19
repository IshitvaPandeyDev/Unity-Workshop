using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float zoomspeed = 2f;
    [SerializeField] private float zoomLerpspeed = 10f;
    [SerializeField] private float minDist = 3f;
    [SerializeField] private float maxDist = 15f;

    private InputSystem_Actions Controls;

    private CinemachineCamera cam;
    private CinemachineOrbitalFollow orbital;
    private Vector2 scrolldelta;

    private float targetzoom;
    private float currentzoom;

    private void Start()
    {
        Controls = new InputSystem_Actions();
        Controls.Enable();
        Controls.MouseControl.MouseZoom.performed += HandleMouseScroll;

        Cursor.lockState = CursorLockMode.Locked;
        
        cam = GetComponent<CinemachineCamera>();
        orbital = GetComponent<CinemachineOrbitalFollow>();

        targetzoom = currentzoom = orbital.Radius;
    
}
    private void Update()
    {
        if(scrolldelta.y != 0)
        {
            if (orbital != null)
            {
                targetzoom = Mathf.Clamp(orbital.Radius - scrolldelta.y*zoomspeed, minDist, maxDist);
                scrolldelta = Vector2.zero;
            }
        }
        currentzoom = Mathf.Lerp(currentzoom, targetzoom, Time.deltaTime * zoomLerpspeed);
        orbital.Radius = currentzoom;

        float bumperdelta = Controls.MouseControl.GamePadZoom.ReadValue<float>();
        if(bumperdelta != 0)
        {
            targetzoom = Mathf.Clamp(orbital.Radius - bumperdelta*zoomspeed, minDist, maxDist) ;
        }
    }

    private void HandleMouseScroll(InputAction.CallbackContext context)
    {
        scrolldelta = context.ReadValue<Vector2>();
        Debug.Log($"Mouse is scrolling Value : {scrolldelta}");
    }
}
    