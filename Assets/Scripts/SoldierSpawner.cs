using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoldierSpawner : MonoBehaviour
{
    public Soldier soldierPrefab;
    public Transform player;
    public Transform planet;
    public List<WaveParameters> waves = new List<WaveParameters>();
    
    [HideInInspector]public bool isGameOver;
    [HideInInspector] public List<Soldier> spawnedSoldiers = new List<Soldier>();
    [HideInInspector] public  int currentWave;
    [HideInInspector] public int MaxEnemyCount = 100;

    private IEnumerator Start()
    {
        if (soldierPrefab == null || player == null || planet == null ||
            waves == null || waves.Count == 0)
        {
            Debug.LogError("Spawner is missing references or waves.", this);
            yield break;
        }

        currentWave = 0;

        while (!isGameOver)
        {
            WaveParameters wave = waves[currentWave];

            // Wait for this wave to finish spawning.
            yield return SpawnWave(wave);

            if (isGameOver)
                yield break;

            yield return new WaitForSeconds(
                Mathf.Max(0f, wave.spawnDelay)
            );

            // Stay on the last wave once all earlier waves have run.
            if (currentWave < waves.Count - 1)
                currentWave++;
        }
    }

    public IEnumerator SpawnWave(WaveParameters wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            // Remove destroyed enemies and wait for an available slot.
            while (!isGameOver)
            {
                spawnedSoldiers.RemoveAll(soldier => soldier == null);

                if (spawnedSoldiers.Count < Mathf.Max(1, MaxEnemyCount))
                    break;

                yield return null;
            }

            if (isGameOver)
                yield break;

            CircleCollider2D planetCollider = planet.GetComponent<CircleCollider2D>();
            if (planetCollider == null || planetCollider.radius <= 0f)
            {
                Debug.LogError("Spawner needs a planet CircleCollider2D with a positive radius.", this);
                yield break;
            }

            // Match the circular surface radius used by Soldier movement.
            Vector3 planetScale = planetCollider.transform.lossyScale;
            float radius = planetCollider.radius * Mathf.Max(Mathf.Abs(planetScale.x), Mathf.Abs(planetScale.y));
            if (radius <= 0f)
            {
                Debug.LogError("Planet collider has a zero world radius.", this);
                yield break;
            }
            Vector3 center = planetCollider.transform.TransformPoint(planetCollider.offset);
            float spawnAngle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 spawnPosition = center + new Vector3(
                Mathf.Cos(spawnAngle), Mathf.Sin(spawnAngle), 0f
            ) * radius;
            Quaternion spawnRotation = Quaternion.Euler(
                0f, 0f, spawnAngle * Mathf.Rad2Deg - 90f
            );

            Soldier soldier = Instantiate(
                soldierPrefab, spawnPosition, spawnRotation, transform
            );
            soldier.Init(planet, player);
            spawnedSoldiers.Add(soldier);

            if (i < wave.enemyCount - 1)
            {
                float interval =
                    Mathf.Max(0f, wave.spawnInterval) +
                    Random.Range(0f, wave.spawnInterval*0.1f);

                yield return new WaitForSeconds(interval);
            }
        }
    }
}

[Serializable]
public struct WaveParameters
{
    [Min(1)] public int enemyCount;
    [Min(0f)] public float spawnInterval;

    [Tooltip("Seconds after this wave finishes spawning.")]
    [Min(0f)] public float spawnDelay;
}