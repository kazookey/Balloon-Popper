using Unity.Netcode;
using UnityEngine;

public class BalloonSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject balloonPrefab;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float xSpawnRange = 8f;
    [SerializeField] private float ySpawnHeight = 6f;

    private float timer = 0f;

    private void Update()
    {
        if (!IsServer || GameManager.Instance == null || !GameManager.Instance.isMatchActive.Value) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnBalloon();
        }
    }

    private void SpawnBalloon()
    {
        if (balloonPrefab == null) return;

        Vector3 spawnPos = new Vector3(Random.Range(-xSpawnRange, xSpawnRange), ySpawnHeight, 0f);
        
        GameObject balloonObj = Instantiate(balloonPrefab, spawnPos, Quaternion.identity);
        
        // 0, 1 = white (normal points, 40% chance)
        // 2 = red (slow rival, 20% chance)
        // 3 = blue (freeze rival, 20% chance)
        // 4 = yellow (speed self, 20% chance)
        Balloon balloonScript = balloonObj.GetComponent<Balloon>();
        if (balloonScript != null)
        {
            balloonScript.balloonType.Value = Random.Range(0, 5); 
        }

        NetworkObject netObj = balloonObj.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(true);
        }
    }
}