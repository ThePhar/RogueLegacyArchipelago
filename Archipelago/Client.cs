﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using WebSocketSharp;

namespace Archipelago
{
    public class Client
    {
        public const int MAXIMUM_RECONNECTION_ATTEMPTS = 3;
        public const string MINIMUM_AP_VERSION = "0.2.2";

        private bool _allowReconnect = false;
        private DeathLinkService _deathLinkService = null;
        private Dictionary<string, Permissions> _permissions = new();
        private int _reconnectionAttempt = 0;
        private string _seed = "0";
        private ArchipelagoSession _session = null;
        private List<string> _tags = new() { "AP" };

        public Client()
        {
            Initialize();
        }

        public ConnectionStatus ConnectionStatus { get; private set; } = ConnectionStatus.Disconnected;
        public DateTime LastDeath { get; private set; } = DateTime.MinValue;
        public ConnectionInfo CachedConnectionInfo { get; private set; } = new();
        public DeathLink DeathLink { get; private set; } = null;
        public Dictionary<int, NetworkItem> LocationCache { get; private set; } = new();
        public SlotData Data { get; private set; } = null;
        public Queue<NetworkItem> ItemQueue { get; private set; } = new();
        public List<int> CheckedLocations { get; private set; } = new();
        public bool CanForfeit => _permissions["forfeit"] is Permissions.Goal or Permissions.Enabled;

        public void Connect(ConnectionInfo info)
        {
            // Cache our connection info in case we get disconnected later.
            CachedConnectionInfo = info;

            // Disconnect from any session we are currently in if we are attempting to connect.
            if (_session != null)
            {
                Disconnect();
            }

            ConnectionStatus = ConnectionStatus.Connecting;
            try
            {
                _session = ArchipelagoSessionFactory.CreateSession(info.Hostname, info.Port);

                // Establish event handlers.
                _session.Socket.SocketClosed += OnSocketDisconnect;
                _session.Socket.ErrorReceived += OnError;
                _session.Items.ItemReceived += OnReceivedItems;
                _session.Socket.PacketReceived += OnPacketReceived;

                // Attempt to connect to the AP server.
                var result = _session.TryConnectAndLogin("Rogue Legacy", info.Name, new Version(MINIMUM_AP_VERSION), _tags, password: info.Password);

                if (result.Successful)
                {
                    ConnectionStatus = ConnectionStatus.Connected;
                    _reconnectionAttempt = 0;
                    return;
                }

                var failure = (LoginFailure) result;
                throw new ArchipelagoSocketClosedException(failure.Errors[0]);
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        public void Disconnect()
        {
            ConnectionStatus = ConnectionStatus.Disconnecting;

            // Clear DeathLink handlers.
            if (_deathLinkService != null)
            {
                _deathLinkService.OnDeathLinkReceived -= OnDeathLink;
            }

            // Clear session handlers.
            if (_session != null)
            {
                _session.Socket.SocketClosed -= OnSocketDisconnect;
                _session.Socket.ErrorReceived -= OnError;
                _session.Items.ItemReceived -= OnReceivedItems;
                _session.Socket.PacketReceived -= OnPacketReceived;
                _session.Socket.Disconnect();
            }

            // Reset all values back to their defaults.
            Initialize();
        }

        private void Initialize()
        {
            _session = null;
            _deathLinkService = null;
            _permissions = new();
            _tags = new() { "AP" };
            _allowReconnect = false;
            _reconnectionAttempt = 0;
            _seed = "0";

            ConnectionStatus = ConnectionStatus.Disconnected;
            CheckedLocations = new();
            LastDeath = DateTime.MinValue;
            DeathLink = null;
            LocationCache = new();
            Data = null;
            ItemQueue = new();
        }

        public void Forfeit()
        {
            // Not sure if there's a better way to do this, but I know this works!
            _session.Socket.SendPacket(new SayPacket { Text = "!forfeit" });
        }

        public void AnnounceVictory()
        {
            _session.Socket.SendPacket(new StatusUpdatePacket { Status = ArchipelagoClientState.ClientGoal });
        }

        public void ClearDeathLink()
        {
            if (_deathLinkService != null)
            {
                DeathLink = null;
            }
        }

        public void SendDeathLink(string cause)
        {
            // Log our current time so we can make sure we ignore our own DeathLink.
            LastDeath = DateTime.Now;

            if (!Data.DeathLink || _deathLinkService == null)
            {
                return;
            }

            var causeWithPlayerName = $"{_session.Players.GetPlayerAlias(Data.Slot)}'s {cause}.";
            _deathLinkService.SendDeathLink(new DeathLink(_session.Players.GetPlayerAlias(Data.Slot), causeWithPlayerName) { Timestamp = LastDeath });
        }

        public void CheckLocations(params int[] locations)
        {
            _session.Locations.CompleteLocationChecks(locations);
        }

        public string GetPlayerName(int slot)
        {
            var name = _session.Players.GetPlayerAlias(slot);
            return string.IsNullOrEmpty(name) ? "Archipelago" : name;
        }

        public string GetItemName(int item)
        {
            var name = _session.Items.GetItemName(item);
            return string.IsNullOrEmpty(name) ? "Unknown Item" : name;
        }

        private void OnSocketDisconnect(CloseEventArgs closeEventArgs)
        {
            // Check to see if we are still in a game, and attempt to reconnect if possible.
            switch (ConnectionStatus)
            {
                // We were failing to connect.
                case ConnectionStatus.Connecting:
                    if (!_allowReconnect)
                    {
                        throw new ArchipelagoSocketClosedException("Unable to establish connection to AP server.");
                    }

                    if (_reconnectionAttempt >= MAXIMUM_RECONNECTION_ATTEMPTS)
                    {
                        throw new ArchipelagoSocketClosedException(
                            "Lost connection to AP server and failed to reconnect. Please save and quit to title " +
                            "screen and attempt to reconnect as client is no longer syncing."
                        );
                    }

                    _reconnectionAttempt += 1;
                    ConnectionStatus = ConnectionStatus.Connecting;
                    Connect(CachedConnectionInfo);
                    break;

                // We're in a current game and lost connection, so attempt to reconnect gracefully.
                case ConnectionStatus.Connected:
                    _allowReconnect = true;

                    // Ignore this is a goto. Thanks for playing.
                    goto case ConnectionStatus.Connecting;
            }
        }

        private void OnReceivedItems(ReceivedItemsHelper helper)
        {
            ItemQueue.Enqueue(helper.DequeueItem());
        }

        private void OnDeathLink(DeathLink deathLink)
        {
            var newDeathLink = deathLink.Timestamp.ToString(CultureInfo.InvariantCulture);
            var oldDeathLink = deathLink.Timestamp.ToString(CultureInfo.InvariantCulture);

            // Ignore deaths that died at the same time as us. Should also prevent the player from dying to themselves.
            if (newDeathLink != oldDeathLink)
            {
                DeathLink = deathLink;
            }
        }

        private void OnPacketReceived(ArchipelagoPacketBase packet)
        {
            Console.WriteLine($"Received a {packet.GetType().Name} packet");
            Console.WriteLine("==========================================");
            foreach (var property in packet.GetType().GetProperties())
            {
                Console.WriteLine($"{property.Name}: {property.GetValue(packet)}");
            }

            Console.WriteLine();

            switch (packet)
            {
                case RoomUpdatePacket roomUpdatePacket:
                    OnRoomUpdate(roomUpdatePacket);
                    break;

                case RoomInfoPacket roomInfoPacket:
                    OnRoomInfo(roomInfoPacket);
                    break;

                case ConnectedPacket connectedPacket:
                    OnConnected(connectedPacket);
                    break;

                case PrintPacket printPacket:
                    OnPrint(printPacket);
                    break;
            }
        }

        private void OnRoomInfo(RoomInfoPacket packet)
        {
            _seed = packet.SeedName;
            _permissions = packet.Permissions;

            // Send this so we have a cache of item/location names.
            _session.Socket.SendPacket(new GetDataPackagePacket());
        }

        private void OnRoomUpdate(RoomUpdatePacket packet)
        {
            if (packet.CheckedLocations != null)
            {
                CheckedLocations = packet.CheckedLocations;
            }
        }

        private void OnConnected(ConnectedPacket packet)
        {
            Data = new SlotData(packet.SlotData, _seed, packet.Slot, CachedConnectionInfo.Name);

            // Check if DeathLink is enabled and establish the appropriate helper.
            if (Data.DeathLink)
            {
                _tags.Add("DeathLink");
                _session.UpdateTags(_tags);

                // Clear old DeathLink handlers.
                if (_deathLinkService != null)
                {
                    _deathLinkService.OnDeathLinkReceived -= OnDeathLink;
                }

                _deathLinkService = _session.CreateDeathLinkServiceAndEnable();
                _deathLinkService.OnDeathLinkReceived += OnDeathLink;
            }

            // Mark our checked locations.
            CheckedLocations = packet.LocationsChecked;

            // Build our location cache.
            var locations = LocationDefinitions.GetAllLocations(Data).Select(location => location.Code);
            _session.Locations.ScoutLocationsAsync(OnReceiveLocationCache, locations.ToArray());
        }

        private void OnReceiveLocationCache(LocationInfoPacket packet)
        {
            foreach (var item in packet.Locations)
            {
                LocationCache.Add(item.Location, item);
            }
        }

        private static void OnError(Exception exception, string message)
        {
            Console.WriteLine("Received an unhandled exception in ArchipelagoClient: {0}\n\n{1}", message, exception);
        }

        private static void OnPrint(PrintPacket packet)
        {
            Console.WriteLine("AP Server: {0}", packet.Text);
        }
    }
}