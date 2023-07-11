using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private LayerMask layerMaskLedge;
    [SerializeField] private Transform attackZoneTrasform;
    [SerializeField] private Vector3 sizeOfAttackZone;
    [SerializeField] private float speed, jumpPower, health, moveTimeMin, moveTimeMax, waitTimeMin, waitTimeMax;
    [SerializeField] private LedgeDetection ledgeDetection;
    [SerializeField] private Vector2 offset1;
    [SerializeField] private CapsuleCollider2D capsuleCollider;

    [SpineAnimation]
    public string idleAnim, runAnim, jumpAnim, hitAnim, deadAnim;
    private SkeletonAnimation skeletonAnimation;
    private Rigidbody2D rb;
    private GameObject target;

    private float tempTime;
    private bool isFindTarget;
    private bool isTargetInLeft = true;
    private bool isJumping;
    private bool isClimbing;
    private bool isDead = false;
    private bool isMoving = true;
    private bool isIdle = true;
    private string currentAnim;

    public bool isTargetInAttackZone;
    public bool isCanAttack;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        rb.velocity = Vector2.left * speed * Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.DrawWireCube(attackZoneTrasform.position, sizeOfAttackZone);
    }
    private void Update()
    {
        GetCurrentAnimation();

        if (!isFindTarget)
        {
            DetectTargetCome();
        }
        else
        {
            DetectTargetInAttackZone();
            MoveToAttackPlayer();
            FlipCharacter();
        }

        if (isJumping)
        {
            if (ledgeDetection.GetColliderCharacterClimb())
            {
                CharacterClimb();
            }
        }

    }

    private void GetCurrentAnimation()
    {
        if (currentAnim != GetCurrentAnimation(0).Name)
        {
            Debug.Log(currentAnim + " | " + GetCurrentAnimation(0).Name+": "+Time.time);
            currentAnim = GetCurrentAnimation(0).Name;
        }
    }

    Spine.Animation GetCurrentAnimation(int layerIndex)
    {
        var currentTrackEntry = skeletonAnimation.AnimationState.GetCurrent(layerIndex);
        return (currentTrackEntry != null) ? currentTrackEntry.Animation : null;
    }

    public void DetectTargetCome()
    {
        if (!isFindTarget)
        {
            Collider2D result = Physics2D.OverlapCircle(transform.position, radius, layerMaskLedge);
            if (result != null)
            {
                isFindTarget = true;
                target = result.gameObject;
            }

        }
    }
    public GameObject GetTarget()
    {
        return target;
    }

    private void DetectTargetInAttackZone()
    {
        if (isTargetInAttackZone)
        {
            bool result= Physics2D.OverlapBox(attackZoneTrasform.position, sizeOfAttackZone, 0, layerMaskLedge); 
            if (!result)
            {
                isTargetInAttackZone = result;
                isMoving = true;
                isIdle = false;
            }
        }
        else
        {
            isTargetInAttackZone = Physics2D.OverlapBox(attackZoneTrasform.position, sizeOfAttackZone, 0, layerMaskLedge);
        }
    }

    private void MoveToAttackPlayer()
    {
        if (target.transform.position.x < transform.position.x)
        {
            isTargetInLeft = true;
        }
        else
        {
            isTargetInLeft = false;
        }


        if (isTargetInAttackZone)
        {
            isMoving = false;
            isIdle = true;
            if(currentAnim!= idleAnim)
            {
                skeletonAnimation.AnimationState.SetAnimation(0, idleAnim, true);
            }
        }
        else
        {
            //moving time

            if (isTargetInLeft)
            {
                if (isMoving)
                {
                    isIdle = false;
                    //if()
                    Debug.Log("Run1");
                    if (target.transform.position.x + sizeOfAttackZone.x > transform.position.x)
                    {

                    }
                    else
                    {
                        rb.velocity = new Vector2(speed * -1f * Time.deltaTime, rb.velocity.y);

                    }

                }

            }
            else
            {
                if (isMoving)
                {
                    isIdle = false;

                    Debug.Log("Run2");
                    if (target.transform.position.x - sizeOfAttackZone.x < transform.position.x)
                    {

                    }
                    else
                    {
                        rb.velocity = new Vector2(speed * Time.deltaTime, rb.velocity.y);
                    }
                }
            }

            //wait time
            if (tempTime <= Time.time)
            {
                if (!isIdle)
                {
                    Debug.Log("idle");
                    rb.velocity = new Vector2(0, 0);
                    isIdle = true;
                    isMoving = false;
                    skeletonAnimation.AnimationState.SetAnimation(0, idleAnim, true);
                    float waitTime = Random.Range(waitTimeMin, waitTimeMax);
                    StartCoroutine(GetTempTimeInWait(waitTime));
                }
            }
        }

    }

    IEnumerator GetTempTimeInWait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        float randomNum = Random.Range(moveTimeMin, moveTimeMax);
        tempTime = Time.time + randomNum;
        isIdle = false;
        isMoving = true;
        skeletonAnimation.AnimationState.SetAnimation(0, runAnim, true);

        yield return new WaitForSeconds(randomNum);
    }

    private void FlipCharacter()
    {
        if (!isTargetInLeft)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    private void CharacterJump()
    {
        rb.velocity=new Vector2(rb.velocity.x,jumpPower);
        Invoke("JumpDone", 1.5f);
    }
    private void JumpDone()
    {
        isJumping = false;
    }
    private void CharacterClimb()
    {
        isJumping = false;
        isClimbing = true;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
        Invoke("CharacterClimbDone", 1f);
    }

    private void CharacterClimbDone()
    {
        if (!isTargetInLeft)
        {
            Vector3 correctOffset = new Vector3(offset1.x, offset1.y);
            transform.position = transform.position + (Vector3)correctOffset;
        }
        else
        {
            Vector3 correctOffset = new Vector3(-offset1.x, offset1.y);
            transform.position = transform.position + (Vector3)correctOffset;
        }

        isClimbing = false;
        rb.gravityScale = 1;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "box" && !isJumping && !isClimbing)
        {
            isJumping = true;
            CharacterJump();
        }
    }
    
}
