using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewItem",menuName = "ShopItem/Item")]
public class ItemSO : ScriptableObject
{
    public string id;
    public string nameOfItem;
    public string description;
    public int maxAmountDefault;
    public Sprite imgDescription;
    public int priceToUpgradeDefault;// lv 1->2->3 v.v
    public int pricePerOneDefault;
    public int currentLevel;
}
[CreateAssetMenu(fileName = "NewItem", menuName = "ShopItem/Gun")]
public class ItemGun : ItemSO
{
    public int damageDefault;
    public int bulletSpeed;
    public float reloadTime;
    public int priceOfGunDefault; //lv 0 ->1 
    public int priceOfABullet;// Cố định
}

[CreateAssetMenu(fileName = "NewItem", menuName = "ShopItem/Melee")]
public class ItemMelee : ItemSO
{
    public int damageDefault;
    public int priceOfWeaponDefault;//lv 0 ->1 
    public int priceToFix;
}

[CreateAssetMenu(fileName = "NewItem", menuName = "ShopItem/Friend")]
public class ItemFriend :ItemSO
{
    public int damageDefault;
    public bool isSelected;
    public int priceOfFriendDefault;//lv 0 ->1 
}
