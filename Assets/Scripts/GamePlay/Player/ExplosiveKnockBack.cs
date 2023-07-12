using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveKnockBack : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Debug.Log("Hit enemy!");
            EnemyController enemyController = collision.transform.parent.root.gameObject.GetComponent<EnemyController>();
            if(enemyController != null )
            {
                enemyController.HandleDameTaken(10, transform, CharacterTakeHitState.FlyAway);
                //gameObject.SetActive(false);
            }
        }
    }
}
