using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {
	public static Inventory Instance;
	public List<InventorySlot> items = new();
	public GameObject slotPrefab;
	public GameObject[] inventory;
	Sprite selectedSlotSprite, slotSprite;
	int selectedSlot;

	void Awake() {
		Instance = this;
		selectedSlotSprite = inventory[0].GetComponent<Image>().sprite;
		slotSprite = inventory[1].GetComponent<Image>().sprite;
	}
	public void AddItem(Item item, int amount = 1) {
		foreach (InventorySlot slot in items) {
			if (slot.item.id == item.id && slot.amount < slot.item.max) {
				int free = slot.item.max - slot.amount;
				slot.amount += amount > free ? free : amount;
				UpdateSlot(slot.item.id);
				if (amount - free > 0)
					AddItemToSlot(item, amount - free);
				return;
			}
		}

		AddItemToSlot(item, amount);
	}
	void UpdateSlot(string id) {
		int slot_id = 0;

		foreach (InventorySlot slot in items) {
			if (slot.item.id == id) {
				foreach (Text text in inventory[slot_id].GetComponentsInChildren<Text>())
					text.text = slot.amount.ToString();
			}
			slot_id++;
		}
	}
	void AddItemToSlot(Item item, int amount) {
		if (items.Count >= 5)
			return;
		InventorySlot newSlot = new(item, amount);
		foreach (GameObject slot in inventory) {
			if (slot.transform.childCount <= 0) {
				GameObject slotItem = Instantiate(slotPrefab, Vector3.zero, Quaternion.identity, slot.transform);
				slotItem.transform.localPosition = Vector3.zero;
				slotItem.transform.localRotation = Quaternion.identity;
				slotItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
				slotItem.GetComponentInChildren<Image>().sprite = item.spr;
				foreach (Text text in slotItem.GetComponentsInChildren<Text>())
					text.text = amount.ToString();
				items.Add(newSlot);
				return;
			}
		}
	}
	public void ChangeSlot(int i) {
		if (i == -1)
			selectedSlot = selectedSlot > 0 ? selectedSlot - 1 : 4;
		else if (i == 1)
			selectedSlot = selectedSlot < 4 ? selectedSlot + 1 : 0;
		foreach (GameObject slot in inventory) {
			slot.TryGetComponent(out RectTransform transf);
			slot.TryGetComponent(out Image image);
			transf.localScale = slot != inventory[selectedSlot] ? Vector3.one : new Vector3(1.15f, 1.15f, 1.15f);
			image.sprite = slot != inventory[selectedSlot] ? slotSprite : selectedSlotSprite;
		}
	}
	public void SelectSlot(int i) {
		if (i <= 5 && i > 0)
			selectedSlot = i - 1;
		foreach (GameObject slot in inventory) {
			slot.TryGetComponent(out RectTransform transf);
			slot.TryGetComponent(out Image image);
			transf.localScale = slot != inventory[selectedSlot] ? Vector3.one : new Vector3(1.15f, 1.15f, 1.15f);
			image.sprite = slot != inventory[selectedSlot] ? slotSprite : selectedSlotSprite;
		}
	}
}

[System.Serializable]
public class InventorySlot {
	public Item item;
	public int amount;
	public InventorySlot(Item item, int amount = 1) {
		this.item = item;
		this.amount = amount;
	}
}