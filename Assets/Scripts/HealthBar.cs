using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBar : MonoBehaviour
{
    [SerializeField] HealthComponent healthComponent;
    private Slider slider;

    public void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void Start()
    {
        healthComponent.OnHealthChange += (value) => slider.value = value;
    }
}
