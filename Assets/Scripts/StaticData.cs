using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticData 
{
    public static float[] playerHPRate = { 1 , 1.2f ,1.56f,2.028f,   2.636f,   3.69f,    4.797f,   6.234f,  8.727f,   11.343f,  14.744f, 19.167f,  24.915f,  32.388f,  42.104f };
    public static float[] playerPowerRate = { 1,1.5f, 2.233333333f, 3.1f, 4.333333333f, 5.633333333f, 7.316666667f, 8.75f,    12.21666667f, 15.85f,   17.41666667f, 20.86666667f, 25.03333333f, 30.03333333f, 36.03333333f };
    public static float[] playerPriceToUpdateRate = { 1 , 4,   10 , 18 , 28 , 40 , 56 , 76 , 100, 128, 160, 196, 236, 286 };

    public static float[] healthPricePerOneRate = { 1, 3, 6, 11, 21, 31, 41, 51 };
    public static float[] healthPriceToUpgradeRate = { 1,2.133333333f, 3.911111111f ,7.466666667f, 11.02222222f, 14.57777778f, 18.13333333f };

    public static float[] skillPricePerOneRate = { 1, 6, 11, 21, 31, 41, 51 };
    public static float[] skillPriceToUpgradeRate = { 1,1.833333333f, 3.5f, 5.166666667f, 6.833333333f, 8.5f };

    public static float[] medicinePricePerOneRate = { 1, 1, 1, 1, 1, 1 };
    public static float[] medicinePriceToUpgradeRate = { 1 , 3 ,  6 ,  11 , 21 };

    public static float[] damageOfGunRate = {1,1.293650794f,1.547619048f,2.007936508f,2.404761905f,2.880952381f,3.738095238f,4.484126984f,5.825396825f };
    public static float[] bulletOfGunRate = {1,1.2f,1.4375f,1.575f,1.9f,2.0875f,2.2875f,2.75f,3.025f};
    public static float[] priceOfGunUpgradeRate = { 1, 3, 6, 11, 21, 31, 41, 51 };
}
