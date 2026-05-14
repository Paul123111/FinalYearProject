using LiteNetLib;
using Mirror;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

public class LiteNetLibTransport : Transport, INetEventListener {
    [Header("Connection Settings")]
    public ushort port = 7777;
    public string clientAddress = "localhost";
    public int disconnectTimeout = 30000;

    private NetManager _serverManager;
    private NetManager _clientManager;
    private NetPeer _clientPeer;

    // Allocate memory buffers upfront to avoid Garbage Collection allocations
    private readonly byte[] _sendBuffer = new byte[kcp2k.Kcp.MTU_DEF];

    public override bool Available() => true;
    public override int GetMaxPacketSize(int channelId = 0) => kcp2k.Kcp.MTU_DEF;

    private void Awake() {
        // Enforce background processing loops cleanly
        Application.runInBackground = true;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
    }

    private void Update() {
        // Continuously poll incoming events from the operating system socket layer
        _serverManager?.PollEvents();
        _clientManager?.PollEvents();
    }

    // Fixes the Mirror abstract compilation error by implementing the required ServerUri hook
    public override Uri ServerUri() {
        UriBuilder builder = new UriBuilder {
            Scheme = "lits", // Or whatever uniform custom string you prefer
            Host = clientAddress,
            Port = port
        };
        return builder.Uri;
    }


    private void OnDestroy() {
        Shutdown();
    }

    #region Client Methods
    public override void ClientConnect(string address) {
        clientAddress = address;
        _clientManager = new NetManager(this) { DisconnectTimeout = disconnectTimeout };
        _clientManager.Start();
        _clientPeer = _clientManager.Connect(clientAddress, port, "PlanetHunterConnectionToken");
    }

    public override bool ClientConnected() => _clientPeer != null && _clientPeer.ConnectionState == ConnectionState.Connected;

    public override void ClientDisconnect() {
        _clientManager?.Stop();
        _clientManager = null;
        _clientPeer = null;
    }

    public override void ClientSend(ArraySegment<byte> segment, int channelId = 0) {
        if (_clientPeer == null) return;
        DeliveryMethod method = (channelId == Channels.Reliable) ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable;
        _clientPeer.Send(segment.Array, segment.Offset, segment.Count, method);
    }
    #endregion

    #region Server Methods
    public override bool ServerActive() => _serverManager != null && _serverManager.IsRunning;

    public override void ServerStart() {
        _serverManager = new NetManager(this) { DisconnectTimeout = disconnectTimeout };
        _serverManager.Start(port);
        Debug.Log($"LiteNetLib Dedicated Server listening dynamically on UDP Port: {port}");
    }

    public override void ServerStop() {
        _serverManager?.Stop();
        _serverManager = null;
    }

    public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId = 0) {
        if (_serverManager == null) return;
        LiteNetPeer targetPeer = _serverManager.GetPeerById(connectionId);
        if (targetPeer == null) return;

        DeliveryMethod method = (channelId == Channels.Reliable) ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable;
        targetPeer.Send(segment.Array, segment.Offset, segment.Count, method);
    }

    public override string ServerGetClientAddress(int connectionId) {
        if (_serverManager == null) return string.Empty;
        LiteNetPeer peer = _serverManager.GetPeerById(connectionId);
        return peer?.Address.ToString() ?? string.Empty;
    }

    public override void ServerDisconnect(int connectionId) {
        if (_serverManager == null) return;
        LiteNetPeer peer = _serverManager.GetPeerById(connectionId);
        if (peer == null) return;
        _serverManager.DisconnectPeer(peer);
    }
    #endregion

    #region LiteNetLib INetEventListener Implementation
    public void OnPeerConnected(NetPeer peer) {
        if (_serverManager != null && peer.NetManager == _serverManager)
            OnServerConnected.Invoke(peer.Id);
        else
            OnClientConnected.Invoke();
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) {
        if (_serverManager != null && peer.NetManager == _serverManager)
            OnServerDisconnected.Invoke(peer.Id);
        else
            OnClientDisconnected.Invoke();
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod) {
        int length = reader.AvailableBytes;
        reader.GetBytes(_sendBuffer, length);
        ArraySegment<byte> segment = new ArraySegment<byte>(_sendBuffer, 0, length);

        if (_serverManager != null && peer.NetManager == _serverManager)
            OnServerDataReceived.Invoke(peer.Id, segment, (deliveryMethod == DeliveryMethod.ReliableOrdered) ? Channels.Reliable : Channels.Unreliable);
        else
            OnClientDataReceived.Invoke(segment, (deliveryMethod == DeliveryMethod.ReliableOrdered) ? Channels.Reliable : Channels.Unreliable);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError) {
        Debug.LogError($"[LiteNetLib Transport Error]: {socketError} from {endPoint}");
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
    public void OnConnectionRequest(ConnectionRequest request) => request.AcceptIfKey("PlanetHunterConnectionToken");
    #endregion

    public override void Shutdown() {
        ServerStop();
        ClientDisconnect();
    }
}

