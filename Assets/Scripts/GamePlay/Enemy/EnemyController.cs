using DG.Tweening;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RotateMode = DG.Tweening.RotateMode;

public class EnemyController : MonoBehaviour
{
    public float thrust, throwUpForce,fallDownForce,flyAwayForce,delayToAttack,currentDelayToAttack;
    private bool isDead = false, isAttack=false;
    private SkeletonAnimation skeletonAnimation;
    [SerializeField] private float health;
    [SpineAnimation]
    public string idleAnim, hitAnim, deadAnim;
    [SerializeField] private CapsuleCollider2D capsuleCollider;
    private Rigidbody2D rb;
    private Coroutine stopHitStateCoroutine;
    private GameObject target;

    [SpineAnimation]
    public string[] enemyAttackAnim;
    private EnemyMovement enemyMovement;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        target=GetComponent<EnemyMovement>().GetTarget();
        enemyMovement = GetComponent<EnemyMovement>();
        skeletonAnimation.AnimationState.Complete += OnCompleteAttackAnim;
    }

    private void Update()
    {
        if (enemyMovement.isTargetInAttackZone)
        {
            if (currentDelayToAttack <= 0)
            {
                EnemyAttack();
            }
            else
            {
                currentDelayToAttack -= Time.deltaTime;
            }
        }
    }

    // Xử lý khi enemy nhận dame và chết
    public void HandleDameTaken(float dame, Transform transformFrom,CharacterTakeHitState state)
    {
        this.health -= dame;
        if (this.health <= 0)
        {
            isDead = true;
            skeletonAnimation.AnimationState.SetAnimation(0, deadAnim, false);
            capsuleCollider.gameObject.SetActive(false);
            rb.gravityScale = 0;
        }
        // Khi nhận sát thương enemy sẽ không di chuyển
        rb.velocity = Vector2.zero;

        switch (state)
        {
            case CharacterTakeHitState.KnockBack:
                {
                    KnockBack(transformFrom);
                    break;
                }
            case CharacterTakeHitState.ThrowUp:
                {
                    ThrowUp(transformFrom);
                    Debug.Log("Throw Up: "+Time.time);
                    break;
                }
            case CharacterTakeHitState.FallDown:
                {
                    FallDown(transformFrom);
                    Debug.Log("Fall Down: " + Time.time);
                    break;
                }
            case CharacterTakeHitState.FlyAway:
                {
                    FlyAway(transformFrom);
                    break;
                }
        }

    }

    private void HandleStopHitState()
    {
        if(stopHitStateCoroutine!= null)
        {
            StopCoroutine(stopHitStateCoroutine);
        }
        stopHitStateCoroutine = StartCoroutine(KnockCo());
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isDead)
        {
            if (collision.gameObject.tag == "PlayerMakeDamage")
            {
                //KnockBack(collision.transform.parent.root);
            }
        }
    }

    private void KnockBack(Transform transformFrom)
    {
        skeletonAnimation.AnimationState.SetAnimation(0, hitAnim, false);
        skeletonAnimation.AnimationState.AddAnimation(0, idleAnim, true, 0.5f);

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
        rb.AddForce(directionForce * thrust, ForceMode2D.Impulse);
        HandleStopHitState();
    }

    private IEnumerator KnockCo()
    {
        yield return new WaitForSeconds(0.5f);
        rb.velocity = Vector2.zero;
    }

    private void ThrowUp(Transform transformFrom)
    {
        skeletonAnimation.AnimationState.SetAnimation(0, hitAnim, false);
        skeletonAnimation.AnimationState.AddAnimation(0, idleAnim, true, 0.5f);
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
        rb.AddForce(new Vector2( directionForce.x*0.75f,directionForce.y) * throwUpForce, ForceMode2D.Impulse);
        HandleStopHitState();

    }

    private void FallDown(Transform transformFrom)
    {
        skeletonAnimation.AnimationState.SetAnimation(0, hitAnim, false);
        skeletonAnimation.AnimationState.AddAnimation(0, idleAnim, true, 0.5f);

        rb.AddForce(Vector2.down * fallDownForce, ForceMode2D.Impulse);
        HandleStopHitState();

    }
    private void FlyAway(Transform transformFrom)
    {
        skeletonAnimation.AnimationState.SetAnimation(0, hitAnim, false);
        skeletonAnimation.AnimationState.AddAnimation(0, idleAnim, true, 0.5f);
        
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
        rb.AddForce(new Vector2(directionForce.x , directionForce.y) * flyAwayForce, ForceMode2D.Impulse);
        //HandleStopHitState();
    }


    //Đang xử lý
    public void WasWrestle()
    {
        skeletonAnimation.AnimationState.SetAnimation(0, idleAnim, false);
        transform.parent.DORotate(new Vector3(0, 0, 270), 0.4f,RotateMode.LocalAxisAdd);

    }
    private void EnemyAttack()
    {
        if (!isAttack)
        {
            isAttack = true;
            int randomAttack = Random.Range(0, enemyAttackAnim.Length);
            skeletonAnimation.AnimationState.SetAnimation(0, enemyAttackAnim[randomAttack], false);
        }

    }
    private void OnCompleteAttackAnim(TrackEntry trackEntry)
    {
        if (enemyAttackAnim.Contains(trackEntry.ToString()))
        {
            currentDelayToAttack = delayToAttack;
            isAttack = false;

        }
    }
}

public enum CharacterTakeHitState
{
    KnockBack,
    ThrowUp,
    FallDown,
    FlyAway
}
