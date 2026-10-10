using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoldierSpawner : MonoBehaviour
{
    public bool isGameOver;
    public Soldier soldierPrefab;
    public Transform player;
    public Transform planet;

    public List<Soldier> spawnedSoldiers = new List<Soldier>();
    public List<WaveParameters> waves = new List<WaveParameters>();
    public int currentWave;

    [Min(1)] public int MaxEnemyCount = 100;

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

            SpriteRenderer planetSprite = planet.GetComponent<SpriteRenderer>();
            if (planetSprite == null || planetSprite.bounds.extents.x <= 0f)
            {
                Debug.LogError("Spawner needs a planet SpriteRenderer with a positive radius.", this);
                yield break;
            }

            // Match the circular surface radius used by Soldier movement.
            float radius = planetSprite.bounds.extents.x;
            float spawnAngle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 spawnPosition = planet.position + new Vector3(
                Mathf.Cos(spawnAngle), Mathf.Sin(spawnAngle), 0f
            ) * radius;
            Quaternion spawnRotation = Quaternion.Euler(
                0f, 0f, spawnAngle * Mathf.Rad2Deg - 90f
            );

            Soldier soldier = Instantiate(
                soldierPrefab, spawnPosition, spawnRotation, planet
            );
            soldier.Init(planet, player);
            spawnedSoldiers.Add(soldier);

            if (i < wave.enemyCount - 1)
            {
                float interval =
                    Mathf.Max(0f, wave.spawnInterval) +
                    Random.Range(0f, 2f);

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