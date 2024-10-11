using UnityEngine;

public class Move : MonoBehaviour {
	public static Move Instance;
	Rigidbody r;

	void Awake() => Instance = this;
	void Start() => r = GetComponent<Rigidbody>();
	void Update() {
		if (Vector3.Distance(PlayerScript.Instance.transform.position, transform.position) > 175)
			Destroy(gameObject);
		r.AddForce(DayNightCycle.windDirection * 1.6f, ForceMode.Force);
	}
}
