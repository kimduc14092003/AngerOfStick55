using DG.Tweening;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Animation = Spine.Animation;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float speed,jumpingPower, delayTimeOfCombo, currentHealth, maxHealth;
    [SerializeField] private Transform groundCheck,headCheck;
    [SerializeField] private LayerMask groundLayer,climbPointLayer,enemyLayer;
    [SerializeField] private int jumpMax, jumpCount;
    [SerializeField] private Vector2 offset1,offsetBeginClimb;
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private Transform bodyTransform;

    [SerializeField] private SpineAnimationData spineAnimationData;
    [Header("Spine Animation")]
    [SpineAnimation]
    public string idleAnim;
    [SpineAnimation] public string runAnim,jumpAnim,hitAnim,deadAnim,jumpKick1Anim,jumpKick2Anim,skillAnim,wrestleAnim,
                                    climbAnim,climbUpAnim,climbDownAnim,crouchAnim;
    [SpineAnimation] public string[] kickComboAnim, punchComboAnim, trampleComboAnim,skillComboAnim;

    private float horizontalValue,tempTime, flyAwayTime, knockBackTime;
    private bool isFacingRight, isDead, isHit;
    private bool isClimb,isBow,isJump, isAttackKick0 = false, isTrample = false, isAttackPunch = false, 
        isThrowEnemy,isThrowOnce=true, isSkill=false,isAttackAnim=false;
    private bool isFalling=false,isSetBeginClimbPos=false;

    [SerializeField] private CapsuleCollider2D capsuleCollider;
    [SerializeField] private LedgeDetection ledgeDetection;
    [SerializeField] private GameObject[] attackColliders;
    [SerializeField] private GameObject explosiveGameObject;
    public Spine.Animation TargetAnimation { get; private set; }

    private int currentIndexAttackCombo;
    private string currentAnim;
    public float skillPowerJump,skillPowerForce;
    private Coroutine stopHitStateCoroutine, hitDoneCoroutine;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        isBow = false;
        isFacingRight = true;
        currentIndexAttackCombo = -1;
        skeletonAnimation.state.Event += HandleEvent;
        skeletonAnimation.AnimationState.End += OnEndAnim;
        skeletonAnimation.AnimationState.Complete += OnCompleteAnim;
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
    }

    private void GetCurrentAnimation()
    {
        if (currentAnim != GetCurrentAnimation(0).Name)
        {
            //Debug.Log(currentAnim + " | " + GetCurrentAnimation(0).Name);
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
        return false;
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
            if (!isAttackKick0 && !isAttackPunch && !isTrample)
            {
                isFacingRight = true;
            }
            
        }
        else
        if(horizontalValue < 0)
        {
            StopBow();
            if (!isAttackKick0 && !isAttackPunch && !isTrample)
            {
                isFacingRight = false;
            }
        }

        // Xử lý tấn công 1
        if (Input.GetKeyDown(KeyCode.U)&&!isJump && !isSkill)
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
        if (Input.GetKeyDown(KeyCode.J) && !isJump && !isSkill)
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
        if (Input.GetKeyDown(KeyCode.K) && !isJump && !isSkill)
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
        if (Input.GetButtonDown("Jump") && !isSkill)
        {
            StopBow();
            StartJump();
        }

        if (rb.velocity.y > 0)
        {
            isJump = true;
        }
        // Xử lý sự kiện nhảy đá 1
        if(isJump&& Input.GetKeyDown(KeyCode.J) && !isSkill)
        {
            if(jumpCount > 0&&!isAttackPunch)
            {
                skeletonAnimation.AnimationState.SetAnimation(0,jumpKick1Anim,false);
                skeletonAnimation.AnimationState.AddAnimation(0, idleAnim, true,0.4f);
                jumpCount--;
            }
        }
        // Xử lý sự kiện nhảy đá 2
        if (isJump && Input.GetKeyDown(KeyCode.K) && !isSkill)
        {
            if(jumpCount>0)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, jumpKick2Anim, false);
                skeletonAnimation.AnimationState.AddAnimation(0, idleAnim, true, 0.4f);
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
                skeletonAnimation.AnimationState.SetAnimation(0,climbUpAnim,false);
                StartCoroutine(SetNewPosClimbUp(0.1f));
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

    IEnumerator SetNewPosClimbUp(float time)
    {
        yield return 0;
        yield return 0;
        yield return 0;
        yield return 0;
        yield return 0;
        yield return 0;
        transform.position = transform.position + Vector3.up*offsetBeginClimb.y;
        //yield return new WaitForSeconds(time);
      
    }

    private void OnEndAnim(TrackEntry trackEntry)
    {

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
        if (trackEntry.ToString() == climbUpAnim)
        {
            HandlePlayerClimbUp();
        }

        if (trackEntry.ToString() == punchComboAnim[5])
        {

        }
        if (trackEntry.ToString() == climbAnim)
        {
            Debug.Log("climb");
           
        }
    }

    private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (e.Data.Name == "hit")
        {
            // Xử lý attack collider
            switch (currentAnim)
            {
                case var value when value == kickComboAnim[0]:
                case var value1 when value1 == kickComboAnim[2]:
                case var value2 when value2 == kickComboAnim[4]:

                case var value3 when value3 == trampleComboAnim[0]:
                    {
                        KickRightHit();
                        break;
                    }
                case var value when value == kickComboAnim[1]:
                case var value1 when value1 == kickComboAnim[3]:
                case var value2 when value2 == kickComboAnim[5]:

                case var value4 when value4 == trampleComboAnim[1]:

                    {
                        KickLeftHit();
                        break;
                    }
                case var value when value == punchComboAnim[1]:
                case var value3 when value3 == punchComboAnim[2]:

                    {
                        PunchLeftHit();
                        break;
                    }
                case var value when value == punchComboAnim[0]:
                case var value1 when value1 == punchComboAnim[3]:
                case var value2 when value2 == punchComboAnim[4]:

                    {
                        PunchRightHit();
                        break;
                    }
            }
            if (currentAnim == punchComboAnim[5])
            {
                explosiveGameObject.GetComponent<ExplosiveKnockBack>().knockBackForce = spineAnimationData.punchKnockBackForce[currentIndexAttackCombo];
                explosiveGameObject.transform.position = attackColliders[1].transform.position;
                explosiveGameObject.SetActive(true);
            }
        }
        
        // Xử lý event khi animation là Combo Kick Anim
        if (kickComboAnim.Contains(currentAnim))
        {
/*            if (currentAnim == kickComboAnim[5])
            {
                rb.velocity = Vector2.right * SpineAnimationData.kickVelocity[5] * ((isFacingRight) ? 1 : -1);
            }
            else*/
            if (e.Data.Name == "begin")
            {
                rb.velocity= Vector2.right * spineAnimationData.GetKickAt(currentIndexAttackCombo) * ((isFacingRight)?1:-1);
            }
            if (e.Data.Name == "end")
            {
                //Debug.Log("Player Stop");
               rb.velocity = Vector2.zero;
            }
        }

        // Xử lý event khi animation là Combo Punch Anim
        if (punchComboAnim.Contains(currentAnim))
        {
        /*if (currentAnim == punchComboAnim[5])
            {
                rb.velocity = Vector2.right * moveAttackSpeed2 * ((isFacingRight) ? 1 : -1);
            }
            else*/
            if (e.Data.Name == "begin")
            {
                if (currentAnim == punchComboAnim[4])
                {
                    rb.velocity = new Vector2(1f * ((isFacingRight) ? 1 : -1), 1) * spineAnimationData.GetPunchAt(currentIndexAttackCombo) ;
                }
                else
                {
                    rb.velocity = Vector2.right * spineAnimationData.GetPunchAt(currentIndexAttackCombo) * ((isFacingRight) ? 1 : -1);
                }
            }
            if (e.Data.Name == "stop1")
            {
                //Debug.Log("Stop Velocity");
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0;

            }
            if (e.Data.Name == "stop2")
            {
                rb.velocity = Vector2.zero;
                rb.gravityScale = 20;

            }
            if (e.Data.Name == "end")
            {
                //Debug.Log("Player Stop " + Time.time);
                rb.velocity = Vector2.zero;
                rb.gravityScale = 1;

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
            if (e.Data.Name == "end")
            {
                rb.gravityScale = 1;

                rb.AddForce(Vector2.down * skillPowerForce);
            }

            if (e.Data.Name == "hit")
            {
                explosiveGameObject.transform.position = attackColliders[1].transform.position;
                explosiveGameObject.SetActive(true);
                Invoke("UsingSkillDone", 0.25f);
            }
        }

        // Xử lý event khi animation là climbUpAnim
        if(currentAnim== climbUpAnim)
        {
            if (e.Data.Name == "begin")
            {
                rb.AddForce(Vector2.up * offsetBeginClimb.y);
            }
            if(e.Data.Name == "end")
            {
                rb.velocity=Vector2.zero;
            }
        }
    }

    private void KickLeftHit()
    {
        attackColliders[0].SetActive(true);
        StartCoroutine(TurnOffAttackColliders(0));
    }
    private void PunchLeftHit()
    {
        attackColliders[1].SetActive(true);
        StartCoroutine(TurnOffAttackColliders(1));
    }
    private void KickRightHit()
    {
        attackColliders[2].SetActive(true);
        StartCoroutine(TurnOffAttackColliders(2));
    }

    private void PunchRightHit()
    {
        attackColliders[3].SetActive(true);
        StartCoroutine(TurnOffAttackColliders(3));
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
           // Debug.Log("Stop velocity!");
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
        if (!isAttackKick0)
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
            if(!isAttackKick0 && !isTrample && !isAttackPunch&&!isSkill)
            {
                if (horizontalValue != 0)
                {
                    //Debug.Log("Player Moving!" + isAttackKick0);
                    rb.velocity = new Vector2(horizontalValue * speed*Time.deltaTime, rb.velocity.y);
                }
                else
                {
                    //Dừng nhân vật khi người chơi không di chuyển nữa
                    //Debug.Log("Stop x velocity!");
                    rb.velocity = new Vector2(0, rb.velocity.y);
                }
                rb.gravityScale = 1;
            }

        }
        else
        {
            //Debug.Log("Stop velocity!");
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            if (isSetBeginClimbPos)
            {
                isSetBeginClimbPos= false;
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
       /* if (isThrowEnemy)
        {
            if (isThrowOnce)
            {
                transform.localScale =new Vector3(transform.localScale.x*-1, transform.localScale.y,transform.localScale.z);
                Invoke("ThrowEnemyDone", 0.8f);
                print("Throw Enemy");
                isThrowOnce = false;
            }
        }*/
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
            if (skeletonAnimation.AnimationState.ToString() != climbAnim && skeletonAnimation.AnimationState.ToString() != climbUpAnim
                && skeletonAnimation.AnimationState.ToString() != climbDownAnim)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, climbAnim, false);
                isSetBeginClimbPos = true;
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
        /*if (isThrowEnemy)
        {
            if (skeletonAnimation.AnimationState.ToString() != wrestleAnim)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, wrestleAnim, false);
            }
        }
        else*/
        if (horizontalValue != 0)
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
                currentIndexAttackCombo = i;
            }

            yield return new WaitForSeconds(duration);
            if (tempTime + duration < Time.time)
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
            currentIndexAttackCombo = i;

            yield return new WaitForSeconds(duration);
            if (tempTime + duration < Time.time)
            {
                ReturnIdleAnim();

                break;
            }
        }
        //ReturnIdleAnim();

        isAttackKick0 = false;
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

    

    private void UsingSkill()
    {
        skeletonAnimation.AnimationState.SetAnimation(0, skillAnim, false);
        rb.velocity = Vector2.zero;
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

    private IEnumerator TurnOffAttackColliders(int index)
    {
        yield return new WaitForSeconds(0.1f);
        attackColliders[index].SetActive(false);
    }

    // Xử lý khi Player nhận dame và chết
    public void HandleDameTaken(float dame, Transform transformFrom, CharacterTakeHitState state, float force)
    {
        if (isDead) return;

        if (hitDoneCoroutine != null)
        {
            StopCoroutine(hitDoneCoroutine);
        }
        isHit = true;
        this.currentHealth -= dame;

        // Khi nhận sát thương enemy sẽ không di chuyển
        rb.velocity = Vector2.zero;

        switch (state)
        {
            case CharacterTakeHitState.KnockBack:
                {
                    KnockBack(transformFrom, force);
                    hitDoneCoroutine = StartCoroutine(HitDone(knockBackTime));
                    break;
                }
            case CharacterTakeHitState.ThrowUp:
                {
                    ThrowUp(transformFrom, force);
                    hitDoneCoroutine = StartCoroutine(HitDone(flyAwayTime));
                    break;
                }
            case CharacterTakeHitState.FallDown:
                {
                    FallDown(transformFrom, force);
                    hitDoneCoroutine = StartCoroutine(HitDone(flyAwayTime));
                    break;
                }
            case CharacterTakeHitState.FlyAway:
                {
                    //Debug.Log("FlyAway: " + Time.time);
                    FlyAway(transformFrom, force);
                    hitDoneCoroutine = StartCoroutine(HitDone(flyAwayTime));
                    break;
                }
        }

        if (this.currentHealth <= 0)
        {
            //StopCoroutine(hitDoneCoroutine);
            isDead = true;
            Debug.Log("dead");
            skeletonAnimation.AnimationState.AddAnimation(0, deadAnim, false, 0.1f);
            capsuleCollider.gameObject.SetActive(false);
            //rb.gravityScale = 0;
        }

    }

    IEnumerator HitDone(float time)
    {
        yield return new WaitForSeconds(time);
        isHit = false;
    }
    private IEnumerator KnockCo()
    {
        yield return new WaitForSeconds(0.5f);
        rb.velocity = Vector2.zero;
    }

    private void KnockBack(Transform transformFrom, float force)
    {
        skeletonAnimation.AnimationState.SetAnimation(0, hitAnim, false);

        Vector2 directionForce = transform.position - transformFrom.position;
        directionForce = directionForce.normalized;
        if (directionForce.x > 0)
        {
            directionForce.x = 1;
        }
        else
        {
            directionForce.x = -1;
        }
        directionForce.y = 0;
        rb.AddForce(directionForce * force, ForceMode2D.Impulse);
        HandleStopHitState();
    }

    private void ThrowUp(Transform transformFrom, float force)
    {
        skeletonAnimation.AnimationState.SetAnimation(0, deadAnim, false);
        Vector2 directionForce = transform.position - transformFrom.position;
        directionForce = directionForce.normalized;
        if (directionForce.x > 0)
        {
            directionForce.x = 1;
        }
        else
        {
            directionForce.x = -1;
        }
        directionForce.y = 1;
        rb.AddForce(new Vector2(directionForce.x * 0.4f, directionForce.y) * force, ForceMode2D.Impulse);
        HandleStopHitState();

    }

    private void FallDown(Transform transformFrom, float force)
    {
        if (currentAnim != deadAnim)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, deadAnim, false);
        }

        rb.AddForce(Vector2.down * force, ForceMode2D.Impulse);
        HandleStopHitState();

    }
    private void FlyAway(Transform transformFrom, float force)
    {

        skeletonAnimation.AnimationState.SetAnimation(0, deadAnim, false);

        Vector2 directionForce = transform.position - transformFrom.position;
        directionForce = directionForce.normalized;
        if (directionForce.x > 0)
        {
            directionForce.x = 1;
        }
        else
        {
            directionForce.x = -1;
        }
        Debug.Log(force);
        rb.AddForce(new Vector2(directionForce.x, 0.75f) * force, ForceMode2D.Impulse);
        //HandleStopHitState();
    }

    private void HandleStopHitState()
    {
        if (stopHitStateCoroutine != null)
        {
            StopCoroutine(stopHitStateCoroutine);
        }
        stopHitStateCoroutine = StartCoroutine(KnockCo());
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            // Xử lý đẩy lùi khi đánh trúng kẻ địch
           
            EnemyController enemyController = collision.transform.parent.root.gameObject.GetComponent<EnemyController>();
            switch (currentAnim)
            {
                case var value when value == punchComboAnim[3]:
                case var value2 when value2 == kickComboAnim[4]:

                    {
                        if (punchComboAnim.Contains(currentAnim))
                        {
                            enemyController.HandleDameTaken(10, transform, CharacterTakeHitState.ThrowUp, spineAnimationData.punchKnockBackForce[currentIndexAttackCombo]);
                        }
                        else
                        if (kickComboAnim.Contains(currentAnim))
                        {
                            enemyController.HandleDameTaken(10, transform, CharacterTakeHitState.ThrowUp, spineAnimationData.kickKnockBackForce[currentIndexAttackCombo]);
                        }
                        break;
                    }
                case var value when value == punchComboAnim[4]:
                    {
                        if (punchComboAnim.Contains(currentAnim))
                        {
                            enemyController.HandleDameTaken(10, transform, CharacterTakeHitState.FallDown, spineAnimationData.punchKnockBackForce[currentIndexAttackCombo]);
                        }
                        else
                        if (kickComboAnim.Contains(currentAnim))
                        {
                            enemyController.HandleDameTaken(10, transform, CharacterTakeHitState.FallDown, spineAnimationData.kickKnockBackForce[currentIndexAttackCombo]);
                        }
                        break;
                    }
                //case var value when value == punchComboAnim[5]:
                case var value2 when value2 == kickComboAnim[5]:

                    {
                         if (punchComboAnim.Contains(currentAnim))
                        {
                            enemyController.HandleDameTaken(10, transform, CharacterTakeHitState.FlyAway, spineAnimationData.punchKnockBackForce[currentIndexAttackCombo]);
                        }
                        else
                        if (kickComboAnim.Contains(currentAnim))
                        {
                            enemyController.HandleDameTaken(10, transform, CharacterTakeHitState.FlyAway, spineAnimationData.kickKnockBackForce[currentIndexAttackCombo]);
                        }
                        break;
                    }
                default:
                    {
                         if (punchComboAnim.Contains(currentAnim))
                        {
                            enemyController.HandleDameTaken(10, transform, CharacterTakeHitState.KnockBack, spineAnimationData.punchKnockBackForce[currentIndexAttackCombo]);
                        }
                        else
                        if (kickComboAnim.Contains(currentAnim))
                        {
                            enemyController.HandleDameTaken(10, transform, CharacterTakeHitState.KnockBack, spineAnimationData.kickKnockBackForce[currentIndexAttackCombo]);
                        }
                        break;
                    }

            }
        }
    }
}

public enum CharacterState
{
    Idle,
    Run,
    Jump,
    Hit,
    Dead,
    
}
