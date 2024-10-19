using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainPlayerController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject mesh;
    
    [SerializeField] private float moveSpeed;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    
    CharacterController controller;
    private Vector3 movementDirection;

    
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        controller.Move(moveSpeed * Time.deltaTime * movementDirection);
    }

    public void Look(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Getting the screen mouse position when the mouse is moved
            Vector2 screenMousePosition = context.ReadValue<Vector2>();

            // 
            float cameraToPlayerDistance = Mathf.Abs(mainCamera.transform.position.y - transform.position.y);
            Vector3 mousePoint = mainCamera.ScreenToWorldPoint(new Vector3(screenMousePosition.x, screenMousePosition.y, cameraToPlayerDistance));
            mousePoint.y = transform.position.y;
        
            mesh.transform.LookAt(mousePoint);
        }
    }
    
    public void StartDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            float dashStartTime = Time.time;
            StartCoroutine(Dash(movementDirection, dashStartTime));
        }
    }
    
    IEnumerator Dash(Vector3 direction, float startTime)
    {
        while (Time.time < startTime + dashDuration)
        {
            controller.Move(dashSpeed * Time.deltaTime * direction);
            
            yield return null;
        }
        
        controller.velocity.Set(0, 0, 0);
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position, mesh.transform.forward * 2, out hit);
            Debug.DrawRay(transform.position, mesh.transform.forward * 2, Color.red, 0.5f);

            // if hit object is not null
            if (hit.collider)
            {
                hit.collider.gameObject.GetComponent<IInteractible>().Interact(gameObject);
            }
        }
    }
}
