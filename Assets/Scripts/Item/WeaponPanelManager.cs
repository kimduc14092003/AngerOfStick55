using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject listItemPanel;
    [SerializeField] private List<ItemSO> listData;
    [SerializeField] private TMP_Text currentPageText;
    [SerializeField] private TMP_Text nameText, levelText,damageText,speedText,amountText,reloadText;
    [SerializeField] private Image chooseItemImage;
    private int currentPageIndex,maxPageIndex;
    private GameObject currentItemChoose=null;
    // Start is called before the first frame update
    void Start()
    {
        currentPageIndex = 1;
        maxPageIndex = 4;
        GetDefaultData();
       
    }
    private void StartChooseItem()
    {
        ItemController itemController= listItemPanel.transform.GetChild(0).GetComponent<ItemController>();
        GetDetailDataOfItem(itemController); 
    }

    private void GetDefaultData()
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
                countTxt.text = listData[currentIndexOfData].currentAmount + " / " + listData[currentIndexOfData].maxAmount;
            }
        }
        currentPageText.text = currentPageIndex + " / " + maxPageIndex;
        StartChooseItem();
    }

    public void NextPage()
    {
        currentPageIndex++;
        if(currentPageIndex>maxPageIndex)
        {
            currentPageIndex = 1;
        }
        GetDefaultData();
    }

    public void BackPage()
    {
        currentPageIndex--;
        if (currentPageIndex <=0 )
        {
            currentPageIndex = maxPageIndex;
        }
        GetDefaultData();
    }

    public void GetDetailDataOfItem(ItemController itemController)
    {
        //Reset màu button cũ 
        if(currentItemChoose != null)
        {
            if(currentItemChoose.GetComponent<Image>())
            currentItemChoose.GetComponent<Image>().color =new Color32(0,0,0,0);
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
        for(int i=0;i<listData.Count;i++)
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
        chooseItemImage.sprite = currentItem.imgDescription;
        nameText.text = currentItem.nameOfItem;
        levelText.text =currentItem.description;
        amountText.text = currentItem.nameOfItem + ": " + currentItem.currentAmount + " / " + currentItem.maxAmount;

        //Thay đổi dữ liệu cho item Gun
        if (currentItem is ItemGun)
        {
            ItemGun itemGun = (ItemGun)currentItem;
            levelText.text = "LV: " + itemGun.currentLevel;
            damageText.text = "Damage: " + itemGun.damage;
            speedText.text = "Speed: " + itemGun.bulletSpeed;
            amountText.text = "Bullet: " + currentItem.currentAmount + " / " + currentItem.maxAmount;
            reloadText.text = "Reload Time: " + itemGun.reloadTime + "s";
        }
        //Thay đổi dữ liệu cho item Melee
        if (currentItem is ItemMelee)
        {
            ItemMelee item = (ItemMelee)currentItem;
            levelText.text = "LV: " + item.currentLevel;
            damageText.text = "Damage: " + item.damage;
            amountText.text = "Bullet: " + currentItem.currentAmount + " / " + currentItem.maxAmount;
        }
        //Thay đổi dữ liệu cho item Friend
        if (currentItem is ItemFriend)
        {
            ItemFriend item = (ItemFriend)currentItem;
            levelText.text = "LV: " + item.currentLevel;
            damageText.text = "Damage: " + item.damage;
            amountText.text = "HP: " + currentItem.currentAmount + " / " + currentItem.maxAmount;
        }

    }
}
