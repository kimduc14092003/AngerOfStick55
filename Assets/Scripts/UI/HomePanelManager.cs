using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HomePanelManager : MonoBehaviour
{
    public TMP_Text goldTxt, jewelTxt;
    public TMP_Text playerHealthTxt, playerLevelTxt, playerPowerTxt;
    public GameObject upgradePlayerPanel;

    [Header("Upgrade Player")]
    public TMP_Text currentLevelTxt;
    public TMP_Text currentHPTxt, currentPowerTxt,nextLevelTxt, nextHPTxt, nextPowerTxt,priceToUpgradeLevelTxt;
    private void Awake()
    {
        
    }
    private void Start()
    {
        SetDefaultInforPlayer();
    }
    private void SetDefaultInforPlayer()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefPlayerKey.playerGold+""))
        {
            PlayerPrefs.SetInt(PlayerPrefPlayerKey.playerGold + "", 1000000);
        }
        if (!PlayerPrefs.HasKey(PlayerPrefPlayerKey.playerJewel + ""))
        {
            PlayerPrefs.SetInt(PlayerPrefPlayerKey.playerJewel + "", 1000000);
        }
        if (!PlayerPrefs.HasKey(PlayerPrefPlayerKey.playerHealth + ""))
        {
            PlayerPrefs.SetInt(PlayerPrefPlayerKey.playerHealth + "", 1000);
        }
        if (!PlayerPrefs.HasKey(PlayerPrefPlayerKey.playerLevel + ""))
        {
            PlayerPrefs.SetInt(PlayerPrefPlayerKey.playerLevel + "", 1);
        }
        if (!PlayerPrefs.HasKey(PlayerPrefPlayerKey.playerPower + ""))
        {
            PlayerPrefs.SetInt(PlayerPrefPlayerKey.playerPower + "", 60);
        }
        if (!PlayerPrefs.HasKey(PlayerPrefPlayerKey.playerUpgradePrice + ""))
        {
            PlayerPrefs.SetInt(PlayerPrefPlayerKey.playerUpgradePrice + "", 500);
        }
        int goldAmout= PlayerPrefs.GetInt("playerGold");
        goldTxt.text= string.Format("{0:N0}", goldAmout);

        int jewelAmount= PlayerPrefs.GetInt("playerJewel");
        jewelTxt.text= string.Format("{0:N0}", jewelAmount);

        SetInforPlayerInHome();
    }

    private void SetInforPlayerInHome()
    {
        playerHealthTxt.text = "HP: " + PlayerPrefs.GetInt(PlayerPrefPlayerKey.playerHealth + "");
        playerLevelTxt.text = "Lv " + PlayerPrefs.GetInt(PlayerPrefPlayerKey.playerLevel + "");
        playerPowerTxt.text = "Power: " + PlayerPrefs.GetInt(PlayerPrefPlayerKey.playerPower + "");
    }

    public void SetActiveUpgradePlayerPanel(bool isActive)
    {
        // luôn tắt panel nâng cấp Player khi level >= max level
        int currentLevel = PlayerPrefs.GetInt(PlayerPrefPlayerKey.playerLevel + "");
        if (currentLevel >= StaticData.playerHPRate.Length+1)
        {
            upgradePlayerPanel.SetActive(false);
            return;
        }
        upgradePlayerPanel.SetActive(isActive);
        if (isActive)
        {
            ResetDataUpgradePlayerPanel();
        }
    }

    private void ResetDataUpgradePlayerPanel()
    {

        int currentLevel = PlayerPrefs.GetInt(PlayerPrefPlayerKey.playerLevel + "");
        int currentHealth = PlayerPrefs.GetInt(PlayerPrefPlayerKey.playerHealth + "");
        int currentPower = PlayerPrefs.GetInt(PlayerPrefPlayerKey.playerPower + "");
        int currentPriceUpgrade = PlayerPrefs.GetInt(PlayerPrefPlayerKey.playerUpgradePrice + "");


        currentLevelTxt.text = currentLevel + "";
        currentHPTxt.text = currentHealth + "";
        currentPowerTxt.text = currentPower + "";

        nextLevelTxt.text = (currentLevel+1) + "";
        nextHPTxt.text =(int) (currentHealth * StaticData.playerHPRate[currentLevel-1]) + "";
        nextPowerTxt.text = (int)(currentPower * StaticData.playerPowerRate[currentLevel-1]) + "";

        priceToUpgradeLevelTxt.text = currentPriceUpgrade + "";
    }

    //Lưu dữ liệu mới cho player
    public void UpgradePlayer()
    {
        int currentLevel= PlayerPrefs.GetInt(PlayerPrefPlayerKey.playerLevel + "");
        int currentHealth= PlayerPrefs.GetInt(PlayerPrefPlayerKey.playerHealth + "");
        int currentPower= PlayerPrefs.GetInt(PlayerPrefPlayerKey.playerPower + "");
        int currentPriceUpgrade= PlayerPrefs.GetInt(PlayerPrefPlayerKey.playerUpgradePrice + "");

        currentLevel++;

        currentHealth = (int)(currentHealth * StaticData.playerHPRate[currentLevel - 2]);//15
        currentPower = (int)(currentPower * StaticData.playerPowerRate[currentLevel - 2]);
        currentPriceUpgrade = (int)(currentPriceUpgrade * StaticData.playerPriceToUpdateRate[currentLevel - 2]);

        PlayerPrefs.SetInt(PlayerPrefPlayerKey.playerLevel + "", currentLevel);
        PlayerPrefs.SetInt(PlayerPrefPlayerKey.playerHealth + "",currentHealth);
        PlayerPrefs.SetInt(PlayerPrefPlayerKey.playerPower + "",currentPower);
        PlayerPrefs.SetInt(PlayerPrefPlayerKey.playerUpgradePrice + "",currentPriceUpgrade);

        SetInforPlayerInHome();

        //Tắt panel upgrade khi nâng cấp lên max level
        if (currentLevel >= StaticData.playerHPRate.Length + 1)
        {
            SetActiveUpgradePlayerPanel(false);
            return;
        }

        ResetDataUpgradePlayerPanel();

    }

}

public enum PlayerPrefPlayerKey
{
    playerGold,
    playerJewel,
    playerHealth,
    playerLevel,
    playerPower,
    playerUpgradePrice
}
