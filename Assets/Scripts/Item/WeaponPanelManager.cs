using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject listItemPanel;

    [SerializeField] private List<ItemSO> listData;
    // Start is called before the first frame update
    void Start()
    {
        GetDefaultData();
    }

    private void GetDefaultData()
    {
        for(int i = 0; i < listItemPanel.transform.childCount; i++)
        {
            GameObject resultFind= listItemPanel.transform.GetChild(i).Find("ImageDes").gameObject;
            if(resultFind != null)
            {
                Image imageDes=resultFind.GetComponent<Image>();
                imageDes.sprite = listData[i].imgDescription;
            
            }
            GameObject resultFind2 = listItemPanel.transform.GetChild(i).Find("countText").gameObject;
            if (resultFind != null)
            {
                TMP_Text countTxt = resultFind2.GetComponent<TMP_Text>();
                countTxt.text = listData[i].currentAmount + " / " + listData[i].maxAmount;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
