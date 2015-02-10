using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Role enumeration of characters, mainly used in character selection.
/// </summary>
public enum EnumRole
{
    NotAvailable = -999,    //not authenticated yet, or attributes not loaded yet
    NotSelected = -1,       //roles are available, but selection not made yet
    Mage = 0,               //mage selected
    Warrior,
}

public enum FC_OBJECT_TYPE
{
	OBJ_NORMAL,
	
	OBJ_AC = (1 <<8),
	
	OBJ_WEAPON = 2*(1<<8),
	
	OBJ_BULLET = 3*(1<<8),
	
	OBJ_OBJECT_TYPE_MAX = 4*(1<<8)
};


public enum EnumLevelState
{
	LOCKED = 0,
	NEW_UNLOCK,
	D,
	C,
	B,
	A,
	S
};

public enum FC_CHEAT_FLAG
{
	Energe = 	1 << 0,
	Hit =		1 << 1,
	Attack =	1 << 2,
	SkillCD = 	1 << 3,
	
	
}

//should not delete
public enum FC_AI_TYPE
{
	PLAYER_MAGE = 0,
	PLAYER_WARRIOR,
	PLAYER_MONK,
	PLAYER_MAX,
	ENEMY_MELEE_NORMAL,
	ENEMY_NPC_TOWN,
	ENEMY_BOSS_TEST,
	NORMAL,
	ENEMY_FLY_SMALL,
	MAX
};

public enum FC_PARTICLE_BULLET_NUMBER
{
	FC_PARTICLE_BULLET_1 = 0,
	FC_PARTICLE_BULLET_5,
	FC_PARTICLE_BULLET_10,
	FC_PARTICLE_BULLET_20,
	FC_PARTICLE_BULLET_COUNT
}

public enum FC_NET_SYNC_TYPE
{
	POSITION,
	HIT_PONIT,
	ATTACK,
	MAX
};

public enum FC_ACTION_SWITCH_FLAG
{
    //IN_NPC,
	IN_GOD,
	IN_SUPER_SAIYAJIN,
	IN_SUPER_SAIYAJIN2,
	IN_SUPER_NAMEKIAN,
    //IN_ATTACK,
    //IN_SKILL,
	IN_RIGIDBODY,
	IN_RIGIDBODY2,
    //IN_DEADLOCK,
    //IN_DEFENCE,
    //IN_FREEZE,
    //IN_CHARGE,
    //NO_COST_WITH_SKILL,
	IN_DEEP_FREEZE,
	CANT_MOVE,
	CANT_ROTATE,
    //NO_GRAVITY,
    IGNORE_DOT,
    //USE_SP_DATA,
    //IN_ATTACK_BUFF,
    //IN_DEF_BUFF,
	COST_NO_ENERGY,
	GAIN_NO_ENERGY,
	GAIN_ENERGY_BY_ATTACK,
    IN_EOT_GODDOWN,
	
}
//in this way ,we can get fction weapon layer quickly
public enum FC_AC_FACTIOH_TYPE
{
	FRIEND = FCConst.LAYER_FRIENT,
	NEUTRAL_1 = FCConst.LAYER_NEUTRAL_1,
	NEUTRAL_2 = FCConst.LAYER_NEUTRAL_2,
	ENEMY = FCConst.LAYER_ENEMY
}
public enum FC_STEALTHY_LEVEL
{
	IN_STEALTHY,
	CAN_BE_SEE,
}


public enum FC_ELITE_TYPE
{
	VeryEasy,
	Easy,
	Normal,
	Hard,
	Elite,
	MiniBoss,
	Boss,
	hero,
	Chest,
	Bigchest,
	Obj,
	BigObj
}

public enum FC_EQUIP_TYPE
{
	NORMAL,
	HELM,
	SHOUDER,
	BREAST,
	ARM,
	LEG,
	WEAPON_LIGHT,
	WEAPON_HEAVY,
	WEAPON_SP,
	MAX
}


//can't change order , because we save int value for this enum.
public enum AIHitParams
{
    None = -1,

	Hp,
    HpPercent,

    Mp,
    MpPercent,

    RunSpeed,
    RunSpeedPercent,

    //AnimationSpeed,
    //AnimationSpeedPercent,

	CriticalChance,
    CriticalChancePercent,

	CriticalDamage,
    CriticalDamagePercent,

    CriticalChanceResist,
    CriticalChanceResistPercent,

    CriticalDamageResist,
    CriticalDamageResistPercent,

    AllElementResist,  //ALL resistance 2014-7-23 18:21:25
    AllElementResistPercent,

    AllElementDamage,
    AllElementDamagePercent,

    Defence,
    DefencePercent,

    Attack,
    AttackPercent,

    FireResist,
    FireResistPercent,

    FireDamage,
    FireDamagePercent,

    PoisonResist,
    PoisonResistPercent,

    PoisonDamage,
    PoisonDamagePercent,

    LightningResist,
    LightningResistPercent,

    LightningDamage,
    LightningDamagePercent,

    IceResist,
    IceResistPercent,

    IceDamage,
    IceDamagePercent, 

    //ParalysisResist,
    //FreezeResist,
    //DamageScale,
    //CurePoint,

    //FreezeTime,
    //FreezePercent,

    //ParalysisTime,
    //ParalysisPercent,

    //StunTime,
    //StunPercent,

    //PoisonTime,
    //PoisonTotal,

    //FireTime,
    //FireTotal,

    //BloodTime,
    //BloodTotal,
	
    //ThresholdMax,
	
	ExpAddition,
    ScAdditon,
    SkillDamageAddition,

    ItemFind,
    ReduceEnergy,

	RageRestore,
	ManaRestore,
	HpRestore,

    Damage,//just added at 2014-06-04 18:09:49
    Resist,//just added at 2014-06-04 18:10:13

    PassiveSkillGodDownCriticalToHp,
    
    Special,

	Max
}

public struct EquipmentAttributeVo
{
    public AIHitParams attribute;
    public float value;

    public EquipmentAttributeVo(AIHitParams attr, float value)
    {
        this.attribute = attr;
        this.value = value;
    }
}

public enum FC_EQUIP_EXTEND_ATTRIBUTE
{
    FUSION = 1,
    SOCKET,
    SOCKET1 = 1001,
    SOCKET2,
    SOCKET3
}

public enum FC_DAMAGE_TYPE
{
	NONE,
	PHYSICAL = 1,
	ICE,
	THUNDER,
	FIRE,
	POISON,
	DRAGON,
	MAX
}

public enum AttackHitType
{
    None,
    HurtNormal,
    HurtTiny,
    HurtFlash,
    KnockDown,
    KnockBack,
    HitFly,
    ParryFail,
    ParrySuccess,
    Dizzy,
    ForceBack,
    BlackHole,
    Lock

    //NONE,
    //NORMAL,
    //PARRYATT,
    //HURT_TINY,
    //HURT_NORMAL,
    //HURT_FLASH,
    //KNOCK,
    //DOWN,
    //DIZZY,
    //BACK,
    //UP,
    //FEAR,
    //BLACKHOLE,
    //LOCK,
    //PARRY_FAIL,
    //FORCE_BACK,
	
    //NORMAL_HURT = 999,
    //NORMAL_PARRYATT = 1000,
    //NORMAL_HURT_TINY,
    //NORMAL_HURT_NORMAL,
    //NORMAL_HURT_FLASH,
    //NORMAL_KNOCK,
    //NORMAL_DOWN,
    //NORMAL_DIZZY,
    //NORMAL_BACK,
    //NORMAL_UP,
    //NORMAL_FEAR,
    //NORMAL_BLACKHOLE,
    //NORMAL_LOCK,
    //NORMAL_PARRY_FAIL,
    //NORMAL_FORCE_BACK,
	
    //BLEND_HURT = 1999,
    //BLEND_PARRYATT = 2000,
    //BLEND_HURT_TINY,
    //BLEND_HURT_NORMAL,
    //BLEND_HURT_FLASH,
    //BLEND_KNOCK,
    //BLEND_DOWN,
    //BLEND_DIZZY,
    //BLEND_BACK,
    //BLEND_UP,
    //BLEND_FEAR,
    //BLEND_BLACKHOLE,
    //BLEND_LOCK,
    //BLEND_PARRY_FAIL,
    //BLEND_FORCE_BACK,
}
//FC_WEAPON_TYPE
//FC_EQUIPMENTS_TYPE
public enum FC_EQUIPMENTS_TYPE
{
	ARMOR = 0,
	WEAPON_HEAD = 1000,
	WEAPON_HEAVY = 1010,
	WEAPON_TRAIL = 1030,
	
	// if > single blade , means the weapon have 2 colliders at least
	WEAPON_SINGLE_BLADE,
	WEAPON_LIGHT = 1500,
	WEAPON_LIGHT_LEFT,
	WEAPON_LIGHT_RIGHT,
	
	WEAPON_THIGH = 1510,
	WEAPON_THIGH_LEFT,
	WEAPON_THIGH_RIGHT,
	
	WEAPON_SHOULDER = 1520,
	MAX = 10000
}

public enum FC_DEBUFFER_TYPE
{
	DAMAGE,
	SLOW,
	FREEZE
}

public enum FC_HURT_SOURCE
{
	WEAPON,
	BULLET
}

public enum FC_PARAM_TYPE
{
	NONE,
	BOOL,
	FLOAT,
	INT,
	VECTOR3,
	QUATERNION,
	OBJECT,
	OTHERS
}

public enum FC_BUFFER_ATTRIBUTES
{
	SKIN_COLOR,
	BODY_SIZE,
	MOVE_SPEED,
	ANI_SPEED,
	DAMAGE,
	DEFENSE
}

[System.Serializable]
public class DebufferSetEnabled
{
	public FC_DEBUFFER_TYPE _debufferType;
	public float _chance;
	public float _lastTime;
	public float _totalValue;
	public AttackHitType _hurtType;
	public FC_DAMAGE_TYPE _damageType;
}

public enum FC_KEY_BIND
{
	NONE = -1,
	DIRECTION = FCConst.FC_KEY_DIRECTION,
	ATTACK_1 = FCConst.FC_KEY_ATTACK_1,
	ATTACK_2 = FCConst.FC_KEY_ATTACK_2,
	ATTACK_3 = FCConst.FC_KEY_ATTACK_3,
	ATTACK_4 = FCConst.FC_KEY_ATTACK_4,
	KEY_FOR_TOUCH = FCConst.FC_KEY_ATTACK_5,
	ATTACK_5 = FCConst.FC_KEY_ATTACK_5,
	ATTACK_6 = FCConst.FC_KEY_ATTACK_6,
	ATTACK_7 = FCConst.FC_KEY_ATTACK_7,
	ATTACK_8 = FCConst.FC_KEY_ATTACK_8,
	ATTACK_9 = FCConst.FC_KEY_ATTACK_9,
	ATTACK_10 = FCConst.FC_KEY_ATTACK_10,
	MAX
}

public enum FC_COMBO_KIND
{
	NONE = -1,
	NORMAL_ATTACK,
	SKILL_ATTACK,
	DEFENSE_ATTACK,
	CHARGE_ATTACK,
	PASSIVE,
	MAX
}


public enum FC_EFFECT
{
	FIRE = 0,
	ICE,
	LIGHTING,

	MAX
}

public enum FC_EFFECT_KIND
{
	FIRE = 0,
	ICE,
	LIGHTING,

	MAX
}

//effect list for global effects
public enum FC_GLOBAL_EFFECT
{
	INVALID = -1,	
	START = 0,
	
	BASH = 0,
	BORN,
	BLOOD,
	ATTACK_PHYSICAL,
	PARRY_SUCCESS,
	PARRY_RECOIL,
	PARRY_FAIL,
	ICE_ARMOR_SUCCESS,
	ICE_ARMOR_RECOIL,
	ICE_ARMOR_FAIL,
	ICE_GROUND,
	ICE_GROUND_DISAPPEAR,
	FLASH_HIDE,
	FLASH_SHOW,
	BLOOD_GROUND,
	THUNDER_NO_TARGET,
	BLOOD_EXPLODE,
	ICE_GROUND1,
	ICE_GROUND2,
	BLOCKADE_FADEOUT,
	DEAD_EFFECT,
	BORN_FLY,
    HEXAGRATE_START,
    HEXAGRATE_HIT,

	
	MAX
}

//effect list for character generic built-in effects
public enum FC_CHARACTER_EFFECT
{
	INVALID = -1,
	START = 0,

	DIZZY = 0,
	LEVEL_UP = 1,
    STORM_ATTACK = 2,
    STORM_CHARGE = 3,
	FIRE_ATTACK = 4,
	STORM_CHARGE_LV1 = 5,
	SPEAR_CHARGE_LV3 = 6,
	SPEAR_CHARGE_BODY = 7,
	ICE_ARMOR = 8,
	STORM_CHARGE_NEW = 9,
	DOT_ICE2 = 10,
	
	//effect on hands
	HAND_LEFT_LIGHT = 11,
	HAND_RIGHT_LIGHT = 12,
	HAND_LEFT_ARCANE = 13,
	HAND_RIGHT_ARCANE = 14,	
	HAND_LEFT_FIRE = 15,
	HAND_RIGHT_FIRE = 16,
	HAND_LEFT_ICE = 17,
	HAND_RIGHT_ICE = 18,

	ICE_ARMOR2 = 19,
	BASH_START = 20,
	PARRY_ATTACK = 21,
	DIRECTION_INDICATOR = 22,
	FLASH_CHAIN_FIRE = 23,
	DAMAGE_PHYSICAL = 24,
	DAMAGE_ICE = 25,
	DAMAGE_FIRE = 26,
	DAMAGE_THUNDER = 27,
	DAMAGE_POISON = 28,
	DOT_PHYSICAL = 29,
	DOT_ICE = 30,
	DOT_FIRE = 31,
	DOT_THUNDER = 32,
	DOT_POISON = 33,
	
	SPEAR_CHARGE_LV0 = 34,
	PARRY_MAGE_ATTACK = 35,
	PSY_ATTACK_WARRIOR = 36,

    //DASH_PARTICLE = 37,
    //DASH_MODEL = 38,

	TRAIL_RED = 39,
	
	START_LIGHTNINGBALL = 40,
	DAMAGE_LIGHTNINGBALL = 41,
	
    //DASH_PARTICLE1 = 42,
    //DASH_MODEL1 = 43,
	
    //DASH_PARTICLE2 = 44,
    //DASH_MODEL2 = 45,
	
	DUST_LARGE = 46,
	MONK_SEVEN_WAVE = 47,
	DODGE = 48,
	KIKOHO = 49,
	DODGE_TAIL = 50,
	MONK_PUNCH_LEFT = 51,
	MONK_PUNCH_RIGHT = 52,
	MONK_PUNCH_SMASH = 53,
	MONK_PUNCH4_EFFECT = 54,
	MONK_KIKOHO_START = 55,
	MONK_SEVEN_LEFT = 56,
	MONK_SEVEN_RIGHT = 57,
	
	WARRIOR_SPIN_END = 58,
    //WARRIOR_DASH_END1 = 59,
	
	DUST_WEAPON_LEFT = 60,
	DUST_WEAPON_RIGHT = 61,
	ARCHER_CHARGE_LEFT = 62,
	CYCLOP_SMASH_FIRE_RIGHT = 63,
	DIE_BOSS_EFFECT = 64,
	REVIVE_HERO_EFFECT = 65,
	HERO_AIMING = 66,
    //WARRIOR_DASH_END2 = 67,
	
	RESTORE_HP_ONCE = 68,
	RESTORE_HP_LOOP = 69,
	RESTORE_MP_ONCE = 70,
	RESTORE_MP_LOOP = 71,
	MONK_PUNCH_TRIAL_LEFT = 72,
	MONK_PUNCH_TRIAL_RIGHT = 73,
	SLIME_MOVE_NORMAL = 74,
	SLIME_MOVE_POISON = 75,
	WOLFMAN_SLASH_LEFT = 76,
	WOLFMAN_SLASH_RIGHT = 77,
	WOLFMAN_SPEED_FORWARD = 78,
	WOLFMAN_SPEED_BACK = 79,
	WOLFMAN_DEFY = 80,
	SLIME_DIE_NORMAL = 81,
	SLIME_DIE_POISON = 82,
	HANMMER_SMASH_EFFECT = 83,
	BALL_SMASH_EFFECT = 84,
	MONK_VORTEX_DRAW = 85,
	MONK_VORTEX_ATTACK = 86,
	MONK_SHADOW_DOWN = 87,
	ERAGON_SHADOW_HIDE = 88,
	ERAGON_SHADOW_SHOW = 89,
	ERAGON_SHADOW_DIE_EXPLODE = 90,
	KYLIN_SMASH_LEFT_EFFECT = 91,
	KYLIN_SMASH_RIGHT_EFFECT = 92,
	KYLIN_CHARGE_EFFECT = 93,
	KYLIN_RELEASE_EFFECT = 94,
	KYLIN_PREPAIRE_EFFECT = 95,
	BASH_HITGROUND_EFFECT = 96,
	BASH_HITGROUND_BONUS_EFFECT = 97,
	BASH_SMASHGROUND_EFFECT_1 = 98,
	BASH_WEAPON_GIANT_1 = 99,
	CATW_SPEED_EFFECT = 100,
	CATW_SLASH_LEFT_EFFECT = 101,
	CATW_SLASH_RIGHT_EFFECT = 102,
	CATW_BORN_EFFECT = 103,
	CATW_DEFY_EFFECT = 104,
	MAGE_FIREPILLAR_RELEASE_EFFECT = 105,
	BASH_WEAPON_GIANT_2 = 106,
	BASH_WEAPON_GIANT_3 = 107,
	BASH_WEAPON_GIANT_4 = 108,
	BASH_SMASHGROUND_EFFECT_2 = 109,
	BASH_SMASHGROUND_EFFECT_3 = 110,
	BASH_SMASHGROUND_EFFECT_4 = 111,
	BASH_WEAPON_FLASH = 112,
	ENEMY_SPIN_EFFECT = 113,
	KIKOHO_END_EFFECT = 114,
	KYLIN_DASH_START_EFFECT = 115,
	KYLIN_DASH_SMASH_EFFECT = 116,
	ICE_SPARKLE_LEFT_EFFECT = 117,
	ICE_SPARKLE_RIGHT_EFFECT = 118,
	ERAGON_BORN_EFFECT = 119,
	BALL_SMASH_START = 120,
	BALL_DASH_TAIL = 121,
	BALL_DASH_EXPLODE = 122,
	BAT_WAVE_FIRE_START = 123,
	BORN_MAGE_FLASH_SHOW = 124,
	SKELETON_DASH_SLASH_EFFECT = 125,
	DIRECTION_INDICATOR_MULTIPLAYER = 126,

    //FC EFFECT
    SKILL_GOD_DOWN_EFFECT = 127,

    DashEffect = 128,
    DashFight = 129,
    DashSword = 130,

    NecromancerInvincible = 140,
    NecromancerSummon = 141,
    NecromancerAllBegin = 142,
    NecromancerSkullEnd = 143,
    NecromancerInvincibleEgg = 144,

    WarriorStormEffect = 150,

    ReviveEffect = 160,

	MAX
}


public enum FC_FIRE_WAY
{
	RANDOM,
	FROM_ONE_SIDE_TO_OTHER
	
}

public enum FC_EFFECT_EVENT_POS
{
	AT_BEGIN,
	AT_FIREBULLET,
	AT_ATTACK_START,
	AT_CHARGE_FULL,
	AT_HIT_TARGET,
	AT_KNOCK_BACK,
	AT_SPEC_TIME,
	AT_ANY_TIME,
    AT_HIT_CRIT,
}

public enum FC_PARRY_EFFECT
{
	NONE,
	SUCCESS,
	PARRY,
	FAIL,
	PARRY_BULLET_FAIL,
}


public enum FC_MULTIPLAY_STATUS
{
	INVALID = -1,
	LOADING,
	DISCONNECTED,
	READY

}

public enum FC_DANGER_LEVEL
{
	//means target is in safe distance, we need not to attack him
	SAFE,
	//means target is in danger distance, we need add attack correct to attack him
	DANGER,
	//we must hit the target to make self safe
	VERY_DANGER,
	//if out of range, monster need to near target
	OUT_OF_ATTACK_RANG,
}

public enum FC_SP_ACTION_CONDITONS
{
	HIT_TARGET_COUNT,
	HIT_TARGET_HP,
	AFTER_HURT,
	WHEN_HURT,
	AFTER_ATTACK,
	AFTER_HIT_TARGET,
	PER_XXX_SECONDS,
	AFTER_IDLE,
}

public enum FC_SP_ACTION_LISTS
{
	AROUNT_PLAYER,
	AWAY_FROM_PLAYER,
	RUN_RANDOM,
	COPY_PLAYER_ACTION
}


public enum FC_All_ANI_ENUM
{
	//state : 0~9
	//Idle state, initial state.
	none = -1,
	idle = 0,
	idle2 = 1,
	born_begin = 2,
	//state : 10~19
	//state Move, like running,walking, etc
	run = 10,
	runFast = 16,
	runCircleL1 = 11,
	runCircleR1 = 12,
	runCircleF1 = 13,
	runCircleB1 = 14,
	jumpLeave1 = 15,
	//state : 20~39
	//state Attack, basic instant attack.
	normalAttack1 = 1020, //(combo1_slahs1,bite1, shoot1)
	normalAttack2 = 1021, //(combo1_slahs2,pounce1, shoot1_loop)
	normalAttack3 = 1022, //(combo1_slahs3, shoot2)
	normalAttack4 = 1023, //(combo1_slahs4, shoot2_loop)
	fakeAttack0 = 1024, //(enemy)
	claw = 1025, //(wolf)
	smash_bellow = 1026, //(wolf)
	skeleon_boss_close_slash = 1027,
	slash_on_running = 1028,

	//state : 40~59
	//state Skill, special skill present.
	//warrior
	bash1 = 3040,
	block1_start = 3041,
	block1_end = 3042,
	block1_parry = 3043,
	block1_beAttack = 3044,
	dash1_start = 3045,
	dash1_end = 3046,
	dash1_end_attack = 3047,
	dash1_attack = 3048,
	bash_start = 3049,
	bash_attack = 3050,
	bash_end = 3051,
    hexagrare_start = 3052,
    hexagrare_end = 3053,
    hexagrare_attack = 3054,
    storm_attack = 3055,
    storm_attack_start = 3056,
    storm_attack_loop = 3057,
    storm_attack_end = 3058,
 
	//mage
	lighting_chain = 4040,
	iceShield_start = 4041,
	iceShield_end = 4042,
	iceShield_blink = 4043,
	iceShield_beAttack = 4044,
	lightning_ball = 4045,
	passive_skill = 4046,
	
	//monk
	dodge_start = 5040,
	dodge_end1 = 5041,
	dodge_end2 = 5042,
	sevenPunch_start = 5043,
	sevenPunch_end1 = 5044,
	sevenPunch_end2_attack = 5045,
	sevenPunch_end3_attack = 5046,
	kikoho_start = 5047,
	kikoho_end = 5048,
	vortex_draw = 5049,
	vortex_attack = 5050,
	shadow_show = 5051,
	shadow_show_end = 5052,
	shadow_bonus_attack = 5053,
	kick_after_run = 5054,

	//hurt and dead animations
	hurt = 8080,
	hurt_groups = 8081,
	dizzy = 8082,
	knockDown = 8083,
	absorb = 8084,
	knock_back = 8085,
	block1_break_iceShield_break = 8086,
	hurt_parryBack = 8087,
	stand = 8088,
	die = 8090,
	hurt_flyup_start = 8091,
	hurt_flyup_loop = 8092,
	hurt_flyup_end = 8093,
	hurt2 = 8094,
	revive = 8100,

    defy = 9030,
    power_up = 9032,
    armor_break = 9034,
    idle_weak = 9035,

    //necromancer
    necromancer_slash_left = 10020,
    necromancer_slash_right = 10021,
    necromancer_skeleton_wave = 10022,
    necromancer_summon_biont = 10023,
    necromancer_melee_attack = 10024,
    necromancer_ranged_attack = 10025,
    necromancer_group_released= 10026,
    necromancer_skeleton_wave2 = 10027,

    ////cyclop
    //combo1_slash = 9022,
    //smash1_dual = 9023,
    //smash2_fire = 9024,
    //dash1_kick = 9025,
    //dash1_slash = 9026,
    //combo2_smash = 9027,
    //rage = 9028,
    //charge = 9029,
    //charge_fire = 9031,
    //dash1_start_loop = 9033,
    //dash1_kick2=9036,
    ////wolfman
    //slash1 = 10020,
    //slash2 = 10021,
    //slash3 = 10022,
    //slash4 = 10023,
    //jump_front =10024,
    //jump_back = 10025,
    //level_up= 10026,
    //charge2 = 10027,
    //charge2_fire = 10028,
    //sskill_Tip = 10029,
    ////catwoman
    //cat_jump_left = 11024,
    //cat_jump_right= 11025,
    //cat_jump_back = 11026,
    //cat_charge = 11027,
    //cat_defy2 = 11028,
	
    ////kylin
    //kylin_charge_start = 12024,
    //kylin_charge_loop = 12025,
    //kylin_charge_fire_start = 12026,
    //kylin_charge_fire_loop = 12027,
    //kylin_charge_end = 12028,
	
    ////eragon
    //eragon_slash1_right = 13020,
    //eragon_slash2_left = 13021,
    //eragon_slash3_skyfall = 13022,
    //eragon_trace_start = 13023,
    //eragon_trace_loop = 13024,
    //eragon_trace_end = 13025,
    //eragon_charge_start = 13026,
    //eragon_charge_loop = 13027,
    //eragon_charge_smash= 13028,
    //eragon_flash_forward  = 13029,
    //eragon_flash_back = 13031,
    //eragon_defy_manual = 13033,
    //eragon_wait_defy = 13036,
    //eragon_rage = 13037,
	
    ////boarman
    //boar_smash = 14022,
    //boar_fire = 14023,
    //boar_dash_start = 14024,
    //boar_dash_end1_head = 14025,
    //boar_dash_end2_head_slash = 14026,
    //boar_dash_loop = 14027,	
	
    ////werebear
    //bear_slash_dual = 15022,
    //bear_fire = 15023,
    //bear_jump_start = 15024,
    //bear_jump_smash_1 = 15025,
    //bear_jump_smash_2 = 15026,
    //bear_jump_end_1 = 15027,	
    //bear_jump_end_2 = 15033,
    //bear_throw_start = 15036,
    //bear_throw_done = 15037,
    //bear_throw_end = 15038,
    //bear_smash = 15039,



}

public enum FCPotionType
{
	Health,
	Mana,
}

//public enum FC_PASSIVE_SKILL_TYPE
//{
//    HIT_POINT,
//    ARMOR,
//    DAMAGE,
//    CRITICLE_DAMAGE_PERCENTS,//criticle damage
//    CRITICLE_CHANCE,//criticle chance
//    MOVE_SPEED,
//    SKILL_GAIN_ENERGY,
//    SPECIAL,
//    NONE
//}

public enum FC_PHOTON_STATIC_SCENE_ID
{
	MATCH_PLAYER_MGR = 1,
	MULTIPLAYER_DATA_MGR,
	LEVEL_MGR,
}

public class FCConst
{
    public const int k_role_count = 2;

    public const string CONST_SAVE_VERSION = "1.1.5";

    public const int k_max_difficulty_level = 1;         //hard mode only, for v0.2.0

    public const int k_difficulty_level_count = k_max_difficulty_level + 1;

    public const int MAX_PLAYERS = 2; //how many players max
    public const int MAX_DAMAGE_TYPE = 5;
    public const int MAX_EQUIP = (int)FC_EQUIP_TYPE.MAX;
    public const string NONE_STR = "none";
    public const int NORMAL_SIGHT_LEVEL = 5;

	//commonly used item ids
	public const string k_potion_hp = "potion_hp";
	public const string k_potion_mp = "potion_mp";
	public const string k_potion_revive = "potion_revive";

	public const int k_max_vip_level = 10;

    #region no use layer
    public const int LAYER_DEFAULT = 0;
    public const int LAYER_NEUTRAL_1 = 9;
    public const int LAYER_NEUTRAL_WEAPON_DAMAGE_1 = LAYER_NEUTRAL_1 + 1;
    public const int LAYER_NEUTRAL_2 = LAYER_NEUTRAL_WEAPON_DAMAGE_1 + 1;
    public const int LAYER_NEUTRAL_WEAPON_DAMAGE_2 = LAYER_NEUTRAL_2 + 1;
    public const int LAYER_NEUTRAL_WEAPON_PHYSICAL_1 = LAYER_NEUTRAL_WEAPON_DAMAGE_2 + 1;
    public const int LAYER_NEUTRAL_WEAPON_PHYSICAL_2 = LAYER_NEUTRAL_WEAPON_PHYSICAL_1 + 1;
    public const int LAYER_NEUTRAL_BODY_1 = LAYER_NEUTRAL_WEAPON_PHYSICAL_2 + 1;
    public const int LAYER_NEUTRAL_BODY_2 = LAYER_NEUTRAL_BODY_1 + 1;
    public const int LAYER_ENEMY = LAYER_NEUTRAL_BODY_2 + 1;
    public const int LAYER_ENEMY_WEAPON = LAYER_ENEMY + 1;
    public const int LAYER_FRIENT = LAYER_ENEMY_WEAPON + 1;
    public const int LAYER_FRIENT_WEAPON = LAYER_FRIENT + 1;
    public const int LAYER_NONE = LAYER_FRIENT_WEAPON + 1;
    public const int LAYER_WALL = 26;           //physics layers: walls, air walls, ground
    public const int LAYER_WALL_AIR = 27;
    public const int LAYER_GROUND = 28;
    public const int LAYER_TRIGGER = 30;        //to detect collision with player only


    #endregion no use layer

	public const float FLASH_NO_TARGET_LENGTH = 3.0f;

    public const int FC_KEY_DIRECTION = 0;
    public const int FC_KEY_ATTACK_1 = 1;
    public const int FC_KEY_ATTACK_2 = 2;
    public const int FC_KEY_ATTACK_3 = 3;
    public const int FC_KEY_ATTACK_4 = 4;
    public const int FC_KEY_ATTACK_5 = 5;
    public const int FC_KEY_FOR_TOUCH = 6;
    public const int FC_KEY_ATTACK_6 = 6;
    public const int FC_KEY_ATTACK_7 = 7;
    public const int FC_KEY_ATTACK_8 = 8;
    public const int FC_KEY_ATTACK_9 = 9;
    public const int FC_KEY_ATTACK_10 = 10;
    public const int FC_KEY_MAX = 11;

    //if attack key is not hold,when player attack
    public const float SLOW_DAMAGE_SCALE = 1.2f;

    public enum DATA_SYNC_TYPE
    {
        POSITION,
        ROTATION,
        AI_STATE,
        NUM,
        //HITPOINT
    }

    public static float[] DATA_REFRESH_TIME = new float[(int)DATA_SYNC_TYPE.NUM - 1]
	{
		0.1f,//position
		0.1f,//rotation
		//0,//hit point
	};

    public static float[] DATA_SYNC_THRESHOLD = new float[(int)DATA_SYNC_TYPE.NUM - 1]
	{
		0.1f,//position
		0.1f,//rotation
	};

    public const int k_network_id_enemy_start = 0;
    public const int k_network_id_hero_start = 10000;

    public static string[] k_role_names = new string[] 
    {
        "Mage",
        "Warrior",
    };

    public static Dictionary<EnumRole,string> ROLE_NAME_KEYS = new Dictionary<EnumRole,string> 
    {
        {EnumRole.Warrior, "IDS_NAME_CLASS_WARRIOR"},
        {EnumRole.Mage, "IDS_NAME_CLASS_MAGE"}
    };

    public const string k_level_town = "town";

	public const string k_level_tutorial = "tutorial";

    //public static float DATA_SEND_TIME = 0.1f;

    public const float NETWORK_POS_RANGE = 0.5f; //in this range, we think that reach target pos	

    public const int GLOBAL_EFFECT_START = 100; // > 100 means that the effect activity is playing a global effect

    public const int EOT_REFRESH_TIME = 1;

    //for bullet
    //for normal ,will seek target -30 ~ 30
    public const int SEEK_ANGLE_NORMAL = 60;
    //for pro, will seek target -90(near) -30(far) ~ 30(far) - 90(far)
    public const int SEEK_ANGLE_PRO = 180;

    public const int HILL_WEIGHT = 99999;

    //how many global effects of one type
    public const int GLOBAL_EFFECT_COUNT = 5;

    //for ani
    public const int MAX_BLEND_FRAMECOUNT = 999;
    //means the ani we want to play is playing now
    public const int MAX_SAFE_FRAMECOUNT = 997;
    public const float ATTACK_COMBO_LAST_TIME = 0.3f;

    public const float TIME_FIXEDSTEP = 0.02f;

    //for mosnter
    public const int ELITE_DANGER_LEVEL = 10;
    public const int BOSS_DANGER_LEVEL = 20;
    public const int UNVIABLE_ATTACK_INDEX = -1;
    public const int MAX_ATTACK_INDEX = 256;
    public const int MAX_WEIGHT_FOR_ACTOR = 550;
    public const int MIN_WEIGHT_FOR_ACTOR = 1;
    public const int MID_WEIGHT_FOR_ACTOR = 50;


    //SuperArmor
    //can ignore normal hit
    public const int SUPER_ARMOR_LVL0 = 0;
    // can absorb damage and all type hit
    public const int SUPER_ARMOR_LVL1 = 1;
    public const int SUPER_ARMOR_LVL2 = 2;
    public const int SUPER_ARMOR_LVL_MAX = 3;


    //evolution consts
    public const int k_max_evolution_level = 4;     //valid indice = 0, 1, 2, 3

    public const int k_evolution_material_count = 3;

    public const float k_juhua_speed = 0.1f;

    public const float k_reconnect_timeout = 16.0f; //time for local reconnect


    public const float BLOCKING_TIMEOUT_1 = 0.001f;

    public const int WEIGHT_FOR_G_EFFECT = 100;
    public const int WEIGHT_MAX_FOR_G_EFFECT = 1000;

    //for poition
    public const float POTION_TIME = 6;

    //for weapon sound
    public const string WEAPON_HIT_SOUND_SLASH = "weapon_slash";
    public const string WEAPON_HIT_SOUND_BLUNT = "weapon_blunt";
    public const string WEAPON_HIT_SOUND_CLAW = "weapon_claw";

    public const int WALKMASK_ALL = -1;
    public const int WALKMASK_NORMAL = 1;

    public const string k_soft_currency = "money";
    public const string k_hard_currency = "hard_currency";
    public const string k_valor_point = "valor_point";

    public const float REAL_G = -15f;

    public static string[] k_difficulty_level_names = 
    {
        "IDS_DIFFICULTY_LEVEL_NORMAL",
        "IDS_DIFFICULTY_LEVEL_HARD",
        "IDS_DIFFICULTY_LEVEL_LEGENDARY",
    };

    public const string k_last_level_id = "Gothic_boss";        //id of last level
    public const int k_required_quest_id_normal = 2024;         //quest id of last level under normal difficulty level
    public const int k_required_quest_id_hard = 2024;           //quest id of last level under hard difficulty level

    public const int DIFFICULTY_LEVEL_NORMAL = 0;
    public const int DIFFICULTY_LEVEL_HARD = 1;
    public const int DIFFICULTY_LEVEL_LEGENDARY = 2;

    public static string[] k_token_names = new string[]
    {
        "token_wood", 
        "token_iron", 
        "token_gold",
		"token_springfestival"
    };

    public enum NET_POSITION_SYNC_LEVEL
    {
        LEVEL_0, // == 0m, means sync all position event
        LEVEL_1, // == 1m, will ignore distance < 1 position event
        LEVEL_2,
        LEVEL_3,
        LEVEL_MAX = 10, // == 10m
    }
    public static int MAX_CURRENCY_VALUE_FOR_DISPLAY = 99999999;

    #region PHOTON
    public static string PHOTON_CONNECT_VERSION_STAGE = "v_1.0.0_stage";
    //public static string PHOTON_CONNECT_VERSION_STAGE = "v_1.0.0_live_" + RuntimePlatform.IPhonePlayer;
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
	public static string PHOTON_CONNECT_VERSION_LIVE = "v_1.0.0_live_" + Application.platform;
#else
    public static string PHOTON_CONNECT_VERSION_LIVE = "v_1.0.0_live";
#endif

    public const int PHOTON_TIME_OUT = 30;  // seconds
    #endregion

	public static string[] LevelLoadingTips = new string[]
	{
		"IDS_TIPS_LOADING_GENERAL_01",
		"IDS_TIPS_LOADING_GENERAL_02",
		"IDS_TIPS_LOADING_GENERAL_03",
		"IDS_TIPS_LOADING_GENERAL_04",
		"IDS_TIPS_LOADING_GENERAL_05",
		"IDS_TIPS_LOADING_GENERAL_06",
		"IDS_TIPS_LOADING_GENERAL_07",
		"IDS_TIPS_LOADING_GENERAL_08",
		"IDS_TIPS_LOADING_GENERAL_09",
		"IDS_TIPS_LOADING_GENERAL_10",
		"IDS_TIPS_LOADING_GENERAL_11",
		"IDS_TIPS_LOADING_GENERAL_12",
	};


    //public static Dictionary<PlayerPropKey, string> PlayerPropKeyMapping = new Dictionary<PlayerPropKey, string>()
    //{
    //    {PlayerPropKey.Attack, "IDS_NAME_GLOBAL_ATTACK"},
    //    {PlayerPropKey.Defense, "IDS_NAME_GLOBAL_DEFENSE"},
    //    {PlayerPropKey.Critical, "IDS_NAME_GLOBAL_CRITICAL"},
    //    {PlayerPropKey.CritDamage, "IDS_NAME_GLOBAL_CRITICALDAMAGE"},
    //    {PlayerPropKey.Damage, "IDS_NAME_GLOBAL_DAMAGE"},
    //    {PlayerPropKey.Resistance, "IDS_NAME_GLOBAL_RESIST"},
    //    {PlayerPropKey.FireDmg, "IDS_NAME_GLOBAL_FIREDAMAGE"},
    //    {PlayerPropKey.FireRes, "IDS_NAME_GLOBAL_FIRERESIST"},
    //    {PlayerPropKey.IceDmg, "IDS_NAME_GLOBAL_ICEDAMAGE"},
    //    {PlayerPropKey.IceRes, "IDS_NAME_GLOBAL_ICERESIST"},
    //    {PlayerPropKey.LightningDmg, "IDS_NAME_GLOBAL_LIGHTNINGDAMAGE"},
    //    {PlayerPropKey.LightningRes, "IDS_NAME_GLOBAL_LIGHTNINGRESIST"},
    //    {PlayerPropKey.PosisonDmg, "IDS_NAME_GLOBAL_POISONDAMAGE"},
    //    {PlayerPropKey.PosisonRes, "IDS_NAME_GLOBAL_POISONRESIST"},
    //    {PlayerPropKey.HP, "IDS_NAME_GLOBAL_HP"}
    //};

	public static Dictionary<AIHitParams, string> EquipmentAttributeMapping = new Dictionary<AIHitParams, string>()
    {
        {AIHitParams.Hp, "IDS_NAME_GLOBAL_HP"},
        {AIHitParams.Mp, "IDS_NAME_GLOBAL_ENERGY"},
        {AIHitParams.Attack, "IDS_NAME_GLOBAL_ATTACK"},
        {AIHitParams.Defence, "IDS_NAME_GLOBAL_DEFENSE"},
        {AIHitParams.CriticalChance, "IDS_NAME_GLOBAL_CRITICAL"},
        {AIHitParams.CriticalDamage, "IDS_NAME_GLOBAL_CRITICALDAMAGE"},
        {AIHitParams.FireDamage, "IDS_NAME_GLOBAL_FIREDAMAGE"},
        {AIHitParams.FireResist, "IDS_NAME_GLOBAL_FIRERESIST"},
        {AIHitParams.IceDamage, "IDS_NAME_GLOBAL_ICEDAMAGE"},
        {AIHitParams.IceResist, "IDS_NAME_GLOBAL_ICERESIST"},
        {AIHitParams.LightningDamage, "IDS_NAME_GLOBAL_LIGHTNINGDAMAGE"},
        {AIHitParams.LightningResist, "IDS_NAME_GLOBAL_LIGHTNINGRESIST"},
        {AIHitParams.RunSpeed, "IDS_NAME_GLOBAL_RUNSPEED"},
        {AIHitParams.ScAdditon, "IDS_NAME_GLOBAL_SCADD"},
        {AIHitParams.ItemFind, "IDS_NAME_GLOBAL_ITEMFIND"},
        {AIHitParams.ReduceEnergy, "IDS_NAME_GLOBAL_REDUCEENERGY"},
        {AIHitParams.ExpAddition, "IDS_NAME_GLOBAL_EXPADD"},
        {AIHitParams.Damage, "IDS_NAME_GLOBAL_DAMAGE"},
        {AIHitParams.Resist, "IDS_NAME_GLOBAL_RESIST"},
    };

	//designed by artist
	//by color name
	public static readonly Color k_color_white = new Color(246f / 255f, 246f / 255f, 246f / 255f);
	public static readonly Color k_color_green = new Color(40f / 255f, 220f / 255f, 0);
	public static readonly Color k_color_blue = new Color(56f / 255f, 164f / 255f, 1);
	public static readonly Color k_color_purple = new Color(146f / 255f, 49f / 255f, 1);
	public static readonly Color k_color_gold = new Color(1, 190f / 255, 0);
	
	public static readonly Color k_color_red = new Color(1, 0, 0);

	//by usage
	public static readonly Color k_color_normal_text = new Color(190f / 255f, 170f / 255f, 130f / 255f);

	public static Dictionary<EnumRareLevel, Color> RareColorMapping = new Dictionary<EnumRareLevel, Color>()
    {
        {EnumRareLevel.white, k_color_white},
        {EnumRareLevel.green, k_color_green},
        {EnumRareLevel.blue, k_color_blue},
        {EnumRareLevel.purple, k_color_purple},
        {EnumRareLevel.gold, k_color_gold}
    };

    public static int PropFactor(PlayerPropKey propKey)
    {
        if ((int)propKey >= 32)
            return 1000;
        else
            return 1;
    }
}


#region network
/// <summary>
///     list the update key of changed from server.
/// </summary>
public enum UpdateKey
{
    RoleProp        = 1,
    RolePropBattle  = 2,
    ItemMove        = 3,
    ItemCount       = 4,
    ItemNew         = 5,
    ItemPropUpdate  = 6,
    //quest           = 7,
    Tattoo          = 8
}
#endregion

[System.Serializable]
public class EquipmentIdx
{
    public string _id;
    public int _evolutionLevel = 0;
    public ItemSubType Part {
        get { return DataManager.Instance.GetItemData(_id).subType; } 
    }
};

public enum WorldmapRegion
{ 
    Faluo = 1,
    LvSen,
    XiuLan
}

public enum PlayerPropKey
{
    //section 1 => for base.
    Vip             = 0,
    Level,
    Exp,
    SoftCurrency,
    HardCurrency,
    InventoryCount,
    Vitality,
    VitalityMax,
    VitalityTime,
    VitalityBuyCount,
    VitalityBuyTime = 10,

    //section 2 => for battle.
    HP              = 32,
    MP,
    Attack,
    Defense,
    Critical,
    CriticalRes,
    CritDamage,
    CritDamageRes,
    Damage          = 40,
    Resistance,
    FireDmg,
    FireRes,
    LightningDmg,
    LightningRes,
    IceDmg,
    IceRes,
    PosisonDmg,
    PosisonRes,
    RunSpeed        = 50,
    ExpAdd,
    SCAdd,
    ItemFind,
    ReduceEnergy
}

public class PrefsKey
{
    public const string Account = "Account";
    public const string Password = "Password";
    public const string ServerID = "ServerID";
    public const string ServerCacheOne = "ServerCacheOne";
    public const string ServerCacheTwo = "ServerCacheTwo";

	//Quest
	public const string ViewedQuests = "ViewedQuests";	//remember all new quests. "2001;2002;1221;"

	//tattoo slots count available
	public const string PreviousPlayerLevel = "PreviousPlayerLevel";
}

public enum EnumGameState
{
	None = -1,
	Launch = 0,
	CharacterSelect,
	InTown,
	InBattle,
	InBattleCinematic,
	BattleQuit,
};

public enum ItemQuality
{
    white = 0,
    green = 1,
    blue = 2,
    purple = 3,
    golden = 4
}

public enum ItemMoveType
{
    EquipOn     = 1,
    EquipOff    = 2
}

public enum InventoryMenuItemOperationType
{
    Equip       = 1,
    Sell        = 2,
    Fusion      = 100,
    Information = 101
}

public enum StaticObjectType
{
	None = -1,
	Pottery,
	Barrel,
	Crate,
	Chest,
	GoldChest,
}

public enum FCCommStatus
{
	Idle,
	Busy,
	ResultOK,          //response arrived successfully
	ResultError        //response arrived with error
}

public enum EnumTutorialState
{
	Inactive = -1,
	Active = 0,
	Finished,
	Max
}

public enum EnumTutorial
{
	None = -1,
	//====In Battle====
	Battle_Move,	//learn how to move with joystick
	Battle_Attack,
	Battle_KillAnEnemy,
	Battle_Skill1,
	Battle_Skill2,
	Battle_Skill3,
	Battle_Skill4,
	Battle_HealthPotion,
	Battle_EnergyPotion,
	Battle_Revive,
	//====In Town =====
	Town_Equip,
	Town_Buy,
	Town_Map,
	Town_Quest1,
	Town_Quest2,
	Town_Skill,
	Town_Fusion,
	Town_Offering,
}

/*
	public const string _level_move_10 = "move";
	public const string _level_destroy_box_20 = "attack";
	public const string _level_kill_enemy_30 = "kill";

	public const string _level_use_skill1_40 = "skill1";
	public const string _level_use_skill2_50 = "skill2";
	public const string _level_use_skill3_60 = "skill3";
	public const string _level_use_skill4_70 = "skill4";

	public const string _level_hp_80 = "hp";
	public const string _level_energy_90 = "energy";
	public const string _level_revive_100 = "revive";

	public const string _level_hp_80_count = "hp_count";
	public const string _level_energy_90_count = "energy_count";
	public const string _level_revive_100_count = "revive_count";

	///town
	public static string _town_equip_10 = "equip_10";
	public static string _town_buy_20 = "buy_20";
	public static string _town_map_30 = "map_30";
    public static string _town_quest_40 = "quest_40";
    public static string _town_quest_41 = "quest_41";
    public static string _town_skill_50 = "skill_50"; 
	public static string _town_fusion_60 = "fusion_60"; 
	public static string _town_evolution_70 = "evolution_70"; 	
	public static string _town_gacha_80 = "gacha_80"; 
	
	public static string _fusionItem_3 = "all_belt_1_white_defense_tutorial";
	public static string _fusionItem_2 = "all_shoulder_1_white_defense_tutorial";
	public static string _fusionItem_1 = "all_helm_1_white_defense_tutorial";
	
	public static string _equipItem = "weapon_1_green_tutorial";
 
*/