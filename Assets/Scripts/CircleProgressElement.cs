using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleProgressElement : MonoBehaviour
{
   [SerializeField] private CirclesProgress[] _circleElements;

   private int numOfWins = 0;
   
   public void Initialize(Color color)
   {
      for (int circleIndex = 0; circleIndex < _circleElements.Length; circleIndex++)
      {
         _circleElements[circleIndex].Initialize(color);
      }
   }

   public void AddProgress()
   {
      _circleElements[numOfWins].ShowCircle();
      
      numOfWins++; 
   }
}
