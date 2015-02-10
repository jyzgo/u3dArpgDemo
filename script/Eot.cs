using UnityEngine;
using System.Collections;

[System.Serializable]
public class Eot
{
    //damage of time
    //1 means param is percent from target hp
    //2 means param is percent from owner normal damage
    // thousand means DOT, DEBUFF, BUFF = effectType
    // handred means TEMP, for all combat = timeType
    // ten means detailType , fire , ice ...
    // one means effectValueType, 1 from target, 0 from self

    public enum EOT_TYPE
    {
        NONE = -1,
        //debuff
        //DAMAGE_OF_TIME = 0,

        EOT_FIRE = 0,
        EOT_ICE,
        EOT_THUNDER,
        EOT_POISON,
        EOT_PHYSICAL,

        EOT_PHYSICAL_ATTACK,
        EOT_PHYSICAL_DEFENSE,
        EOT_SPEED,
        EOT_ELEMENTAL_ATTACK,
        EOT_ELEMENTAL_RESISTANCE,

        //buff for seconds
        EOT_HP,//health
        EOT_MANA,// mana
        EOT_ELEMENTAL_EFFECT,//消除负面的效果不能被击晕击倒
    }

    public enum EOT_EVENT
    {
        START,
        REFRESH,
        END
    }

    public EOT_TYPE eotType = EOT_TYPE.NONE;
    public float eotPercent = 0;
    public float eotValue = 0;
    public float lastTime = 0;

    private bool _isActive = false;
    public bool IsActive
    {
        get { return _isActive; }
        set { _isActive = value; }
    }

    private int _jumpCount;
    public int JumpCount
    {
        get { return _jumpCount; }
        set { _jumpCount = value; }
    }

    private bool _isFrom2P = false;
    public bool IsFrom2p
    {
        get { return _isFrom2P; }
        set { _isFrom2P = value; }
    }

    private float _ownerDamage = 0;
    public float OwnerDamage
    {
        get { return _ownerDamage; }
        set { _ownerDamage = value; }
    }

    public static void Copy(Eot target, Eot source)
    {
        target.eotType = source.eotType;
        target.eotPercent = source.eotPercent;
        target.eotValue = source.eotValue;
        target.lastTime = source.lastTime;
        target._jumpCount = (int)source.lastTime;
        target._ownerDamage = source._ownerDamage;
    }
}
