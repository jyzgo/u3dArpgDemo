using UnityEngine;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;


using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public enum SaveValueType
{
	Invalid,
	Object,
	Array,
	String,
	Number,
	Bool
}

public abstract class SaveValue
{	
	
	public static implicit operator SaveValue(string val)
	{
		return new SaveString(val);
	}

	public static explicit operator string(SaveValue val)
	{
		SaveString str = val as SaveString;
		if (str == null)
			return "";
		
		return str.Value;
	}
	
	
	public static implicit operator SaveValue(float val)
	{
		return new SaveNumber(val);
	}

	public static explicit operator float(SaveValue val)
	{
		SaveNumber snum = val as SaveNumber;
		if (snum == null)
			return default(float);
		
		return (float)snum;
	}

	public static implicit operator SaveValue(int val)
	{
		return new SaveNumber(val);
	}
	
	public static explicit operator int(SaveValue val)
	{
		SaveNumber snum = val as SaveNumber;
		if (snum == null)
			return default(int);
		
		return (int)snum;
	}		
	
	public static implicit operator SaveValue(long val)
	{
		return new SaveNumber(val);
	}
	
	public static explicit operator long(SaveValue val)
	{
		SaveNumber snum = val as SaveNumber;
		if (snum == null)
			return default(long);
		
		return (long)snum;
	}		


	public static implicit operator SaveValue(bool val)
	{
		return new SaveBool(val);
	}
	
	public static explicit operator bool(SaveValue val)
	{
		SaveBool sbool = val as SaveBool;
		if (sbool == null)
			return default(bool);
	
		return (bool)sbool;
	}

	
	public static SaveValue Instantiate(string typeName)
	{
		switch (typeName)
		{
			case "object":
				return new SaveObject();
			
			case "array":
				return new SaveArray();

			case "string":
				return new SaveString();

			case "number":
				return new SaveNumber();

			case "bool":
				return new SaveBool();
		}
		
		return null;
	}

	
	public static SaveValue Instantiate(int type)
	{
		SaveValueType eType = (SaveValueType)type;
		switch (eType)
		{
			case SaveValueType.Object:
				return new SaveObject();
			
			case SaveValueType.Array:
				return new SaveArray();

			case SaveValueType.String:
				return new SaveString();

			case SaveValueType.Number:
				return new SaveNumber();

			case SaveValueType.Bool:
				return new SaveBool();
		}
		
		return null;
	}	
}

public class SaveBool : SaveValue, IXmlSerializable

{
	public bool Value { get; set; }
	
	public SaveBool()
	{
	}
	
	public SaveBool(bool val)
	{
		Value = val;	
	}

	public static implicit operator SaveBool(bool val)
	{
		return new SaveBool(val);
	}	
	
	public static implicit operator bool(SaveBool val)
	{
		return val.Value;
	}	
	
	public XmlSchema GetSchema ()
	{
		return null;
	}
	

	public void ReadXml (XmlReader reader)
	{
		Value = reader.ReadElementContentAsBoolean();
	}

	public void WriteXml (XmlWriter writer)
	{
		writer.WriteAttributeString("type", "bool");
		writer.WriteValue(Value);
	}

}

public class SaveNumber : SaveValue, 
						IXmlSerializable

{
	public double Value { get; set; }
	
	public SaveNumber()
	{
	}
	
	
	public SaveNumber(double val)
	{
		Value = val;
	}

	public static implicit operator SaveNumber(double val)
	{
		return new SaveNumber(val);
	}		
	
	public static explicit operator double(SaveNumber val)
	{
		return val.Value;
	}	
	
	
	public SaveNumber(int val)
	{
		Value = (double)val;
	}
	
	public static implicit operator SaveNumber(int val)
	{
		return new SaveNumber(val);
	}		
	
	public static explicit operator int(SaveNumber val)
	{
		return (int)val.Value;
	}	
	
	
	
	public SaveNumber(float val)
	{
		Value = (double)val;
	}	
	
	public static implicit operator SaveNumber(float val)
	{
		return new SaveNumber(val);
	}		

	public static explicit operator float(SaveNumber val)
	{
		return (float)val.Value;
	}			
	
	
	
	public SaveNumber(long val)
	{
		Value = (double)val;
	}	
	
	public static implicit operator SaveNumber(long val)
	{
		return new SaveNumber(val);
	}		

	public static explicit operator long(SaveNumber val)
	{
		return (long)val.Value;
	}	
	
	public XmlSchema GetSchema ()
	{
		return null;
	}

	public void ReadXml (XmlReader reader)
	{
		Value = reader.ReadElementContentAsDouble();
	}

	public void WriteXml (XmlWriter writer)
	{
		writer.WriteAttributeString("type", "number");
		writer.WriteValue(Value);
	}

}

public class SaveString : SaveValue
						,IXmlSerializable

{
	public string Value { get; set; }

	public SaveString()
	{
		Value = "";
	}

	public SaveString(string val)
	{
		Value = val;
	}

	public static implicit operator SaveString(string val)
	{
		return new SaveString(val);
	}	
	
	public static implicit operator string(SaveString val)
	{
		return val.Value;
	}	
	
	public XmlSchema GetSchema ()
	{
		return null;
	}

	public void ReadXml (XmlReader reader)
	{
		Value = reader.ReadElementContentAsString();
	}
	public void WriteXml (XmlWriter writer)
	{
		writer.WriteAttributeString("type", "string");
		writer.WriteString(Value);
	}

}

public class SaveObject : SaveValue, IEnumerable<KeyValuePair<string, SaveValue>>
						,IXmlSerializable

{
	private Dictionary<string, SaveValue> _items;
	
	public SaveObject()
	{
		_items = new Dictionary<string, SaveValue>();
	}
	
	public SaveValue this[string key]
	{ 
		get {
			SaveValue val;
			if (_items.TryGetValue(key, out val)) {
				return val;
			}
			
			return null;
		} 
		
		set {
			_items[key] = value;
		} 
	}
	
	public bool Has(string key)
	{
		return _items.ContainsKey(key);
	}
	
	public IEnumerator<KeyValuePair<string, SaveValue>> GetEnumerator()
	{
		return _items.GetEnumerator();
	}
	
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}	
	

	public XmlSchema GetSchema ()
	{
		return null;
	}

	public void ReadXml (XmlReader reader)
	{
		if (reader.IsEmptyElement)
		{
			reader.Read();
			return;
		}
	
		reader.ReadStartElement();
						
		while (reader.NodeType != XmlNodeType.EndElement) {
			string key = reader.LocalName;
			string type = reader.GetAttribute("type");

			SaveValue saveValue = SaveValue.Instantiate(type);
			IXmlSerializable xmlSerializable = saveValue as IXmlSerializable;
			if (xmlSerializable != null) {
				xmlSerializable.ReadXml(reader);
				_items[key] = saveValue;
			}
			
			if (reader.EOF)
				throw new IOException("Malformed XML file");
		}
		
		reader.ReadEndElement();
	}

	public void WriteXml (XmlWriter writer)
	{
		writer.WriteAttributeString("type", "object");
		
		foreach(KeyValuePair<string, SaveValue> pair in _items) {
			IXmlSerializable xmlSerializable = pair.Value as IXmlSerializable;
			if (xmlSerializable != null) {
				writer.WriteStartElement(pair.Key);
				xmlSerializable.WriteXml(writer);
				writer.WriteEndElement();
			}
		}
	}
	
	
	
}

public class SaveArray : SaveValue, IEnumerable<SaveValue>
						,IXmlSerializable

{
	private List<SaveValue> _items;
	
	public SaveArray()
	{
		_items = new List<SaveValue>();
	}
	
	public void Add(SaveValue val)
	{
		_items.Add(val);
	}
	
	public SaveValue this[int index] {
		get {
			return _items[index];
		} 
	}
	
	public int Count {
		get {
			return _items.Count;
		}
	}
	
	public IEnumerator<SaveValue> GetEnumerator()
	{
		return _items.GetEnumerator();
	}
	
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public XmlSchema GetSchema ()
	{
		return null;
	}

	public void ReadXml (XmlReader reader)
	{
		if (reader.IsEmptyElement)
		{
			reader.Read();
			return;
		}
		
		reader.ReadStartElement();
		
		while (reader.NodeType != XmlNodeType.EndElement) {
			string type = reader.GetAttribute("type");

			SaveValue saveValue = SaveValue.Instantiate(type);
			IXmlSerializable xmlSerializable = saveValue as IXmlSerializable;
			if (xmlSerializable != null) {
				xmlSerializable.ReadXml(reader);
				_items.Add(saveValue);
			}
		}
		
		reader.ReadEndElement();
	}

	public void WriteXml (XmlWriter writer)
	{
		writer.WriteAttributeString("type", "array");
		
		int curIndex = 0;
		
		foreach(SaveValue item in _items) {
			IXmlSerializable xmlSerializable = item as IXmlSerializable;
			if (xmlSerializable != null) {
				writer.WriteStartElement("Item");
				writer.WriteAttributeString("index", curIndex.ToString());
				xmlSerializable.WriteXml(writer);
				writer.WriteEndElement();
				curIndex++;
			}
		}		
	}


}
