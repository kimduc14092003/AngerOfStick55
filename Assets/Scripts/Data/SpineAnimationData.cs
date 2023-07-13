using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpineAnimationData", menuName = "SpineAnimationData")]
public class SpineAnimationData : ScriptableObject
{
    public List<float> punchVelocity;
    public List<float> kickVelocity;
    public List<float> punchKnockBackForce;
    public List<float> kickKnockBackForce;


    public float GetPunchAt(int index)
    {
        return punchVelocity[index];
    }

    public float GetKickAt(int index)
    {
        return kickVelocity[index];
    }

}

