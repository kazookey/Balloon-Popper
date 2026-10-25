using Unity.Netcode;
using UnityEngine;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public NetworkVariable<int> p1Score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> p2Score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> matchTimer = new NetworkVariable<float>(60f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isMatchActive = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private TextMeshProUGUI uiText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        
        if (IsServer)
        {
            isMatchActive.Value = true;
            matchTimer.Value = 60f;
        }
    }

    private void Update()
    {
        
        if (uiText != null)
        {
            uiText.text = Mathf.CeilToInt(matchTimer.Value).ToString();
        }

        
        if (!IsServer || !isMatchActive.Value) return;

        if (matchTimer.Value > 0)
        {
            matchTimer.Value -= Time.deltaTime;
        }
        else
        {
            matchTimer.Value = 0;
            TriggerMatchEnd();
        }
    }

    private void TriggerMatchEnd()
    {
        isMatchActive.Value = false;
        int winner = p1Score.Value > p2Score.Value ? 1 : (p2Score.Value > p1Score.Value ? 2 : 0);
        EndMatchClientRpc(winner);
    }

    [Rpc(SendTo.Everyone)]
    private void EndMatchClientRpc(int winningPlayerId)
    {
        if (uiText != null)
        {
            uiText.text = winningPlayerId == 0 ? "Match Tied!" : $"Player {winningPlayerId} Wins!";
        }
    }
}