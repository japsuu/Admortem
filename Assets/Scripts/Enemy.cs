using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    public void Damage(float amount)
    {
        Debug.LogWarning("Not implemented");
    }
}
