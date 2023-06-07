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
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private LedgeDetection ledgeDetection;
    [SerializeField] private Vector2 offset1;
    [SerializeField] private CapsuleCollider2D capsuleCollider;

    [SpineAnimation]
    public string idleAnim, runAnim, jumpAnim, hitAnim, deadAnim;
    private SkeletonAnimation skeletonAnimation;
                
    private Rigidbody2D rb;
    private bool isFindTarget;
    private GameObject target;
    private bool isTargetInAttackZone;
    private bool isTargetInLeft=true;
    private bool isJumping;
    private bool isClimbing;
    private float maxHealth=100;
    private float currentHealth;
    public float thrust;
    private void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        skeletonAnimation= GetComponent<SkeletonAnimation>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.DrawWireCube(attackZoneTrasform.position, sizeOfAttackZone);
    }
    private void Update()
    {
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
            if (ledgeDetection.IsCharacterClimb())
            {
                CharacterClimb();
            }
        }
    }
    private void FixedUpdate()
    {
        
    }

    public void DetectTargetCome()
    {
        if (!isFindTarget)
        {
             Collider2D result = Physics2D.OverlapCircle(transform.position, radius, layerMaskLedge);
             if(result != null)
            {
                isFindTarget = true;
                target = result.gameObject;
            }
            
        }
    }

    private void DetectTargetInAttackZone()
    {
       isTargetInAttackZone= Physics2D.OverlapBox(attackZoneTrasform.position, sizeOfAttackZone,0,layerMaskLedge);
    }

    private void MoveToAttackPlayer()
    {
        if (target.transform.position.x < transform.position.x)
        {
            isTargetInLeft = true;
        }
        else
        {
            isTargetInLeft= false;
        }

        if (isTargetInAttackZone)
        {
            
        }
        else
        {
            if (isTargetInLeft)
            {
                if (target.transform.position.x + sizeOfAttackZone.x > transform.position.x) return;

                rb.velocity = new Vector2(speed*-1f * Time.deltaTime, rb.velocity.y);
            }
            else
            {
                if (target.transform.position.x - sizeOfAttackZone.x  < transform.position.x ) return;
                rb.velocity = new Vector2(speed * Time.deltaTime, rb.velocity.y);
            }
        }
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
    // Xử lý khi enemy nhận dame và chết
    public void HandleDameTaken(float dame)
    {
        this.currentHealth -= dame;
        if(this.currentHealth <= 0)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, deadAnim, false);
            capsuleCollider.gameObject.SetActive(false);
            rb.gravityScale = 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "box"&& !isJumping&&!isClimbing)
        {
            isJumping = true;
            CharacterJump();
        }
        
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "PlayerMakeDamage")
        {
            skeletonAnimation.AnimationState.SetAnimation(0, hitAnim, false);
            skeletonAnimation.AnimationState.AddAnimation(0, idleAnim, true,0.1f);
            HandleDameTaken(10);

            Vector2 directionForce= transform.position- collision.transform.position;
            directionForce= directionForce.normalized* thrust;

            rb.AddForce(directionForce,ForceMode2D.Impulse);
            StartCoroutine(KnockCo());
            Debug.Log(directionForce);
        }


    }

    private IEnumerator KnockCo()
    {
        yield return new WaitForSeconds(0.5f);
        rb.velocity = Vector2.zero;
    }
}
