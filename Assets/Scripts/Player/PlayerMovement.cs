using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animation = Spine.Animation;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float speed,jumpingPower, delayTimeOfCombo;
    [SerializeField] private Transform groundCheck,headCheck;
    [SerializeField] private LayerMask groundLayer,climbPointLayer,enemyLayer;
    [SerializeField] private int jumpMax, jumpCount;
    [SerializeField] private Vector2 offset1;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private Transform bodyTransform;
    [SpineAnimation] public string idleAnim,runAnim,jumpAnim,hitAnim,deadAnim,
                jumpKick1Anim,jumpKick2Anim,skillAnim,wrestleAnim,comboKick0Anim,comboKick1Anim,comboPunch;

    private float horizontalValue,tempTime;
    private bool isFacingRight;
    private bool isClimb,isBow,isJump, isAttackKick0 = false, isAttackKick1 = false, isAttackPunch = false, isThrowEnemy,isThrowOnce=true;
    private bool isFalling=false;

    [SerializeField] private CapsuleCollider2D capsuleCollider;
    [SerializeField] private LedgeDetection ledgeDetection;
    [SerializeField] private GameObject[] attackColliders;

    private List<float> listDeltaTimeInAnim;
    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        isBow = false;
        skeletonAnimation.state.Event += HitEvent;
        skeletonAnimation.AnimationState.Complete += OnEndAttackCombo;
        skeletonAnimation.AnimationState.End += OnEndAttackCombo;
        listDeltaTimeInAnim = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        HandlePlayerInput();
        StartCoroutine(FlipPlayer());
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

    private bool IsCanThrowEnemy()
    {
        return Physics2D.OverlapCircle(attackColliders[1].transform.position, 0.5f, enemyLayer);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        Gizmos.DrawWireSphere(attackColliders[1].transform.position, 0.5f);
    }

    public void ClickJ()
    {
        isAttackPunch = true;
        tempTime = Time.time;
    }

    public void ClickK()
    {
        isAttackKick1 = true;
        tempTime = Time.time;
    }

    public void ClickU()
    {
        isAttackKick0 = true;
        tempTime = Time.time;
    }

    private void HandlePlayerInput()
    {
        // Xử lý di chuyển trái phải 
        horizontalValue = Input.GetAxisRaw("Horizontal");
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
        if (Input.GetKeyDown(KeyCode.U)&&!isJump )
        {
            isAttackKick0 = true;
            isAttackKick1 = false;
            isAttackPunch = false;
            tempTime = Time.time;
        }
        // Xử lý tấn công 2
        if (Input.GetKeyDown(KeyCode.J) && !isJump)
        {
            isAttackPunch = true;
            isAttackKick1 = false;
            isAttackKick0 = false;
            tempTime = Time.time;
        }
        // Xử lý tấn công 3
        if (Input.GetKeyDown(KeyCode.K) && !isJump)
        {
            isAttackKick1 = true;
            isAttackPunch = false;
            isAttackKick0 = false;
            tempTime = Time.time;
        }
        // Xử lý sự kiện nhảy của Player
        if (Input.GetButtonDown("Jump"))
        {
            StopBow();
            StartJump();
        }

        if (rb.velocity.y > 0)
        {
            isJump = true;
        }
        // Xử lý sự kiện nhảy đá 1
        if(isJump&& Input.GetKeyDown(KeyCode.J))
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

    private void OnEndAttackCombo(TrackEntry trackEntry)
    {
        string[] listAttackEntry = new string[] {comboKick0Anim,comboKick1Anim,comboPunch };
        for(int i = 0; i < listAttackEntry.Length; i++)
        {
            if (listAttackEntry[i] == trackEntry.ToString())
            {
                StartCoroutine(MoveToNewPos());
                Debug.Log("End Anim"+trackEntry);

            }
        }
    }

    private IEnumerator MoveToNewPos()
    {
        Vector3 target = new(bodyTransform.position.x, transform.position.y, transform.position.z);
        CheckAndStopAllCombo();
        //10 điểm cho việc chờ 1 frame
        yield return 0;
        transform.position = target;
    }

    private void CheckAndStopAllCombo()
    {
        if (isAttackKick0 || isAttackKick1 || isAttackPunch)
        {
            isAttackKick0 = false;
            isAttackKick1 = false;
            isAttackPunch = false;
        }
       
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
        /*if (isBow)
        {
            isBow = false;
            capsuleCollider.offset = capsuleCollider.offset * 2;
            capsuleCollider.size = new Vector2(capsuleCollider.size.x, capsuleCollider.size.y * 2);
        }*/
    }

    private IEnumerator FlipPlayer()
    {
        if(isFacingRight)
        {
            if(transform.rotation!=Quaternion.Euler(0f, 0f, 0f))
            {
                CheckAndStopAllCombo();
                StartCoroutine (MoveToNewPos());
                isThrowEnemy= IsCanThrowEnemy();
                yield return 0;
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                for (int i = 0; i < attackColliders.Length; i++)
                {
                    attackColliders[i].SetActive(true);
                    StartCoroutine(TurnOffAttackColliders(i));
                }
                ThrowEnemy();
            }
        }
        else
        {
            if (transform.rotation.y != -1&& transform.rotation.y != 1)
            {
                CheckAndStopAllCombo();
                StartCoroutine(MoveToNewPos());
                isThrowEnemy = IsCanThrowEnemy();
                yield return 0;
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                for (int i = 0; i < attackColliders.Length; i++)
                {
                    attackColliders[i].SetActive(true);
                    StartCoroutine(TurnOffAttackColliders(i));
                }
                ThrowEnemy();
            }
        }
        yield return null;
    }

    private void HandleCharacterClimbAndMove()
    {
        SetIsClimbValue(ledgeDetection.IsCharacterClimb());

        if (!isClimb&&!isThrowEnemy)
        {
            //Xử lý di chuyển trái phải cho player 
            if (horizontalValue != 0&& !isAttackKick0 && !isAttackKick1 && !isAttackPunch )
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

    private void ThrowEnemy()
    {
        if (isThrowEnemy)
        {
            if (isThrowOnce)
            {
                transform.localScale =new Vector3(transform.localScale.x*-1, transform.localScale.y,transform.localScale.z);
                Invoke("ThrowEnemyDone", 0.8f);
                print("Throw Enemy");
                isThrowOnce = false;
            }
        }
    }
    private void ThrowEnemyDone()
    {
        isThrowEnemy=false;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        isThrowOnce=true;
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
        if (isThrowEnemy)
        {
            if (skeletonAnimation.AnimationState.ToString() != wrestleAnim)
            {
                print(skeletonAnimation.AnimationState.ToString());
                skeletonAnimation.AnimationState.SetAnimation(0, wrestleAnim, false);
            }
        }
        else
        if (isAttackKick0)
        {
            if (skeletonAnimation.AnimationState.ToString() != comboKick0Anim)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, comboKick0Anim, false);

            }
        }
        else
        if (isAttackKick1)
        {
            if (skeletonAnimation.AnimationState.ToString() != comboKick1Anim)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, comboKick1Anim, false);
            }
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

        GetListDeltaTimeOfEvent(skeletonAnimation.AnimationState.ToString());
    }

    private void HitEvent(TrackEntry trackEntry, Spine.Event e)
    {
        for(int i = 0; i < attackColliders.Length; i++)
        {
            attackColliders[i].SetActive(true);
            StartCoroutine(TurnOffAttackColliders(i));
        }
        if (e.Data.Name == "hit")
        {
            float deltaTime = 0;
            for (int i=0;i<listDeltaTimeInAnim.Count;i++)
            {
                if (e.Time == listDeltaTimeInAnim[i])
                {
                    try
                    {
                        deltaTime = listDeltaTimeInAnim[i] - listDeltaTimeInAnim[i-1];

                    }
                    catch
                    {
                        deltaTime =
                        listDeltaTimeInAnim[i];
                    }
                    break;
                }
            }
            if (tempTime+ deltaTime < Time.time)
            {
                OnEndAttackCombo(trackEntry);
            }
        }
    }
    public void GetListDeltaTimeOfEvent(string animationName)
    {
        listDeltaTimeInAnim.Clear();
        var skeletonData = skeletonAnimation.Skeleton.Data;
        var animation = skeletonData.FindAnimation(animationName);

        foreach (var timeline in animation.Timelines)
        {
            var eventTimeline = timeline as Spine.EventTimeline;
            if (eventTimeline != null)
            {
                foreach (var spineEvent in eventTimeline.Events)
                {
                    listDeltaTimeInAnim.Add(spineEvent.Time);
                }
            }
        }

    }

    private IEnumerator TurnOffAttackColliders(int index)
    {
        yield return new WaitForSeconds(0.1f);
        attackColliders[index].SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
           // isThrowEnemy = true;
        }
    }

}
