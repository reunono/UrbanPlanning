using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.TopDownEngine;

namespace MoreMountains.LDJAM42
{
    [RequireComponent(typeof(AgentController))]
    public class AIBehaviour : MonoBehaviour
    {
        public float ShootDistance = 8f;

        public Vector3 MinTargetRange;
        public Vector3 MaxTargetRange;

        public AnimationCurve SpawnExplosionCurve;
        public float ExpansionDuration = 0.5f;

        [Range(0f, 1f)]
        /// the maximum Gas Pedal Amount 
        public float FullThrottle = 1f;

        [Range(0f, 1f)]
        /// the minimum Gas Pedal Amount
        public float SmallThrottle = 1f;

        [Header("Intervention Zone")]
        public float InterventionDistance = 25f;

        public LayerMask ObstaclesLayerMask;

        protected const float _largeAngleDistance = 90f; // When angle between front of the vehicle and target waypoint are distant 
        protected const float _smallAngleDistance = 5f;  // When angle between front of the vehicle and target waypoint are near
        protected const float _minimalSpeedForBrakes = 0.5f; // When vehicle is at least at this speed, AI can use brakes
        protected const float _maximalDistanceStuck = 0.5f; // Distance to consider vehicle stuck

        public AgentController Controller { get { return _agentController; } }

        public Vector2 DecisionTimeRange = new Vector2(2f, 4f);

        protected float _targetAngleAbsolute;
        protected int _newDirection;
        
        public Vector3 _targetPosition;
        protected AgentController _agentController;
        protected float _distanceToTarget;

        protected GameObject _player;
        protected float _distanceToPlayer;

        protected float _direction = 0f;
        protected float _acceleration = 0f;

        protected Vector3 _newForwardForce;
        protected Vector3 _newTorque;

        protected Vector3 _leftRaycast;
        protected Vector3 _rightRaycast;

        protected float _timeToNextDecision = 0f;
        protected float _lastDecisionTimeStamp = 0f;

        protected bool _moving;
        protected bool _expanding;
        protected float _spawnTime;

        protected SphereCollider _sphereCollider;
        protected float _initialRadius;
        protected Rigidbody _rigidbody;
        protected EnemyHandleWeapon _enemyHandleWeapon;

        protected virtual void Awake()
        {
            Initialization();
            _sphereCollider = this.gameObject.GetComponent<SphereCollider>();
            _initialRadius = _sphereCollider.radius;
            _rigidbody = this.gameObject.GetComponent<Rigidbody>();
            _enemyHandleWeapon = this.gameObject.GetComponent<EnemyHandleWeapon>();
            _player = GameObject.FindGameObjectWithTag("Player");
        }

        protected virtual void Initialization()
        {
            _agentController = GetComponent<AgentController>();
            //_target = GameObject.FindGameObjectWithTag("Player");
            //TODO : point towards player
        }

        protected virtual void Expand()
        {
            /*
            _sphereCollider.radius = _initialRadius + SpawnExplosionCurve.Evaluate(MMMaths.Remap(Time.time - _spawnTime, 0f, ExpansionDuration, 0f, 1f));
            if (Time.time - _spawnTime > ExpansionDuration)
            {
                _expanding = false;
                _rigidbody.isKinematic = false;
            }*/
            _rigidbody.isKinematic = false;
        }

        protected virtual void FixedUpdate()
        {
            if (_expanding)
            {
                Expand();
            }

            if ((GameManager.Instance.GameState.CurrentState == GameStates.GameInProgress) && (!_agentController.Dead))
            {
                if (Time.time - _lastDecisionTimeStamp >= _timeToNextDecision)
                {
                    Decide();
                }
                if (!TryToShoot())
                {
                    EvaluateDirection();
                    EvalutateAccelerationAndDirection();
                    RequestMovement();
                }
            }
            else
            {
                KillMovement();
            }
        }

        protected virtual void Decide()
        {
            _timeToNextDecision = Random.Range(DecisionTimeRange.x, DecisionTimeRange.y);
            _lastDecisionTimeStamp = Time.time;

            int dice = MMMaths.RollADice(2);

            if (dice == 2)
            {
                PickNewTarget();
            }
            else
            {
                StopMoving();
            }

        }

        protected virtual bool TryToShoot()
        {
            if (_enemyHandleWeapon == null)
            {
                return false;
            }
            if (Vector3.Distance(this.transform.position, _player.transform.position) < ShootDistance)
            {
                _enemyHandleWeapon.ShootStart();
                //_enemyHandleWeapon.CurrentWeapon._aimableWeapon.SetCurrentAim(_player.transform.position - this.transform.position);
                _enemyHandleWeapon.CurrentWeapon.transform.LookAt(_player.transform);
                //KillMovement();
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual void StopMoving()
        {
            _moving = false;
        }

        protected virtual void PickNewTarget()
        {
            _targetPosition.x = Random.Range(MinTargetRange.x, MaxTargetRange.x);
            _targetPosition.y = Random.Range(MinTargetRange.y, MaxTargetRange.y);
            _targetPosition.z = Random.Range(MinTargetRange.z, MaxTargetRange.z);
            _moving = true;
        }

        protected virtual void KillMovement()
        {
            _newForwardForce = Vector3.zero;
            _newTorque = Vector3.zero;
            _agentController.SetForwardForce(_newForwardForce);
            _agentController.SetTorque(_newTorque);
        }

        protected virtual void EvaluateDirection()
        {
            // we compute the target vector between the vehicle and the next waypoint on a plane (without Y axis)
            Vector3 targetVector = _targetPosition - transform.position;
            _distanceToTarget = targetVector.magnitude;
            targetVector.y = 0;
            Vector3 transformForwardPlane = transform.forward;
            transformForwardPlane.y = 0;
            // then we measure the angle from vehicle forward to target Vector
            _targetAngleAbsolute = Vector3.Angle(transformForwardPlane, targetVector);
            // we also compute the cross product in order to find out if the angle is positive 
            Vector3 cross = Vector3.Cross(transformForwardPlane, targetVector);

            // this value indicates if the vehicle has to move towards the left or right
            _newDirection = cross.y >= 0 ? 1 : -1;

            // we handle the situation where the target could be behind us
            Vector3 targetLocal = transform.InverseTransformPoint(_targetPosition);
            if (targetLocal.z < 0)
            {
                _newDirection = (targetLocal.x < 0) ? 1 : -1;
            }
        }

        protected virtual void EvalutateAccelerationAndDirection()
        {
            // now, we apply _direction & _acceleration values 
            // if the vehicle is looking towards the opposite direction ?
            if (_targetAngleAbsolute > _largeAngleDistance)
            {
                // we steer to the proper direction
                _direction = -_newDirection;
                // if we have enough speed, we brake to rotate faster
                if (_agentController.Velocity.magnitude > _minimalSpeedForBrakes)
                {
                    //_acceleration = -FullThrottle;
                }
                else
                {
                    // otherwise we accelerate slowly
                    _acceleration = SmallThrottle;
                }
                // else if the vehicle is not pointing towards the waypoint but also not too far ? 
            }
            else if (_targetAngleAbsolute > _smallAngleDistance)
            {
                // we steer to the proper direction
                _direction = _newDirection;
                // we acceleration slowly
                _acceleration = SmallThrottle;
            }
            else
            {
                // if the vehicle is facing the waypoint, we switch to full speed
                _direction = 0f;
                _acceleration = FullThrottle;
            }

            // we try to detect walls and avoid them
            _leftRaycast = transform.forward;
            _leftRaycast = Quaternion.AngleAxis(-25, Vector3.up) * _leftRaycast;
            _rightRaycast = transform.forward;
            _rightRaycast = Quaternion.AngleAxis(25, Vector3.up) * _rightRaycast;

            RaycastHit raycast3DLeft = MMDebug.Raycast3D(transform.position, _leftRaycast, 25f, ObstaclesLayerMask, Color.blue, true);
            RaycastHit raycast3DRight = MMDebug.Raycast3D(transform.position, _rightRaycast, 25f, ObstaclesLayerMask, Color.blue, true);
            if (raycast3DLeft.transform != null)
            {
                if (raycast3DLeft.distance < _distanceToTarget)
                {
                    _direction = 1;
                }
            }
            if (raycast3DRight.transform != null)
            {
                if (raycast3DRight.distance < _distanceToTarget)
                {
                    _direction = -1;
                }
            }
        }

        protected virtual void RequestMovement()
        {
            _newForwardForce = transform.forward * Mathf.Abs(_acceleration);
            _newTorque = transform.up * _direction;

            if ((_distanceToTarget > InterventionDistance) && (InterventionDistance != 0f) )
            {
                _newForwardForce = Vector3.zero;
            }

            if ((_distanceToTarget < InterventionDistance))
            {
                _newForwardForce *= 5;
            }

            _agentController.SetForwardForce(_newForwardForce);
            _agentController.SetTorque(_newTorque);
        }

        protected virtual void Reset()
        {
            _agentController.RobotDance = false;
        }

        protected virtual void OnDrawGizmos()
        {
            Debug.DrawLine(this.transform.position, _targetPosition, Color.white);
            MMDebug.DrawGizmoPoint(this.transform.position, InterventionDistance, Color.yellow);
            if (_enemyHandleWeapon != null)
            {
                MMDebug.DrawGizmoPoint(this.transform.position, ShootDistance, Colors.AliceBlue);
            }            
        }

        protected virtual void OnSpawnComplete()
        {
            GameManager.Instance.CurrentEnemies++;
            //_rigidbody.isKinematic = true;
            _expanding = true;
            _spawnTime = Time.time;
        }

        protected virtual void OnDeath()
        {
            GameManager.Instance.DeadEnemies++;

            int enemiesLeft = (GameManager.Instance.TotalEnemies - GameManager.Instance.DeadEnemies);
            if (enemiesLeft > 1)
            {
                GUIManager.Instance.EnemiesText.text =  enemiesLeft.ToString() + " enemies left";
            }
            else
            {
                GUIManager.Instance.EnemiesText.text = enemiesLeft.ToString() + " enemy left";
            }
            
        }

        void OnEnable()
        {
            GetComponent<Health>().OnDeath += OnDeath;
            GetComponent<MMPoolableObject>().OnSpawnComplete += OnSpawnComplete;
        }

        void OnDisable()
        {
            GetComponent<Health>().OnDeath -= OnDeath;
            GetComponent<MMPoolableObject>().OnSpawnComplete -= OnSpawnComplete;
        }
    }
}