using UnityEngine;

public class GameManager : MonoBehaviour {
	public static bool Rand() => Random.Range(0, 2) == 0;
}
