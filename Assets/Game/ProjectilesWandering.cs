using MoreMountains.LDJAM42;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilesWandering : MonoBehaviour
{
    public float MovementSpeed = 5f;
    public float RotationSpeed = 5f;

    protected Rigidbody _rigidbody;
    protected Vector3 _newDirection;

    public Vector2 DecisionTimeRange = new Vector2(2f, 4f);
    protected float _timeToNextDecision = 0f;
    protected float _lastDecisionTimeStamp = 0f;
    public bool Moving = false;
    protected Vector3 _force;

    protected virtual void Start()
    {
        _rigidbody = this.gameObject.GetComponent<Rigidbody>();
    }

    protected virtual void Update()
    {
        if (GameManager.Instance.GameState.CurrentState == GameStates.GameInProgress) 
        {
            if (Time.time - _lastDecisionTimeStamp >= _timeToNextDecision)
            {
                Decide();
            }            
        }        
    }

    protected virtual void FixedUpdate()
    {
        Move();
    }

    protected virtual void Decide()
    {
        _timeToNextDecision = Random.Range(DecisionTimeRange.x, DecisionTimeRange.y);
        _lastDecisionTimeStamp = Time.time;

        int dice = MMMaths.RollADice(5);

        if (dice == 1)
        {
            PickNewTarget();
        }
        if (dice == 2)
        {
            StopMoving();
        }
        if ((dice == 3) || (dice == 4) || (dice == 5))
        {
            StartMoving();
        }
    }

    protected virtual void PickNewTarget()
    {
        _newDirection.x = Random.Range(0f, 360f);
        _newDirection.y = 0f;
        _newDirection.z = Random.Range(0f, 360f);
        transform.eulerAngles = _newDirection;

        _force.x = Random.Range(-1f, 1f);
        _force.y = 0f;
        _force.z = Random.Range(-1f, 1f);
    }

    protected virtual void StopMoving()
    {
        Moving = false;
    }

    protected virtual void StartMoving()
    {
        Moving = true;
    }

    protected virtual void Move()
    {   
        if (Moving)
        {
            _rigidbody.AddForce(_force.normalized * MovementSpeed);
        }
    }
	
    protected virtual void OnDrawGizmos()
    {
        Debug.DrawLine(this.transform.position, this.transform.position + _force.normalized * 10f);
    }
}
