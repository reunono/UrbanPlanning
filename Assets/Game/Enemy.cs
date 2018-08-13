using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MoreMountains.LDJAM42
{
    public class Enemy : MonoBehaviour
    {
        public Vector3 MinTargetRange;
        public Vector3 MaxTargetRange;

        public float MovementForce = 10f;
        public Vector2 DecisionTimeRange = new Vector2(2f, 4f);
        public LayerMask ObstaclesLayerMask;

        protected Rigidbody _rigidbody;
        protected bool _moving = false;
        protected float _timeToNextDecision = 0f;
        protected float _lastDecisionTimeStamp = 0f;
        protected float _distanceToTarget;

        public Vector3 NewTarget = Vector3.zero;

        protected Vector3 _leftRaycast;
        protected Vector3 _rightRaycast;
        protected float _direction;
        protected GameObject _target;

        protected virtual void Awake()
        {
            _rigidbody = this.gameObject.GetComponent<Rigidbody>();
            _target = GameObject.FindGameObjectWithTag("Player");
        }

        protected virtual void Update()
        {

        }

        protected virtual void FixedUpdate()
        {
            Antennas();

            if (Time.time - _lastDecisionTimeStamp >= _timeToNextDecision)
            {
                Decide();
            }

            Move();
        }

        protected virtual void Antennas()
        {
            // we try to detect walls and avoid them
            _leftRaycast = transform.forward;
            _leftRaycast = Quaternion.AngleAxis(-25, Vector3.up) * _leftRaycast;
            _rightRaycast = transform.forward;
            _rightRaycast = Quaternion.AngleAxis(25, Vector3.up) * _rightRaycast;

            Vector3 targetVector = _target.transform.position - transform.position;
            _distanceToTarget = targetVector.magnitude;

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

        protected virtual void Move()
        {

            _rigidbody.AddTorque(Vector3.Cross(transform.forward, _rigidbody.velocity), ForceMode.Force);
        }

        protected virtual void Decide()
        {
            _timeToNextDecision = Random.Range(DecisionTimeRange.x, DecisionTimeRange.y);
            _lastDecisionTimeStamp = Time.time;
            int dice = MMMaths.RollADice(2);
            if (dice == 2)
            {
                TryToMove();
            }
            else
            {
                StopMoving();
            }
        }

        protected virtual void StopMoving()
        {
            _moving = false;
        }

        protected virtual void TryToMove()
        {
            NewTarget.x = Random.Range(MinTargetRange.x, MaxTargetRange.x);
            NewTarget.y = Random.Range(MinTargetRange.y, MaxTargetRange.y);
            NewTarget.z = Random.Range(MinTargetRange.z, MaxTargetRange.z);
            _moving = true;
        }
    }
}
