using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.InteropServices;

public class StorageManager
{
	/// <summary>
	/// Gets the version.
	/// </summary>
	/// <value>
	/// The version.
	/// </value>
	public static Version Version {
		get { return new Version (1, 1, 1); }
	}
	
	/// <summary>
	/// StorageManager-specific exception class.
	/// </summary>
	public class StorageManagerException : System.Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StorageManager.StorageManagerException"/> class.
		/// </summary>
		public StorageManagerException ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StorageManager.StorageManagerException"/> class.
		/// </summary>
		/// <param name='message'>
		/// Message.
		/// </param>
		public StorageManagerException (string message) : base(message)
		{
		}
	}
	
	public static string PersistentDataPath {
		get { return Application.persistentDataPath; }
	}
	
	public static string TemporaryCachePath {
		get { return Application.temporaryCachePath; }
	}

	/// <summary>
	/// Writes byte array into the specified file.
	/// </summary>
	/// <param name="fileName">
	/// A <see cref="System.String"/> representing local file name. File will be put into the platform dependent persistent data directory.
	/// </param>
	/// <param name="data">
	/// A <see cref="System.Byte[]"/> to write into the file.
	/// </param>
	/// <summary>
	/// Write the specified fileName and data.
	/// </summary>
	/// <param name='fileName'>
	/// File name.
	/// </param>
	/// <param name='data'>
	/// Data.
	/// </param>
	public static void Write (string fileName, byte[] data)
	{
		string temporaryPathName = Path.Combine (TemporaryCachePath, fileName);
		string pathName = Path.Combine (PersistentDataPath, fileName);
		WriteToLocation (pathName, temporaryPathName, data);
	}
	
	/// <summary>
	/// Writes byte array into the file at the specified location, using specified temporary file.
	/// </summary>
	/// <param name="pathName">
	/// A <see cref="System.String"/> representing full pathname of the file for storing data in.
	/// </param>
	/// <param name="temporaryPathName">
	/// A <see cref="System.String"/> representing full pathname of the temporary file.
	/// </param>
	/// <param name="data">
	/// A <see cref="System.Byte[]"/> to write into the file.
	/// </param>
	/// <summary>
	/// Writes to location.
	/// </summary>
	/// <param name='pathName'>
	/// Path name.
	/// </param>
	/// <param name='temporaryPathName'>
	/// Temporary path name.
	/// </param>
	/// <param name='data'>
	/// Data.
	/// </param>
	public static void WriteToLocation (string pathName, string temporaryPathName, byte[] data)
	{
		//string checkPathName = pathName + ".check";
		//string temporaryCheckPathName = temporaryPathName + ".check.tmp";
		temporaryPathName += ".tmp";

		using (var fs = File.Create(temporaryPathName)) 
		{
			fs.Write (data, 0, data.Length);
		}
		
		if (File.Exists (pathName))
			File.Delete (pathName);
		File.Move (temporaryPathName, pathName);
		
		byte[] hash = new SHA256Managed ().ComputeHash (data);
		WriteHash(pathName, temporaryPathName, hash);
	}
	
	/// <summary>
	/// Reads data from the specified file.
	/// </summary>
	/// <param name="fileName">
	/// A <see cref="System.String"/> representing local file name. File will be searched for in the platform dependent persistent data directory.
	/// </param>
	/// <returns>
	/// A <see cref="System.Byte[]"/> containing data from the file.
	/// </returns>
	/// <summary>
	/// Read the specified fileName.
	/// </summary>
	/// <param name='fileName'>
	/// File name.
	/// </param>
	public static byte[] Read (string fileName)
	{
		string pathName = Path.Combine (PersistentDataPath, fileName);
		return ReadFromLocation (pathName);
	}
	
	/// <summary>
	/// Reads data from the file in specified location.
	/// </summary>
	/// <param name="pathName">
	/// A <see cref="System.String"/> representing full pathname of the file to read data from.
	/// </param>
	/// <returns>
	/// A <see cref="System.Byte[]"/>  containing data from the file.
	/// </returns>
	/// <summary>
	/// Reads from location.
	/// </summary>
	/// <returns>
	/// The from location.
	/// </returns>
	/// <param name='pathName'>
	/// Path name.
	/// </param>
	/// <exception cref='StorageManagerException'>
	/// Is thrown when the storage manager exception.
	/// </exception>
	public static byte[] ReadFromLocation (string pathName)
	{
		byte[] data = File.ReadAllBytes (pathName);
		byte[] realHash = new SHA256Managed().ComputeHash (data);
		CheckHash(pathName, realHash);
		return data;
	}
	
	/// <summary>
	/// Reads data from the specified file ignoring separate header.
	/// </summary>
	/// <param name="fileName">
	/// A <see cref="System.String"/> representing local file name. File will be searched for in the platform dependent persistent data directory.
	/// </param>
	/// <returns>
	/// A <see cref="System.Byte[]"/> containing data from the file.
	/// </returns>
	/// <summary>
	/// Read the specified fileName.
	/// </summary>
	/// <param name='fileName'>
	/// File name.
	/// </param>
	public static byte[] ReadWithoutChecking (string fileName)
	{
		string pathName = Path.Combine (PersistentDataPath, fileName);
		return File.ReadAllBytes (pathName);
	}
	
	/// <summary>
	/// Reads data from the file in specified location ignoring separate header.
	/// </summary>
	/// <param name="pathName">
	/// A <see cref="System.String"/> representing full pathname of the file to read data from.
	/// </param>
	/// <returns>
	/// A <see cref="System.Byte[]"/>  containing data from the file.
	/// </returns>
	/// <summary>
	/// Reads from location.
	/// </summary>
	/// <returns>
	/// The from location.
	/// </returns>
	/// <param name='pathName'>
	/// Path name.
	/// </param>
	/// <exception cref='StorageManagerException'>
	/// Is thrown when the storage manager exception.
	/// </exception>
	public static byte[] ReadFromLocationWithoutChecking (string pathName)
	{
		return File.ReadAllBytes (pathName);
	}
	
	/// <summary>
	/// Writes a serializable object to file using XML format.
	/// </summary>
	/// <param name="fileName">
	/// A <see cref="System.String"/> representing local file name. File will be put into the platform dependent persistent data directory.
	/// </param>
	/// <param name="data">
	/// A <see cref="System.Object"/> to save using XML format.
	/// </param>
	/// <summary>
	/// Writes the xml.
	/// </summary>
	/// <param name='fileName'>
	/// File name.
	/// </param>
	/// <param name='data'>
	/// Data.
	/// </param>
	public static void WriteXml (string fileName, object data)
	{
		string temporaryPathName = Path.Combine (TemporaryCachePath, fileName);
		string pathName = Path.Combine (PersistentDataPath, fileName);
		WriteXmlToLocation (pathName, temporaryPathName, data);
	}
	
	/// <summary>
	/// Writes a serializable object to file in the specified location using XML format.
	/// </summary>
	/// <param name="pathName">
	/// A <see cref="System.String"/> representing full pathname of the file for storing data in.
	/// </param>
	/// <param name="temporaryPathName">
	/// A <see cref="System.String"/> representing full pathname of the temporary file.
	/// </param>
	/// <param name="data">
	/// A <see cref="System.Object"/> to save using XML format.
	/// </param>
	/// <summary>
	/// Writes the xml to location.
	/// </summary>
	/// <param name='pathName'>
	/// Path name.
	/// </param>
	/// <param name='temporaryPathName'>
	/// Temporary path name.
	/// </param>
	/// <param name='data'>
	/// Data.
	/// </param>
	public static void WriteXmlToLocation (string pathName, string temporaryPathName, object data)
	{
		temporaryPathName += ".tmp";

		MemoryStream ms = new MemoryStream ();
		XmlSerializer serializer = new XmlSerializer (data.GetType ());
		serializer.Serialize (XmlWriter.Create (ms), data);

		ms.Position = 0;
		using (var fs = File.Create(temporaryPathName)) 
		{
			ms.WriteTo (fs);
		}

		if (File.Exists (pathName))
			File.Delete (pathName);
		File.Move (temporaryPathName, pathName);
		
		byte[] hash = new SHA256Managed().ComputeHash (ms);
		WriteHash(pathName, temporaryPathName, hash);
	}
	
	/// <summary>
	/// Reads an object stored in XML file at specified location.
	/// </summary>
	/// <param name="pathName">
	/// A <see cref="System.String"/> representing full pathname of the file to read XML data from.
	/// </param>
	/// <param name="type">
	/// A <see cref="System.Type"/> of object to be created from XML data.
	/// </param>
	/// <returns>
	/// A <see cref="System.Object"/> constructed from XML file.
	/// </returns>
	/// <summary>
	/// Reads the xml from location.
	/// </summary>
	/// <returns>
	/// The xml from location.
	/// </returns>
	/// <param name='pathName'>
	/// Path name.
	/// </param>
	/// <param name='type'>
	/// Type.
	/// </param>
	/// <exception cref='StorageManagerException'>
	/// Is thrown when the storage manager exception.
	/// </exception>
	public static object ReadXmlFromLocation (string pathName, System.Type type)
	{
		MemoryStream ms = null;
		using (var fs = File.OpenRead (pathName)) 
		{
			byte [] buffer = new byte[fs.Length];
			fs.Read (buffer, 0, buffer.Length);
			ms = new MemoryStream (buffer);
		}
		
		byte[] realHash = new SHA256Managed().ComputeHash (ms);
		CheckHash(pathName, realHash);
		
		ms.Position = 0;
		XmlSerializer serializer = new XmlSerializer (type);
		return serializer.Deserialize(XmlReader.Create (ms));
	}
	
	/// <summary>
	/// Reads an object stored in XML file.
	/// </summary>
	/// <param name="fileName">
	/// A <see cref="System.String"/> representing local file name. File will be searched for in the platform dependent persistent data directory.
	/// </param>
	/// <param name="type">
	/// A <see cref="System.Type"/> of object to be created from XML data.
	/// </param>
	/// <returns>
	/// A <see cref="System.Object"/> constructed from XML file.
	/// </returns>
	/// <summary>
	/// Reads the xml.
	/// </summary>
	/// <returns>
	/// The xml.
	/// </returns>
	/// <param name='fileName'>
	/// File name.
	/// </param>
	/// <param name='type'>
	/// Type.
	/// </param>
	public static object ReadXml (string fileName, System.Type type)
	{
		string pathName = Path.Combine (PersistentDataPath, fileName);
		return ReadXmlFromLocation (pathName, type);
	}
	
	/// <summary>
	/// Reads an object stored in XML file ignoring separate header.
	/// </summary>
	/// <returns>
	/// A <see cref="System.Object"/> constructed from XML file.
	/// </returns>
	/// <param name='fileName'>
	/// A <see cref="System.String"/> representing local file name. File will be searched for in the platform dependent persistent data directory.
	/// </param>
	/// <param name='type'>
	/// A <see cref="System.Type"/> of object to be created from XML data.
	/// </param>
	/// <summary>
	/// Reads the xml without checking.
	/// </summary>
	/// <returns>
	/// The xml without checking.
	/// </returns>
	/// <param name='fileName'>
	/// File name.
	/// </param>
	/// <param name='type'>
	/// Type.
	/// </param>
	public static object ReadXmlWithoutChecking (string fileName, System.Type type)
	{
		string pathName = Path.Combine (PersistentDataPath, fileName);
		return ReadXmlFromLocationWithoutChecking (pathName, type);
	}
	
	/// <summary>
	/// Reads an object stored in XML file at specified location ignoring separate header.
	/// </summary>
	/// <returns>
	/// A <see cref="System.Object"/> constructed from XML file.
	/// </returns>
	/// <param name='pathName'>
	/// A <see cref="System.String"/> representing full pathname of the file to read XML data from.
	/// </param>
	/// <param name='type'>
	/// A <see cref="System.Type"/> of object to be created from XML data.
	/// </param>
	/// <summary>
	/// Reads the xml from location without checking.
	/// </summary>
	/// <returns>
	/// The xml from location without checking.
	/// </returns>
	/// <param name='pathName'>
	/// Path name.
	/// </param>
	/// <param name='type'>
	/// Type.
	/// </param>
	public static object ReadXmlFromLocationWithoutChecking (string pathName, System.Type type)
	{
		object data = null;
		using (var reader = new FileInfo (pathName).OpenText ()) {
			XmlSerializer serializer = new XmlSerializer (type);
			data = serializer.Deserialize (reader);
		}
		return data;
	}
	
	private static void WriteHash(string pathName, string temporaryPathName, byte[] hash)
	{
#if (UNITY_IPHONE && !UNITY_EDITOR)
		string key = PathToKey (pathName);
		WriteHashKeychain(key,  hash);
#else
		string checkPathName = pathName + ".check";
		string temporaryCheckPathName = temporaryPathName + ".check.tmp";
		
		using (var cs = File.Create(temporaryCheckPathName)) 
		{
			cs.Write (hash, 0, hash.Length);
		}
		
		if (File.Exists (checkPathName))
			File.Delete (checkPathName);
		File.Move (temporaryCheckPathName, checkPathName);
#endif
	}
	
	private static void CheckHash(string pathName, byte[] realHash)
	{
#if (UNITY_IPHONE && !UNITY_EDITOR)
		string key = PathToKey (pathName);
		try
		{
			CheckHashKeychain(key, realHash);
		}
		catch(HashNotFoundException e)
		{
			throw new StorageManagerException ("File " + pathName + " is corrupted");
		}
		catch(KeychainRecordNotFoundException e)
		{
#if STORAGE_UPGRADE_ALLOWED
			byte[] hash = File.ReadAllBytes (pathName + ".check");
			if (!ByteArrayEquals (hash, realHash)) 
			{
				throw new StorageManagerException ("File " + pathName + " is corrupted");
			}
			SetSafeDataForKey(key, Convert.ToBase64String(hash));
#else
			throw new StorageManagerException ("Stored hash for " + pathName + " not found");		
#endif			
		}
#else
		string checkPathName = pathName + ".check";
		
		if (!File.Exists (checkPathName)) 
		{
			throw new StorageManagerException ("Hash file for " + pathName + " not found");
		}
		
		if (!ByteArrayEquals (File.ReadAllBytes(checkPathName), realHash)) 
		{
			throw new StorageManagerException ("File " + pathName + " is corrupted");
		}
#endif
	}
	
	/// <summary>
	/// Compares two byte arrays.
	/// </summary>
	/// <returns>
	/// True if arrays are equal, false otherwise.
	/// </returns>
	/// <param name='strA'>
	/// First byte array to compare.
	/// </param>
	/// <param name='strB'>
	/// Second byte array to compare.
	/// </param>
	private static bool ByteArrayEquals (byte[] strA, byte[] strB)
	{
		int length = strA.Length;
		if (length != strB.Length) {
			return false;
		}
		for (int i = 0; i < length; i++) {
			if (strA [i] != strB [i])
				return false;
		}
		return true;
	}
	
#if (UNITY_IPHONE && !UNITY_EDITOR)
	public class KeychainRecordNotFoundException : System.Exception
	{
		public KeychainRecordNotFoundException()
		{
		}
	}
	
	public class HashNotFoundException : System.Exception
	{
		public HashNotFoundException()
		{
		}
	}
	
	private const int deltaTime = 60; //delta in seconds
	
	private static void WriteHashKeychain(string key, byte[] hash)
	{
		List<int> times = new List<int>();
		List<string> hashes = new List<string>();
			
		string[] records  = Marshal.PtrToStringAnsi(GetSafeDataForKey(key)).Split(';');
		
		if ((records.Length % 2) != 0) //Backward compatibility
		{
			Array.Resize(ref records, 0);
		}
		
		for(int i = 0; i < records.Length / 2; ++i)
		{
			times.Add(Convert.ToInt32(records[2*i]));
			hashes.Add(records[2*i+1]);
		}

		int timeNow = UnixTime();
		times.Add(timeNow);
		hashes.Add(Convert.ToBase64String(hash));
		
		int n = times.BinarySearch(timeNow - deltaTime);
		if (n < 0) n = ~n;
		n = n - 1;
		
		if (times.Count > 0 && n > times.Count - 1)
			n = times.Count - 1;		
		if (n < 0) 
			n = 0;
			
		List<string> newRecords = new List<string>();
		for(int i = n; i < times.Count; ++i)
		{
			newRecords.Add(times[i].ToString());
			newRecords.Add(hashes[i]);
		}
		
		if (!SetSafeDataForKey(key, String.Join(";", newRecords.ToArray())))
		{
			throw new StorageManagerException ("Error setting keychain value");
		}
	}
	
	private static void CheckHashKeychain(string key, byte[] hash)
	{
		string base64Hash = Convert.ToBase64String(hash);
		
		string recordsStr = Marshal.PtrToStringAnsi(GetSafeDataForKey(key));
		if (recordsStr.Equals(""))
			throw new KeychainRecordNotFoundException();
		
		string[] records  = recordsStr.Split(';');
		
		if ((records.Length % 2) != 0) //Backward compatibility
		{
			if (!base64Hash.Equals(records[0]))
				throw new HashNotFoundException();
			return;
		}
			
		List<int> times = new List<int>();
		List<string> hashes = new List<string>();
			
		for(int i = 0; i < records.Length / 2; ++i)
		{
			times.Add(Convert.ToInt32(records[2*i]));
			hashes.Add(records[2*i+1]);
		}
			
		int n = -1;
		for(int i = 0; i < hashes.Count; ++i)
		{
			if (base64Hash.Equals(hashes[i]))
			{
				n = i;
				break;
			}
		}
			
		if (n < 0)
			throw new HashNotFoundException();
		
		Debug.Log("Storage manager: valid hash age is " + (times[times.Count - 1] - times[n]).ToString() + " seconds");
	}
	
	private static int UnixTime()
	{
		return (int)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
	}
	
	private static string PathToKey (string path)
	{
		if(Path.IsPathRooted(path))
		{
			string [] tokens = {"/Documents/", "/Library/", "/tmp/"};
			int index = -1;
			foreach (string token in tokens) {
				index = path.IndexOf (token);
				if (index >= 0)
					break;
			}
			if (index == -1){
				throw new StorageManagerException ("The file path " + path + " is invalid on iOS");
			}
			return path.Substring(index);
		} else {
			throw new StorageManagerException ("Cannot use relative file path for access on iOS");
		}
	}
	
	[DllImport ("__Internal")]
	private static extern IntPtr GetSafeDataForKey (string key);
	
	[DllImport ("__Internal")]
	private static extern bool SetSafeDataForKey (string key, string data);
#endif
}
