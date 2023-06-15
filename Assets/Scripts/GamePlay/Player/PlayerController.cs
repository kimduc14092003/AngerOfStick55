using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float speed, jumpingPower, delayTimeOfCombo;
    [SerializeField] private Transform groundCheck, headCheck;
    [SerializeField] private LayerMask groundLayer, climbPointLayer, enemyLayer;
    [SerializeField] private int jumpMax, jumpCount;
    [SerializeField] private Vector2 offset1;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private Transform bodyTransform;

    [Header("Spine Animation")]
    [SpineAnimation]
    public string idleAnim;
    [SpineAnimation]
    public string runAnim, jumpAnim, hitAnim, deadAnim,
                jumpKick1Anim, jumpKick2Anim, skillAnim, wrestleAnim;
    [SpineAnimation] public string[] kickComboAnim, punchComboAnim, trampleComboAnim;

    private float horizontalValue, tempTime;
    private bool isFacingRight;
    private bool isFalling = false;
    private bool isClimb, isBow, isJump;

    [SerializeField] private CapsuleCollider2D capsuleCollider;
    [SerializeField] private LedgeDetection ledgeDetection;
    [SerializeField] private GameObject[] attackColliders;

    private List<float> listDeltaTimeInAnim;

    Spine.EventData eventData;
    public string eventName;

    private void Start()
    {
        eventData = skeletonAnimation.Skeleton.Data.FindEvent(eventName);
        skeletonAnimation.AnimationState.Event += HandleAnimationStateEvent;
    }


    private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
    {
        Debug.Log("Event fired! " + e.Data.Name);
        //bool eventMatch = string.Equals(e.Data.Name, eventName, System.StringComparison.Ordinal); // Testing recommendation: String compare.
        bool eventMatch = (eventData == e.Data); // Performance recommendation: Match cached reference instead of string.
        if (eventMatch)
        {
            Debug.Log("Run Event Here!");
        }
    }

    private void Update()
    {
        HandlePlayerInput();
    }

    private void HandlePlayerInput()
    {
        // Xử lý di chuyển trái phải 
        horizontalValue = Input.GetAxisRaw("Horizontal");
        if (horizontalValue > 0)
        {
            //StopBow();
            isFacingRight = true;

        }
        else
        if (horizontalValue < 0)
        {
            //StopBow();
            isFacingRight = false;

        }

        // Xử lý tấn công 1
        if (Input.GetKeyDown(KeyCode.U) && !isJump)
        {
            /*isAttackKick0 = true;
            isAttackKick1 = false;
            isAttackPunch = false;*/
            tempTime = Time.time;
        }
        // Xử lý tấn công 2
        if (Input.GetKeyDown(KeyCode.J) && !isJump)
        {
            /*isAttackPunch = true;
            isAttackKick1 = false;
            isAttackKick0 = false;*/
            tempTime = Time.time;
        }
        // Xử lý tấn công 3
        if (Input.GetKeyDown(KeyCode.K) && !isJump)
        {
           /* isAttackKick1 = true;
            isAttackPunch = false;
            isAttackKick0 = false;*/
            tempTime = Time.time;
        }
        // Xử lý sự kiện nhảy của Player
        if (Input.GetButtonDown("Jump"))
        {
           /* StopBow();
            StartJump();*/
        }

        if (rb.velocity.y > 0)
        {
            isJump = true;
        }
        // Xử lý sự kiện nhảy đá 1
        if (isJump && Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("Jump Kick 1");
        }
        // Xử lý sự kiện nhảy đá 2
        if (isJump && Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Jump Kick 2");
        }
        // Xử lý khi người chơi đang leo
        if (isClimb)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                isClimb = false;
                isFalling = true;
                Invoke("FallingDone", 0.25f);
            }
            else
            if (Input.GetKeyDown(KeyCode.W))
            {
                isClimb = false;
                if (isFacingRight)
                {
                    Vector3 correctOffset = new Vector3(offset1.x, offset1.y);
                    transform.position = transform.position + (Vector3)correctOffset;
                }
                else
                {
                    Vector3 correctOffset = new Vector3(-offset1.x, offset1.y);
                    transform.position = transform.position + (Vector3)correctOffset;
                }
            }
        }
        //else
        // Xử lý cúi người
        /*if (!isClimb)
        {
            if (Input.GetKeyDown(KeyCode.S)&&!isBow)
            {
                isBow = true;
                capsuleCollider.offset = capsuleCollider.offset * 0.5f;
                capsuleCollider.size = new Vector2( capsuleCollider.size.x,capsuleCollider.size.y*0.5f);
            }
        }*/
    }

}
