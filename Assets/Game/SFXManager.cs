using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    public class SFXManager : Singleton<SFXManager>
    {
        public AudioClip BloopSfx;
        public AudioClip UrbanPlannerSfx;
        public AudioClip OhnoSfx;
        public AudioClip PomSfx;

        public virtual void Bloop()
        {
            SoundManager.Instance.PlaySound(BloopSfx, Vector3.zero, false);
        }
        public virtual void UrbanPlanner()
        {
            SoundManager.Instance.PlaySound(UrbanPlannerSfx, Vector3.zero, false);
        }
        public virtual void OhNo()
        {
            SoundManager.Instance.PlaySound(OhnoSfx, Vector3.zero, false);
        }
        public virtual void Pom()
        {
            SoundManager.Instance.PlaySound(PomSfx, Vector3.zero, false);
        }
    }
}