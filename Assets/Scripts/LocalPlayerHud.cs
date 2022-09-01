using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalPlayerHud : MonoBehaviour
{
	[SerializeField]
	TextMeshProUGUI scoreText;
	[SerializeField]
	TextMeshProUGUI winningMessageText;
	public int Score
	{
		set
		{
			scoreText.text = "Score : " + value;
		}
	}

	public string WinningMessage
	{
		get => winningMessageText.text;
		set
		{
			if (value != null)
			{
				winningMessageText.text = value;
				winningMessageText.gameObject.SetActive(true);
			}
			else
			{
				winningMessageText.gameObject.SetActive(false);
			}
		}
	}

}
