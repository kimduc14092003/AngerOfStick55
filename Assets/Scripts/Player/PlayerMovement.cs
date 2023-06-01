using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float horizontalValue;
    private bool isFacingRight;
    [SerializeField] private float speed,jumpingPower;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck,headCheck;
    [SerializeField] private LayerMask groundLayer,climbPointLayer;
    [SerializeField] private int jumpMax, jumpCount;
    [SerializeField] private Vector2 offset1; 
    private bool isClimb,isBow;
    private bool isFalling=false;

    private CapsuleCollider2D capsuleCollider;
    [SerializeField] private LedgeDetection ledgeDetection;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }
    private void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        isBow = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandlePlayerInput();
        FlipPlayer();
        HandleCharacterClimb();

        if (IsCharacterClimb())
        {
            jumpCount = jumpMax;
        }
    }

    public bool IsCharacterClimb()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
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
        // Xử lý sự kiện nhảy của Player
        if (Input.GetButtonDown("Jump"))
        {
            StopBow();
            if (jumpCount > 0)
            {
                jumpCount--;
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            }
        }

    /*    if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f*Time.deltaTime);
        }
*/
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

    private void HandleCharacterClimb()
    {
        SetIsClimbValue(ledgeDetection.IsCharacterClimb());

        if (!isClimb)
        {
            rb.velocity = new Vector2(horizontalValue * speed*Time.deltaTime, rb.velocity.y);
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


    private void OnCollisionEnter2D(Collision2D collision)
    {

        /*for (int i = 0; i < 32; i++) // 32 là số lượng tối đa các layer trong Unity
        {
            if ((groundLayer.value & (1 << i)) != 0)
            {
                if (collision.gameObject.layer == i)
                {
                    //jumpCount = jumpMax;
                    break;
                }
            }
        }*/

    }

}
