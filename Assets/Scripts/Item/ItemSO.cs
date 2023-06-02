using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewItem",menuName = "ShopItem/Item")]
public class ItemSO : ScriptableObject
{
    public string nameOfItem;
    public string description;
    public int maxAmount;
    public int currentAmount;
    public Sprite imgDescription;
    public int priceToUpgrade;
    public int currentLevel;

}
[CreateAssetMenu(fileName = "NewItem", menuName = "ShopItem/Gun")]
public class ItemGun : ItemSO
{
    public int damage;
    public int bulletSpeed;
    public float reloadTime;
    public int priceOfABullet;
}
