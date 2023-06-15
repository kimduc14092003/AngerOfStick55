using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticData 
{
    public static float[] playerHPRate = { 1.3f, 1.3f, 1.3f, 1.3f, 1.3f, 1.3f, 1.3f, 1.3f, 1.3f, 1.3f, 1.3f, 1.3f, 1.3f, 1.3f };
    public static float[] playerPowerRate = { 1.5f,   1.488888889f, 1.388059701f, 1.397849462f, 1.3f, 1.298816568f, 1.195899772f, 1.396190476f, 1.297407913f, 1.098843323f, 1.198086124f, 1.199680511f, 1.199733688f, 1.199778024f };
    public static float[] playerPriceToUpdateRate = { 4, 2.5f, 1.8f, 1.555555556f, 1.428571429f, 1.4f, 1.357142857f, 1.315789474f, 1.28f,    1.25f,    1.225f,   1.204081633f, 1.211864407f, 1.211864407f };
    // Tỷ lệ so với level trước


    // Tỷ lệ so với giá trị ban đầu
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
