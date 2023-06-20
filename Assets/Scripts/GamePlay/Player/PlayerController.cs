using DG.Tweening;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Animation = Spine.Animation;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float speed,jumpingPower, delayTimeOfCombo,moveAttackSpeed,moveAttackSpeed2;
    [SerializeField] private Transform groundCheck,headCheck;
    [SerializeField] private LayerMask groundLayer,climbPointLayer,enemyLayer;
    [SerializeField] private int jumpMax, jumpCount;
    [SerializeField] private Vector2 offset1,offsetBeginClimb;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private Transform bodyTransform;

    [Header("Spine Animation")]
    [SpineAnimation]
    public string idleAnim;
    [SpineAnimation] public string runAnim,jumpAnim,hitAnim,deadAnim,jumpKick1Anim,jumpKick2Anim,skillAnim,wrestleAnim,comboKick0Anim,
                                    comboKick1Anim,comboPunch,climpAnim,climpUpAnim,climpDownAnim,crouchAnim;
    [SpineAnimation] public string[] kickComboAnim, punchComboAnim, trampleComboAnim,skillComboAnim;

    private float horizontalValue,tempTime;
    private bool isFacingRight;
    private bool isClimb,isBow,isJump, isAttackKick0 = false, isTrample = false, isAttackPunch = false, 
        isThrowEnemy,isThrowOnce=true, isSkill=false,isAttackAnim=false;
    private bool isFalling=false,isSetBeginClimpPos=false,isDelayForCombo=false;

    [SerializeField] private CapsuleCollider2D capsuleCollider;
    [SerializeField] private LedgeDetection ledgeDetection;
    [SerializeField] private GameObject[] attackColliders;
    public Spine.Animation TargetAnimation { get; private set; }

    private List<float> listDeltaTimeInAnim;
    private int currentIndexAttackCombo;
    private string currentAnim;
    public float skillPowerJump,skillPowerForce;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        isBow = false;
        isFacingRight = true;
        currentIndexAttackCombo = -1;
        skeletonAnimation.state.Event += HandleEvent;
        skeletonAnimation.AnimationState.Complete += OnEndAnim;
        skeletonAnimation.AnimationState.End += OnCompleteAnim;
        listDeltaTimeInAnim = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        HandlePlayerInput();
        StartCoroutine(FlipPlayer());
        HandleCharacterClimbAndMove();
        HandlePlayerBasicAnimation();
        IsCharacterOnGround();
        GetCurrentAnimation();

        if (isAttackKick0)
        {
            isDelayForCombo = true;
        }
    }

    private void GetCurrentAnimation()
    {
        if (currentAnim != GetCurrentAnimation(0).Name)
        {
           // Debug.Log(currentAnim + " | " + GetCurrentAnimation(0).Name);
            currentAnim = GetCurrentAnimation(0).Name;
        }
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
        /*Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        Gizmos.DrawWireSphere(attackColliders[1].transform.position, 0.5f);*/
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
            if (!isTrample)
            {
                CheckAndStopAllCombo();
                StopAllCoroutines();
                StartCoroutine(TrampledAttackCombo());
            }
            tempTime = Time.time;
        }
        // Xử lý tấn công 2
        if (Input.GetKeyDown(KeyCode.J) && !isJump)
        {
            if (!isAttackPunch)
            {
                CheckAndStopAllCombo();
                StopAllCoroutines();

                StartCoroutine(PunchAttackCombo());
            }
            tempTime = Time.time;
        }
        // Xử lý tấn công 3
        if (Input.GetKeyDown(KeyCode.K) && !isJump)
        {
            if (!isAttackKick0)
            {
                CheckAndStopAllCombo();
                StopAllCoroutines();

                StartCoroutine(KickAttackCombo());
            }
            tempTime = Time.time;
        }
        // Xử lý sự kiện kĩ năng của Player
        if (Input.GetKeyDown(KeyCode.I) && !isJump)
        {
            if (!isSkill)
            {
                isSkill = true;
                UsingSkill();
            }
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
            if(jumpCount > 0)
            {
                skeletonAnimation.AnimationState.SetAnimation(0,jumpKick1Anim,false);
                skeletonAnimation.AnimationState.AddAnimation(0, idleAnim, true,0.2f);
                jumpCount--;
            }
        }
        // Xử lý sự kiện nhảy đá 2
        if (isJump && Input.GetKeyDown(KeyCode.K))
        {
            if(jumpCount>0)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, jumpKick2Anim, false);
                skeletonAnimation.AnimationState.AddAnimation(0, idleAnim, true, 0.2f);
                jumpCount--;
            }
        }
        // Xử lý khi người chơi đang leo
        if (isClimb)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                isClimb = false;
                isFalling = true;
                Invoke("FallingDone", 0.25f);
                SetIsClimbValue(false);
                ReturnIdleAnim();

            }
            else
            if (Input.GetKeyDown(KeyCode.W))
            {
                skeletonAnimation.AnimationState.SetAnimation(0,climpUpAnim,false);
                StartCoroutine(SetNewPosClimpUp(0.1f));
            }
        }
        // Xử lý cúi người
        else
        {
            if (Input.GetKeyDown(KeyCode.S) && !isBow)
            {
                isBow = true;
                capsuleCollider.offset = capsuleCollider.offset * 0.5f;
                capsuleCollider.size = new Vector2(capsuleCollider.size.x, capsuleCollider.size.y * 2/3);
            }
        }
    }

    IEnumerator SetNewPosClimpUp(float time)
    {
        yield return 0;
        yield return 0;
        yield return 0;
        yield return 0;
        yield return 0;
        yield return 0;
        yield return 0;
        transform.position = transform.position + Vector3.up*offsetBeginClimb.y;
    }

    private void OnEndAnim(TrackEntry trackEntry)
    {
        if (trackEntry.ToString() == climpUpAnim)
        {
            Debug.Log("Climp Up Done");
            HandlePlayerClimbUp();
        }
        //Debug.Log("End Anim " + trackEntry);
        // 
        /*if()
        PlayAnimation(idleAnim, 0);*/
        /*string[] listAttackEntry = new string[] {comboKick0Anim,comboKick1Anim,comboPunch };
        for(int i = 0; i < listAttackEntry.Length; i++)
        {
            if (listAttackEntry[i] == trackEntry.ToString())
            {
                StartCoroutine(MoveToNewPos());

            }
        }*/

        // Test Skill
        /*
        if (trackEntry.ToString() == skillComboAnim[0])
        {
            rb.AddForce(Vector2.up * jumpingPower);
            skeletonAnimation.AnimationState.SetAnimation(0, skillComboAnim[1], false);

        }
        if (trackEntry.ToString() == skillComboAnim[2])
        {
            TestFall();
        }
        if (trackEntry.ToString() == skillComboAnim[3])
        {
            //skeletonAnimation.AnimationState.SetAnimation(0, skillComboAnim[4], false);
        }

        if (trackEntry.ToString() == skillComboAnim[4])
        {
        }*/


        /*if (trackEntry.ToString() == skillComboAnim[1])
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
        }*/
    }

    private void HandlePlayerClimbUp()
    {
        isClimb = false;
        /*if (isFacingRight)
        {
            Vector3 correctOffset = new Vector3(offset1.x, offset1.y);
            transform.position = transform.position + (Vector3)correctOffset;
        }
        else
        {
            Vector3 correctOffset = new Vector3(-offset1.x, offset1.y);
            transform.position = transform.position + (Vector3)correctOffset;
        }*/
    }

    private void OnCompleteAnim(TrackEntry trackEntry)
    {
       

    }

    private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
    {
        for (int i = 0; i < attackColliders.Length; i++)
        {
            attackColliders[i].SetActive(true);
            StartCoroutine(TurnOffAttackColliders(i));
        }
        if (e.Data.Name == "hit")
        {
            float deltaTime = 0;
            for (int i = 0; i < listDeltaTimeInAnim.Count; i++)
            {
                if (e.Time == listDeltaTimeInAnim[i])
                {
                    try
                    {
                        deltaTime = listDeltaTimeInAnim[i] - listDeltaTimeInAnim[i - 1];

                    }
                    catch
                    {
                        deltaTime =
                        listDeltaTimeInAnim[i];
                    }
                    break;
                }
            }
            if (tempTime + deltaTime < Time.time)
            {
                //OnEndAttackCombo(trackEntry);
            }
        }

        // Xử lý event khi animation là Combo Kick Anim
        if (kickComboAnim.Contains(currentAnim))
        {
            if (currentAnim == kickComboAnim[5])
            {
                rb.velocity = Vector2.right * moveAttackSpeed2 * ((isFacingRight) ? 1 : -1);
            }
            else
            if (e.Data.Name == "begin")
            {
                Debug.Log("Player Move");
                rb.velocity= Vector2.right * moveAttackSpeed*((isFacingRight)?1:-1);
            }
            if (e.Data.Name == "end")
            {
                Debug.Log("Player Stop");
               rb.velocity = Vector2.zero;
            }
        }

        // Xử lý event khi animation là skillAnim
        if (currentAnim == skillAnim)
        {

            if (e.Data.Name == "begin")
            {
                rb.AddForce(Vector2.up * skillPowerJump);
            }
            if (e.Data.Name == "stop1")
            {
                Debug.Log("Stop Velocity");
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0;

            }
            if (e.Data.Name == "hit")
            {
                rb.gravityScale = 1;

                rb.AddForce(Vector2.down * skillPowerForce);
            }

            if (e.Data.Name == "hit")
            {
                Invoke("UsingSkillDone", 0.25f);
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
        if (isAttackKick0 || isTrample || isAttackPunch)
        {
            isAttackKick0 = false;
            isTrample = false;
            isAttackPunch = false;
        }
            Debug.Log("Stop velocity!");
            rb.velocity = Vector2.zero;
       
    }

    private void StartJump()
    {
        if (jumpCount > 0)
        {
            jumpCount--;
            rb.velocity=(Vector2.up*jumpingPower);
            skeletonAnimation.AnimationState.SetAnimation(0, jumpAnim, false);
        }
    }

    private void StopBow()
    {
        if (isBow)
        {
            isBow = false;
            capsuleCollider.offset = capsuleCollider.offset * 2;
            capsuleCollider.size = new Vector2(capsuleCollider.size.x, capsuleCollider.size.y * 3/2);
        }
    }

    private IEnumerator FlipPlayer()
    {
        if(isFacingRight)
        {
            if(transform.rotation!=Quaternion.Euler(0f, 0f, 0f))
            {
                isClimb = false;
                //CheckAndStopAllCombo();
                isThrowEnemy= IsCanThrowEnemy();
                yield return 0;
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                SetIsClimbValue(ledgeDetection.GetColliderCharacterClimb());

                /* for (int i = 0; i < attackColliders.Length; i++)
                {
                    attackColliders[i].SetActive(true);
                    StartCoroutine(TurnOffAttackColliders(i));
                }*/
                ReturnIdleAnim();

                ThrowEnemy();
            }
        }
        else
        {
            if (transform.rotation.y != -1&& transform.rotation.y != 1)
            {
                isClimb = false;
                //CheckAndStopAllCombo();
                isThrowEnemy = IsCanThrowEnemy();
                yield return 0;
                ReturnIdleAnim();
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                SetIsClimbValue(ledgeDetection.GetColliderCharacterClimb());
                /*                for (int i = 0; i < attackColliders.Length; i++)
                                {
                                    attackColliders[i].SetActive(true);
                                    StartCoroutine(TurnOffAttackColliders(i));
                                }*/
                ThrowEnemy();
            }
        }
        yield return null;
    }

    private void HandleCharacterClimbAndMove()
    {
        if (!isClimb)
        {
            SetIsClimbValue(ledgeDetection.GetColliderCharacterClimb());
        }

        CapsuleCollider2D collider=GetComponent<CapsuleCollider2D>();
        collider.enabled = !isClimb;

        if (!isClimb&&!isThrowEnemy)
        {
            //Xử lý di chuyển trái phải cho player 
            if(!isAttackKick0 && !isTrample && !isAttackPunch)
            {
                if (horizontalValue != 0&&!isDelayForCombo)
                {
                    //Debug.Log("Player Moving!");
                    rb.velocity = new Vector2(horizontalValue * speed*Time.deltaTime, rb.velocity.y);
                }
                else
                {
                    //Dừng nhân vật khi không người chơi không di chuyển nữa
                    //Debug.Log("Stop x velocity!");
                    rb.velocity = new Vector2(0, rb.velocity.y);
                }
            }

            rb.gravityScale = 1;
        }
        else
        {
            //Debug.Log("Stop velocity!");
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            if (isSetBeginClimpPos)
            {
                isSetBeginClimpPos= false;
                SetBeginClimbPos();
            }
        }
    }

    private void SetBeginClimbPos()
    {
        Collider2D collider = ledgeDetection.GetColliderCharacterClimb();
        if(collider != null)
        {
            Vector3 newPos = new Vector3(collider.transform.position.x, collider.transform.position.y - offsetBeginClimb.y);
            transform.position = newPos;
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

    private void HandlePlayerBasicAnimation()
    {
        //Nếu đang sử dụng combo thì return hàm
        if (isAttackPunch||isAttackKick0||isTrample||isSkill)
        {
            return;
        }

        if (isClimb)
        {
            if (skeletonAnimation.AnimationState.ToString() != climpAnim && skeletonAnimation.AnimationState.ToString() != climpUpAnim
                && skeletonAnimation.AnimationState.ToString() != climpDownAnim)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, climpAnim, false);
                isSetBeginClimpPos = true;
            }
        }else
        if (isBow)
        {
            if (skeletonAnimation.AnimationState.ToString() != crouchAnim)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, crouchAnim, false);
            }
        }
        else
        if (isJump)
        {
            if (skeletonAnimation.AnimationState.ToString() != jumpAnim)
            {
               // skeletonAnimation.AnimationState.SetAnimation(0, jumpAnim, false);
            };
        }
        else
        if (isThrowEnemy)
        {
            if (skeletonAnimation.AnimationState.ToString() != wrestleAnim)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, wrestleAnim, false);
            }
        }
        else
        if (horizontalValue != 0&&!isDelayForCombo)
        {
            if (skeletonAnimation.AnimationState.ToString() != runAnim)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, runAnim, true);
            };
        }

        else
        {
            ReturnIdleAnim();
        }

        GetListDeltaTimeOfEvent(skeletonAnimation.AnimationState.ToString());
    }
    IEnumerator PunchAttackCombo()
    {
        isAttackPunch = true;

        for (int i = 0; i < punchComboAnim.Length; i++)
        {
            var myAnimation = skeletonAnimation.Skeleton.Data.FindAnimation(punchComboAnim[i]);
            float duration = myAnimation.Duration;
            if(currentAnim!= punchComboAnim[i])
            {
                skeletonAnimation.AnimationState.SetAnimation(0, punchComboAnim[i], false);
            }

            yield return new WaitForSeconds(duration);
            if (tempTime+duration < Time.time)
            {
                ReturnIdleAnim();
                
                break;
            }
        }
        ReturnIdleAnim();
        isAttackPunch = false;
    }

    IEnumerator KickAttackCombo()
    {
        isAttackKick0 = true;

        for (int i = 0; i < kickComboAnim.Length; i++)
        {
            var myAnimation = skeletonAnimation.Skeleton.Data.FindAnimation(kickComboAnim[i]);
            float duration = myAnimation.Duration;
            skeletonAnimation.AnimationState.SetAnimation(0, kickComboAnim[i], false);

            yield return new WaitForSeconds(duration);
            if (tempTime + duration < Time.time)
            {
                ReturnIdleAnim();

                break;
            }
        }
        ReturnIdleAnim();

        isAttackKick0 = false;
        Invoke("ComboComplete", 0.3f);
    }

    IEnumerator TrampledAttackCombo()
    {
        isTrample = true;
        
        skeletonAnimation.AnimationState.SetAnimation(0, trampleComboAnim[0], false);
        int count=1;
        var myAnimation = skeletonAnimation.Skeleton.Data.FindAnimation(trampleComboAnim[0]);
        float duration = myAnimation.Duration;
        for (int i = 0; i < count; i++)
        {
            if (isTrample)
            {
                yield return new WaitForSeconds(duration);
                skeletonAnimation.AnimationState.SetAnimation(0, trampleComboAnim[1], false);
                /*if(currentAnim== trampleComboAnim[1])
                {
                    skeletonAnimation.AnimationState.AddAnimation(0, trampleComboAnim[1], false,0);
                }
                else
                {
                }*/

                if (tempTime + duration < Time.time)
                {
                    ReturnIdleAnim();
                    break;
                }
                else
                {
                    count++;
                }
            }
        }

        isTrample = false;
    }

    private void ComboComplete()
    {
        isDelayForCombo = false;
    }

    private void UsingSkill()
    {
        skeletonAnimation.AnimationState.SetAnimation(0, skillAnim, false);
    }

    private void ReturnIdleAnim()
    {
        if (GetCurrentAnimation(0).Name != idleAnim)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, idleAnim, true);
        }
    }

    Spine.Animation GetCurrentAnimation(int layerIndex)
    {
        var currentTrackEntry = skeletonAnimation.AnimationState.GetCurrent(layerIndex);
        return (currentTrackEntry != null) ? currentTrackEntry.Animation : null;
    }

    
    private void UsingSkillDone()
    {
        isSkill = false;
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
