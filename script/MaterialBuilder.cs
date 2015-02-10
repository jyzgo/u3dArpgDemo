using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MaterialInst
{
    private List<int> _currentAnimations = new List<int>();

    public void SetState(int state, float time)
    {
        _animations[state].Start(GetMaterials(), _weaponMaterials, true, false, time);
        _currentAnimations.Insert(0, state);
    }

    public void SetState(int state, bool enable)
    {
        _animations[state].Start(GetMaterials(), _weaponMaterials, false, enable, 0.0f);

        if (enable)
        {
            _currentAnimations.Insert(0, state);
        }       
    }



    public void SetShadowMatrix(Matrix4x4 parameter, Vector4 shadowRange)
    {
        Material[] mats = GetMaterials();
        foreach (Material m in mats)
        {
            m.SetVector("_shadowMatrix0", parameter.GetRow(0));
            m.SetVector("_shadowMatrix1", parameter.GetRow(1));
            m.SetVector("_shadowMatrix2", parameter.GetRow(2));
            m.SetVector("_shadowMatrix3", parameter.GetRow(3));
            m.SetVector("_pntShadow", shadowRange);
        }
        foreach (Material m in _weaponMaterials)
        {
            m.SetVector("_shadowMatrix0", parameter.GetRow(0));
            m.SetVector("_shadowMatrix1", parameter.GetRow(1));
            m.SetVector("_shadowMatrix2", parameter.GetRow(2));
            m.SetVector("_shadowMatrix3", parameter.GetRow(3));
            m.SetVector("_pntShadow", shadowRange);
        }
    }



    virtual protected Material[] GetMaterials() 
    {
        return new Material[0]; 
    }


    protected Material[] _deadMaterials;

    virtual public void ClearWeapon()
    {
        _weaponMaterials = new Material[0];
    }

    virtual public void SetDeadMaterial(Material deadMat)
    {
        Assertion.Check(deadMat != null);

        Material[] mats = GetMaterials();

        foreach (Material m in mats)
        {
            Texture tex = m.mainTexture;
            m.shader = deadMat.shader;
            m.CopyPropertiesFromMaterial(deadMat);
            m.mainTexture = tex;
        }
    }

    virtual public void Destroy()
    {
        //foreach(Material mat in _weaponMaterials) {
        //	GameObject.Destroy(mat);
        //}
    }

    public void Update()
    {
        for (int i = _currentAnimations.Count - 1; i >= 0; i--)
        {
            int state = _currentAnimations[i];
            int resultState = _animations[state].UpdateState(GetMaterials(), _weaponMaterials);
            if (resultState == 0)
            {
                _currentAnimations.Remove(state);
            }
        }
        //_currentAnimation = _animations[_currentAnimation].UpdateState(GetMaterials(), _weaponMaterials);
    }

    MaterialAnimation[] _animations = new MaterialAnimation[]
    {
		new MaterialAnimation(),                                    //flag is 0
		new ColorAnimation(),                                       // flag is 1
		new IceAnimation(),                                         // flag is 2
		new SuperManAnimation(),                                    // flag is 3
		new SinAnimation(new Color(1.0f, 1.0f, 1.0f, 1.0f)),        // flag is 4
		new SinAnimation(new Color(1.0f, 0.0f, 0.0f, 1.0f)),        // flag is 5
		new SinAnimation(new Color(0.839f, 0.8f, 0.156f, 1.0f)),    // flag is 6
        new SuperManAnimation(),                                    // flag is 7
        //new SinAnimation(new Color(0.839f, 0.8f, 0.156f, 1.0f)),  // flag is 7
	};

    // weapon materials
    Material[] _weaponMaterials = new Material[0];
    public void GetWeaponMaterial(Renderer[] renderers)
    {
        Material[] mats = new Material[_weaponMaterials.Length + renderers.Length];
        for (int i = 0; i < _weaponMaterials.Length; ++i)
        {
            mats[i] = _weaponMaterials[i];
        }
        for (int i = 0; i < renderers.Length; ++i)
        {
            mats[_weaponMaterials.Length + i] = Utils.CloneMaterial(renderers[i].sharedMaterial);
            renderers[i].sharedMaterial = mats[_weaponMaterials.Length + i];
        }
        _weaponMaterials = mats;
    }

    // this animation does nothing
    class MaterialAnimation
    {
        public bool needTimer = false;
        public bool needEnable = false;
        public float duration = 0.0f;

        virtual public int UpdateState(Material[] materials, Material[] materials2) { return 0; }
        virtual public void Start(Material[] materials, Material[] materials2, bool isTimer, bool isEnable, float time) { }
        virtual public void End(Material[] materials, Material[] materials2) { }
        
    }
    // change specular color.
    class ColorAnimation : MaterialAnimation
    {
        public override int UpdateState(Material[] materials, Material[] materials2)
        {
            if (needTimer)
            {
                duration -= Time.deltaTime;

                if (duration <= 0.0f)
                {
                    End(materials, materials2);
                    return 0;
                }
            }
            else
            {
                if (!needEnable)
                {
                    End(materials, materials2);
                    return 0;
                }
            }

            return 1;
        }

        public override void Start(Material[] materials, Material[] materials2, bool isTimer, bool isEnable, float time)
        {
            needTimer = isTimer;
            needEnable = isEnable;
            duration = time;

            foreach (Material m in materials)
            {
                m.SetColor("_RimLightColor", Color.gray);
            }
        }

        public override void End(Material[] materials, Material[] materials2)
        {
            foreach (Material m in materials)
            {
                m.SetColor("_RimLightColor", Color.black);
            }
        }
    }


    // change specular color.
    class IceAnimation : MaterialAnimation
    {
        public override int UpdateState(Material[] materials, Material[] materials2)
        {
            foreach (Material m in materials)
            {
                m.SetVector("_RimLightColor2", new Color(0, 0.203f, 1, 0));
            }

            foreach (Material m in materials2)
            {
                m.SetVector("_RimLightColor2", new Color(0, 0.203f, 1, 0));
            }

            if (needTimer)
            {
                duration -= Time.deltaTime;

                if (duration <= 0.0f)
                {
                    End(materials, materials2);
                    return 0;
                }
            }
            else
            {
                if (!needEnable)
                {
                    End(materials, materials2);
                    return 0;
                }
            }

            return 2;
        }


        public override void Start(Material[] materials, Material[] materials2, bool isTimer, bool isEnable, float time)
        {
            needTimer = isTimer;
            needEnable = isEnable;
            duration = time;
        }


        public override void End(Material[] materials, Material[] materials2)
        {
            foreach (Material m in materials)
            {
                m.SetVector("_RimLightColor2", Color.black);
            }

            foreach (Material m in materials2)
            {
                m.SetVector("_RimLightColor2", Color.black);
            }
        }
    }


    // change specular color.
    class SuperManAnimation : MaterialAnimation
    {
        public override int UpdateState(Material[] materials, Material[] materials2)
        {
            foreach (Material m in materials)
            {
                m.SetVector("_RimLightColor2", new Color(0.839f * 0.5f, 0.8f * 0.5f, 0.156f * 0.5f, 1.0f * 0.5f));
            }

            foreach (Material m in materials2)
            {
                m.SetVector("_RimLightColor2", new Color(0.839f * 0.5f, 0.8f * 0.5f, 0.156f * 0.5f, 1.0f * 0.5f));
            }

            if (needTimer)
            {
                duration -= Time.deltaTime;

                if (duration <= 0.0f)
                {
                    End(materials, materials2);
                    return 0;
                }
            }
            else
            {
                if (!needEnable)
                {
                    End(materials, materials2);
                    return 0;
                }
            }
            return 3;
        }


        public override void Start(Material[] materials, Material[] materials2, bool isTimer, bool isEnable, float time)
        {
            needTimer = isTimer;
            needEnable = isEnable;
            duration = time;
        }


        public override void End(Material[] materials, Material[] materials2)
        {
            foreach (Material m in materials)
            {
                m.SetVector("_RimLightColor2", Color.black);
            }

            foreach (Material m in materials2)
            {
                m.SetVector("_RimLightColor2", Color.black);
            }
        }


    }

    class SinAnimation : MaterialAnimation
    {
        public override int UpdateState(Material[] materials, Material[] materials2)
        {
            float sin = Mathf.Sin(Time.timeSinceLevelLoad) * 0.5f + 0.6f;
            Color c = new Color(_baseColor.r * sin, _baseColor.g * sin, _baseColor.b * sin, _baseColor.a * sin);
            foreach (Material m in materials)
            {
                m.SetVector("_RimLightColor2", c);
            }

            foreach (Material m in materials2)
            {
                m.SetVector("_RimLightColor2", c);
            }

            if (needTimer)
            {
                duration -= Time.deltaTime;

                if (duration <= 0.0f)
                {
                    End(materials, materials2);
                    return 0;
                }
            }
            else
            {
                if (!needEnable)
                {
                    End(materials, materials2);
                    return 0;
                }
            }

            return 4;
        }


        public override void Start(Material[] materials, Material[] materials2, bool isTimer, bool isEnable, float time)
        {
            needTimer = isTimer;
            needEnable = isEnable;
            duration = time;
        }


        public override void End(Material[] materials, Material[] materials2)
        {
            foreach (Material m in materials)
            {
                m.SetVector("_RimLightColor2", Color.black);
            }

            foreach (Material m in materials2)
            {
                m.SetVector("_RimLightColor2", Color.black);
            }
        }

        Color _baseColor;

        public SinAnimation(Color baseColor)
        {
            _baseColor = baseColor;
        }
    }

}

// Class MaterialBuilder is used for creating and maintaining material and update texture for characters.
public class MaterialBuilder : MonoBehaviour
{

    virtual public MaterialInst CreateMaterialForBody(Renderer[] renderers, Material origin, CharacterGraphicsQuality quality) { return null; }
    virtual public void UpdateMaterialOfEquipment(MaterialInst inst, Renderer[] equipments, string[] avatarPartNames, Texture2D source, Texture2D normal, Color blendColor, CharacterGraphicsQuality quality) { }
}