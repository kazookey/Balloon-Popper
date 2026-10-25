using Unity.Netcode;
using UnityEngine;

public class Balloon : NetworkBehaviour
{
    [SerializeField] private float fallSpeed = 3f;
    
  
    public NetworkVariable<int> balloonType = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Audio SFX per Balloon Type")]
    [SerializeField] private AudioClip normalSfx;   // white (normal points) SFX
    [SerializeField] private AudioClip redSfx;      // red debuff SFX
    [SerializeField] private AudioClip blueSfx;     // blue debuff SFX
    [SerializeField] private AudioClip yellowSfx;   // yellow power-up SFX

    public override void OnNetworkSpawn()
    {
        balloonType.OnValueChanged += (oldVal, newVal) => ApplyColor(newVal);
        ApplyColor(balloonType.Value);

        if (IsServer)
        {
            Destroy(gameObject, 10f);
        }
    }

    private void ApplyColor(int type)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (type == 0 || type == 1) spriteRenderer.color = Color.white; 
            else if (type == 2) spriteRenderer.color = Color.red;           
            else if (type == 3) spriteRenderer.color = Color.blue;          
            else if (type == 4) spriteRenderer.color = Color.yellow;        
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out NetworkObject playerNetObj))
        {
            if (playerNetObj.IsOwner)
            {
                RequestCollectBalloonServerRpc(balloonType.Value, playerNetObj.OwnerClientId);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestCollectBalloonServerRpc(int type, ulong collectorClientId)
    {
        if (!GameManager.Instance.isMatchActive.Value) return;

        
        if (collectorClientId == 0) GameManager.Instance.p1Score.Value += 1;
        else GameManager.Instance.p2Score.Value += 1; 

       
        PlayPickupEffectsClientRpc(type, transform.position, collectorClientId);

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void PlayPickupEffectsClientRpc(int type, Vector3 pos, ulong collectorClientId)
    {
        
        AudioClip clipToPlay = null;
        if (type == 0 || type == 1) clipToPlay = normalSfx;
        else if (type == 2) clipToPlay = redSfx;
        else if (type == 3) clipToPlay = blueSfx;
        else if (type == 4) clipToPlay = yellowSfx;

        if (clipToPlay != null)
        {
            AudioSource.PlayClipAtPoint(clipToPlay, pos, 1.0f);
        }

        // 2. Apply Speed / Sabotage logic
        if (type >= 2)
        {
            PlayerController2D[] players = FindObjectsByType<PlayerController2D>(FindObjectsSortMode.None);

            foreach (var player in players)
            {
                bool isCollector = player.OwnerClientId == collectorClientId;

                if (type == 4 && isCollector)
                {
                    player.ApplySpeedMultiplier(1.5f, 3f); // yellow: speed slef
                }
                else if (type == 2 && !isCollector)
                {
                    player.ApplySpeedMultiplier(0.5f, 3f); // red: slow rival
                }
                else if (type == 3 && !isCollector)
                {
                    player.ApplySpeedMultiplier(0f, 1.5f);  // blue: freeze rival
                }
            }
        }
    }
}