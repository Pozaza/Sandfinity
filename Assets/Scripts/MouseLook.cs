using UnityEngine;
using UnityEngine.UI;
public class MouseLook : MonoBehaviour {
	public static MouseLook Instance;
	public float sensivity = 100f;
	public Transform player, playerCam;
	public Sprite[] sprites;
	public Image crosshair;
	public GameObject contextMenu;

	RectTransform rect;
	float yRotation;
	bool lockRotation;
	Ray lastRay;
	Interactable interactable;
	Transform[] contextMenuButtons;

	void Awake() {
		Instance = this;
		Cursor.lockState = CursorLockMode.Locked;
		rect = crosshair.GetComponent<RectTransform>();
		contextMenuButtons = contextMenu.GetComponentsInChildren<Transform>();
	}
	void Update() {
		transform.localPosition = playerCam.localPosition;

		transform.parent.position = new(playerCam.parent.position.x, Mathf.Lerp(transform.parent.position.y, playerCam.parent.position.y, Time.deltaTime * 10), playerCam.parent.position.z);

		Ray ray = lockRotation ? lastRay : Camera.main.ScreenPointToRay(Input.mousePosition);
		lastRay = ray;
		if (Physics.Raycast(ray, out RaycastHit hit, 3.5f)) {
			Transform obj = hit.transform;
			bool interact = obj.CompareTag("Interactable");
			obj.TryGetComponent(out interactable);

			if (!interact) {
				interactable = null;
				DisableAllButtons();
				lockRotation = false;
				PlayerScript.Instance.moveLock = false;
				crosshair.gameObject.SetActive(true);
				contextMenu.SetActive(false);
				Cursor.lockState = CursorLockMode.Locked;
			}
			crosshair.sprite = interact ? interactable.pickup ? sprites[1] : sprites[2] : sprites[0];
			rect.sizeDelta = interact ? new Vector2(50, 50) : new Vector2(15, 15);

			if (interact && Input.GetMouseButtonDown(1)) {
				lockRotation = true;
				PlayerScript.Instance.moveLock = true;
				crosshair.gameObject.SetActive(false);
				contextMenu.SetActive(true);
				Cursor.lockState = CursorLockMode.None;
			} else if (interact && Input.GetMouseButtonUp(1)) {
				lockRotation = false;
				PlayerScript.Instance.moveLock = false;
				crosshair.gameObject.SetActive(true);
				contextMenu.SetActive(false);
				Cursor.lockState = CursorLockMode.Locked;
			}

			if (interact && Input.GetMouseButton(1)) {
				ActionButton("RemoveNeedles", interactable.item.removeNeedles && !interactable.pickup);
				ActionButton("Grab", interactable.pickup);
				ActionButton("GrabAll", !interactable.item.removeNeedles && interactable.pickup);
			}

			if (interact && interactable.pickup && Input.GetMouseButtonDown(0) && !contextMenu.activeSelf)
				interactable.Grab();
		} else {
			DisableAllButtons();
			contextMenu.SetActive(false);
			crosshair.sprite = sprites[0];
			rect.sizeDelta = new Vector2(15, 15);
			Cursor.lockState = CursorLockMode.Locked;
			PlayerScript.Instance.moveLock = false;
			lockRotation = false;
			interactable = null;
			crosshair.gameObject.SetActive(true);
		}

		if (!lockRotation) {
			float mouseX = Input.GetAxis("Mouse X") * sensivity * Time.deltaTime;
			float mouseY = Input.GetAxis("Mouse Y") * sensivity * Time.deltaTime;

			yRotation -= mouseY;
			yRotation = Mathf.Clamp(yRotation, -100, 100);

			player.Rotate(Vector3.up * mouseX);

			transform.eulerAngles = new(yRotation + playerCam.eulerAngles.x, player.eulerAngles.y, playerCam.eulerAngles.z);
		}

		Inventory.Instance.ChangeSlot((int)Input.mouseScrollDelta.y);

		if (int.TryParse(Input.inputString, out int result))
			Inventory.Instance.SelectSlot(result);
	}
	public void GetRequest(string type) {
		interactable.Invoke(type, 0);
	}
	void ActionButton(string name, bool i) {
		if (i)
			contextMenu.transform.Find(name).gameObject.SetActive(true);
		else
			contextMenu.transform.Find(name).gameObject.SetActive(false);
	}
	void DisableAllButtons() {
		foreach (Transform i in contextMenuButtons)
			if (!i.name.Contains("sprite") && !i.name.Contains("text"))
				i.gameObject.SetActive(false);
	}
}