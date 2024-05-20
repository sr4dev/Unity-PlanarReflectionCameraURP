using UnityEngine;

public class Player : MonoBehaviour {
    
    [Range(0f, 1f)]
    public float sensitivity = 0.5f;

    private void Start ()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update ()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float rotX = transform.eulerAngles.y;
            float rotY = transform.eulerAngles.x;
            if (rotY > 180)
            {
                rotY -= 360;
            }
            
            rotX += Input.GetAxis("Mouse X") * (1f + sensitivity * 9f);
            rotY -= Input.GetAxis("Mouse Y") * (1f + sensitivity * 9f);
            rotY = Mathf.Clamp(rotY, -80f, 80f);
            transform.eulerAngles = new Vector3(rotY, rotX, 0f);
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
        
        //move to the front
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * Time.deltaTime * 5f;
        }
        
        //move to the back
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * Time.deltaTime * 5f;
        }
        
        //move to the left
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * Time.deltaTime * 5f;
        }
        
        //move to the right
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * Time.deltaTime * 5f;
        }
    }
}