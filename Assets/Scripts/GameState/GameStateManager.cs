using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UniRx;
using System.Linq;


public class GameStateManager : SingletonMonoBehaviour<GameStateManager>
{
    CancellationToken cancellationToken;

    private EState currentState;
    private EState nextState;
    private EState lastState;
    public EState CurrentState { get { return this.currentState; } }
    public EState NextState { get { return this.nextState; } }
    public EState LastState { get { return this.lastState; } }

    IState currentIState;

    public IState GetCurrentState { get { return this.currentIState; } }
    public T GetCurrentStateClass<T>() where T : class, IState
    {
        return this.currentIState as T;
    }

    public IState GetStateWithKey(EState state) => stateMap[state];

    Dictionary<EState, IState> stateMap;

    bool initialized = false;

    Dictionary<EState, EState[]> stateMatrix = new Dictionary<EState, EState[]>
        {
            {EState.LoadGame,new [] {EState.MainMenu} },
            {EState.MainMenu,new [] {EState.InGame } },
            {EState.InGame,new [] {EState.MainMenu } },
        };
    Dictionary<EState, IState> CreateStateMap()
    {
        var map = new Dictionary<EState, IState>();
        map.Add(EState.None, new BaseState());
        map.Add(EState.LoadGame, new LoadGameState());
        map.Add(EState.MainMenu, new MainMenuState());
        map.Add(EState.InGame, new InGameState());
        return map;
    }

    public async UniTask Install(CancellationToken cancellationToken)
    {
        this.cancellationToken = cancellationToken;
        BaseState.SetCancellationTokenInGame(cancellationToken);
        this.stateMap = CreateStateMap();
        foreach (var state in this.stateMap.Values)
        {
            state.Initialize();
        }
        this.currentState = EState.None;
        this.nextState = EState.LoadGame;
        this.currentIState = null;
        this.initialized = true;
        await GameViewsManager.Instance.Initialize(cancellationToken);
    }
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach (var state in this.stateMap.Values)
        {
            state.Dispose();
        }
        EndCurrentState();
        this.initialized = false;
    }

    private void Update()
    {
        if (!this.initialized) return;
        if (this.nextState != currentState)
        {
            EndCurrentState();
            StartCurrentState(nextState);
        }
        if (this.currentIState != null)
        {
            this.currentIState.Update();
        }
    }
    void StartCurrentState(EState nextState)
    {
        this.currentState = nextState;
        if (!this.stateMap.ContainsKey(this.currentState))
        {
            return;
        }
        this.currentIState = this.stateMap[this.currentState];
        this.currentIState.Start();
        UnityEngine.Debug.Log("state currrent: " + this.currentState);
        this.currentIState.OnRequestStateChange().Subscribe(result =>
        {
            this.nextState = GetNextState(this.currentState, result);
        }).AddTo(this.cancellationToken);
    }
    void EndCurrentState()
    {
        this.lastState = this.currentState;
        if (this.currentIState != null)
        {
            this.currentIState.End();
            this.currentIState = null;
        }
    }

    EState GetNextState(EState state, int result)
    {
        if (result == 0) return this.currentState;
        result--;

        var stateList = this.stateMatrix[state];
        if ((result < 0) || (result >= stateList.Length))
        {
            return this.currentState;
        }

        var nextState = stateList[result];

        return nextState;
    }

}
