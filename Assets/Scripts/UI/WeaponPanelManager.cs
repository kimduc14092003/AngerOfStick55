using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class WeaponPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject listItemPanel,playerTeamPanel,buyAmountButton,upgradeButton,nextPageButton,backPageButton;
    [SerializeField] private List<ItemSO> listData;
    [SerializeField] private TMP_Text currentPageText;
    [SerializeField] private TMP_Text nameText, levelText,damageText,speedText,amountText,reloadText;
    [SerializeField] private TMP_Text priceToUpgradeText,priceToReloadText;
    [SerializeField] private Image chooseItemImage;
    private int currentPageIndex,maxPageIndex;
    private GameObject currentItemChoose=null;
    private List<string> listFriendSelected;
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
        SetAnimationDefautl();
        GetListFriendDefault();
    }

    private void SetAnimationDefautl()
    {
        nextPageButton.transform.DOMoveX(nextPageButton.transform.position.x + 0.2f, 0.35f).SetLoops(-1,LoopType.Restart);
        backPageButton.transform.DOMoveX(backPageButton.transform.position.x - 0.2f, 0.35f).SetLoops(-1,LoopType.Restart);
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
                ItemController itemController = listItemPanel.transform.GetChild(i).GetComponent<ItemController>();
                itemController.itemId = listData[currentIndexOfData].id;

                // Active ô lựa chọn bạn đồng hành
                if(listData[currentIndexOfData] is ItemFriend)
                {
                    if (itemController.checkBoxSelectFriend)
                    {
                        itemController.checkBoxSelectFriend.SetActive(true);

                        Toggle toggle = itemController.checkBoxSelectFriend.GetComponent<Toggle>();
                        // Kiểm tra data hiện đang chọn bạn đồng hành hay không?
                        if(toggle != null)
                        {
                            int intSelectValue= PlayerPrefs.GetInt(itemController.itemId + PlayerPrefItemKey.IsSelected);
                            if (intSelectValue == 0)
                            {
                                toggle.isOn = false;
                            }
                            else
                            {
                                toggle.isOn= true;
                            }
                        }
                    }
                }
                else
                {

                    if (itemController.checkBoxSelectFriend)
                    {
                        itemController.checkBoxSelectFriend.SetActive(false);
                    }
                }
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
                itemController.GetComponent<Image>().color = new Color32(115, 114, 114, 198);
                itemController.checkBoxSelectFriend.SetActive(false);
            }
            else
            {
                itemController.lockPanel.SetActive(false);
                itemController.GetComponent<Image>().color = new Color32(0, 0, 0, 0);
            }
        }
    }

    private void GetListFriendDefault()
    {
        listFriendSelected = new List<string>();
        foreach (ItemSO item in listData)
        {
            if(item is ItemFriend)
            {
                int intSelected= PlayerPrefs.GetInt(item.id +PlayerPrefItemKey.IsSelected);
                if (intSelected != 0)
                {
                    listFriendSelected.Add(item.id);
                }
            }
        }
        SetListFriendDefaultToPanel();
    }

    private void SetListFriendDefaultToPanel()
    {
        for (int i = 0;i< playerTeamPanel.transform.childCount;i++)
        {
            ItemInTeamController item = playerTeamPanel.transform.GetChild(i).GetComponent<ItemInTeamController>();
            if(item != null)
            {
                try
                {
                    if (i <= listFriendSelected.Count)
                    {
                        item.noFriend.SetActive(false);
                        item.hasFriend.SetActive(true);

                        ItemFriend itemFriend = ScriptableObject.CreateInstance<ItemFriend>(); ;
                        foreach (ItemSO item1 in listData)
                        {
                            if (listFriendSelected[i - 1] == item1.id)
                            {
                                itemFriend = item1 as ItemFriend;
                                break;
                            }
                        }
                        item.imgThumb.sprite = itemFriend.imgDescription;
                        item.friendLevel.text="Lv "+ PlayerPrefs.GetInt(itemFriend.id +PlayerPrefItemKey.CurrentLevel)+"";
                    }
                    else
                    {
                        item.noFriend.SetActive(true);
                        item.hasFriend.SetActive(false);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.Log("No Friend!");
                }

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

        //Kiểm tra lại item background đã đúng chưa
        CheckAllItemInListIsLock();

        //Thay đổi màu button mới
        if (currentItemChoose.GetComponent<Image>() != null)
        {
            currentItemChoose.GetComponent<Image>().color = new Color32(26, 255, 123, 65);
        }

        int maxLevelItem;

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
        if (currentAmount<maxAmount || currentLevel==0)
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
                priceToReload = itemGun.priceOfGunDefault;
            }
            else
            {
                levelText.text = "LV: " + currentLevel;
                int amountOfItemReload = maxAmount - currentAmount;
                priceToReload *= amountOfItemReload;
            }
            damageText.text = "Damage: " + PlayerPrefs.GetInt(id + PlayerPrefItemKey.Damage, 0); ;
            speedText.text = "Speed: " + itemGun.bulletSpeed;
            amountText.text = "Bullet: " + ": " + currentAmount + " / " + maxAmount;
            reloadText.text = "Reload Time: " + itemGun.reloadTime + "s";

           
        }

        //Thay đổi dữ liệu cho item Melee
        if (currentItem is ItemMelee)
        {
            ItemMelee item = currentItem as ItemMelee;
            if (currentLevel == 0)
            {
                levelText.text = "LV: " + ++currentLevel;
                priceToReload = item.priceOfWeaponDefault;

            }
            else
            {
                levelText.text = "LV: " + currentLevel;
                int amountOfItemReload = maxAmount - currentAmount;
                priceToReload *= amountOfItemReload;
            }
            damageText.text = "Damage: " + PlayerPrefs.GetInt(id + PlayerPrefItemKey.Damage, 0); ;
            amountText.text = "Durability: " + currentAmount + " / " + maxAmount;
            

        }
        //Thay đổi dữ liệu cho item Friend
        if (currentItem is ItemFriend)
        {
            ItemFriend item = currentItem as ItemFriend;

            if (currentLevel == 0)
            {
                levelText.text = "LV: " + ++currentLevel;
                priceToReload = item.priceOfFriendDefault;
            }
            else
            {
                levelText.text = "LV: " + currentLevel;
                int amountOfItemReload = maxAmount - currentAmount;
                priceToReload *= amountOfItemReload;
            }
            damageText.text = "Damage: " + PlayerPrefs.GetInt(id + PlayerPrefItemKey.Damage, 0); ;
            amountText.text = "HP: " + currentAmount + " / " + maxAmount;

           
        }


        //Thay đổi dữ liệu cho button mua/ nâng cấp
        priceToUpgradeText.text = string.Format("{0:N0}", priceToUpgrade);
        priceToReloadText.text = string.Format("{0:N0}", priceToReload);

        // Kiểm tra xem đã max level nâng cấp chưa
        if(IsItemMaxLevel(id, currentLevel))
        {
            upgradeButton.SetActive(false);
        }
        else
        {
            upgradeButton.SetActive(true);
        }

    }

    private bool IsItemMaxLevel(string id,int currentLevel)
    {
        switch (id)
        {
            case "ItemHealth":
                {
                    int maxLevel = StaticData.healthPriceToUpgradeRate.Length;
                    if (currentLevel >= maxLevel)
                    {
                        return true;
                    }
                    break;
                }
            case "ItemSkill":
                {
                    int maxLevel = StaticData.skillPriceToUpgradeRate.Length;
                    if (currentLevel >= maxLevel)
                    {
                        return true;
                    }
                    break;
                }
            case "ItemMedicine":
                {
                    int maxLevel = StaticData.medicinePriceToUpgradeRate.Length;
                    if (currentLevel >= maxLevel)
                    {
                        return true;
                    }
                    break;
                }
            default:
                {
                    int maxLevel = StaticData.priceOfGunUpgradeRate.Length;
                    if (currentLevel >= maxLevel+1)
                    {
                        return true;
                    }
                    break;
                }
        }

        return false;
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

                    int priceToUpgrade = (int)(currentItem.priceToUpgradeDefault * StaticData.healthPriceToUpgradeRate[currentLevel - 1]);
                    PlayerPrefs.SetInt(id + PlayerPrefItemKey.PriceToUpgrade, priceToUpgrade);
                    break;
                }
            case "ItemSkill":
                {
                    int maxAmount = PlayerPrefs.GetInt(id + PlayerPrefItemKey.MaxAmount);
                    PlayerPrefs.SetInt(id + PlayerPrefItemKey.MaxAmount, ++maxAmount);

                    int priceToUpgrade = (int)(currentItem.priceToUpgradeDefault * StaticData.skillPriceToUpgradeRate[currentLevel - 1]);
                    PlayerPrefs.SetInt(id + PlayerPrefItemKey.PriceToUpgrade, priceToUpgrade);
                    break;
                }
            case "ItemMedicine":
                {
                    int maxAmount = PlayerPrefs.GetInt(id + PlayerPrefItemKey.MaxAmount);
                    maxAmount += 5;
                    PlayerPrefs.SetInt(id + PlayerPrefItemKey.MaxAmount, maxAmount);

                    int priceToUpgrade = (int)(currentItem.priceToUpgradeDefault * StaticData.skillPriceToUpgradeRate[currentLevel - 1]);
                    PlayerPrefs.SetInt(id + PlayerPrefItemKey.PriceToUpgrade, priceToUpgrade);
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
        else if(item as ItemFriend) 
        {
            ItemFriend _item = (ItemFriend)item;
            int damage = (int)(_item.damageDefault * StaticData.damageOfGunRate[currentLevel-1]);
            PlayerPrefs.SetInt(id + PlayerPrefItemKey.Damage, damage);
            GetListFriendDefault();
        }

        // Cập nhật giá nâng cấp vũ khí
        try
        {
            int priceToUpgrade = (int)(item.priceToUpgradeDefault * StaticData.priceOfGunUpgradeRate[currentLevel-1]);
            PlayerPrefs.SetInt(id + PlayerPrefItemKey.PriceToUpgrade, priceToUpgrade);
        }
        catch (System.Exception e)
        {
            upgradeButton.SetActive(false);
        }
        
        // Cập nhật giá mua đạn
        try
        {
            int bullet = (int)(item.maxAmountDefault * StaticData.bulletOfGunRate[currentLevel-1]);
            PlayerPrefs.SetInt(id + PlayerPrefItemKey.MaxAmount, bullet);

        }catch (System.Exception e)
        {
            buyAmountButton.SetActive(false);
        }

    }

    public void ReloadAmountItem()
    {
        string id = currentItemChoose.GetComponent<ItemController>().itemId;
        int currentLevel = PlayerPrefs.GetInt(id + PlayerPrefItemKey.CurrentLevel);
        if (currentLevel == 0)
        {
            PlayerPrefs.SetInt(id + PlayerPrefItemKey.CurrentLevel,++currentLevel);
        }
        int currentAmount = PlayerPrefs.GetInt(id + PlayerPrefItemKey.CurrentAmount, 0);
        int maxAmount = PlayerPrefs.GetInt(id + PlayerPrefItemKey.MaxAmount, 0);

        //Reload Lại toàn bộ số lượng còn lại
        PlayerPrefs.SetInt(id + PlayerPrefItemKey.CurrentAmount, maxAmount);

        //Load lại dữ liệu trang và chi tiết sản phẩm hiện tại
        GetDefaultData(false);
        GetDetailDataOfItem(currentItemChoose.GetComponent<ItemController>());
    }

    public void ToggleSelectFriend(ItemController itemController)
    {
        Toggle toggle = itemController.checkBoxSelectFriend.GetComponent<Toggle>();
        if(toggle != null) 
        {
            if (listFriendSelected.Count >= 3)
            {
                toggle.isOn = false;
                bool isSelected = toggle.isOn;
                int intSelected = isSelected ? 1 : 0;

                PlayerPrefs.SetInt(itemController.itemId + PlayerPrefItemKey.IsSelected, intSelected);
            }
            else
            {
                bool isSelected = toggle.isOn;
                int intSelected= isSelected? 1:0;

                PlayerPrefs.SetInt(itemController.itemId + PlayerPrefItemKey.IsSelected, intSelected);
            }
        }
        else
        {
            Debug.Log("toggle is null");
        }
        GetListFriendDefault();

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
