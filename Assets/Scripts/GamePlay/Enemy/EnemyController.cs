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
    public float delayToAttack,currentDelayToAttack,knockBackTime,flyAwayTime;
    public bool isHit=false, isAttack = false, isDead = false;
    private SkeletonAnimation skeletonAnimation;
    [SerializeField] private float health;
    [SpineAnimation]
    public string idleAnim, hitAnim, deadAnim,getUpAnim;
    [SpineAnimation]
    public string[] enemyAttackAnim;
    [SerializeField] private CapsuleCollider2D capsuleCollider;
    private Rigidbody2D rb;
    private Coroutine stopHitStateCoroutine, hitDoneCoroutine;
    private GameObject target;
    private EnemyMovement enemyMovement;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        target=GetComponent<EnemyMovement>().GetTarget();
        enemyMovement = GetComponent<EnemyMovement>();
        skeletonAnimation.AnimationState.Complete += OnCompleteAttackAnim;
        skeletonAnimation.AnimationState.Interrupt += OnCompleteAttackAnim;
    }

    private void Update()
    {
        //Debug.Log("Hit state: " + isHit);
        //Kiểm tra enemy đã dead chưa
        if (isDead) return;

        if (enemyMovement.isTargetInAttackZone)
        {
            if (!isHit)
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
            else
            {
                if(currentDelayToAttack <= 0)
                {
                    currentDelayToAttack = 0.5f * delayToAttack;
                }
            }
        }
    }

    // Xử lý khi enemy nhận dame và chết
    public void HandleDameTaken(float dame, Transform transformFrom,CharacterTakeHitState state,float force)
    {
        if(isDead) return;

        if (hitDoneCoroutine != null)
        {
            StopCoroutine(hitDoneCoroutine);
        }
        isHit = true;
        this.health -= dame;

        // Khi nhận sát thương enemy sẽ không di chuyển
        rb.velocity = Vector2.zero;

        switch (state)
        {
            case CharacterTakeHitState.KnockBack:
                {
                    KnockBack(transformFrom,force);
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

        if (this.health <= 0)
        {
            //StopCoroutine(hitDoneCoroutine);
            isDead = true;
            Debug.Log("dead");
            skeletonAnimation.AnimationState.AddAnimation(0, deadAnim, false, 0.1f);
            capsuleCollider.gameObject.SetActive(false);
            //rb.gravityScale = 0;
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

    IEnumerator HitDone(float time)
    {
        yield return new WaitForSeconds(time);
        isHit = false;
    }


    private void KnockBack(Transform transformFrom,float force)
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

    private IEnumerator KnockCo()
    {
        yield return new WaitForSeconds(0.5f);
        rb.velocity = Vector2.zero;
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
        rb.AddForce(new Vector2( directionForce.x*0.4f,directionForce.y) * force, ForceMode2D.Impulse);
        HandleStopHitState();

    }

    private void FallDown(Transform transformFrom, float force)
    {
        if(enemyMovement.currentAnim != deadAnim)
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
        rb.AddForce(new Vector2(directionForce.x ,0.75f) * force, ForceMode2D.Impulse);
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


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // Xử lý đẩy lùi khi đánh trúng kẻ địch

            PlayerController player = collision.transform.parent.root.gameObject.GetComponent<PlayerController>();
            string currentAnim = enemyMovement.currentAnim;
            switch (currentAnim)
            {
                case var value when value == punchComboAnim[3]:
                case var value2 when value2 == kickComboAnim[4]:

                    {
                        if (punchComboAnim.Contains(currentAnim))
                        {
                            enemyController.HandleDameTaken(10, transform, CharacterTakeHitState.ThrowUp, spineAnimationData.punchKnockBackForce[
                                
                                
                                
                                
                                
                                
                                
                                
                                ]);
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
                       /* if (currentAnim.Contains("PUNCH"))
                        {
                            char lastChar = currentAnim[currentAnim.Length - 1];
                            int indexAnim = 0;
                            try
                            {
                                indexAnim = System.Convert.ToInt32(lastChar);
                            }
                            catch
                            {
                                Debug.Log("Cant parse number!");
                            }
                            player.HandleDameTaken(10, transform, CharacterTakeHitState.KnockBack, spineAnimationData.punchKnockBackForce[currentIndexAttackCombo]);
                        }
                        else
                       if (kickComboAnim.Contains(currentAnim))
                        {
                            player.HandleDameTaken(10, transform, CharacterTakeHitState.KnockBack, spineAnimationData.kickKnockBackForce[currentIndexAttackCombo]);
                        }*/
                        break;
                    }

            }
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
