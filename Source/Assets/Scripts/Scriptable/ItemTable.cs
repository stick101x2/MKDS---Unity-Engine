using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MKDS/Items/ItemTable")]
public class ItemTable : ScriptableObject
{
	public ItemPlaceChance[] chances = new ItemPlaceChance[8];
	
	public Item GetItem(int placing)
	{
		return chances[placing].GetRandomItem();
	}

}

[System.Serializable]
public class ItemPlaceChance
{
	public Item[] items = new Item[0];
	public Sprite[] images;
	public Item GetRandomItem()
	{
		int rand = Random.Range(0, items.Length);
		return items[rand];
	}
}
