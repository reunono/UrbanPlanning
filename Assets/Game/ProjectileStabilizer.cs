using MoreMountains.LDJAM42;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileStabilizer : MonoBehaviour
{
    public AnimationCurve GrowingCurve;
    public Transform GrowingModel;
    public float GrowingDuration = 0.3f;
    public ParticleSystem Ripple;

    public float LayerChangeDuration = 0.3f;
    public float StableMass = 100f;
    public float StableDrag = 10f;
    public bool StableKinematic = false;
    public float StabilizationDelay = 1f;
    [ReadOnly]
    public bool Active = true;

    public bool Wandering = true;

    protected float _activeSince = 0f;
    protected Rigidbody _rigidbody;
    protected DamageOnTouch _damageOnTouch;
    protected float _initialMass;
    protected float _initialDrag;
    protected AgentController _agentController;
    protected MoreMountains.LDJAM42.AIBehaviour _aiBehaviour;
    protected ProjectilesWandering _wandering;
    protected Health _health;
    protected ThrownObject _thrownObject;

    protected virtual void Awake()
    {
        _rigidbody = this.gameObject.GetComponent<Rigidbody>();
        _damageOnTouch = this.gameObject.GetComponent<DamageOnTouch>();
        _aiBehaviour = this.gameObject.GetComponent<MoreMountains.LDJAM42.AIBehaviour>();
        _agentController = this.gameObject.GetComponent<AgentController>();
        _wandering = this.gameObject.GetComponent<ProjectilesWandering>();
        _health = this.gameObject.GetComponent<Health>();
        _thrownObject = this.gameObject.GetComponent<ThrownObject>();

        Ripple.Stop();

        _initialDrag = _rigidbody.drag;
        _initialMass = _rigidbody.mass;
    }

    protected virtual void OnSpawnComplete()
    {
        Activate();
    }

    protected virtual void Update()
    {
        //clamp velocity
        //if (_rigidbody.velocity.magnitude > MaxSpeed)
        //{
        //    _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, MaxSpeed);
        //}

        if (Active && (Time.time - _activeSince < GrowingDuration))
        {
            Grow();
        }

        if (Active && (Time.time - _activeSince > StabilizationDelay))
        {
            Desactivate();
        }
        if (Active && (Time.time - _activeSince > LayerChangeDuration))
        {
            this.gameObject.layer = LayerMask.NameToLayer("Projectiles");
        }
    }

    protected virtual void Grow()
    {
        GrowingModel.localScale = Vector3.one * GrowingCurve.Evaluate(MMMaths.Remap(Time.time - _activeSince, 0f, GrowingDuration, 0f, 1f));
    }

    protected virtual void Activate()
    {
        GrowingModel.localScale = Vector3.zero;
        _rigidbody.velocity = Vector3.zero;
        _activeSince = Time.time;
        Active = true;
        _rigidbody.drag = _initialDrag;
        _rigidbody.mass = _initialMass;
        this.gameObject.layer = LayerMask.NameToLayer("ProjectilesStart");
        _rigidbody.isKinematic = false;

        _health.enabled = true;
        _damageOnTouch.enabled = true;
        _thrownObject.enabled = true;

        if (Wandering)
        {
            _wandering.enabled = false;
        }
        
    }

    protected virtual void Desactivate()
    {
        Active = false;
        _rigidbody.drag = StableDrag;
        _rigidbody.mass = StableMass;
        _rigidbody.isKinematic = StableKinematic;

        _damageOnTouch.enabled = false;
        _health.enabled = false;
        _thrownObject.enabled = false;

        if (Wandering)
        {
            _wandering.enabled = true;
        }        
    }

    protected virtual void DamageApplied()
    {
        _damageOnTouch.enabled = false;
    }

    protected virtual void OnHit()
    {
        _damageOnTouch.enabled = false;
        Ripple.Clear();
        Ripple.Play();
    }

    void OnEnable()
    {
        GetComponent<MMPoolableObject>().OnSpawnComplete += OnSpawnComplete;
        GetComponent<DamageOnTouch>().OnHit += OnHit;
    }

    void OnDisable()
    {
        GetComponent<MMPoolableObject>().OnSpawnComplete -= OnSpawnComplete;
        GetComponent<DamageOnTouch>().OnHit += OnHit;
    }
}