using System.Collections.Generic;
using UnityEngine;

public class CameraBobbing : MonoBehaviour {
	public static CameraBobbing Instance;
	public AnimationCurve cameraX, cameraY, cameraXRot;
	public float speed;
	public Transform cameraTransform, cameraRoot;
	List<float> time = new() { 0, 0, 0 };

	void Awake() => Instance = this;
	void Update() {
		if (PlayerScript.Instance.move.z != 0 || PlayerScript.Instance.move.x != 0) {
			time = time.ConvertAll(i => i >= 1 ? 0 : i);

			for (int i = 0; i < 2; i++)
				time[i] += i == 0
				? Time.deltaTime * speed / 2 * (PlayerScript.Instance.run ? 2 : 1) * (PlayerScript.Instance.crouch ? 0.5f : 1)
				: Time.deltaTime * speed * (PlayerScript.Instance.run ? 2 : 1) * (PlayerScript.Instance.crouch ? 0.5f : 1);

			cameraTransform.localPosition = new Vector3(cameraX.Evaluate(time[0]), cameraY.Evaluate(time[1]), cameraTransform.localPosition.z);
		}

		time[2] = time[2] >= 1 ? 0 : time[2] + Time.deltaTime / 1.35f;

		cameraRoot.localRotation = Quaternion.Euler(cameraXRot.Evaluate(time[2]) * (1 - LifeSystem.Instance.energy.fillAmount), 0, -cameraX.Evaluate(time[0]) * 2.5f);
	}

	public float GetYValue() => cameraY.Evaluate(time[1]);
}