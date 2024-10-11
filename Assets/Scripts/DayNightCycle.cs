using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Events;

public class DayNightCycle : MonoBehaviour {
	public static DayNightCycle Instance;
	[Range(0f, 1f)] public float time;
	public float fullDayLenght, startTime, waitForNight;
	public Vector3 noon;
	public ParticleSystem sandstorm;
	public UnityEvent onDayChange;
	float timeRate;
	int day = 0;

	[Header("Солнце")]
	public Light sun;
	public Gradient sunColor;
	public AnimationCurve sunIntensity, sunY, sunZ;
	public Transform sunTransform;

	[Header("Луна")]
	public Material moonMaterial;
	public Light moon;
	public Gradient moonColor;
	public AnimationCurve moonIntensity;

	[Header("Туман")]
	public Material skybox;
	public Gradient fogColor;

	[Header("Другое")]
	public AudioSource ambientSource, sandstormSource;
	public AnimationCurve ambientVolume;
	public LensFlareComponentSRP srp;
	public Gradient sky, sandstormColor;
	public AnimationCurve lightning, reflections;

	public static bool isNight, isSandStorm;
	public static Vector3 windDirection = Vector3.one;

	void Awake() {
		Instance = this;
	}
	void Start() {
		timeRate = 1 / fullDayLenght;
		time = startTime;
		StartCoroutine(ChangeWindDirection());
		StartCoroutine(ChangeWeather());
	}
	void Update() {
		srp.enabled = time < .775f && time > .24f;

		if (!isNight)
			StartCoroutine(CheckNight());

		sun.transform.eulerAngles = (time - .25f) * noon * 4;
		moon.transform.eulerAngles = (time - .75f) * noon * 4;

		sun.intensity = sunIntensity.Evaluate(time);
		moon.intensity = moonIntensity.Evaluate(time);

		ambientSource.volume = ambientVolume.Evaluate(time);

		sun.color = sunColor.Evaluate(time);
		moon.color = moonColor.Evaluate(time);

		RenderSettings.ambientIntensity = lightning.Evaluate(time);
		RenderSettings.reflectionIntensity = reflections.Evaluate(time);
		RenderSettings.fogColor = fogColor.Evaluate(time);

		var sandstorm_main = sandstorm.main;
		sandstorm_main.startColor = sandstormColor.Evaluate(time);

		skybox.SetColor("_SkyGradientTop", sky.Evaluate(time));
		skybox.SetColor("_SkyGradientBottom", fogColor.Evaluate(time));
		skybox.SetColor("_SunDiscColor", sunColor.Evaluate(time));

		sunTransform.position = PlayerScript.Instance.transform.position + new Vector3(0, sunY.Evaluate(time), sunZ.Evaluate(time));
		sandstorm.transform.position = PlayerScript.Instance.transform.position;

		var sandstorm_velocity = sandstorm.velocityOverLifetime;
		sandstorm_velocity.x = new ParticleSystem.MinMaxCurve(windDirection.x * 25);
		sandstorm_velocity.z = new ParticleSystem.MinMaxCurve(windDirection.z * 25);

		Vector3 fromPosition = sunTransform.position;
		Vector3 toPosition = PlayerScript.Instance.transform.position;
		Vector3 direction = toPosition - fromPosition;
		Physics.Raycast(sunTransform.position, direction, out RaycastHit hit, Mathf.Infinity);

		sandstormSource.volume += isSandStorm ? Time.deltaTime / 50 : -(Time.deltaTime / 50);

		if (hit.transform != null)
			LifeSystem.Instance.inTheShadow = hit.transform.gameObject.layer == 7 || isNight || time >= 8 || time <= .2f || isSandStorm;

		if (sun.intensity == 0 && sun.gameObject.activeInHierarchy)
			sun.gameObject.SetActive(false);
		else if (sun.intensity > 0 && !sun.gameObject.activeInHierarchy)
			sun.gameObject.SetActive(true);
		if (moon.intensity == 0 && moon.gameObject.activeInHierarchy)
			moon.gameObject.SetActive(false);
		else if (moon.intensity > 0 && !moon.gameObject.activeInHierarchy)
			moon.gameObject.SetActive(true);
	}
	void ChangeDay() {
		day++;
		onDayChange.Invoke();
	}
	IEnumerator CheckNight() {
		time += timeRate * Time.deltaTime;
		if (time >= 0.9f) {
			isNight = true;
			time = 0;
			RenderSettings.skybox = moonMaterial;
			yield return new WaitForSeconds(waitForNight);
			ChangeDay();
			isNight = false;
			RenderSettings.skybox = skybox;
		}
	}
	IEnumerator ChangeWindDirection() {
		yield return new WaitForSeconds(Random.Range(30f, 90f));

		windDirection = isNight ?
			new Vector3(Random.Range(-0.75f, 0.75f), Random.Range(0.75f, 1f), Random.Range(-0.75f, 0.75f)) : isSandStorm ?
			new Vector3(Random.Range(-4f, 4f), Random.Range(1f, 1.25f), Random.Range(-4f, 4f)) :
			new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(1f, 1.25f), Random.Range(-1.5f, 1.5f));

		StartCoroutine(ChangeWindDirection());
	}
	IEnumerator ChangeWeather() {
		yield return new WaitForSeconds(Random.Range(90f, 180f));
		if (!isNight)
			isSandStorm = GameManager.Rand();
		else
			isSandStorm = false;
		if (isSandStorm)
			windDirection = new Vector3(Random.Range(-4f, 4f), Random.Range(1f, 1.25f), Random.Range(-4f, 4f));
		if (isSandStorm && !sandstorm.isPlaying)
			sandstorm.Play();
		else if (!isSandStorm && sandstorm.isPlaying)
			sandstorm.Stop();
		StartCoroutine(ChangeWeather());
	}
}
