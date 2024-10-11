using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour {
	public GameObject startChunk, startRail;
	public GameObject[] cosmetics, tiles, big, canyon, moving, railPrefabs;
	public Interactables[] interactables;
	public int startChunks = 5;

	List<GameObject> lastGenerated = new(), chunks = new(), second = new();
	int railsMultiplier = 1;

	void Start() {
		chunks = GenerateChunk(startChunk);

		for (int i = 0; i <= startChunks; i++)
			GeneratingTask();

		StartCoroutine(GenerateNearest());
	}
	void GeneratingTask() {
		for (int i = 0; i < chunks.Count; i++)
			second.AddRange(GenerateChunk(chunks[i]));
		chunks.Clear();
		AddRails();

		for (int i = 0; i < second.Count; i++)
			chunks.AddRange(GenerateChunk(second[i]));
		second.Clear();
		AddRails();
	}
	public List<GameObject> GenerateChunk(GameObject chunk) {
		List<GameObject> objects = new(),
		temp = new(),
		deleteChunks = new();

		CheckFaces(chunk, ref temp);

		foreach (GameObject obj in lastGenerated)
			for (int i2 = 0; i2 < temp.Count; i2++)
				if (obj.transform.position == temp[i2].transform.position)
					deleteChunks.Add(temp[i2]);
		// Определение одинаковых чанков

		foreach (GameObject obj in deleteChunks) {
			temp.Remove(obj);
			Destroy(obj);
		}
		// Удаление одинаковых чанков

		foreach (GameObject tile in temp)
			objects.Add(AddCosmetics(tile));
		// Добавление косметики

		lastGenerated.Remove(chunk);
		lastGenerated.AddRange(objects);
		return objects;
	}
	GameObject AddCosmetics(GameObject tile) {
		Transform tileChild = tile.transform.GetChild(0);
		tileChild.gameObject.layer = 8;
		tileChild.gameObject.isStatic = true;
		tileChild.GetComponent<MeshCollider>().convex = true;

		tile.transform.eulerAngles = new Vector3(0, GameManager.Rand() ? GameManager.Rand() ? GameManager.Rand() ? 270 : 180 : 90 : 0, 0);

		if (Random.Range(0f, 1f) < .15f) {
			GameObject cosm;
			if (Random.Range(0f, 1f) < .075f) {
				cosm = InstantiateRandomObject(canyon, ref tile);
				cosm.transform.position = tile.transform.position + new Vector3(0, 0.4f, 0);
			} else
				cosm = InstantiateRandomObject(big, ref tile);
			RandomScale(ref cosm);
			cosm.transform.GetChild(0).gameObject.layer = 7;
		} else if (Random.Range(0f, 1f) < .45f) {
			bool hasSounds = false;
			for (int i = 0; i <= Random.Range(0, 3); i++) {
				if (GameManager.Rand()) {
					Interactables randomItem = interactables[Random.Range(0, interactables.Length)];
					GameObject interact = Instantiate(randomItem.model, tile.transform.position + new Vector3(Random.Range(-3, 4), 0.4f, Random.Range(-3, 4)), Quaternion.Euler(0, Random.Range(-175, 176), 0), transform.parent);
					RandomScale(ref interact);

					Transform interactChild = interact.transform.GetChild(0);
					interactChild.gameObject.layer = 7;
					interactChild.gameObject.AddComponent<Interactable>().item = randomItem.item;
					BoxCollider c = interactChild.gameObject.AddComponent<BoxCollider>();
					c.isTrigger = true;
					c.size *= 1.05f;
					interactChild.tag = "Interactable";
					CheckIfObject(ref interact, ref hasSounds);
					interactChild.GetComponent<MeshCollider>().convex = true;
				} else {
					GameObject cosm = InstantiateRandomObject(cosmetics, ref tile);
					RandomScale(ref cosm);
					cosm.transform.GetChild(0).gameObject.layer = 7;
					CheckIfObject(ref cosm, ref hasSounds);
				}
			}
			if (Random.Range(0f, 1f) < .05f) {
				GameObject move = InstantiateRandomObject(moving, ref tile);
				RandomScale(ref move);
				move.transform.position = tile.transform.position + new Vector3(Random.Range(-2, 3), 5, Random.Range(-2, 3));
				move.AddComponent<Move>().gameObject.AddComponent<Rigidbody>();
				move.transform.GetChild(0).gameObject.layer = 7;
				move.transform.GetChild(0).GetComponent<MeshCollider>().convex = true;
				if (GameManager.Rand()) {
					GameObject child = InstantiateRandomObject(moving, ref tile);
					child.transform.SetParent(move.transform, false);
					RandomScale(ref child);
					child.transform.localPosition = Vector3.up;
					child.transform.Rotate(new Vector3(Random.Range(-175, 176), Random.Range(-175, 176), Random.Range(-175, 176)));
					child.layer = 7;
					child.transform.GetChild(0).GetComponent<MeshCollider>().convex = true;
				}
			}
		}
		return tile;
	}
	void AddRails() {
		int layerMask = 1 << 7;
		GameObject forward = Instantiate(Random.Range(0f, 1f) >= 0.9f ? railPrefabs[0] : railPrefabs[1], startRail.transform.position + Vector3.forward * 14 * railsMultiplier, Quaternion.identity, transform);
		for (int i = 0; i < 10; i++) {
			if (Physics.BoxCast(forward.transform.position - (Vector3.up * 12f), forward.transform.GetChild(0).lossyScale * 4, new Vector3(0, 0.001f, 0), out RaycastHit hitForward, Quaternion.identity, Mathf.Infinity, layerMask)) {
				Destroy(hitForward.transform.parent.gameObject);
			}
		}
		GameObject back = Instantiate(Random.Range(0f, 1f) >= 0.9f ? railPrefabs[0] : railPrefabs[1], startRail.transform.position + Vector3.back * 14 * railsMultiplier, Quaternion.identity, transform);
		for (int i = 0; i < 10; i++) {
			if (Physics.BoxCast(back.transform.position - (Vector3.up * 10f), back.transform.GetChild(0).lossyScale * 4, new Vector3(0, 0.001f, 0), out RaycastHit hitBack, Quaternion.identity, Mathf.Infinity, layerMask)) {
				Destroy(hitBack.transform.parent.gameObject);
			}
		}
		railsMultiplier++;
	}
	void CheckIfObject(ref GameObject cosm, ref bool hasSounds) {
		if (cosm.transform.name.Contains("grass")) {
			cosm.transform.GetChild(0).GetComponent<MeshCollider>().convex = true;
			cosm.transform.GetChild(0).GetComponent<MeshCollider>().isTrigger = true;
		}
		if (Random.Range(0f, 1f) < .02f && !hasSounds) {
			GameObject go = new("crickets");
			AudioSource audioSource = go.AddComponent<AudioSource>();
			audioSource.loop = true;
			audioSource.clip = PlayerScript.Instance.otherSounds[0];
			audioSource.maxDistance = 1000;
			audioSource.spatialBlend = 1;
			audioSource.Play();
			go.transform.SetParent(transform.parent);
			go.transform.position = cosm.transform.position;
			hasSounds = true;
		}
	}
	GameObject InstantiateRandomObject(GameObject[] type, ref GameObject tile) {
		return Instantiate(type[Random.Range(0, type.Length)], tile.transform.position + new Vector3(Random.Range(-3, 4), 0.4f, Random.Range(-3, 4)), Quaternion.Euler(0, Random.Range(-175, 176), 0), transform.parent);
	}
	void RandomScale(ref GameObject obj) {
		obj.transform.localScale = new Vector3(GameManager.Rand() ? obj.transform.localScale.x * -1 : obj.transform.localScale.x, obj.transform.localScale.y, GameManager.Rand() ? obj.transform.localScale.z * -1 : obj.transform.localScale.z);
	}
	IEnumerator GenerateNearest() {
		List<GameObject> toGenerate = new();

		foreach (GameObject obj in lastGenerated) // выбираем все чанки которые в радиусе игрока
			if (Vector3.Distance(obj.transform.position, PlayerScript.Instance.gameObject.transform.position) < 150)
				toGenerate.Add(obj);

		foreach (GameObject obj in toGenerate)
			GenerateChunk(obj);

		yield return new WaitForSeconds(1);
		StartCoroutine("GenerateNearest");
	}
	void CheckFaces(GameObject chunk, ref List<GameObject> temp) {
		Vector3 pos;
		for (int i = 0; i < 4; i++) {
			pos = i == 0 ? Vector3.left
				: i == 1 ? Vector3.right
				: i == 2 ? Vector3.forward
				: Vector3.back;

			if (Physics.RaycastAll(chunk.transform.position + Vector3.up / 5, pos, 9).Length == 0)
				temp.Add(Instantiate(tiles[Random.Range(0, tiles.Length)], chunk.transform.position + pos * 16, Quaternion.identity, transform));
		}
	}
}

[System.Serializable]
public class Interactables {
	public GameObject model;
	public Item item;
}