using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class ChunkPositionEvent : UnityEvent<Vector3Int> {}

public class Player : MonoBehaviour
{
    private static readonly string[] DIRECTIONS = new string[]{ "North (+Z)", "East (+X)","South (-Z)","West (-X)",};

    [Header("Camera Control")]
    public float lookSpeed;

    [Header("Movement")]
    public float gravity;
    public float moveSpeed;
    public float jumpSpeed;
    public Text debugText;

    public ChunkPositionEvent onPlayerChunkChanged = new ChunkPositionEvent();

    // Reference to camera
    private Camera camera;
    private float yLook;

    CharacterController character;
    private float ySpeed;

    Vector3 startPos;

    Vector3Int chunkPos;

    private void Start()
    {
        // Setup camera
        camera = GetComponentInChildren<Camera>();
        SetCursorLock(true);
        character = GetComponent<CharacterController>();

        startPos = transform.position;
        chunkPos = World.instance.GetChunkPos(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        HandleLook();
        HandleMovement();

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position,camera.transform.forward,out hit))
            {
                // Convert to block position
                BlockPos hitPos = new BlockPos(hit.point - hit.normal * 0.1f);

                //Debug.Log(string.Format("{0} @ {1} in chunk {2}",World.instance.GetBlock(hitPos),hitPos,hitPos/Chunk.CHUNK_SIZE));
                if (Input.GetMouseButton(0))
                {
                    World.instance.SetBlock(hitPos, Blocks.AIR);
                }
                else
                {
                    World.instance.SetBlock(hitPos+(BlockPos)hit.normal, Blocks.DIRT);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ySpeed = 0;
            transform.position = startPos;
        }

        // Update chunk pos
        Vector3Int newChunkPos = World.instance.GetChunkPos(transform.position);
        if (newChunkPos != chunkPos)
        {
            chunkPos = newChunkPos;
            onPlayerChunkChanged.Invoke(chunkPos);
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

        Vector3 motion = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        float amp = motion.sqrMagnitude;

        motion = motion.normalized * moveSpeed * Mathf.Clamp01(amp);

        if (Input.GetKey(KeyCode.LeftShift)) motion *= 2;
        motion = transform.TransformDirection(motion);
        motion.y = ySpeed;

        character.Move(motion * Time.deltaTime);

        int dir = (int)Math.Round(transform.localEulerAngles.y / 90) % 4;
        debugText.text = string.Format("X:{0:0.##}\nY:{1:0.##}\nZ:{2:0.##}\nFacing:{3}",transform.position.x,transform.position.y,transform.position.z, DIRECTIONS[dir]);
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
