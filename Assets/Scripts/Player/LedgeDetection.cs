using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeDetection : MonoBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private LayerMask layerMaskLedge;
    [SerializeField] private PlayerMovement playerMovement;
    private bool canDetected;

    private void Start()
    {
        canDetected = true;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    public bool IsCharacterClimb()
    {
        if(canDetected)
        {
            return Physics2D.OverlapCircle(transform.position, radius, layerMaskLedge);
        }

        return false;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("climbPoint"))
        {
            canDetected = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("climbPoint"))
        {
            canDetected = true;
        }
    }
}
