using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.LDJAM42
{
    public class AgentController : MonoBehaviour
    {
        [Header("Speed")]
        public float RunSpeed = 1000.0F;
        public float RotateSpeed = 200.0F;
        public bool ShouldAngleAgent = false;

        [Header("Dependencies")]
        public Animator CharacterAnimator;
        public Animator DeathAnimator;
        public GameObject ModelContainer;
        public ParticleSystem FootStepsParticleSystem;
        public ParticleSystem ChargeParticleSystem;
        public ParticleSystem ChargeAreaEffect;
        public Animator TeleportAnimator;

        [Header("Debug")]
        public bool DrawDebug = true;

        public bool Goal { get; set; }
        public bool GoalInProgress { get; set; }
        public bool RobotDance { get; set; }
        public bool Dead { get; set; }

        public Vector3 Velocity { get { return _rigidBody.velocity; } }

        protected const float _groundRayLength = 5f;

        protected Rigidbody _rigidBody;
        protected Animator _animator;

        protected List<string> _animatorParameters;

        protected Vector3 _forwardForce;
        protected Vector3 _torque;
        protected Vector3 _sidestepForce;

        protected float _currentSpeed;
        protected float _currentRotationSpeed;

        protected bool _grounded = false;
        protected float _sidestepCountdown = 0;

        protected float _maximumCharacterAngle = 20f;

        protected ParticleSystem.EmissionModule _footstepsEmissionModule;
        protected ParticleSystem.EmissionModule _chargeEmissionModule;
        //protected ParticleSystem.EmissionModule _chargeAreaEmissionModule;
        protected SphereCollider _sphereCollider;

        protected virtual void Start()
        {
            Initialization();
        }

        public virtual void Initialization()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _sphereCollider = GetComponent<SphereCollider>();
            _animator = CharacterAnimator;
            _footstepsEmissionModule = FootStepsParticleSystem.emission;

            if (ChargeParticleSystem != null)
            {
                _chargeEmissionModule = ChargeParticleSystem.emission;
            }
            /*if (ChargeAreaEffect != null)
		    {
			    _chargeAreaEmissionModule = ChargeAreaEffect.emission;
		    }*/
            InitializeAnimatorParameters();
        }

        protected virtual void Update()
        {
            UpdateAnimators();
            ResetAnimatorStates();
            AngleAgent();
            HandleFootsteps();
            HandleCharge();
        }

        protected virtual void FixedUpdate()
        {
            MoveCharacter();
        }

        public virtual void SetForwardForce(Vector3 forwardForce)
        {
            _forwardForce = forwardForce;
        }

        public virtual void SetTorque(Vector3 torque)
        {
            _torque = torque;
        }

        protected virtual void MoveCharacter()
        {
            _rigidBody.AddTorque(_torque * RotateSpeed);
            _rigidBody.AddForce(_forwardForce * RunSpeed);

            _currentSpeed = _rigidBody.velocity.magnitude;
            _currentRotationSpeed = _torque.magnitude;
        }

        protected virtual void AngleAgent()
        {
            if (!ShouldAngleAgent)
            {
                return;
            }
            Vector3 newAngle = -Mathf.Clamp(_torque.y, -1f, 1f)
                * _maximumCharacterAngle
                * Mathf.InverseLerp(0f, 12f, Mathf.Abs(_currentSpeed))
                * Vector3.forward;
            ModelContainer.transform.localEulerAngles = newAngle;
        }

        protected virtual void HandleFootsteps()
        {
            if (FootStepsParticleSystem != null)
            {
                _footstepsEmissionModule.enabled = (_currentSpeed > 5f);
            }
            
        }

        protected virtual void HandleCharge()
        {

        }

        protected virtual void DetermineIfGrounded()
        {
            RaycastHit raycast3D = MMDebug.Raycast3D(transform.position, Vector3.down, _groundRayLength, 1 << LayerMask.NameToLayer("Ground"), Color.green, true);
            if (raycast3D.transform != null)
            {
                //TODO grounded
            }
        }

        public virtual void GoalScored(bool status)
        {
            Goal = status;
            GoalInProgress = status;
        }

        public virtual void Teleport()
        {
            if (TeleportAnimator == null)
            {
                return;
            }
            TeleportAnimator.SetTrigger("Teleport");
        }

        public virtual void Die()
        {
            Dead = true;
            this.tag = "DeadRobot";
            this.gameObject.layer = 14;

            _sphereCollider.radius = _sphereCollider.radius / 1.2f;

            if (DeathAnimator != null)
            {
                DeathAnimator.SetTrigger("Death");
            }
        }

        public virtual void Resurrect()
        {
            Dead = false;
            this.tag = "Player";
            this.gameObject.layer = 9;
            _sphereCollider.radius = _sphereCollider.radius * 1.2f;
        }

        protected virtual void OnDrawGizmos()
        {
            if (DrawDebug)
            {
                MMDebug.DebugDrawArrow(this.transform.position, this.transform.forward, Color.blue, 5f);
            }
        }

        /// <summary>
        /// Initializes the animator parameters.
        /// </summary>
        protected virtual void InitializeAnimatorParameters()
        {
            if (_animator == null) { return; }
            if (!_animator.isActiveAndEnabled) { return; }

            _animatorParameters = new List<string>();

            MMAnimator.AddAnimatorParamaterIfExists(_animator, "Grounded", AnimatorControllerParameterType.Bool, _animatorParameters);
            MMAnimator.AddAnimatorParamaterIfExists(_animator, "ForwardSpeed", AnimatorControllerParameterType.Float, _animatorParameters);
            MMAnimator.AddAnimatorParamaterIfExists(_animator, "RotationSpeed", AnimatorControllerParameterType.Float, _animatorParameters);
            MMAnimator.AddAnimatorParamaterIfExists(_animator, "Goal", AnimatorControllerParameterType.Bool, _animatorParameters);
            MMAnimator.AddAnimatorParamaterIfExists(_animator, "GoalInProgress", AnimatorControllerParameterType.Bool, _animatorParameters);
            MMAnimator.AddAnimatorParamaterIfExists(_animator, "RobotDance", AnimatorControllerParameterType.Bool, _animatorParameters);
            MMAnimator.AddAnimatorParamaterIfExists(_animator, "Dead", AnimatorControllerParameterType.Bool, _animatorParameters);
        }

        /// <summary>
        /// This is called at Update() and sets each of the animators parameters to their corresponding State values
        /// </summary>
        protected virtual void UpdateAnimators()
        {
            if (_animator != null)
            {
                MMAnimator.UpdateAnimatorBool(_animator, "Grounded", true, _animatorParameters);
                MMAnimator.UpdateAnimatorFloat(_animator, "ForwardSpeed", _currentSpeed, _animatorParameters);
                MMAnimator.UpdateAnimatorFloat(_animator, "RotationSpeed", _currentRotationSpeed, _animatorParameters);
                MMAnimator.UpdateAnimatorBool(_animator, "Goal", Goal, _animatorParameters);
                MMAnimator.UpdateAnimatorBool(_animator, "GoalInProgress", GoalInProgress, _animatorParameters);
                MMAnimator.UpdateAnimatorBool(_animator, "RobotDance", RobotDance, _animatorParameters);
                MMAnimator.UpdateAnimatorBool(_animator, "Dead", Dead, _animatorParameters);
            }
        }

        protected virtual void ResetAnimatorStates()
        {
            Goal = false;
        }

        public virtual void Reset()
        {

        }


    }
}