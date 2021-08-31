using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Camera Control")]
    public float lookSpeed;

    [Header("Movement")]
    public float gravity;
    public float moveSpeed;
    public float jumpSpeed;

    [Header("World manipulation")]
    public GameObject breakParticles;

    // Reference to camera
    private Camera camera;
    private float yLook;

    CharacterController character;
    private float ySpeed;

    private void Start()
    {
        // Setup camera
        camera = GetComponentInChildren<Camera>();
        SetCursorLock(true);

        character = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleLook();

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position,camera.transform.forward,out hit))
            {
                Chunk chunk = hit.collider.GetComponent<Chunk>();
                if (chunk)
                {
                    bool breaking = Input.GetMouseButton(0);

                    Vector3Int offset = chunk.chunkPos * Chunk.CHUNK_SIZE;
                    Vector3 hitPos = hit.point - hit.normal * 0.5f;
                    Vector3 facePos = hit.point + hit.normal * 0.5f;

                    if (!breaking)
                    {
                        hitPos = facePos;
                    }

                    Vector3Int blockPos = new Vector3Int(
                        (int)hitPos.x,
                        (int)hitPos.y,
                        (int)hitPos.z
                        ) - offset;
                    print(blockPos);
                    
                    chunk.SetBlock(blockPos,(byte)(breaking ? 0 : 1));
                    if (breaking)
                    {
                        hitPos.x = Mathf.Round(hitPos.x);
                        hitPos.y = Mathf.Round(hitPos.y);
                        hitPos.z = Mathf.Round(hitPos.z);
                        Instantiate(breakParticles, hitPos, Quaternion.identity, chunk.transform);
                    }
                    chunk.GenerateMesh();
                }
            }
        }
    }

    void HandleMovement()
    {
        if (character.isGrounded)
        {
            ySpeed = -1;
            if (Input.GetButtonDown("Jump"))
            {
                ySpeed = jumpSpeed;
            }
        }
        ySpeed -= gravity * Time.deltaTime;

        Vector3 motion = new Vector3(Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical")).normalized * moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) motion *= 2;
        motion = transform.TransformDirection(motion);
        motion.y = ySpeed;

        character.Move(motion * Time.deltaTime);
    }
    void HandleLook()
    {
        transform.Rotate(Input.GetAxis("Mouse X") * Time.deltaTime * lookSpeed * Vector3.up);
        // Vertical look
        yLook -= Input.GetAxis("Mouse Y") * Time.deltaTime * lookSpeed;
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
