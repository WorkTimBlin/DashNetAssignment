using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerScore : NetworkBehaviour
{
    [SerializeField]
    [SyncVar(hook = nameof(SetScore))]
    int score = 0;

    [SyncVar]
    int index;

    public int Index => index;

    [SerializeField]
    LocalPlayerHud displayer;

    public event Action<PlayerScore> OnScoreChanged;

	public override void OnStartLocalPlayer()
	{
        //displayer = Instantiate(displayerPrefab);
        displayer = FindObjectOfType<LocalPlayerHud>();
        displayer.Score = score;
        SetIndex(FindObjectsOfType<NetworkRoomPlayer>().First(player => player.isLocalPlayer).index);
	}

	// Start is called before the first frame update
	void Start()
    {
        FindObjectOfType<MatchManager>().RegisterPlayerScore(this);
    }

    public int Score
	{
        get => score;
		set
		{
            score = value;
            if (isLocalPlayer)
                displayer.Score = score;
            OnScoreChanged?.Invoke(this);
		}
	}

    //[Command]
    public void AddPoint()
	{
        Score += 1;
	}

    [Command]
    void SetIndex(int index)
	{
        this.index = index;
	}

    void SetScore(int oldValue, int newValue)
	{
        Score = newValue;
	}
}
