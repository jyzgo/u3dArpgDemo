using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InJoy.RuntimeDataProtection
{
    [System.Serializable]
    public class EncryptValue : System.ICloneable
    {
        private byte key;
        private byte[] data;

        private static System.Random random;
        public EncryptValue()
        { }
        public EncryptValue(EncryptValue other)
        {
            SetData(other.GetData());
        }

        protected void SetData(byte[] srcData)
        {
            key = GetNewKey();
            data = EncryptData(srcData);
        }

        public EncryptValue Clone()
        {
            return new EncryptValue(this);
        }

        object System.ICloneable.Clone()
        {
            return this.Clone();
        }

        protected byte[] GetData()
        {
            return EncryptData(data);
        }

        protected byte GetNewKey()
        {
            if (random == null)
            {
                random = new System.Random();
            }

            // System.Random generates nonnegative numbers.
            byte Byte = (byte)random.Next(256);
            return Byte;
        }

        protected byte[] EncryptData(byte[] SrcBytes)
        {
            byte[] DestByte = new byte[SrcBytes.Length];
            for (int i = 0; i < SrcBytes.Length; ++i)
                DestByte[i] = (byte)(SrcBytes[i] ^ key);
            return DestByte;
        }
    }

    public class EncryptInt : EncryptValue
    {
        public EncryptInt()
        { }

        public EncryptInt(int value)
        {
            Set(value);
        }

        public void Set(int value)
        {
            SetData(BitConverter.GetBytes(value));
        }

        public int Get()
        {
            return BitConverter.ToInt32(GetData(), 0);
        }

        // Implicit conversions
        public static implicit operator EncryptInt(int value)
        {
            return new EncryptInt(value);
        }

        public static implicit operator int(EncryptInt encryptedValue)
        {
            return encryptedValue.Get();
        }
    }

    public class EncryptFloat : EncryptValue
    {
        public EncryptFloat()
        { }

        public EncryptFloat(float value)
        {
            Set(value);
        }

        public void Set(float value)
        {
            SetData(BitConverter.GetBytes(value));
        }

        public float Get()
        {
            return BitConverter.ToSingle(GetData(), 0);
        }

        // Implicit conversions
        public static implicit operator EncryptFloat(float value)
        {
            return new EncryptFloat(value);
        }

        public static implicit operator float(EncryptFloat encryptedValue)
        {
            return encryptedValue.Get();
        }
    }
}
