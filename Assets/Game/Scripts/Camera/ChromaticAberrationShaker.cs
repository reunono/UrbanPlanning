using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using MoreMountains.Tools;

public struct MMChromaticAberrationShake
{
    public int ID;
    public MMChromaticAberrationShake(int id)
    {
        ID = id;
    }
}

public class ChromaticAberrationShaker : MonoBehaviour, MMEventListener<MMChromaticAberrationShake>
{
    public AnimationCurve ShakeIntensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
    public float ShakeDuration = 0.2f;
    [ReadOnly]
    public bool Shaking = false;

    [InspectorButton("StartShaking")]
    public bool TestShakeButton;

    protected ChromaticAberration _chromaticAberration;
    protected PostProcessVolume _volume;
    protected float _shakeStartedTimestamp;
    protected float _remappedTimeSinceStart;
    
    protected virtual void Awake()
    {
        _volume = this.gameObject.GetComponent<PostProcessVolume>();
        _volume.profile.TryGetSettings(out _chromaticAberration);
        Shaking = false;
    }

    public virtual void StartShaking()
    {
        if (Shaking)
        {
            return;
        }
        else
        {
            _shakeStartedTimestamp = Time.time;
            Shaking = true;
        }
    }
    
    protected virtual void Update()
    {
        if (Shaking)
        {
            Shake();
        }

        if (Shaking && (Time.time - _shakeStartedTimestamp > ShakeDuration))
        {
            Shaking = false;
        }
    }

    protected virtual void Shake()
    {
        _remappedTimeSinceStart = MMMaths.Remap(Time.time - _shakeStartedTimestamp, 0f, ShakeDuration, 0f, 1f);
        _chromaticAberration.intensity.value = ShakeIntensity.Evaluate(_remappedTimeSinceStart);
    }


    public virtual void OnMMEvent(MMChromaticAberrationShake shakeEvent)
    {
        this.StartShaking();
    }

    protected virtual void OnEnable()
    {
        this.MMEventStartListening<MMChromaticAberrationShake>();
    }

    protected virtual void OnDisable()
    {
        this.MMEventStopListening<MMChromaticAberrationShake>();
    }
}
