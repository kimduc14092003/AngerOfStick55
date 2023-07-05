using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float thrust, throwUpForce,fallDownForce;
    private bool isDead = false;
    private SkeletonAnimation skeletonAnimation;
    [SerializeField] private float health;
    [SpineAnimation]
    public string idleAnim, hitAnim, deadAnim;
    [SerializeField] private CapsuleCollider2D capsuleCollider;
    private Rigidbody2D rb;
    private Coroutine stopHitStateCoroutine;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
    }

    private void FixedUpdate()
    {
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
                    KnockBack(transformFrom);
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
        rb.AddForce(new Vector2( directionForce.x*0.3f,directionForce.y) * throwUpForce, ForceMode2D.Impulse);
        HandleStopHitState();

    }

    private void FallDown(Transform transformFrom)
    {
        skeletonAnimation.AnimationState.SetAnimation(0, hitAnim, false);
        skeletonAnimation.AnimationState.AddAnimation(0, idleAnim, true, 0.5f);

        rb.AddForce(Vector2.down * fallDownForce, ForceMode2D.Impulse);
        HandleStopHitState();

    }

}

public enum CharacterTakeHitState
{
    KnockBack,
    ThrowUp,
    FallDown,
    FlyAway
}
