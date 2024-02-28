using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BangerText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private void Start()
    {
        
    }

    private void Update()
    {
        // hue shift
        text.color = Color.HSVToRGB(Mathf.PingPong(Time.time * .2f, 1), 1, 1);
    }
}