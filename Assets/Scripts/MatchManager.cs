using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class MatchManager : NetworkBehaviour
{
    [SerializeField]
    private LocalPlayerHud hud;
    [SerializeField]
    private int minWinningScore = 3;
    [SerializeField]
    float matchRestartDelay = 3;

    public void RegisterPlayerScore(PlayerScore playerScore)
	{
        playerScore.OnScoreChanged += OnScoreChanged;
	}

    public void UngeristerPlayerScore(PlayerScore playerScore)
	{
        playerScore.OnScoreChanged -= OnScoreChanged;
	}

    private void OnScoreChanged(PlayerScore playerScore)
	{
        if (playerScore.Score >= minWinningScore)
		{
            hud.WinningMessage =
                playerScore.isLocalPlayer
                    ? "YOU WIN!"
                    : $"Player {playerScore.Index} win with the score {playerScore.Score}";
            if(isServer) StartCoroutine(DelayedMatchRestartRoutine());
        }
	}

    IEnumerator DelayedMatchRestartRoutine()
	{
        yield return new WaitForSeconds(matchRestartDelay);
		NetworkManager.singleton.ServerChangeScene(SceneManager.GetActiveScene().name);
	}

	private void OnValidate()
	{
        if (hud == null)
            hud = GetComponent<LocalPlayerHud>();
	}
}
