using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class LifeSystem : MonoBehaviour {
	public static LifeSystem Instance;
	public Image health, food, water, sleep, energy;
	public float energyMultiply = 5, heatTimer, painStrength;
	public bool inTheShadow;
	public Animator animator;
	int daysWithoutSleep;
	Volume postProcessing;
	Vignette vignette;
	FilmGrain filmGrain;
	ColorAdjustments colorAdjustments;
	ChromaticAberration chromaticAberration;
	LensDistortion lensDistortion;
	MotionBlur motionBlur;

	void Awake() => Instance = this;
	void Start() {
		DayNightCycle.Instance.onDayChange.AddListener(() => {
			if (sleep.fillAmount >= 1)
				daysWithoutSleep++;
			else
				daysWithoutSleep = 0;

			if (daysWithoutSleep >= 5)
				LostMind();
		});
		StartCoroutine(DecraseStats());
		postProcessing = Camera.main.GetComponent<Volume>();
		postProcessing.profile.TryGet(out vignette);
		postProcessing.profile.TryGet(out filmGrain);
		postProcessing.profile.TryGet(out colorAdjustments);
		postProcessing.profile.TryGet(out chromaticAberration);
		postProcessing.profile.TryGet(out lensDistortion);
		postProcessing.profile.TryGet(out motionBlur);
	}

	void Update() {
		if (heatTimer >= 1)
			LostMind();
		else {
			if (!inTheShadow)
				heatTimer += Time.deltaTime / 125;
			else if (heatTimer > 0)
				heatTimer -= Time.deltaTime / 50;

			vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, DayNightCycle.isSandStorm ? .5f : heatTimer / 1.75f, Time.deltaTime);
			filmGrain.intensity.value = heatTimer * 1.25f;
			lensDistortion.intensity.value = -heatTimer / 1.5f;
			chromaticAberration.intensity.value = heatTimer;
			motionBlur.intensity.value = heatTimer;
			colorAdjustments.postExposure.value = heatTimer + painStrength;

			animator.speed = 1 + (1 - energy.fillAmount) / 1.5f;

			painStrength = Mathf.Lerp(painStrength, 0, Time.deltaTime * 2);
			Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, PlayerScript.Instance.run ? 100 : 80 - (heatTimer * 20) + painStrength * (PlayerScript.Instance.run ? 10 : 20), Time.deltaTime * 6);
		}

		energyMultiply = 5 - sleep.fillAmount * 3.5f + energy.fillAmount * 2;

		energy.fillAmount += PlayerScript.Instance.run ? -0.001f - sleep.fillAmount / 1000 * (DayNightCycle.isSandStorm ? 1.5f : 1) : PlayerScript.Instance.crouch ? 0.0015f : 0.00075f - sleep.fillAmount / 1300 / (DayNightCycle.isSandStorm ? 1.5f : 1);
	}
	void LostMind() {
		heatTimer = 0;
		Debug.Log("YOU DIED");
	}
	public void GetDamage(float hp) {
		health.fillAmount -= hp / 100;
		painStrength = 1;
	}
	IEnumerator DecraseStats() {
		yield return new WaitForSeconds(1f);
		food.fillAmount -= 0.04f / 30 * (PlayerScript.Instance.run || (PlayerScript.Instance.crouch && PlayerScript.Instance.move.magnitude > 0) ? 1.25f : 1);
		water.fillAmount -= 0.065f / 30 * (PlayerScript.Instance.run || (PlayerScript.Instance.crouch && PlayerScript.Instance.move.magnitude > 0) ? 1.25f : 1) * (inTheShadow ? 1 : 1.5f);

		if (health.fillAmount >= 0.95f && health.fillAmount < 1 && food.fillAmount > 0 && water.fillAmount > 0)
			health.fillAmount += 0.01f;

		if (food.fillAmount <= 0 || water.fillAmount <= 0)
			health.fillAmount -= 0.01f;

		sleep.fillAmount += PlayerScript.Instance.run ? 0.005f * (DayNightCycle.isSandStorm ? 1.5f : 1) : 0.001f * (DayNightCycle.isSandStorm ? 2 : 1);
		StartCoroutine(DecraseStats());
	}
}
