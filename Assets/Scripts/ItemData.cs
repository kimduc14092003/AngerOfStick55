using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemData
{
    public static Dictionary<int, ItemDefault> LifeItemData = new Dictionary<int, ItemDefault>()
    {
        { 1, new ItemDefault(2,150,450) },
        { 2, new ItemDefault(3,450,960) },
        { 3, new ItemDefault(4,900,1760) },
        { 4, new ItemDefault(5,1650,3360) },
        { 5, new ItemDefault(6,3150,4960) },
        { 6, new ItemDefault(7,4650,6560) },
        { 7, new ItemDefault(8,6150,8160) },
        { 8, new ItemDefault(9,1650,-1) }
    };

    public static Dictionary<int, ItemDefault> SkillItemData = new Dictionary<int, ItemDefault>()
    {
        { 1, new ItemDefault(3,20,240) },
        { 2, new ItemDefault(4,120,440) },
        { 3, new ItemDefault(5,220,840) },
        { 4, new ItemDefault(6,420,1240) },
        { 5, new ItemDefault(7,620,1640) },
        { 6, new ItemDefault(8,820,2040) },
        { 7, new ItemDefault(9,1020,-1) },
    };

    public static Dictionary<int, ItemDefault> MedicineItemData = new Dictionary<int, ItemDefault>()
    {
        { 1, new ItemDefault(5,50,220) },
        { 2, new ItemDefault(10,50,660) },
        { 3, new ItemDefault(15,50,1320) },
        { 4, new ItemDefault(20,50,2420) },
        { 5, new ItemDefault(25,50,4620) },
        { 6, new ItemDefault(30,50,-1) },
    };

    public static Dictionary<int, ItemGunDefault> GunARG170ItemData = new Dictionary<int, ItemGunDefault>()
    {
        { 1, new ItemGunDefault(252,80,8000) },
        { 2, new ItemGunDefault(326,96,100) },
        { 3, new ItemGunDefault(390,115,300) },
        { 4, new ItemGunDefault(506,126,600) },
        { 5, new ItemGunDefault(606,152,1100) },
        { 6, new ItemGunDefault(726,167,2100) },
        { 7, new ItemGunDefault(942,183,3100) },
        { 8, new ItemGunDefault(1130,220,4100) },
        { 9, new ItemGunDefault(1468,242,5100) },
    };
}

public class ItemDefault
{
    public int maxCount;
    public int pricePerLife;
    public int priceToUpgrade;

    public ItemDefault(int maxCount, int pricePerLife, int priceToUpgrade)
    {
        this.maxCount = maxCount;
        this.pricePerLife = pricePerLife;
        this.priceToUpgrade = priceToUpgrade;
    }
}

public class ItemGunDefault
{
    public int damage;
    public int maxBullet;
    public int priceOfUpgrade;
    public ItemGunDefault(int damage, int maxBullet, int priceOfUpgrade)
    {
        this.damage = damage;
        this.maxBullet = maxBullet;
        this.priceOfUpgrade = priceOfUpgrade;
    }
}

