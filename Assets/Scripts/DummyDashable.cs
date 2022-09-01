using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyDashable : MonoBehaviour, IDashTouchable
{
	public void OnTouchedDuringDash(GameObject gameObject)
	{
		gameObject.GetComponent<PlayerScore>().Score += 1;
	}

}
