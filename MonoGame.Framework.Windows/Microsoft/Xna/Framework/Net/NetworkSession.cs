﻿// Type: Microsoft.Xna.Framework.Net.NetworkSession
// Assembly: MonoGame.Framework.Windows, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2107839-320D-467B-B82A-28CB452CC584
// Assembly location: F:\Program Files (x86)\FEZ\MonoGame.Framework.Windows.dll

using Microsoft.Xna.Framework.GamerServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;

namespace Microsoft.Xna.Framework.Net
{
  public sealed class NetworkSession : IDisposable
  {
    internal static List<NetworkSession> activeSessions = new List<NetworkSession>();
    private bool isHost = false;
    private int hostGamerIndex = -1;
    private bool _AllowHostMigration = false;
    private bool _AllowJoinInProgress = false;
    private bool _isDisposed = false;
    private TimeSpan defaultSimulatedLatency = new TimeSpan(0, 0, 0);
    private float simulatedPacketLoss = 0.0f;
    private NetworkSessionState sessionState;
    private GamerCollection<NetworkGamer> _allGamers;
    private GamerCollection<LocalNetworkGamer> _localGamers;
    private GamerCollection<NetworkGamer> _remoteGamers;
    private GamerCollection<NetworkGamer> _previousGamers;
    internal Queue<CommandEvent> commandQueue;
    private bool disposed;
    private NetworkSessionType sessionType;
    private int maxGamers;
    private int privateGamerSlots;
    private NetworkSessionProperties sessionProperties;
    private NetworkGamer hostingGamer;
    internal MonoGamerPeer networkPeer;

    public GamerCollection<NetworkGamer> AllGamers
    {
      get
      {
        return this._allGamers;
      }
    }

    public bool AllowHostMigration
    {
      get
      {
        return this._AllowHostMigration;
      }
      set
      {
        if (this._AllowHostMigration == value)
          return;
        this._AllowHostMigration = value;
      }
    }

    public bool AllowJoinInProgress
    {
      get
      {
        return this._AllowJoinInProgress;
      }
      set
      {
        if (this._AllowJoinInProgress == value)
          return;
        this._AllowJoinInProgress = value;
      }
    }

    public int BytesPerSecondReceived
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public int BytesPerSecondSent
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public NetworkGamer Host
    {
      get
      {
        return this.hostingGamer;
      }
    }

    public bool IsDisposed
    {
      get
      {
        return this._isDisposed;
      }
    }

    public bool IsEveryoneReady
    {
      get
      {
        if (this._allGamers.Count == 0)
          return false;
        foreach (NetworkGamer networkGamer in (ReadOnlyCollection<NetworkGamer>) this._allGamers)
        {
          if (!networkGamer.IsReady)
            return false;
        }
        return true;
      }
    }

    public bool IsHost
    {
      get
      {
        return this.isHost;
      }
    }

    public GamerCollection<LocalNetworkGamer> LocalGamers
    {
      get
      {
        return this._localGamers;
      }
    }

    public int MaxGamers
    {
      get
      {
        return this.maxGamers;
      }
      set
      {
        this.maxGamers = value;
      }
    }

    public GamerCollection<NetworkGamer> PreviousGamers
    {
      get
      {
        return this._previousGamers;
      }
    }

    public int PrivateGamerSlots
    {
      get
      {
        return this.privateGamerSlots;
      }
      set
      {
        this.privateGamerSlots = value;
      }
    }

    public GamerCollection<NetworkGamer> RemoteGamers
    {
      get
      {
        return this._remoteGamers;
      }
    }

    public NetworkSessionProperties SessionProperties
    {
      get
      {
        return this.sessionProperties;
      }
    }

    public NetworkSessionState SessionState
    {
      get
      {
        return this.sessionState;
      }
    }

    public NetworkSessionType SessionType
    {
      get
      {
        return this.sessionType;
      }
    }

    public TimeSpan SimulatedLatency
    {
      get
      {
        return this.defaultSimulatedLatency;
      }
      set
      {
        this.defaultSimulatedLatency = value;
      }
    }

    public float SimulatedPacketLoss
    {
      get
      {
        if (this.networkPeer != null)
          this.simulatedPacketLoss = this.networkPeer.SimulatedPacketLoss;
        return this.simulatedPacketLoss;
      }
      set
      {
        if (this.networkPeer != null)
          this.networkPeer.SimulatedPacketLoss = value;
        this.simulatedPacketLoss = value;
      }
    }

    public event EventHandler<GameEndedEventArgs> GameEnded;

    public event EventHandler<GamerJoinedEventArgs> GamerJoined;

    public event EventHandler<GamerLeftEventArgs> GamerLeft;

    public event EventHandler<GameStartedEventArgs> GameStarted;

    public event EventHandler<HostChangedEventArgs> HostChanged;

    public static event EventHandler<InviteAcceptedEventArgs> InviteAccepted;

    public event EventHandler<NetworkSessionEndedEventArgs> SessionEnded;

    static NetworkSession()
    {
    }

    private NetworkSession()
    {
      NetworkSession.activeSessions.Add(this);
    }

    private NetworkSession(NetworkSessionType sessionType, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, bool isHost, int hostGamer)
      : this(sessionType, maxGamers, privateGamerSlots, sessionProperties, isHost, hostGamer, (AvailableNetworkSession) null)
    {
    }

    private NetworkSession(NetworkSessionType sessionType, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, bool isHost, int hostGamer, AvailableNetworkSession availableSession)
      : this()
    {
      if (sessionProperties == null)
        throw new ArgumentNullException("sessionProperties");
      this._allGamers = new GamerCollection<NetworkGamer>();
      this._localGamers = new GamerCollection<LocalNetworkGamer>();
      this._remoteGamers = new GamerCollection<NetworkGamer>();
      this._previousGamers = new GamerCollection<NetworkGamer>();
      this.hostingGamer = (NetworkGamer) null;
      this.commandQueue = new Queue<CommandEvent>();
      this.sessionType = sessionType;
      this.maxGamers = maxGamers;
      this.privateGamerSlots = privateGamerSlots;
      this.sessionProperties = sessionProperties;
      this.isHost = isHost;
      this.hostGamerIndex = hostGamer;
      if (isHost)
        this.networkPeer = new MonoGamerPeer(this, (AvailableNetworkSession) null);
      else if (this.networkPeer == null)
        this.networkPeer = new MonoGamerPeer(this, availableSession);
      this.commandQueue.Enqueue(new CommandEvent((ICommand) new CommandGamerJoined(hostGamer, this.isHost, true)));
    }

    ~NetworkSession()
    {
      this.Dispose(false);
    }

    public static NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
    {
      try
      {
        return NetworkSession.EndCreate(NetworkSession.BeginCreate(sessionType, localGamers, maxGamers, privateGamerSlots, sessionProperties, (AsyncCallback) null, (object) null));
      }
      finally
      {
      }
    }

    public static NetworkSession Create(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers)
    {
      try
      {
        return NetworkSession.EndCreate(NetworkSession.BeginCreate(sessionType, maxLocalGamers, maxGamers, (AsyncCallback) null, (object) null));
      }
      finally
      {
      }
    }

    public static NetworkSession Create(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
    {
      try
      {
        return NetworkSession.EndCreate(NetworkSession.BeginCreate(sessionType, maxLocalGamers, maxGamers, privateGamerSlots, sessionProperties, (AsyncCallback) null, (object) null));
      }
      finally
      {
      }
    }

    private static NetworkSession Create(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, int hostGamer, bool isHost)
    {
      NetworkSession networkSession = (NetworkSession) null;
      try
      {
        if (sessionProperties == null)
          sessionProperties = new NetworkSessionProperties();
        networkSession = new NetworkSession(sessionType, maxGamers, privateGamerSlots, sessionProperties, isHost, hostGamer);
      }
      finally
      {
      }
      return networkSession;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    public void Dispose(bool disposing)
    {
      if (this._isDisposed)
        return;
      if (disposing)
      {
        foreach (Gamer gamer in (ReadOnlyCollection<NetworkGamer>) this._allGamers)
          gamer.Dispose();
        if (this.networkPeer != null)
          this.networkPeer.ShutDown();
        if (this.networkPeer != null)
          this.networkPeer.ShutDown();
      }
      this._isDisposed = true;
    }

    public void AddLocalGamer(SignedInGamer gamer)
    {
      if (gamer == null)
        throw new ArgumentNullException("gamer");
    }

    public static IAsyncResult BeginCreate(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, AsyncCallback callback, object asyncState)
    {
      int hostingGamerIndex = NetworkSession.GetHostingGamerIndex(localGamers);
      return NetworkSession.BeginCreate(sessionType, hostingGamerIndex, 4, maxGamers, privateGamerSlots, sessionProperties, callback, asyncState);
    }

    public static IAsyncResult BeginCreate(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, AsyncCallback callback, object asyncState)
    {
      return NetworkSession.BeginCreate(sessionType, -1, maxLocalGamers, maxGamers, 0, (NetworkSessionProperties) null, callback, asyncState);
    }

    public static IAsyncResult BeginCreate(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, AsyncCallback callback, object asyncState)
    {
      return NetworkSession.BeginCreate(sessionType, -1, maxLocalGamers, maxGamers, privateGamerSlots, sessionProperties, callback, asyncState);
    }

    private static IAsyncResult BeginCreate(NetworkSessionType sessionType, int hostGamer, int maxLocalGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, AsyncCallback callback, object asyncState)
    {
      if (maxLocalGamers < 1 || maxLocalGamers > 4)
        throw new ArgumentOutOfRangeException("Maximum local players must be between 1 and 4.");
      if (maxGamers < 2 || maxGamers > 32)
        throw new ArgumentOutOfRangeException("Maximum number of gamers must be between 2 and 32.");
      try
      {
        return new NetworkSessionAsynchronousCreate(NetworkSession.Create).BeginInvoke(sessionType, maxLocalGamers, maxGamers, privateGamerSlots, sessionProperties, hostGamer, true, callback, asyncState);
      }
      finally
      {
      }
    }

    internal static int GetHostingGamerIndex(IEnumerable<SignedInGamer> localGamers)
    {
      SignedInGamer signedInGamer1 = (SignedInGamer) null;
      if (localGamers == null)
        throw new ArgumentNullException("localGamers");
      foreach (SignedInGamer signedInGamer2 in localGamers)
      {
        if (signedInGamer2 == null)
          throw new ArgumentException("gamer can not be null in list of localGamers.");
        if (signedInGamer2.IsDisposed)
          throw new ObjectDisposedException("localGamers", "A gamer is disposed in the list of localGamers");
        if (signedInGamer1 == null)
          signedInGamer1 = signedInGamer2;
      }
      if (signedInGamer1 == null)
        throw new ArgumentException("Invalid gamer in localGamers.");
      else
        return (int) signedInGamer1.PlayerIndex;
    }

    public static IAsyncResult BeginFind(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties, AsyncCallback callback, object asyncState)
    {
      int hostingGamerIndex = NetworkSession.GetHostingGamerIndex(localGamers);
      return NetworkSession.BeginFind(sessionType, hostingGamerIndex, 4, searchProperties, callback, asyncState);
    }

    public static IAsyncResult BeginFind(NetworkSessionType sessionType, int maxLocalGamers, NetworkSessionProperties searchProperties, AsyncCallback callback, object asyncState)
    {
      return NetworkSession.BeginFind(sessionType, -1, 4, searchProperties, callback, asyncState);
    }

    private static IAsyncResult BeginFind(NetworkSessionType sessionType, int hostGamer, int maxLocalGamers, NetworkSessionProperties searchProperties, AsyncCallback callback, object asyncState)
    {
      if (sessionType == NetworkSessionType.Local)
        throw new ArgumentException("NetworkSessionType cannot be NetworkSessionType.Local");
      if (maxLocalGamers < 1 || maxLocalGamers > 4)
        throw new ArgumentOutOfRangeException("maxLocalGamers must be between 1 and 4.");
      try
      {
        return new NetworkSessionAsynchronousFind(NetworkSession.Find).BeginInvoke(sessionType, hostGamer, maxLocalGamers, searchProperties, callback, asyncState);
      }
      finally
      {
      }
    }

    public static IAsyncResult BeginJoin(AvailableNetworkSession availableSession, AsyncCallback callback, object asyncState)
    {
      if (availableSession == null)
        throw new ArgumentNullException();
      try
      {
        return new NetworkSessionAsynchronousJoin(NetworkSession.JoinSession).BeginInvoke(availableSession, callback, asyncState);
      }
      finally
      {
      }
    }

    public static IAsyncResult BeginJoinInvited(IEnumerable<SignedInGamer> localGamers, AsyncCallback callback, object asyncState)
    {
      try
      {
        throw new NotImplementedException();
      }
      finally
      {
      }
    }

    public static IAsyncResult BeginJoinInvited(int maxLocalGamers, AsyncCallback callback, object asyncState)
    {
      if (maxLocalGamers < 1 || maxLocalGamers > 4)
        throw new ArgumentOutOfRangeException("maxLocalGamers must be between 1 and 4.");
      try
      {
        return new NetworkSessionAsynchronousJoinInvited(NetworkSession.JoinInvited).BeginInvoke(maxLocalGamers, callback, asyncState);
      }
      finally
      {
      }
    }

    public static NetworkSession EndCreate(IAsyncResult result)
    {
      NetworkSession networkSession = (NetworkSession) null;
      try
      {
        AsyncResult asyncResult = (AsyncResult) result;
        result.AsyncWaitHandle.WaitOne();
        if (asyncResult.AsyncDelegate is NetworkSessionAsynchronousCreate)
          networkSession = ((NetworkSessionAsynchronousCreate) asyncResult.AsyncDelegate).EndInvoke(result);
      }
      finally
      {
        result.AsyncWaitHandle.Close();
      }
      return networkSession;
    }

    public static AvailableNetworkSessionCollection EndFind(IAsyncResult result)
    {
      AvailableNetworkSessionCollection sessionCollection = (AvailableNetworkSessionCollection) null;
      List<AvailableNetworkSession> networkSessions = new List<AvailableNetworkSession>();
      try
      {
        AsyncResult asyncResult = (AsyncResult) result;
        result.AsyncWaitHandle.WaitOne();
        if (asyncResult.AsyncDelegate is NetworkSessionAsynchronousFind)
        {
          sessionCollection = ((NetworkSessionAsynchronousFind) asyncResult.AsyncDelegate).EndInvoke(result);
          MonoGamerPeer.FindResults(networkSessions);
        }
      }
      finally
      {
        result.AsyncWaitHandle.Close();
      }
      return new AvailableNetworkSessionCollection((IList<AvailableNetworkSession>) networkSessions);
    }

    public void EndGame()
    {
      try
      {
        this.commandQueue.Enqueue(new CommandEvent((ICommand) new CommandSessionStateChange(NetworkSessionState.Lobby, this.sessionState)));
      }
      finally
      {
      }
    }

    public static NetworkSession EndJoin(IAsyncResult result)
    {
      NetworkSession networkSession = (NetworkSession) null;
      try
      {
        AsyncResult asyncResult = (AsyncResult) result;
        result.AsyncWaitHandle.WaitOne();
        if (asyncResult.AsyncDelegate is NetworkSessionAsynchronousJoin)
          networkSession = ((NetworkSessionAsynchronousJoin) asyncResult.AsyncDelegate).EndInvoke(result);
      }
      finally
      {
        result.AsyncWaitHandle.Close();
      }
      return networkSession;
    }

    public static NetworkSession EndJoinInvited(IAsyncResult result)
    {
      NetworkSession networkSession = (NetworkSession) null;
      try
      {
        AsyncResult asyncResult = (AsyncResult) result;
        result.AsyncWaitHandle.WaitOne();
        if (asyncResult.AsyncDelegate is NetworkSessionAsynchronousJoinInvited)
          networkSession = ((NetworkSessionAsynchronousJoinInvited) asyncResult.AsyncDelegate).EndInvoke(result);
      }
      finally
      {
        result.AsyncWaitHandle.Close();
      }
      return networkSession;
    }

    public static AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties)
    {
      int hostingGamerIndex = NetworkSession.GetHostingGamerIndex(localGamers);
      return NetworkSession.EndFind(NetworkSession.BeginFind(sessionType, hostingGamerIndex, 4, searchProperties, (AsyncCallback) null, (object) null));
    }

    public static AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, int maxLocalGamers, NetworkSessionProperties searchProperties)
    {
      return NetworkSession.EndFind(NetworkSession.BeginFind(sessionType, -1, maxLocalGamers, searchProperties, (AsyncCallback) null, (object) null));
    }

    private static AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, int hostGamer, int maxLocalGamers, NetworkSessionProperties searchProperties)
    {
      try
      {
        if (maxLocalGamers < 1 || maxLocalGamers > 4)
          throw new ArgumentOutOfRangeException("maxLocalGamers must be between 1 and 4.");
        List<AvailableNetworkSession> list = new List<AvailableNetworkSession>();
        MonoGamerPeer.Find(sessionType);
        return new AvailableNetworkSessionCollection((IList<AvailableNetworkSession>) list);
      }
      finally
      {
      }
    }

    public NetworkGamer FindGamerById(byte gamerId)
    {
      try
      {
        foreach (NetworkGamer networkGamer in (ReadOnlyCollection<NetworkGamer>) this._allGamers)
        {
          if ((int) networkGamer.Id == (int) gamerId)
            return networkGamer;
        }
        return (NetworkGamer) null;
      }
      finally
      {
      }
    }

    public static NetworkSession Join(AvailableNetworkSession availableSession)
    {
      return NetworkSession.EndJoin(NetworkSession.BeginJoin(availableSession, (AsyncCallback) null, (object) null));
    }

    private static NetworkSession JoinSession(AvailableNetworkSession availableSession)
    {
      NetworkSession networkSession = (NetworkSession) null;
      try
      {
        NetworkSessionType sessionType = availableSession.SessionType;
        int maxGamers = 32;
        int privateGamerSlots = 0;
        bool isHost = false;
        int hostGamer = -1;
        NetworkSessionProperties sessionProperties = availableSession.SessionProperties ?? new NetworkSessionProperties();
        networkSession = new NetworkSession(sessionType, maxGamers, privateGamerSlots, sessionProperties, isHost, hostGamer, availableSession);
      }
      finally
      {
      }
      return networkSession;
    }

    public static NetworkSession JoinInvited(IEnumerable<SignedInGamer> localGamers)
    {
      try
      {
        throw new NotImplementedException();
      }
      finally
      {
      }
    }

    public static NetworkSession JoinInvited(int maxLocalGamers)
    {
      if (maxLocalGamers < 1 || maxLocalGamers > 4)
        throw new ArgumentOutOfRangeException("maxLocalGamers must be between 1 and 4.");
      try
      {
        throw new NotImplementedException();
      }
      finally
      {
      }
    }

    public void ResetReady()
    {
      foreach (NetworkGamer networkGamer in (ReadOnlyCollection<LocalNetworkGamer>) this._localGamers)
        networkGamer.IsReady = false;
    }

    public void StartGame()
    {
      try
      {
        this.commandQueue.Enqueue(new CommandEvent((ICommand) new CommandSessionStateChange(NetworkSessionState.Playing, this.sessionState)));
      }
      finally
      {
      }
    }

    public void Update()
    {
      try
      {
        while (this.commandQueue.Count > 0 && this.networkPeer.IsReady)
        {
          CommandEvent commandEvent = this.commandQueue.Dequeue();
          if (commandEvent != null)
          {
            switch (commandEvent.Command)
            {
              case CommandEventType.GamerJoined:
                this.ProcessGamerJoined((CommandGamerJoined) commandEvent.CommandObject);
                break;
              case CommandEventType.GamerLeft:
                this.ProcessGamerLeft((CommandGamerLeft) commandEvent.CommandObject);
                break;
              case CommandEventType.SessionStateChange:
                this.ProcessSessionStateChange((CommandSessionStateChange) commandEvent.CommandObject);
                break;
              case CommandEventType.SendData:
                this.ProcessSendData((CommandSendData) commandEvent.CommandObject);
                break;
              case CommandEventType.ReceiveData:
                this.ProcessReceiveData((CommandReceiveData) commandEvent.CommandObject);
                break;
              case CommandEventType.GamerStateChange:
                this.ProcessGamerStateChange((CommandGamerStateChange) commandEvent.CommandObject);
                break;
            }
          }
        }
      }
      catch (Exception ex)
      {
      }
      finally
      {
      }
    }

    private void ProcessGamerStateChange(CommandGamerStateChange command)
    {
      this.networkPeer.SendGamerStateChange(command.Gamer);
    }

    private void ProcessSendData(CommandSendData command)
    {
      this.networkPeer.SendData(command.data, command.options);
      CommandReceiveData commandReceiveData = new CommandReceiveData(command.sender.RemoteUniqueIdentifier, command.data);
      commandReceiveData.gamer = (NetworkGamer) command.sender;
      foreach (LocalNetworkGamer localNetworkGamer in (ReadOnlyCollection<LocalNetworkGamer>) this._localGamers)
        localNetworkGamer.receivedData.Enqueue(commandReceiveData);
    }

    private void ProcessReceiveData(CommandReceiveData command)
    {
      foreach (NetworkGamer networkGamer in (ReadOnlyCollection<NetworkGamer>) this._allGamers)
      {
        if (networkGamer.RemoteUniqueIdentifier == command.remoteUniqueIdentifier)
          command.gamer = networkGamer;
      }
      if (command.gamer == null)
        return;
      foreach (LocalNetworkGamer localNetworkGamer in (ReadOnlyCollection<LocalNetworkGamer>) this.LocalGamers)
      {
        lock (localNetworkGamer.receivedData)
          localNetworkGamer.receivedData.Enqueue(command);
      }
    }

    private void ProcessSessionStateChange(CommandSessionStateChange command)
    {
      if (this.sessionState == command.NewState)
        return;
      this.sessionState = command.NewState;
      switch (command.NewState)
      {
        case NetworkSessionState.Playing:
          if (this.GameStarted != null)
          {
            this.GameStarted((object) this, new GameStartedEventArgs());
            break;
          }
          else
            break;
        case NetworkSessionState.Ended:
          this.ResetReady();
          if (this.SessionEnded != null)
          {
            this.SessionEnded((object) this, new NetworkSessionEndedEventArgs(NetworkSessionEndReason.HostEndedSession));
            break;
          }
          else
            break;
      }
      if (command.NewState != NetworkSessionState.Lobby || command.OldState != NetworkSessionState.Playing)
        return;
      this.ResetReady();
      if (this.GameEnded != null)
        this.GameEnded((object) this, new GameEndedEventArgs());
    }

    private void ProcessGamerJoined(CommandGamerJoined command)
    {
      NetworkGamer networkGamer;
      if ((command.State & GamerStates.Local) != (GamerStates) 0)
      {
        networkGamer = (NetworkGamer) new LocalNetworkGamer(this, (byte) command.InternalIndex, command.State);
        this._allGamers.AddGamer(networkGamer);
        this._localGamers.AddGamer((LocalNetworkGamer) networkGamer);
        if (Gamer.SignedInGamers.Count >= this._localGamers.Count)
          ((LocalNetworkGamer) networkGamer).SignedInGamer = ((List<SignedInGamer>) Gamer.SignedInGamers)[this._localGamers.Count - 1];
        networkGamer.PropertyChanged += new PropertyChangedEventHandler(this.HandleGamerPropertyChanged);
      }
      else
      {
        networkGamer = new NetworkGamer(this, (byte) command.InternalIndex, command.State);
        networkGamer.DisplayName = command.DisplayName;
        networkGamer.Gamertag = command.GamerTag;
        networkGamer.RemoteUniqueIdentifier = command.remoteUniqueIdentifier;
        this._allGamers.AddGamer(networkGamer);
        this._remoteGamers.AddGamer(networkGamer);
      }
      if ((command.State & GamerStates.Host) != (GamerStates) 0)
        this.hostingGamer = networkGamer;
      networkGamer.Machine = new NetworkMachine();
      networkGamer.Machine.Gamers.AddGamer(networkGamer);
      if (this.GamerJoined != null)
        this.GamerJoined((object) this, new GamerJoinedEventArgs(networkGamer));
      if (this.networkPeer != null && (command.State & GamerStates.Local) == (GamerStates) 0)
        this.networkPeer.SendPeerIntroductions(networkGamer);
      if (this.networkPeer == null)
        return;
      this.networkPeer.UpdateLiveSession(this);
    }

    private void ProcessGamerLeft(CommandGamerLeft command)
    {
      for (int index = 0; index < this._remoteGamers.Count; ++index)
      {
        if (this._remoteGamers[index].RemoteUniqueIdentifier == command.remoteUniqueIdentifier)
        {
          NetworkGamer aGamer = this._remoteGamers[index];
          this._remoteGamers.RemoveGamer(aGamer);
          this._allGamers.RemoveGamer(aGamer);
          if (this.GamerLeft != null)
            this.GamerLeft((object) this, new GamerLeftEventArgs(aGamer));
        }
      }
      if (this.networkPeer == null)
        return;
      this.networkPeer.UpdateLiveSession(this);
    }

    private void HandleGamerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      NetworkGamer gamer = sender as NetworkGamer;
      if (gamer == null || !gamer.IsLocal)
        return;
      this.commandQueue.Enqueue(new CommandEvent((ICommand) new CommandGamerStateChange(gamer)));
    }

    internal static void Exit()
    {
      if (NetworkSession.activeSessions == null || NetworkSession.activeSessions.Count <= 0)
        return;
      foreach (NetworkSession networkSession in NetworkSession.activeSessions)
      {
        if (!networkSession.IsDisposed)
          networkSession.Dispose();
      }
    }
  }
}
