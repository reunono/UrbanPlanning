using MoreMountains.LDJAM42;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Vector3 MinTargetRange;
    public Vector3 MaxTargetRange;

    public int MaxAtATime;
    public int EnemiesToSpawn = 15;
    public int EnemiesSpawned = 0;
    public int EnemiesAlive = 15;

    public float SpawnFrequencyMin;
    public float SpawnFrequencyMax;

    public float InitialDelay = 10f;

    public MMObjectPooler ObjectPooler { get; set; }

    protected float _lastSpawnAt = 0f;
    protected float _nextSpawnIn ;

    protected virtual void Start()
    {
        ObjectPooler = GetComponent<MMMultipleObjectPooler>();
        _nextSpawnIn = InitialDelay;
    }

    protected virtual void Update()
    {
        if (GameManager.Instance.GameState.CurrentState != GameStates.GameInProgress)
        {
            return;
        }

        if (EnemiesSpawned >= EnemiesToSpawn)
        {
            return;
        }

        if (GameManager.Instance.CurrentEnemies - GameManager.Instance.DeadEnemies >= MaxAtATime)
        {
            return;
        }

        if (Time.time - _lastSpawnAt > _nextSpawnIn)
        {
            Spawn();
        }
    }

    protected virtual void Spawn()
    {  
        


        if (GameManager.Instance.DeadEnemies >= GameManager.Instance.TotalEnemies)
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
            nextGameObject.GetComponent<MMPoolableObject>().TriggerOnSpawnComplete();
            EnemiesSpawned++;
        }
        else
        {
            MMDebug.DebugLogTime("pooled object is null");
        }

        //EditorApplication.isPaused = true;
    }
}
