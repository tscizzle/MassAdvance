using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pack
{
    public Pack(string packId_arg, string packName_arg, List<CardInfo> cardList_arg)
    {
        packId = packId_arg;
        packName = packName_arg;
        cardList = cardList_arg;
    }

    public string packId;
    public string packName;
    public List<CardInfo> cardList;
}
