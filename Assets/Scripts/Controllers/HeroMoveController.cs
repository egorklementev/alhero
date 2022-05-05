using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroMoveController : MonoBehaviour
{

    public GameObject visualObj;
    public GameObject mainCamera;
    public Animator anim;

    [Header("Controls")]
    [Space(20f)]
    public float fingerEpsilon = 100f;
    [Range(0f, 100f)]
    public float movementSpeed = 1.5f;
    [Range(1f, 100f)]
    public float maxSpeed = 5f;

    [Header("Item throwing")]
    [Range(0f, 1f)]
    public float minTapTime = .444f;
    [Range(0f, 1f)]
    public float maxTapTime = .666f;
    public float throwForce = 10f;

    [Space(20f)]
    public UIController ui;
    public LogicController logic;
    public DataController data;

    [Space(20f)]
    public Vector3 cameraOffset = new Vector3(-2f, 10f, -2f);

    public static bool uiTookControl = false;

    private Rigidbody body;
    private Vector2 dragStart; // Starting point of tap. Used for player controlling.
    private float angle = -1f;
    private float tapCounter = 0f;
    private bool isMoving = false;
    private Direction moveDir = Direction.STATIONARY;
    private Direction faceDir = Direction.NORTH;
    private const string walkParam = "IsWalking"; // Animation parameter

    void Awake()
    {
        dragStart = new Vector2(0f, 0f);
        body = GetComponent<Rigidbody>();
        uiTookControl = false;
    }

    void FixedUpdate()
    {
        MoveCharacter(movementSpeed, moveDir);
    }

    void Update()
    {
        if (!uiTookControl)
        {
            angle = -1f;
            isMoving = false;
            moveDir = Direction.STATIONARY;

            #region WASD movement
            if (Input.GetKey(KeyCode.W))
            {
                moveDir = Direction.NORTH;
                angle = 0f;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                moveDir = Direction.WEST;
                angle = -90f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                moveDir = Direction.SOUTH;
                angle = -180f;

            }
            else if (Input.GetKey(KeyCode.D))
            {
                moveDir = Direction.EAST;
                angle = -270f;
            }
            #endregion

            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    dragStart = touch.position;
                }
                else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    if (Vector2.Distance(touch.position, dragStart) > fingerEpsilon)
                    {
                        float x = touch.position.x - dragStart.x;
                        float y = touch.position.y - dragStart.y;
                        if (x > 0 && y > 0)
                        {
                            moveDir = Direction.EAST;
                            faceDir = moveDir;
                            angle = -270f;
                        }
                        else if (x > 0 && y < 0)
                        {
                            moveDir = Direction.SOUTH;
                            faceDir = moveDir;
                            angle = -180f;
                        }
                        else if (x < 0 && y < 0)
                        {
                            moveDir = Direction.WEST;
                            faceDir = moveDir;
                            angle = -90f;
                        }
                        else
                        {
                            moveDir = Direction.NORTH;
                            faceDir = moveDir;
                            angle = 0f;
                        }
                    }

                    // Player tap
                    Ray raycast = Camera.main.ScreenPointToRay(touch.position);
                    RaycastHit raycastHit;
                    if (Physics.Raycast(raycast, out raycastHit, Mathf.Infinity, 7))
                    {
                        if (raycastHit.collider.CompareTag("Player"))
                        {
                            tapCounter += Time.deltaTime;
                        }
                    }
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    dragStart = touch.position;

                    // Player tap -> throw item
                    if (tapCounter > minTapTime && tapCounter < maxTapTime && LogicController.PickedItems[0] != null)
                    {
                        LogicController.PickedItems[0].SetPickedUp(false);
                        LogicController.PickedItems[0].GetBody().AddRelativeForce(
                            faceDir switch
                            {
                                Direction.NORTH => new Vector3(0f, .25f * throwForce, throwForce * 1.5f),
                                Direction.SOUTH => new Vector3(0f, .25f * throwForce, -throwForce * 1.5f),
                                Direction.WEST => new Vector3(-throwForce * 1.5f, .25f * throwForce, 0f),
                                Direction.EAST => new Vector3(throwForce * 1.5f, .25f * throwForce, 0f),
                                _ => new Vector3(0f, 0f, 0f)
                            },
                            ForceMode.Impulse);
                        LogicController.PickedItems[0] = null;

                        // Try to rotate items for better UX
                        logic.SwitchItems();
                        if (LogicController.PickedItems[0] == null)
                        {
                            logic.SwitchItems();
                        }
                    }
                    tapCounter = 0f;
                }
                break;
            }

            if (angle != -1f)
            {
                visualObj.transform.rotation = Quaternion.Euler(0f, angle, 0f);
                isMoving = true;
            }
        }
        else
        {
            moveDir = Direction.STATIONARY;
        }

        anim.SetBool(walkParam, isMoving);

        mainCamera.transform.position = visualObj.transform.position + cameraOffset;

        if (Input.GetKey(KeyCode.Escape))
        {
            data.Autosave();
            Application.Quit();
        }
    }

    private void MoveCharacter(float speed, Direction dir)
    {
        ForceMode fm = ForceMode.Force;
        if (body.velocity.sqrMagnitude < maxSpeed * maxSpeed)
        {
            switch (dir)
            {
                case Direction.NORTH:
                    body.AddForce(new Vector3(0f, 0f, speed), fm);
                    break;
                case Direction.SOUTH:
                    body.AddForce(new Vector3(0f, 0f, -speed), fm);
                    break;
                case Direction.WEST:
                    body.AddForce(new Vector3(-speed, 0f, 0f), fm);
                    break;
                case Direction.EAST:
                    body.AddForce(new Vector3(speed, 0f, 0f), fm);
                    break;
                default:
                    break;
            }
        }
    }

    enum Direction
    {
        NORTH,
        SOUTH,
        WEST,
        EAST,
        STATIONARY
    }
}
