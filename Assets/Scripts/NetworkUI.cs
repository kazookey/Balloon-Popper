using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    private void OnGUI()
    {
        
        if (NetworkManager.Singleton == null) return;

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            
            if (GUILayout.Button("Start Host (Player 1)", GUILayout.Height(50)))
            {
                NetworkManager.Singleton.StartHost();
            }
            if (GUILayout.Button("Start Client (Player 2)", GUILayout.Height(50)))
            {
                NetworkManager.Singleton.StartClient();
            }
            if (GUILayout.Button("Start Server Only", GUILayout.Height(50)))
            {
                NetworkManager.Singleton.StartServer();
            }

            GUILayout.EndArea();
        }
        else
        {
            
            if (GameManager.Instance != null && !GameManager.Instance.isMatchActive.Value)
            {
                GUILayout.BeginArea(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 200), GUI.skin.window);
                GUILayout.Space(10);
                GUILayout.Label("<size=16><b>GAME OVER</b></size>");
                GUILayout.Label($"Player 1 Score: {GameManager.Instance.p1Score.Value}");
                GUILayout.Label($"Player 2 Score: {GameManager.Instance.p2Score.Value}");
                
                string result = GameManager.Instance.p1Score.Value > GameManager.Instance.p2Score.Value ? "Player 1 Wins!" : 
                               (GameManager.Instance.p2Score.Value > GameManager.Instance.p1Score.Value ? "Player 2 Wins!" : "It's a Tie!");
                GUILayout.Label($"<b>{result}</b>");
                GUILayout.EndArea();
            }
            else
            {
                
                GUILayout.BeginArea(new Rect(10, 10, 300, 100));
                string mode = NetworkManager.Singleton.IsHost ? "Host" : (NetworkManager.Singleton.IsServer ? "Server" : "Client");
                GUILayout.Label($"<b>Status: Connected as {mode}</b>");
                GUILayout.EndArea();
            }
        }
    }
}

