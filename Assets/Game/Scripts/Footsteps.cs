using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{

    protected ParticleSystem _particles;
    protected bool _playing;
    public float _velocity;
    protected Vector3 _previousPosition;



    protected virtual void Awake()
    {

    }

    protected virtual void OnEnable()
    {
        _particles = this.gameObject.GetComponent<ParticleSystem>();
        _playing = false;
        _particles.Stop();
    }

    protected virtual void Update()
    {
        _velocity = (_previousPosition - this.transform.position).magnitude;

        if ((_velocity > 0.2f) && !_playing)
        {
            _playing = true;
            _particles.Play();
        }

        if ((_velocity <= 0.2f) && _playing)
        {
            _playing = false;
            _particles.Stop();
        }

        if (_particles.isEmitting && !_playing)
        {
            _particles.Stop();
        }

        _previousPosition = this.transform.position;
    }

    
}
