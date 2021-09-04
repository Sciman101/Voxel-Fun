using UnityEngine;

public class Player : MonoBehaviour
{

    [Header("Camera Control")]
    public float lookSpeed;

    [Header("Movement")]
    public float moveSpeed;

    // Reference to camera
    private Camera camera;
    private float yLook;

    private void Start()
    {
        // Setup camera
        camera = GetComponentInChildren<Camera>();
        SetCursorLock(true);
    }

    // Update is called once per frame
    void Update()
    {
        HandleLook();
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector3 motion = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        float amp = motion.sqrMagnitude;

        motion = motion.normalized * moveSpeed * Mathf.Clamp01(amp);

        if (Input.GetKey(KeyCode.LeftShift)) {
            motion *= 2;
        }

        transform.position += camera.transform.TransformVector(motion) * Time.deltaTime;
    }
    void HandleLook()
    {
        transform.Rotate(Input.GetAxisRaw("Mouse X") * Time.deltaTime * lookSpeed * Vector3.up);
        // Vertical look
        yLook -= Input.GetAxisRaw("Mouse Y") * Time.deltaTime * lookSpeed;
        yLook = Mathf.Clamp(yLook, -90, 90);
        camera.transform.localEulerAngles = Vector3.right * yLook;

        // Unlock mouse
        if (Input.GetKeyDown(KeyCode.Escape) && !Cursor.visible)
        {
            SetCursorLock(false);
        }
        if (Input.GetMouseButtonDown(0) && Cursor.visible)
        {
            SetCursorLock(true);
        }
    }

    void SetCursorLock(bool locked)
    {
        Cursor.visible = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
