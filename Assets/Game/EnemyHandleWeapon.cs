using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    public class EnemyHandleWeapon : MonoBehaviour
    {
        /// the initial weapon owned by the character
        public Weapon InitialWeapon;
        /// the position the weapon will be attached to. If left blank, will be this.transform.
        public Transform WeaponAttachment;
        /// returns the current equipped weapon
        public Weapon CurrentWeapon { get; set; }
        /// if this is true this animator will be automatically bound to the weapon
        public bool AutomaticallyBindAnimator = true;

        public Animator CharacterAnimator { get; set; }

        public WeaponAim WeaponAimComponent { get { return _weaponAim; } }

        protected float _fireTimer = 0f;
        protected WeaponAim _weaponAim;

        [InspectorButton("ShootStart")]
        public bool ShootStartButton;

        protected virtual void Start()
        {
            if (WeaponAttachment == null)
            {
                WeaponAttachment = transform;
            }
            Setup();
        }
        
        /// <summary>
        /// Grabs various components and inits stuff
        /// </summary>
        public virtual void Setup()
        {
            // filler if the WeaponAttachment has not been set
            if (WeaponAttachment == null)
            {
                WeaponAttachment = transform;
            }
            // we set the initial weapon
            if (InitialWeapon != null)
            {
                ChangeWeapon(InitialWeapon, null);
            }
        }

        /// <summary>
        /// Every frame we check if it's needed to update the ammo display
        /// </summary>
        public virtual void Update()
        {
        }


        /// <summary>
        /// Causes the character to start shooting
        /// </summary>
        public virtual void ShootStart()
        {
            // if the Shoot action is enabled in the permissions, we continue, if not we do nothing.  If the player is dead we do nothing.
            if (CurrentWeapon == null)
            {
                return;
            }

            CurrentWeapon.WeaponInputStart();
        }

        /// <summary>
        /// Causes the character to stop shooting
        /// </summary>
        public virtual void ShootStop()
        {
            // if the Shoot action is enabled in the permissions, we continue, if not we do nothing
            if (CurrentWeapon == null)
            {
                return;
            }

            if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle)
            {
                return;
            }

            if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload)
                || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart)
                || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop))
            {
                return;
            }

            if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse) && (!CurrentWeapon.DelayBeforeUseReleaseInterruption))
            {
                return;
            }

            if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses) && (!CurrentWeapon.TimeBetweenUsesReleaseInterruption))
            {
                return;
            }

            CurrentWeapon.TurnWeaponOff();
        }

        /// <summary>
        /// Reloads the weapon
        /// </summary>
        protected virtual void Reload()
        {
            if (CurrentWeapon != null)
            {
                CurrentWeapon.InitiateReloadWeapon();
            }
        }

        /// <summary>
        /// Changes the character's current weapon to the one passed as a parameter
        /// </summary>
        /// <param name="newWeapon">The new weapon.</param>
        public virtual void ChangeWeapon(Weapon newWeapon, string weaponID, bool combo = false)
        {
            // if the character already has a weapon, we make it stop shooting
            if (CurrentWeapon != null)
            {
                if (!combo)
                {
                    ShootStop();
                    if (_weaponAim != null) { _weaponAim.RemoveReticle(); }
                    Destroy(CurrentWeapon.gameObject);
                }
            }

            if (newWeapon != null)
            {
                if (!combo)
                {
                    CurrentWeapon = (Weapon)Instantiate(newWeapon, WeaponAttachment.transform.position + newWeapon.WeaponAttachmentOffset, WeaponAttachment.transform.rotation);
                }
                CurrentWeapon.transform.parent = WeaponAttachment.transform;
                CurrentWeapon.transform.localPosition += newWeapon.WeaponAttachmentOffset;
                CurrentWeapon.WeaponID = weaponID;
                _weaponAim = CurrentWeapon.GetComponent<WeaponAim>();
                
                // we turn off the gun's emitters.
                CurrentWeapon.Initialization();
                CurrentWeapon.InitializeComboWeapons();
                CurrentWeapon.InitializeAnimatorParameters();
            }
            else
            {
                CurrentWeapon = null;
            }
        }
    }
}