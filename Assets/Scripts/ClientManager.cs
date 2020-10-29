using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance;

    public UnityClient client;
    public ClientPlayerController clientPlayer;
    public NetworkedPlayerController networkedPlayerPrefab;
    public GameObject networkedPlayersRootNode;

    private Dictionary<ushort, NetworkedPlayerController> networkedPlayers = new Dictionary<ushort, NetworkedPlayerController>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        client.MessageReceived += OnMessageReceived;
        DontDestroyOnLoad(this);
        clientPlayer.gameObject.SetActive(true);
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        switch ((Tags)e.Tag) {
            case Tags.StateMessage:
                clientPlayer.stateMessages.Enqueue(e.GetMessage().Deserialize<StateMessage>());
                break;
            case Tags.Movement:
                OnMovementMessageReceived(e.GetMessage());
                break;

            default:
                throw new NotImplementedException("Unimplemented message received");
        }
    }

    private void OnMovementMessageReceived(Message message)
    {
        var moveMsg = message.Deserialize<MovementMessage>();
        if (!networkedPlayers.ContainsKey(moveMsg.ID))
            SpawnNetworkedPlayer(moveMsg);

        var player = networkedPlayers[moveMsg.ID];
        player.rb.position = moveMsg.position;
        player.rb.rotation = moveMsg.rotation;
        player.rb.velocity = moveMsg.velocity;
        player.rb.angularVelocity = moveMsg.angularVelocity;
    }

    private void SpawnNetworkedPlayer(MovementMessage moveMsg)
    {
        var go = Instantiate(networkedPlayerPrefab, networkedPlayersRootNode.transform);
        networkedPlayers.Add(moveMsg.ID, go);
    }
}
