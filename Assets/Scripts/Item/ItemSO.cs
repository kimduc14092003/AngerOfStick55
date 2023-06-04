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
    public int priceOfGun;
    public int priceOfABullet;
}

[CreateAssetMenu(fileName = "NewItem", menuName = "ShopItem/Melee")]
public class ItemMelee : ItemSO
{
    public int damage;
    public int priceOfWeapon;
    public int priceToFix;
}

[CreateAssetMenu(fileName = "NewItem", menuName = "ShopItem/Friend")]
public class ItemFriend :ItemSO
{
    public int damage;
    public bool isSelected;
    public int priceOfFriend;
}
