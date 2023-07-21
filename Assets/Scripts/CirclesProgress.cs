using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class CirclesProgress : MonoBehaviour
{
   [SerializeField] private Image _innerCircle;

   public void Initialize(Color circleColor)
   {
      _innerCircle.color = circleColor;
      _innerCircle.color = GeneralUtils.MakeColorTransparent(_innerCircle.color);      
   }

   public void ShowCircle()
   {
      _innerCircle.DOFade(1, 0.5f);
   }
}