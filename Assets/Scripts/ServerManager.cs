using System;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using UnityEngine;


public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance;
    public XmlUnityServer XmlServer;
    public DarkRiftServer Server;
    public NetworkedPlayerController NetworkedPlayerControllerPrefab;
    public GameObject networkedPlayersRootNode;

    private Dictionary<ushort, NetworkedPlayerController> networkedPlayers = new Dictionary<ushort, NetworkedPlayerController>();
    private Queue<KeyValuePair<ushort, Message>> inputMessageQueue = new Queue<KeyValuePair<ushort, Message>>();

    void Start()
    {
        Server = XmlServer.Server;
        Server.ClientManager.ClientConnected += OnClientConnect;
        Server.ClientManager.ClientDisconnected += OnClientDisconnect;
    }
    void OnDestroy()
    {
        Server.ClientManager.ClientConnected -= OnClientConnect;
        Server.ClientManager.ClientDisconnected -= OnClientDisconnect;
    }
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    // Re-enable this, to enable client reconciliation and prediction. (Currently buggy)
    private void _Update()
    {
        while(inputMessageQueue.Count > 0)
        {
            var pair = inputMessageQueue.Dequeue();
            var ID = pair.Key;
            var msg = pair.Value.Deserialize<InputMessage>();
            
            var controller = networkedPlayers[ID];
            controller.Rotate(msg.inputs);
            controller.addForcesToPlayer(msg.inputs);

            Physics.Simulate(Time.fixedDeltaTime);

            using (var stateMsg = Message.Create((ushort)Tags.StateMessage, new StateMessage(controller.rb.position,
                controller.rb.rotation,
                controller.rb.velocity,
                controller.rb.angularVelocity,
                msg.tickNumber + 1))) 
            {
                Server.ClientManager.GetClient(ID).SendMessage(stateMsg, SendMode.Unreliable);
            }
        }
    }

    private void OnClientConnect(object sender, ClientConnectedEventArgs e)
    {
        e.Client.MessageReceived += OnMessage;
        var go = Instantiate(NetworkedPlayerControllerPrefab, networkedPlayersRootNode.transform);
        networkedPlayers.Add(e.Client.ID, go);
    }

    private void OnClientDisconnect(object sender, ClientDisconnectedEventArgs e)
    {
        e.Client.MessageReceived -= OnMessage;
        Destroy(networkedPlayers[e.Client.ID]);
        networkedPlayers.Remove(e.Client.ID);
    }

    private void OnMessage(object sender, MessageReceivedEventArgs e)
    {
        IClient client = (IClient)sender;
        using (Message m = e.GetMessage())
        {
            switch ((Tags)m.Tag)
            {
                case Tags.Input:
                    inputMessageQueue.Enqueue(new KeyValuePair<ushort, Message>(e.Client.ID, e.GetMessage()));
                    break;
                case Tags.Movement:
                    OnMovementMessageReceived(e.GetMessage());
                    break;
                default:
                    throw new NotImplementedException("Unknown message type received");
            }
        }
    }

    private void OnMovementMessageReceived(Message message)
    {
        var moveMsg = message.Deserialize<MovementMessage>();
        var players = Server.ClientManager.GetAllClients();
        foreach (var p in players)
        {
            // Proxy the message
            if(p.ID != moveMsg.ID)
                p.SendMessage(message, SendMode.Unreliable);
        }

        // Apply the message
        
        var player = networkedPlayers[moveMsg.ID];
        player.rb.position = moveMsg.position;
        player.rb.rotation = moveMsg.rotation;
        player.rb.velocity = moveMsg.velocity;
        player.rb.angularVelocity = moveMsg.angularVelocity;
        Physics.Simulate(Time.fixedDeltaTime);
    }
}
