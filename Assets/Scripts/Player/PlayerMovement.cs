using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float speed,jumpingPower, delayTimeOfCombo;
    [SerializeField] private Transform groundCheck,headCheck;
    [SerializeField] private LayerMask groundLayer,climbPointLayer;
    [SerializeField] private int jumpMax, jumpCount;
    [SerializeField] private Vector2 offset1;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private Transform bodyTransform;
    [SpineAnimation] public string idleAnim,runAnim,jumpAnim,hitAnim,deadAnim,
                jumpKick1Anim,jumpKick2Anim,skillAnim,wrestleAnim,comboKick0Anim,comboKick1Anim,comboPunch;

    private float horizontalValue,tempTime;
    private bool isFacingRight;
    private bool isClimb,isBow,isJump, isAttackKick0 = false, isAttackKick1 = false, isAttackPunch = false;
    private bool isFalling=false;

    private CapsuleCollider2D capsuleCollider;
    [SerializeField] private LedgeDetection ledgeDetection;

    private void Awake()
    {
    }
    private void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        isBow = false;
        skeletonAnimation.state.Event += HitEvent;
        skeletonAnimation.AnimationState.Complete += OnEndAttackCombo;

    }

    // Update is called once per frame
    void Update()
    {
        HandlePlayerInput();
        FlipPlayer();
        HandleCharacterClimbAndMove();
        HandlePlayerAnimation();
        IsCharacterOnGround();
    }

    public void IsCharacterOnGround()
    {
        if( Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer))
        {
            isJump = false;
            jumpCount = jumpMax;
        };
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
    }

    private void HandlePlayerInput()
    {
        // Xử lý di chuyển trái phải 
        horizontalValue = Input.GetAxis("Horizontal");
        if (horizontalValue > 0)
        {
            StopBow();
            isFacingRight = true;
            
        }
        else
        if(horizontalValue < 0)
        {
            StopBow();
            isFacingRight = false;
            
        }

        // Xử lý tấn công 1
        if (Input.GetKeyDown(KeyCode.K) )
        {
            isAttackKick0 = true;
            tempTime = Time.time;
        }
        // Xử lý tấn công 2
        if (Input.GetKeyDown(KeyCode.J))
        {
            isAttackPunch = true;
            tempTime = Time.time;
        }
        // Xử lý tấn công 3
        if (Input.GetKeyDown(KeyCode.U))
        {
            isAttackKick1 = true;
            tempTime = Time.time;
        }
        // Xử lý sự kiện nhảy của Player
        if (Input.GetButtonDown("Jump"))
        {
            StopBow();
            Invoke("StartJump", 0.3f);
        }

        if (rb.velocity.y > 0)
        {
            isJump = true;
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
                    Vector3 correctOffset = new Vector3(offset1.x,offset1.y);
                    transform.position = transform.position + (Vector3)correctOffset;
                }
                else
                {
                    Vector3 correctOffset = new Vector3(-offset1.x, offset1.y);
                    transform.position = transform.position + (Vector3)correctOffset;
                }
            }
        }
        else
        // Xử lý cúi người
        if (!isClimb)
        {
            if (Input.GetKeyDown(KeyCode.S)&&!isBow)
            {
                isBow = true;
                capsuleCollider.offset += Vector2.up * -0.5f;
                capsuleCollider.size += Vector2.up * -1f;
            }
        }
    }

    private void OnEndAttackCombo(TrackEntry trackEntry)
    {
        transform.position = new Vector3(bodyTransform.position.x, transform.position.y);
        isAttackKick0 = false;
        isAttackKick1 = false;
        isAttackPunch = false;
    }
    private void StartJump()
    {
        if (jumpCount > 0)
        {
            jumpCount--;
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }
    }

    private void StopBow()
    {
        if (isBow)
        {
            isBow = false;
            capsuleCollider.offset += Vector2.up * 0.5f;
            capsuleCollider.size += Vector2.up * 1f;
        }
    }

    private void FlipPlayer()
    {
        if(isFacingRight)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    private void HandleCharacterClimbAndMove()
    {
        SetIsClimbValue(ledgeDetection.IsCharacterClimb());

        if (!isClimb)
        {
            //Xử lý di chuyển trái phải cho player 
            if (horizontalValue != 0)
            {
                rb.velocity = new Vector2(horizontalValue * speed*Time.deltaTime, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            rb.gravityScale = 1;
        }
        else
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
        }
    }

    private void FallingDone()
    {
        isFalling = false;
    }



    private void SetIsClimbValue(bool value)
    {
        if (!isFalling)
        {
            isClimb = value;
        }
    }

    private void HandlePlayerAnimation()
    {
        if (isJump)
        {
            if (skeletonAnimation.AnimationState.ToString() != jumpAnim)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, jumpAnim, false);
            };
        }
        else
        if (isAttackKick0)
        {
            if (skeletonAnimation.AnimationState.ToString() != comboKick0Anim)
                skeletonAnimation.AnimationState.SetAnimation(0, comboKick0Anim, false);
        }
        else
        if (isAttackKick1)
        {
            if (skeletonAnimation.AnimationState.ToString() != comboKick1Anim)
                skeletonAnimation.AnimationState.SetAnimation(0, comboKick1Anim, false);
        }
        else
        if (isAttackPunch)
        {
            if (skeletonAnimation.AnimationState.ToString() != comboPunch)
                skeletonAnimation.AnimationState.SetAnimation(0, comboPunch, false);
        }
        else
        if (horizontalValue != 0)
        {
            if (skeletonAnimation.AnimationState.ToString() != runAnim)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, runAnim, true);
            };
        }

        else
        {
            if (skeletonAnimation.AnimationState.ToString() != idleAnim)
                skeletonAnimation.AnimationState.SetAnimation(0, idleAnim, true);
        }
    }

    private void HitEvent(TrackEntry trackEntry, Spine.Event e)
    {
        Debug.Log(e.Data);
        if (tempTime+delayTimeOfCombo<Time.time)
        {
            skeletonAnimation.AnimationState.SetEmptyAnimation(0, 0f);
            OnEndAttackCombo(new TrackEntry());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
    }

}
