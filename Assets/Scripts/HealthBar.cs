using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;


public class HealthBar : MonoBehaviour
{
    public List<Sprite> barStages;
    public HealthComponent entityToTrack;
    public Animator animator;
    public Image barImg;

    private void Start()
    {
        entityToTrack.OnHealthChange += (hp) =>
        {
            Debug.Log("Hit taken");
            int bars = (int)Math.Ceiling(hp * 10f);
            if (bars > 0) barImg.sprite = barStages[10 - bars];
            animator.SetTrigger("TakeHit");
        };
    }
}
