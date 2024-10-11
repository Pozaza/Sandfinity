using UnityEngine;

[CreateAssetMenu]
public class Item : ScriptableObject {
	public string id;
	public Sprite spr;
	public int max;
	public int maxFind;
	public bool pickup;
	public bool removeNeedles;
}
