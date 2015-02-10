using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MODEL_LIST
{
	public string     _nodeName;
	public GameObject _model;
}

[System.Serializable]
public class EquipExtendAttribute
{
    public FC_EQUIP_EXTEND_ATTRIBUTE propKey;
    public int propValue;
}

[AddComponentMenu("FC/Logic/FCObject/WeaponAndArmor/FCEquipmentsBase")]
public class FCEquipmentsBase : FCObject
{
	//we should not add the weapon to bip, instead into root node
	public string _specNode = "";
	public bool _isLogicEquipment;
	
	public FC_EQUIP_TYPE _equipType = FC_EQUIP_TYPE.NORMAL;
    //texture of the armor. For weapons, leave this empty.
    public string _texturePath;

    //which area the texture will be applied to? There could be 1+
    public EnumAvatarArea[] _avatarAreas;

    [System.Serializable]
    public class EquipmentSlot
    {
        //which node this equipment will be put on
        public EnumEquipSlot _equipSlot;
		
		public FC_EQUIPMENTS_TYPE _equipmentType = FC_EQUIPMENTS_TYPE.ARMOR;
        //which model to use? Leave this empty if the equipment does not have a mesh
        public string _modelPath;
    }

    /// <summary>
    /// an equipment, weapon or armor, can have more than one slots.
    /// For example, armor for forearm, it could have two different models for left and right arm.
    /// The equipment can change the avatar (body) texture as well.
    /// </summary>
    public EquipmentSlot[] _equipmentSlots;
	
	[HideInInspector]
	public int _evolutionLevel = 0;
	public EquipmentSlot []_upgradeSlots;
	
	protected override void Awake()
	{
		base.Awake();
	}
	

    public virtual void SetOwner(FCObject owner)
    {
    }

    virtual public void OnAssembled(EnumEquipSlot slot, GameObject go, FC_EQUIPMENTS_TYPE equipmentType)
    {
    }


    private static string[] nodeNames = new string[]
    {
        "node_helm",            //head
        "node_chest",           //chest
        "node_left_shoulder",   //shoulder_left
        "node_right_shoulder",  //shoulder_right
        "node_left_armpiece",   //left arm
        "node_right_armpiece",  //right arm
        "node_left_weapon",     //left weapon
        "node_right_weapon",    //right weapon
        "node_left_thigh",      //thigh_left
        "node_right_thigh",     //thigh_right
        "node_left_calf",       //calf_left
        "node_right_calf",      //calf_right
        "node_belt",            //belt
		"node_foot_point",		//foot
		"node_head_point",		//head
		"A",		//weapon
		"B",		//weapon
		"node_smash_effect",	//for cyclo boss		
		"node_weapon_hang" //for hang weapon
    };

    public static string GetNodeByEquipSlot(EnumEquipSlot slot)
    {
        return nodeNames[(int)slot];
    }

    private static string[] avatarAreaNames = new string[]
    {
        "face",
        "helmet",
        "shoulder",
        "chest",
        "arm",
        "arm_armor",
        "leg",
        "leg_armor",
        "belt"
    };

    public static string GetAvatarAreaNameByArea(EnumAvatarArea area)
    {
        return avatarAreaNames[(int)area];
    }
}

public enum EnumEquipSlot
{
	//left for ac, right is for bullet
    head, //a 
    chest, //b
    shoulder_left,  //c
    shoulder_right, //d
    arm_left, //e
    arm_right, //f
    weapon_left, //g
    weapon_right, //h
    thigh_left, //i
    thigh_right, //j
    calf_left, //k
    calf_right, //l
    belt, //m
	foot_point, //n
	head_point, //o
	damage_main_A, //p
	damage_main_B, //q
	smash_effect,//r
	weapon_hang,
    MAX
}

public enum EnumAvatarArea
{
    face,
    helmet,
    shoulder,
    chest,
    arm,
    arm_armor,
    leg,
    leg_armor,
    belt,
    MAX
}