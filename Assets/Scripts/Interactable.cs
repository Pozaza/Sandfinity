using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class Interactable : MonoBehaviour {
	public Item item;
	public bool pickup;
	int count;
	Renderer render;
	void Start() {
		pickup = item.pickup;
		count = Random.Range(1, item.maxFind + 1);
		render = GetComponent<Renderer>();
	}
	public void Grab() {
		Inventory.Instance.AddItem(item, 1);
		count -= 1;
		if (count <= 0)
			Destroy(transform.parent.gameObject);
	}
	public void GrabAll() {
		Inventory.Instance.AddItem(item, count);
		Destroy(transform.parent.gameObject);
	}
	public void RemoveNeedles() {
		pickup = true;
		render.material = PlayerScript.Instance.modelMaterials[0];
	}
}
