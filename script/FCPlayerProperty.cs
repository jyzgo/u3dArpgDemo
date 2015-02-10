using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class FCPlayerProperty : MonoBehaviour
{
    public void RefreshKeyAndValue(PlayerPropKey key, float value)
    {
        UILabel label = GetComponent<UILabel>();
        PlayerPropData playerPropData = DataManager.Instance.PlayerPropDataList.GetPlayerPropDataByProp(key);

        if (PlayerPropKey.Level == key)
        {
            label.text = Localization.Localize("IDS_MESSAGE_GLOBAL_LEVEL") + " : [BEAA82]" + (int)value + "[-]";
        }
        else
        {
            string labelString = Localization.Localize(playerPropData.ids);
            switch (key)
            { 
                case PlayerPropKey.FireDmg:
                case PlayerPropKey.FireRes:
                case PlayerPropKey.IceDmg:
                case PlayerPropKey.IceRes:
                case PlayerPropKey.LightningDmg:
                case PlayerPropKey.LightningRes:
                case PlayerPropKey.PosisonDmg:
                case PlayerPropKey.PosisonRes:
                    labelString = labelString + " : ";
                    break;
                default:
                    labelString = labelString + " : ";
                    break;
            }

            if (DataManager.Instance.IsPercentFormat(key))
            {
                value *= 100;
                value = (int)(value * 100) / 100.00f;
                label.text = labelString + "[BEAA82]" + value + "%[-]";
            }
            else
            {
                value = (int)(value * 100) / 100.00f;
                label.text = labelString + "[BEAA82]" + value + "[-]";
            }
        }
    }
}
