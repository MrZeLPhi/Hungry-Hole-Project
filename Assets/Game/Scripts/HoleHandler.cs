using System;
using UnityEngine;

public class HoleHandler : MonoBehaviour
{
   public int NormalSphereLayer;
   public int FallingSphereLayer;

   private void OnTriggerEnter(Collider other)
   {
      if (other.gameObject.layer == NormalSphereLayer)
      {
         other.gameObject.layer = FallingSphereLayer;
      }
   }

   private void OnTriggerExit(Collider other)
   {
      if (other.gameObject.layer == FallingSphereLayer)
      {
         other.gameObject.layer = NormalSphereLayer;
      }
   }
}
