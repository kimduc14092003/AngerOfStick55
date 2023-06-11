using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject listItemPanel,buyAmountButton;
    [SerializeField] private List<ItemSO> listData;
    [SerializeField] private TMP_Text currentPageText;
    [SerializeField] private TMP_Text nameText, levelText,damageText,speedText,amountText,reloadText;
    [SerializeField] private TMP_Text priceToUpgradeText,priceToReloadText;
    [SerializeField] private Image chooseItemImage;
    private int currentPageIndex,maxPageIndex;
    private GameObject currentItemChoose=null;

    private void Awake()
    {
        currentPageIndex = 1;
        maxPageIndex = 4;
        SetDefaultDataPlayerPrefs();
    }

    private void SetDefaultDataPlayerPrefs()
    {
        for (int i = 0; i < listData.Count; i++)
        {
            if (!PlayerPrefs.HasKey(listData[i].id + PlayerPrefItemKey.CurrentLevel))
            {
                PlayerPrefs.SetInt(listData[i].id + PlayerPrefItemKey.CurrentLevel, listData[i].currentLevel);
            }
            if (!PlayerPrefs.HasKey(listData[i].id + PlayerPrefItemKey.MaxAmount))
            {
                PlayerPrefs.SetInt(listData[i].id + PlayerPrefItemKey.MaxAmount, listData[i].maxAmountDefault);
            }
            if (!PlayerPrefs.HasKey(listData[i].id + PlayerPrefItemKey.CurrentAmount))
            {
                PlayerPrefs.SetInt(listData[i].id + PlayerPrefItemKey.CurrentAmount, listData[i].maxAmountDefault);
            }
            if (!PlayerPrefs.HasKey(listData[i].id + PlayerPrefItemKey.PricePerOne))
            {
                PlayerPrefs.SetInt(listData[i].id + PlayerPrefItemKey.PricePerOne, listData[i].pricePerOneDefault);
            }
            if (!PlayerPrefs.HasKey(listData[i].id + PlayerPrefItemKey.PriceToUpgrade))
            {
                PlayerPrefs.SetInt(listData[i].id + PlayerPrefItemKey.PriceToUpgrade, listData[i].priceToUpgradeDefault);
            }

            if (listData[i] is ItemGun)
            {
                ItemGun item = (ItemGun)listData[i];
                if (!PlayerPrefs.HasKey(item.id + PlayerPrefItemKey.Damage))
                {
                    PlayerPrefs.SetInt(item.id + PlayerPrefItemKey.Damage, item.damageDefault);
                }
            }
            if (listData[i] is ItemMelee)
            {
                ItemMelee item = (ItemMelee)listData[i];
                if (!PlayerPrefs.HasKey(item.id + PlayerPrefItemKey.Damage))
                {
                    PlayerPrefs.SetInt(item.id + PlayerPrefItemKey.Damage, item.damageDefault);
                }
            }
            if (listData[i] is ItemFriend)
            {
                ItemFriend item = (ItemFriend)listData[i];
                if (!PlayerPrefs.HasKey(item.id + PlayerPrefItemKey.Damage))
                {
                    PlayerPrefs.SetInt(item.id + PlayerPrefItemKey.Damage, item.damageDefault);
                }
                if(!PlayerPrefs.HasKey(item.id+ PlayerPrefItemKey.IsSelected))
                {
                    PlayerPrefs.SetInt(item.id + PlayerPrefItemKey.IsSelected, 0);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GetDefaultData(true);
    }
    private void StartChooseItem()
    {
        ItemController itemController= listItemPanel.transform.GetChild(0).GetComponent<ItemController>();
        GetDetailDataOfItem(itemController); 
    }

    private void GetDefaultData(bool returnFirstItem)
    {
        for(int i = 0; i < listItemPanel.transform.childCount; i++)
        {
            GameObject resultFind= listItemPanel.transform.GetChild(i).Find("ImageDes").gameObject;
            int currentIndexOfData = i + (currentPageIndex - 1) * listItemPanel.transform.childCount;
            
            // Nếu index vượt qua danh sách thì sẽ deactive gameobject đó
            if (currentIndexOfData >= listData.Count)
            {
                listItemPanel.transform.GetChild(i).gameObject.SetActive(false);
                continue;
            }
            else
            {
                listItemPanel.transform.GetChild(i).gameObject.SetActive(true);
                listItemPanel.transform.GetChild(i).GetComponent<ItemController>().itemId = listData[currentIndexOfData].id;
            }
            if (resultFind != null)
            {
                Image imageDes=resultFind.GetComponent<Image>();
                imageDes.sprite = listData[currentIndexOfData].imgDescription;
            
            }
            GameObject resultFind2 = listItemPanel.transform.GetChild(i).Find("countText").gameObject;
            if (resultFind != null)
            {
                TMP_Text countTxt = resultFind2.GetComponent<TMP_Text>();

                int maxAmount = PlayerPrefs.GetInt(listData[currentIndexOfData].id + PlayerPrefItemKey.MaxAmount, 0);
                int currentAmount = PlayerPrefs.GetInt(listData[currentIndexOfData].id + PlayerPrefItemKey.CurrentAmount, 0);
                countTxt.text = currentAmount + " / " + maxAmount;
            }
        }
        currentPageText.text = currentPageIndex + " / " + maxPageIndex;
        CheckAllItemInListIsLock();
        if (returnFirstItem)
        {
            StartChooseItem();
        }
    }

    private void CheckAllItemInListIsLock()
    {
        for(int i = 0; i < listItemPanel.transform.childCount; i++) 
        {
            ItemController itemController = listItemPanel.transform.GetChild(i).GetComponent<ItemController>();
            string id = itemController.itemId;

            
            // Kiểm tra xem level có mở khóa chưa
            if (PlayerPrefs.GetInt(id + PlayerPrefItemKey.CurrentLevel) == 0)
            {
                itemController.lockPanel.SetActive(true);
            }
            else
            {
                itemController.lockPanel.SetActive(false);
            }
        }
    }

    public void NextPage()
    {
        currentPageIndex++;
        if(currentPageIndex>maxPageIndex)
        {
            currentPageIndex = 1;
        }
        GetDefaultData(true);
    }

    public void BackPage()
    {
        currentPageIndex--;
        if (currentPageIndex <=0 )
        {
            currentPageIndex = maxPageIndex;
        }
        GetDefaultData(true);
    }

    public void GetDetailDataOfItem(ItemController itemController)
    {
        //Reset màu button cũ 
        if (currentItemChoose != null)
        {
            if (currentItemChoose.GetComponent<Image>())
                currentItemChoose.GetComponent<Image>().color = new Color32(0, 0, 0, 0);
        }
        currentItemChoose = itemController.gameObject;

        //Thay đổi màu button mới
        if (currentItemChoose.GetComponent<Image>() != null)
        {
            currentItemChoose.GetComponent<Image>().color = new Color32(26, 255, 123, 65);
        }


        string id = itemController.itemId;

        ItemSO currentItem = ScriptableObject.CreateInstance<ItemSO>();

        // Lọc tìm id của Item trong danh sách dữ liệu
        for (int i = 0; i < listData.Count; i++)
        {
            if (id == listData[i].id)
            {
                currentItem = listData[i];
                break;
            }
        }

        //Reset tất cả text thông tin về rỗng
        nameText.text = "";
        levelText.text = "";
        damageText.text = "";
        speedText.text = "";
        amountText.text = "";
        reloadText.text = "";

        //Thay đổi dữ liệu cho item thường
        int currentLevel = PlayerPrefs.GetInt(id + PlayerPrefItemKey.CurrentLevel, 0);
        int currentAmount = PlayerPrefs.GetInt(id + PlayerPrefItemKey.CurrentAmount, 0);
        int maxAmount = PlayerPrefs.GetInt(id + PlayerPrefItemKey.MaxAmount, 0);
        int priceToUpgrade= PlayerPrefs.GetInt(id + PlayerPrefItemKey.PriceToUpgrade, 0);
        int priceToReload= PlayerPrefs.GetInt(id + PlayerPrefItemKey.PricePerOne, 0);

        chooseItemImage.sprite = currentItem.imgDescription;
        nameText.text = currentItem.nameOfItem;
        levelText.text = currentItem.description;
        amountText.text = currentItem.nameOfItem + ": " + currentAmount + " / " + maxAmount;


        // Kiểm tra số lượng còn lại đã full hay chưa
        if (currentAmount<maxAmount)
        {
            buyAmountButton.SetActive(true);
        }
        else { buyAmountButton.SetActive(false);}

        //Thay đổi dữ liệu cho item Gun
        if (currentItem is ItemGun)
        {
            ItemGun itemGun = currentItem as ItemGun;
            if (currentLevel == 0)
            {
                levelText.text = "LV: " + ++currentLevel;
            }
            else
            {
                levelText.text = "LV: " + currentLevel;
            }
            damageText.text = "Damage: " + PlayerPrefs.GetInt(id + PlayerPrefItemKey.Damage, 0); ;
            speedText.text = "Speed: " + itemGun.bulletSpeed;
            amountText.text = "Bullet: " + ": " + currentAmount + " / " + maxAmount;
            reloadText.text = "Reload Time: " + itemGun.reloadTime + "s";

            int amountOfItemReload = maxAmount - currentAmount;
            priceToReload*= amountOfItemReload;
        }

        //Thay đổi dữ liệu cho item Melee
        if (currentItem is ItemMelee)
        {
            if (currentLevel == 0)
            {
                levelText.text = "LV: " + ++currentLevel;
            }
            else
            {
                levelText.text = "LV: " + currentLevel;
            }
            damageText.text = "Damage: " + PlayerPrefs.GetInt(id + PlayerPrefItemKey.Damage, 0); ;
            amountText.text = "Durability: " + currentAmount + " / " + maxAmount;
            
            int amountOfItemReload = maxAmount - currentAmount;
            priceToReload *= amountOfItemReload;
        }
        //Thay đổi dữ liệu cho item Friend
        if (currentItem is ItemFriend)
        {
            if (currentLevel == 0)
            {
                levelText.text = "LV: " + ++currentLevel;
            }
            else
            {
                levelText.text = "LV: " + currentLevel;
            }
            damageText.text = "Damage: " + PlayerPrefs.GetInt(id + PlayerPrefItemKey.Damage, 0); ;
            amountText.text = "HP: " + currentAmount + " / " + maxAmount;

            int amountOfItemReload = maxAmount - currentAmount;
            priceToReload *= amountOfItemReload;
        }

        //Thay đổi dữ liệu cho button mua/ nâng cấp
        priceToUpgradeText.text = priceToUpgrade + "";
        priceToReloadText.text = priceToReload + "";

        // Kiểm tra xem đã max level nâng cấp chưa


    }

    public void UpdateItem()
    {
        string id = currentItemChoose.GetComponent<ItemController>().itemId;
        ItemSO currentItem = ScriptableObject.CreateInstance<ItemSO>();

        // Lọc tìm id của Item trong danh sách dữ liệu
        for (int i = 0; i < listData.Count; i++)
        {
            if (id == listData[i].id)
            {
                currentItem = listData[i];
                break;
            }
        }
        
        int currentLevel = PlayerPrefs.GetInt(id + PlayerPrefItemKey.CurrentLevel);
        PlayerPrefs.SetInt(id + PlayerPrefItemKey.CurrentLevel, ++currentLevel);

        switch (id)
        {
            case "ItemHealth":
                {
                    int maxAmount= PlayerPrefs.GetInt(id + PlayerPrefItemKey.MaxAmount);
                    PlayerPrefs.SetInt(id + PlayerPrefItemKey.MaxAmount, ++maxAmount);
                    break;
                }
            case "ItemSkill":
                {
                    int maxAmount = PlayerPrefs.GetInt(id + PlayerPrefItemKey.MaxAmount);
                    PlayerPrefs.SetInt(id + PlayerPrefItemKey.MaxAmount, ++maxAmount);
                    break;
                }
            case "ItemMedicine":
                {
                    int maxAmount = PlayerPrefs.GetInt(id + PlayerPrefItemKey.MaxAmount);
                    maxAmount += 5;
                    PlayerPrefs.SetInt(id + PlayerPrefItemKey.MaxAmount, maxAmount);
                    break;
                }
            default:
                {
                    UpgradeWeapon(currentItem,id, currentLevel);
                    break;
                }
        }
        GetDefaultData(false);
        GetDetailDataOfItem(currentItemChoose.GetComponent<ItemController>());
    }

    public void ReloadAmountItem()
    {
        string id = currentItemChoose.GetComponent<ItemController>().itemId;

        int currentAmount = PlayerPrefs.GetInt(id + PlayerPrefItemKey.CurrentAmount, 0);
        int maxAmount = PlayerPrefs.GetInt(id + PlayerPrefItemKey.MaxAmount, 0);

        //Reload Lại toàn bộ số lượng còn lại
        PlayerPrefs.SetInt(id + PlayerPrefItemKey.CurrentAmount, maxAmount);

        //Load lại dữ liệu trang và chi tiết sản phẩm hiện tại
        GetDefaultData(false);
        GetDetailDataOfItem(currentItemChoose.GetComponent<ItemController>());
    }

    private void UpgradeWeapon(ItemSO item,string id,int currentLevel)
    {

        if(item as ItemMelee)
        {
            ItemMelee _item = (ItemMelee)item;
            int damage = (int)(_item.damageDefault * StaticData.damageOfGunRate[currentLevel-1]);
            PlayerPrefs.SetInt(id + PlayerPrefItemKey.Damage, damage);
        }
        else
        if (item as ItemGun)
        {
            ItemGun _item = (ItemGun)item;
            int damage = (int)(_item.damageDefault * StaticData.damageOfGunRate[currentLevel-1]);
            PlayerPrefs.SetInt(id + PlayerPrefItemKey.Damage, damage);
        }
        else if(item as ItemMelee) 
        {
            ItemFriend _item = (ItemFriend)item;
            int damage = (int)(_item.damageDefault * StaticData.damageOfGunRate[currentLevel-1]);
            PlayerPrefs.SetInt(id + PlayerPrefItemKey.Damage, damage);
        }

        int bullet = (int)(item.maxAmountDefault * StaticData.bulletOfGunRate[currentLevel-1]);
        PlayerPrefs.SetInt(id + PlayerPrefItemKey.MaxAmount, bullet);

        int priceToUpgrade = (int)(item.priceToUpgradeDefault * StaticData.priceOfGunUpgradeRate[currentLevel-1]);
        PlayerPrefs.SetInt(id + PlayerPrefItemKey.PriceToUpgrade, priceToUpgrade);
        
    }

}

public enum PlayerPrefItemKey
{
    CurrentLevel,
    MaxAmount,
    CurrentAmount,
    PricePerOne,
    PriceToUpgrade,
    Damage,
    IsSelected
}
