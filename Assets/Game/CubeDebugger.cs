using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeDebugger : MonoBehaviour
{
    public Vector3 MinTargetRange;
    public Vector3 MaxTargetRange;

    public int MaxEnemies = 3;
    public int EnemiesSpawned = 0;

    public float SpawnFrequencyMin;
    public float SpawnFrequencyMax;

    public float InitialDelay = 10f;

    public MMObjectPooler ObjectPooler { get; set; }

    protected float _lastSpawnAt = 0f;
    protected float _nextSpawnIn;

    protected virtual void Start()
    {
        ObjectPooler = GetComponent<MMMultipleObjectPooler>();
        _nextSpawnIn = InitialDelay;

        for (int i=0; i<MaxEnemies; i++)
        {
            Spawn();
        }
    }

    

    protected virtual void Spawn()
    {
        if (EnemiesSpawned >= MaxEnemies)
        {
            return;
        }

        _lastSpawnAt = Time.time;
        _nextSpawnIn = Random.Range(SpawnFrequencyMin, SpawnFrequencyMax);

        GameObject nextGameObject = ObjectPooler.GetPooledGameObject();
        if (nextGameObject != null)
        {
            nextGameObject.transform.position = MMMaths.RandomVector3(MinTargetRange, MaxTargetRange);
            nextGameObject.gameObject.SetActive(true);

            EnemiesSpawned++;
        }
        else
        {
            MMDebug.DebugLogTime("pooled object is null");
        }

    }
}
