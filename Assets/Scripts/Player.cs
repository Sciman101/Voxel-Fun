using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Camera Control")]
    public float lookSpeed;

    [Header("Movement")]
    public float gravity;
    public float moveSpeed;
    public float jumpSpeed;
    public Text debugText;

    // Reference to camera
    private Camera camera;
    private float yLook;

    CharacterController character;
    private float ySpeed;

    Vector3 startPos;

    private void Start()
    {
        // Setup camera
        camera = GetComponentInChildren<Camera>();
        SetCursorLock(true);

        startPos = transform.position;

        character = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleLook();
        HandleMovement();

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position,camera.transform.forward,out hit))
            {
                // Convert to block position
                BlockPos hitPos = new BlockPos(hit.point - hit.normal * 0.5f);

                foreach (BlockFace face in Enum.GetValues(typeof(BlockFace)))
                {
                //    Debug.Log(face.ToString() + ", " + World.instance.GetBlock(hitPos.offset(face)));
                }

                //Debug.Log(string.Format("{0} @ {1} in chunk {2}",World.instance.GetBlock(hitPos),hitPos,hitPos/Chunk.CHUNK_SIZE));
                World.instance.SetBlock(hitPos, Blocks.AIR);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ySpeed = 0;
            transform.position = startPos;
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

        debugText.text = string.Format("X:{0:0.##}\nY:{1:0.##}\nZ:{2:0.##}",transform.position.x,transform.position.y,transform.position.z);
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
