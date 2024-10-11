using System.Collections;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
	public static PlayerScript Instance;
	public CharacterController controller;
	public float modifiedSpeed = 9, gravity = -9.81f, groundDistance = .4f;
	public Transform groundCheck;
	public LayerMask mask;
	public AnimationCurve movement;

	[Header("Sounds")]
	public AudioClip[] grassSounds, otherSounds;
	public AudioSource audioSource, audioSourceSteps, audioSourceSteps2;

	[Header("Materials for models")]
	public Material[] modelMaterials;
	public Vector3 move;
	[HideInInspector] public bool moveLock, noControl, crouch, run;

	Vector3 velocity;
	bool isGrounded = true, gettingDamage, inTriggerArea, runNeedRest, canRun = true, madeStep;
	float time, speed;

	void Awake() => Instance = this;
	void Start() => speed = modifiedSpeed;
	void Update()
	{
		isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, mask);

		if (isGrounded && velocity.y < 0)
			velocity.y = -2;

		if (!moveLock && !noControl)
		{
			CheckCrouch(Input.GetKey(KeyCode.LeftControl));
			CheckRun(Input.GetKey(KeyCode.LeftShift) && canRun);
		}

		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");
		float value = movement.Evaluate(time);

		if (!noControl)
		{
			Vector3 forward = transform.forward * z;
			Vector3 right = transform.right * x;

			move = moveLock ? Vector3.zero : right + forward;
		}

		if (z == -1 && x == 0)
			move += new Vector3(move.z < 0 ? value : move.z > 0 ? -value : move.x, move.y, move.x < 0 ? -value : move.x > 0 ? value : move.z);

		velocity.y += gravity * Time.deltaTime;
		time = time >= 1 ? 0 : time;
		time += Time.deltaTime / 5;

		controller.Move(move.normalized * modifiedSpeed * Time.deltaTime + (velocity * Time.deltaTime));

		if (LifeSystem.Instance.heatTimer >= 0.7f && canRun)
			canRun = false;
		if (LifeSystem.Instance.heatTimer < 1)
			modifiedSpeed = (speed - LifeSystem.Instance.heatTimer * 2.5f - (DayNightCycle.isSandStorm ? 3.5f : 0)) < 0 ? 0.5f : (speed - LifeSystem.Instance.heatTimer * 2.5f - (DayNightCycle.isSandStorm ? 3.5f : 0));

		if ((move.z > .2f || move.x > .2f) && !crouch && !madeStep && !inTriggerArea && CameraBobbing.Instance.GetYValue() < 1.5f)
			StartCoroutine(PlayStepSound());
		else if (((move.z <= .2f && move.z > 0) || (move.x <= 2f && move.x > 0)) && !madeStep && !inTriggerArea && CameraBobbing.Instance.GetYValue() < 1.5f)
			StartCoroutine(PlayStepSound(true));

		madeStep = CameraBobbing.Instance.GetYValue() < 1.5f;
	}
	IEnumerator PlayStepSound(bool silent = false)
	{
		if (audioSourceSteps2.isPlaying)
		{
			audioSourceSteps.pitch = silent ? Random.Range(.4f, .7f) : Random.Range(.8f, 1.1f);
			audioSourceSteps.volume = silent ? Random.Range(.15f, .4f) : Random.Range(.6f, 1.2f);
			audioSourceSteps.Play();
		}
		else
		{
			audioSourceSteps2.pitch = silent ? Random.Range(.4f, .7f) : Random.Range(.8f, 1.1f);
			audioSourceSteps2.volume = silent ? Random.Range(.15f, .4f) : Random.Range(.6f, 1.2f);
			audioSourceSteps2.Play();
		}

		yield return null;
	}
	IEnumerator GetDamage(Transform transf = null)
	{ // есть transf = отпрыгнуть
		if (gettingDamage)
			yield break;

		gettingDamage = true;
		LifeSystem.Instance.GetDamage(Random.Range(5f, 10f));

		if (transf != null)
		{
			StartCoroutine(Goto(transform.position + new Vector3(transform.position.x - transf.position.x, 0, transform.position.z - transf.position.z), true));
		}

		audioSource.PlayOneShot(otherSounds[Random.Range(1, 3)], Random.Range(2f, 2.5f));
		yield return new WaitForSeconds(1f);
		gettingDamage = false;
	}
	IEnumerator Goto(Vector3 pos, bool run)
	{
		noControl = true;

		while (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(pos.x, pos.z)) > 0.75f)
		{
			CheckRun(run);
			move = new Vector3(pos.x - transform.position.x, 0, pos.z - transform.position.z);
			yield return null;
		}

		yield return new WaitForSeconds(0.15f);

		noControl = false;
	}
	void CheckCrouch(bool value)
	{
		transform.localScale = new Vector3(1, value ? 0.5f : 1.05f, 1);
		speed = value ? 3 : 9;
		crouch = value;

		if (LifeSystem.Instance.heatTimer < 0.5f)
			canRun = !value;
	}
	void CheckRun(bool i)
	{
		if (LifeSystem.Instance.energy.fillAmount <= 0)
			runNeedRest = true;
		if (LifeSystem.Instance.energy.fillAmount >= 0.2f)
			runNeedRest = false;

		if (run = i && !runNeedRest && (move.x > 0 || move.z > 0))
			speed = 9 + LifeSystem.Instance.energyMultiply;
		else if (!crouch)
			speed = 9;
	}
	void OnTriggerStay(Collider other)
	{
		if (move.z == 0 && move.x == 0)
			return;

		if (!audioSource.isPlaying && other.transform.parent.name.Contains("grass"))
		{
			audioSource.clip = grassSounds[Random.Range(0, grassSounds.Length)];
			audioSource.Play();
		}

		if (other.transform.parent.name.Contains("cactus") && !other.gameObject.GetComponent<Interactable>().pickup)
			StartCoroutine(GetDamage(other.transform.parent));
	}
	void OnTriggerEnter(Collider other)
	{
		if (other.transform.parent.name.Contains("grass"))
		{
			inTriggerArea = true;
			audioSource.PlayOneShot(grassSounds[Random.Range(0, grassSounds.Length)], Random.Range(.2f, 1.5f));
		}
	}
	void OnTriggerExit(Collider other)
	{
		if (other.transform.parent.name.Contains("grass"))
		{
			inTriggerArea = false;
			audioSource.PlayOneShot(grassSounds[Random.Range(0, grassSounds.Length)], Random.Range(.2f, 1.5f));
		}
	}
}