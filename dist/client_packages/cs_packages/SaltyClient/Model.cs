﻿// Copyright (c) 2019 saltmine.de - https://github.com/saltminede

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaltyClient
{
    #region GameInstance
    /// <summary>
    /// Used for <see cref="Command.Initiate"/>
    /// </summary>
    public class GameInstance
    {
        #region Properties
        /// <summary>
        /// Unique id of the server the player must be connected to
        /// </summary>
        public string ServerUniqueIdentifier { get; set; }

        /// <summary>
        /// TeamSpeak name that should be set (max length is 30)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Id of the TeamSpeak channel the player should be moved to
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// Password of the TeamSpeak channel
        /// </summary>
        public string ChannelPassword { get; set; }

        /// <summary>
        /// Foldername of the sound pack that will be used (%AppData%\TS3Client\Plugins\SaltyChat\{SoundPack}\)
        /// </summary>
        public string SoundPack { get; set; }

        /// <summary>
        /// IDs of channels which the player can join, while the game instace is running
        /// </summary>
        public ulong[] SwissChannelIds { get; set; }
        #endregion

        #region CTOR
        public GameInstance(string serverUniqueIdentifier, string name, ulong channelId, string channelPassword, string soundPack, ulong[] swissChannels)
        {
            this.ServerUniqueIdentifier = serverUniqueIdentifier;
            this.Name = name;
            this.ChannelId = channelId;
            this.ChannelPassword = channelPassword;
            this.SoundPack = soundPack;
            this.SwissChannelIds = swissChannels;
        }
        #endregion
    }
    #endregion

    #region PluginError
    public class PluginError
    {
        public Error Error { get; set; }
        public string Message { get; set; }
        public string ServerIdentifier { get; set; }
    }
    #endregion

    #region PluginState
    /// <summary>
    /// Will be received from the WebSocket if e.g. the mic muted/unmuted
    /// </summary>
    public class PluginState
    {
        public string UpdateBranch { get; set; }
        public string Version { get; set; }
        public bool IsConnectedToServer { get; set; }
        public bool IsReady { get; set; }
        public bool IsTalking { get; set; }
        public bool IsMicrophoneMuted { get; set; }
        public bool IsSoundMuted { get; set; }
    }
    #endregion

    #region PluginCommand
    public class PluginCommand
    {
        #region Properties
        public Command Command { get; set; }
        public string ServerUniqueIdentifier { get; set; }
        public Newtonsoft.Json.Linq.JObject Parameter { get; set; }
        #endregion

        #region CTOR
        /// <summary>
        /// For deserialization only
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        internal PluginCommand()
        {

        }

        /// <summary>
        /// Use this for <see cref="Command.Pong"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        internal PluginCommand(string serverUniqueIdentifier)
        {
            this.Command = Command.Pong;
            this.ServerUniqueIdentifier = serverUniqueIdentifier;
        }

        /// <summary>
        /// Use this with <see cref="Command.Initiate"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        internal PluginCommand(Command command, object parameter)
        {
            this.Command = command;
            this.Parameter = Newtonsoft.Json.Linq.JObject.FromObject(parameter);
        }

        internal PluginCommand(Command command, string serverUniqueIdentifier, object parameter)
        {
            this.Command = command;
            this.ServerUniqueIdentifier = serverUniqueIdentifier;
            this.Parameter = Newtonsoft.Json.Linq.JObject.FromObject(parameter);
        }
        #endregion

        #region Methods
        public string Serialize()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static PluginCommand Deserialize(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PluginCommand>(json);
        }

        public bool TryGetState(out PluginState pluginState)
        {
            if (this.Command == Command.StateUpdate)
            {
                try
                {
                    pluginState = this.Parameter.ToObject<PluginState>();

                    return true;
                }
                catch
                {
                    // do nothing
                }
            }

            pluginState = default;
            return false;
        }
        #endregion

        #region Conditional Property Serialization
        public bool ShouldSerializeParameter() => this.Parameter != null;
        #endregion
    }
    #endregion

    #region PlayerState
    /// <summary>
    /// Used for <see cref="Command.SelfStateUpdate"/> and <see cref="Command.PlayerStateUpdate"/>
    /// </summary>
    public class PlayerState
    {
        #region Properties
        public string Name { get; set; }
        public TSVector Position { get; set; }
        public float? Rotation { get; set; }
        public float? VoiceRange { get; set; }
        public bool IsAlive { get; set; }
        public float? VolumeOverride { get; set; }
        public bool DistanceCulled { get; set; }
        #endregion

        #region CTOR
        /// <summary>
        /// Used for <see cref="Command.RemovePlayer"/>
        /// </summary>
        /// <param name="name"></param>
        public PlayerState(string name)
        {
            this.Name = name;
            this.Position = TSVector.Zero;
        }

        /// <summary>
        /// Used for <see cref="Command.SelfStateUpdate"/>
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public PlayerState(RAGE.Vector3 position, float rotation)
        {
            this.Position = new TSVector(position); // Needs to be converted to "SaltyChat.TSVector" due to a bug - client will crash if "RAGE.Vector3" will be serialized with "Newtonsoft.Json.Linq.JObject.FromObject()"
            this.Rotation = rotation;
        }

        /// <summary>
        /// Used for <see cref="Command.PlayerStateUpdate"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="voiceRange"></param>
        /// <param name="isAlive"></param>
        public PlayerState(string name, RAGE.Vector3 position, float voiceRange, bool isAlive)
        {
            this.Name = name;
            this.Position = new TSVector(position); // Needs to be converted to "SaltyChat.TSVector" due to a bug - client will crash if "RAGE.Vector3" will be serialized with "Newtonsoft.Json.Linq.JObject.FromObject()"
            this.VoiceRange = voiceRange;
            this.IsAlive = isAlive;
        }

        /// <summary>
        /// Used for <see cref="Command.PlayerStateUpdate"/> with volume override
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="voiceRange"></param>
        /// <param name="isAlive"></param>
        /// <param name="volumeOverride">Overrides the volume (phone, radio and proximity) - from 0 (0%) to 1.5 (150%)</param>
        public PlayerState(string name, RAGE.Vector3 position, float voiceRange, bool isAlive, float volumeOverride)
        {
            this.Name = name;
            this.Position = new TSVector(position); // Needs to be converted to "SaltyChat.TSVector" due to a bug - client will crash if "RAGE.Vector3" will be serialized with "Newtonsoft.Json.Linq.JObject.FromObject()"
            this.VoiceRange = voiceRange;
            this.IsAlive = isAlive;

            if (volumeOverride > 1.5f)
                this.VolumeOverride = 1.5f;
            else if (volumeOverride < 0f)
                this.VolumeOverride = 0f;
            else
                this.VolumeOverride = volumeOverride;
        }
        #endregion

        #region Conditional Property Serialization
        public bool ShouldSerializeName() => !String.IsNullOrEmpty(this.Name);

        public bool ShouldSerializeRotation() => this.Rotation.HasValue;

        public bool ShouldSerializeVoiceRange() => this.VoiceRange.HasValue;

        public bool ShouldSerializeIsAlive() => this.IsAlive;

        public bool ShouldSerializeVolumeOverride() => this.VolumeOverride.HasValue;

        public bool ShouldSerializeDistanceCulled() => this.DistanceCulled;
        #endregion
    }
    #endregion

    #region Phone
    /// <summary>
    /// Used for <see cref="Command.PhoneCommunicationUpdate"/> and <see cref="Command.StopPhoneCommunication"/>
    /// </summary>
    public class PhoneCommunication
    {
        #region Properties
        public string Name { get; set; }
        public int? SignalStrength { get; set; }
        public float? Volume { get; set; }

        public bool Direct { get; set; }
        public string[] RelayedBy { get; set; }
        #endregion

        #region CTOR
        public PhoneCommunication(string name)
        {
            this.Name = name;
        }

        public PhoneCommunication(string name, int signalStrength)
        {
            this.Name = name;
            this.SignalStrength = signalStrength;

            this.Direct = true;
        }

        public PhoneCommunication(string name, int signalStrength, float volume)
        {
            this.Name = name;
            this.SignalStrength = signalStrength;
            this.Volume = volume;

            this.Direct = true;
        }

        public PhoneCommunication(string name, int signalStrength, bool direct, string[] relayedBy)
        {
            this.Name = name;
            this.SignalStrength = signalStrength;

            this.Direct = direct;
            this.RelayedBy = relayedBy;
        }

        public PhoneCommunication(string name, int signalStrength, float volume, bool direct, string[] relayedBy)
        {
            this.Name = name;
            this.SignalStrength = signalStrength;
            this.Volume = volume;

            this.Direct = direct;
            this.RelayedBy = relayedBy;
        }
        #endregion

        #region Conditional Property Serialization
        public bool ShouldSerializeSignalStrength() => this.SignalStrength.HasValue;

        public bool ShouldSerializeVolume() => this.Volume.HasValue;

        public bool ShouldSerializeDirect() => this.Direct;

        public bool ShouldSerializeRelayedBy() => this.RelayedBy != null && this.RelayedBy.Length > 0;
        #endregion
    }
    #endregion

    #region Radio
    /// <summary>
    /// Used for <see cref="Command.RadioTowerUpdate"/>
    /// </summary>
    public class RadioTower
    {
        #region Properties
        public TSVector[] Towers { get; set; }
        #endregion

        #region CTOR
        public RadioTower(params TSVector[] towers)
        {
            this.Towers = towers;
        }

        public RadioTower(params RAGE.Vector3[] towers)
        {
            this.Towers = towers.Select(t => new TSVector(t)).ToArray();
        }
        #endregion
    }

    /// <summary>
    /// Used for <see cref="Command.RadioCommunicationUpdate"/> and <see cref="Command.StopRadioCommunication"/>
    /// </summary>
    public class RadioCommunication
    {
        #region Properties
        public string Name { get; set; }
        public RadioType SenderRadioType { get; set; }
        public RadioType OwnRadioType { get; set; }
        public bool PlayMicClick { get; set; }
        public float? Volume { get; set; }

        public bool Direct { get; set; }
        public bool Secondary { get; set; }
        public string[] RelayedBy { get; set; }
        #endregion

        #region CTOR
        public RadioCommunication(string name, RadioType senderRadioType, RadioType ownRadioType, bool playMicClick, bool isSecondary)
        {
            this.Name = name;
            this.SenderRadioType = senderRadioType;
            this.OwnRadioType = ownRadioType;
            this.PlayMicClick = playMicClick;

            this.Direct = true;
            this.Secondary = isSecondary;
        }

        public RadioCommunication(string name, RadioType senderRadioType, RadioType ownRadioType, bool playMicClick, bool direct, bool isSecondary, string[] relayedBy)
        {
            this.Name = name;
            this.SenderRadioType = senderRadioType;
            this.OwnRadioType = ownRadioType;
            this.PlayMicClick = playMicClick;

            this.Direct = direct;
            this.Secondary = isSecondary;
            this.RelayedBy = relayedBy;
        }
        #endregion

        #region Conditional Property Serialization
        public bool ShouldSerializePlayMicClick() => this.PlayMicClick;

        public bool ShouldSerializeVolume() => this.Volume.HasValue;

        public bool ShouldSerializeDirect() => this.Direct;

        public bool ShouldSerializeSecondary() => this.Secondary;

        public bool ShouldSerializeRelayedBy() => this.RelayedBy != null && this.RelayedBy.Length > 0;
        #endregion
    }

    [Flags]
    public enum RadioType
    {
        /// <summary>
        /// No radio communication
        /// </summary>
        None = 1,

        /// <summary>
        /// Short range radio communication - appx. 3 kilometers
        /// </summary>
        ShortRange = 2,

        /// <summary>
        /// Long range radio communication - appx. 8 kilometers
        /// </summary>
        LongRange = 4,

        /// <summary>
        /// Distributed radio communication, depending on <see cref="RadioTower"/> - appx. 1.8 (ultra short range),  appx. 3 (short range) or 8 (long range) kilometers
        /// </summary>
        Distributed = 8,

        /// <summary>
        /// Ultra short range radio communication - appx. 1.8 kilometers
        /// </summary>
        UltraShortRange = 16,
    }
    #endregion

    #region Sound
    /// <summary>
    /// Used for <see cref="Command.PlaySound"/>
    /// </summary>
    public class Sound
    {
        #region Properties
        public string Filename { get; set; }
        public bool IsLoop { get; set; }
        public string Handle { get; set; }
        #endregion

        #region CTOR
        public Sound(string filename)
        {
            this.Filename = filename;
            this.Handle = filename;
        }

        public Sound(string filename, bool loop)
        {
            this.Filename = filename;
            this.IsLoop = loop;
            this.Handle = filename;
        }

        public Sound(string filename, bool loop, string handle)
        {
            this.Filename = filename;
            this.IsLoop = loop;
            this.Handle = handle;
        }
        #endregion
    }
    #endregion

    #region Command
    public enum Command
    {
        /// <summary>
        /// Will be sent by the WebSocket when resetting the instance
        /// </summary>
        Reset = -1,

        /// <summary>
        /// Use <see cref="GameInstance"/> as parameter
        /// </summary>
        Initiate = 0,

        /// <summary>
        /// Will be sent by the WebSocket and should be answered with a <see cref="Command.Pong"/>
        /// </summary>
        Ping = 1,

        /// <summary>
        /// Answer to a <see cref="Command.Ping"/> request
        /// </summary>
        Pong = 2,

        /// <summary>
        /// Will be sent by the WebSocket on state changes (e.g. mic muted/unmuted) and received by <see cref="VoiceManager.OnPluginMessage(object[])"/> - uses <see cref="PluginState"/> as parameter
        /// </summary>
        StateUpdate = 3,

        /// <summary>
        /// Use <see cref="PlayerState"/> as parameter
        /// </summary>
        SelfStateUpdate = 4,

        /// <summary>
        /// Use <see cref="PlayerState"/> as parameter
        /// </summary>
        PlayerStateUpdate = 5,

        /// <summary>
        /// Use <see cref="PlayerState"/> as parameter
        /// </summary>
        RemovePlayer = 6,

        /// <summary>
        /// Use <see cref="PhoneCommunication"/> as parameter
        /// </summary>
        PhoneCommunicationUpdate = 7,

        /// <summary>
        /// Use <see cref="PhoneCommunication"/> as parameter
        /// </summary>
        StopPhoneCommunication = 8,

        /// <summary>
        /// Use <see cref="RadioTower"/> as parameter
        /// </summary>
        RadioTowerUpdate = 9,

        /// <summary>
        /// Use <see cref="RadioCommunication"/> as parameter
        /// </summary>
        RadioCommunicationUpdate = 10,

        /// <summary>
        /// Use <see cref="RadioCommunication"/> as parameter
        /// </summary>
        StopRadioCommunication = 11,

        /// <summary>
        /// Use <see cref="Sound"/> as parameter
        /// </summary>
        PlaySound = 12,

        /// <summary>
        /// Use <see cref="Sound"/> as parameter
        /// </summary>
        StopSound = 13
    }
    #endregion

    #region Error
    public enum Error
    {
        OK = 0,
        InvalidJson = 1,
        NotConnectedToServer = 2,
        AlreadyInGame = 3,
        ChannelNotAvailable = 4,
        NameNotAvailable = 5,
        InvalidValue = 6
    }
    #endregion

    #region UpdateBranch
    internal enum UpdateBranch
    {
        Stable = 0,
        Testing = 1,
        PreBuild = 2
    }
    #endregion

    #region SoundEventArgs
    public class SoundEventArgs : EventArgs
    {
        public bool IsTalking => VoiceManager.IsTalking;
        public bool IsMicrophoneMuted => VoiceManager.IsMicrophoneMuted;
        public bool IsSoundMuted => VoiceManager.IsSoundMuted;
    }
    #endregion

    #region TSVector
    public class TSVector
    {
        public float X;
        public float Y;
        public float Z;

        public static readonly TSVector Zero = new TSVector(0f, 0f, 0f);

        [Newtonsoft.Json.JsonConstructor]
        public TSVector()
        {

        }

        public TSVector(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public TSVector(RAGE.Vector3 position)
        {
            this.X = position.X;
            this.Y = position.Y;
            this.Z = position.Z;
        }
    }
    #endregion

    #region Voice Client
    public class VoiceClient
    {
        public RAGE.Elements.Player Player { get; set; }
        public string TeamSpeakName { get; set; }
        public float VoiceRange { get; set; }
        public bool IsAlive { get; set; }

        public VoiceClient (RAGE.Elements.Player player, string teamSpeakName, float voiceRange)
        {
            this.Player = player;
            this.TeamSpeakName = teamSpeakName;
            this.VoiceRange = voiceRange;

            this.IsAlive = true;
        }
    }
    #endregion
}
