using UnityEditor;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace InJoy.AssetBundles.Internal
{
	/// <summary>
	/// Utils for the Builder. It is not designed to be used outside of AssetBundles space.
	/// </summary>
	public static class ETUtils
	{
		#region Interface
		
		public class ProgressBar : IDisposable
		{
			public string Title
			{
				set { m_title = value; ShowUpdate(); }
				get { return m_title; }
			}
			
			public string Text
			{
				set { m_text = value; ShowUpdate(); }
				get { return m_text; }
			}
			
			public float Progress
			{
				set { m_progress = value; ShowUpdate(); }
				get { return m_progress; }
			}
			
			public ProgressBar(string title, string text)
			{
				m_disposed = false;
				m_title = title;
				m_text = text;
				m_progress = (float)0;
				Show();
				stack.Push(this);
			}
			
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			
			~ProgressBar()
			{
				Dispose(false);
			}
			
			private void Show()
			{
				if(!IsUnityInBatchMode())
				{
					EditorUtility.DisplayProgressBar(Title, Text, Progress);
					m_latestShow = DateTime.Now;
				}
			}
			
			private void Hide()
			{
				if(!IsUnityInBatchMode())
				{
					EditorUtility.ClearProgressBar();
				}
			}
			
			private void ShowUpdate()
			{
				const int DELAY_BEFORE_UPDATE = 1000; // in ms
				if((DateTime.Now - m_latestShow).TotalMilliseconds > DELAY_BEFORE_UPDATE)
				{
					Show();
				}
			}
			
			private void Dispose(bool disposing)
			{
				if(!m_disposed)
				{
					if(disposing)
					{
						// nothing to do
						Progress = (float)1;
					}
					Hide();
					m_disposed = true;
					
					// show previous progress bar, if any
					if(stack.Count > 0)
					{
						stack.Pop();
						ProgressBar pb = (stack.Count > 0) ? stack.Peek() : null;
						if(pb != null)
						{
							pb.Show();
						}
					}
				}
			}
			
			private static Stack<ProgressBar> stack
			{
				get
				{
					if(m_stack != null)
					{
						return m_stack;
					}
					else
					{
						m_stack = new Stack<ProgressBar>();
						m_stack.Clear();
						return m_stack;
					}
				}
			}
			
			private static Stack<ProgressBar> m_stack = null;
			private bool m_disposed = false;
			private string m_title = null;
			private string m_text = null;
			private float m_progress = (float)0;
			private DateTime m_latestShow = DateTime.Now;
		}
		
		public static bool MessageBox(string title, string text)
		{
			return MessageBox(title, text, null);
		}
		
		public static bool MessageBox(string title, string text, string btnOK)
		{
			bool ret = false;
			if(!IsUnityInBatchMode())
			{
				if(string.IsNullOrEmpty(btnOK))
				{
					btnOK = "OK";
				}
				ret = EditorUtility.DisplayDialog(title, text, btnOK);
			}
			return ret;
		}
		
		public static bool DialogBox(string title, string text)
		{
			return DialogBox(title, text, null, null);
		}
		
		public static bool DialogBox(string title, string text, string btnOK, string btnCancel)
		{
			bool ret = false;
			if(!IsUnityInBatchMode())
			{
				if(string.IsNullOrEmpty(btnOK))
				{
					btnOK = "OK";
				}
				if(string.IsNullOrEmpty(btnCancel))
				{
					btnCancel = "Cancel";
				}
				ret = EditorUtility.DisplayDialog(title, text, btnOK, btnCancel);
			}
			return ret;
		}
		
		public static bool IsUnityInBatchMode()
		{
			return (SystemInfo.graphicsDeviceID == 0);
		}
		
		public static bool IsAssetFilenameValid(string filename)
		{
			filename = filename.Replace('\\', '/').ToLower();
			bool isFileScene = filename.EndsWith(".unity");
			return (!isFileScene || LongSceneFilenamesAllowed || Path.GetFileNameWithoutExtension(filename).Length < 16);
		}
		
		public static string CombinePaths(params string[] paths)
		{
			string ret = null;
			if(paths != null)
			{
				ret = "";
				if(paths.Length > 0)
				{
					ret = paths[0].Replace('\\', '/');
					for(int idx = 1; idx < paths.Length; ++idx)
					{
						ret = Path.Combine(ret, paths[idx]).Replace('\\', '/');
					}
				}
			}
			return ret;
		}
		
		public static void CreateDirectory(string pathname)
		{
			try
			{
				pathname = pathname.Replace('\\', '/');
				if(!Directory.Exists(pathname))
				{
					Directory.CreateDirectory(pathname);
				}
			}
			catch(Exception e)
			{
				Debug.LogWarning(string.Format("CreateDirectory - Caught exception: {0}", e.ToString()));
			}
		}
		
		public static void CreateDirectoryForFile(string filename)
		{
			CreateDirectory(Path.GetDirectoryName(filename.Replace('\\', '/')));
		}
		
		public static void DeleteDirectory(string pathname)
		{
			try
			{
				if(Directory.Exists(pathname))
				{
					Directory.Delete(pathname, true);
				}
			}
			catch(Exception e)
			{
				Debug.LogWarning(string.Format("DeleteDirectory - Caught exception: {0}", e.ToString()));
			}
		}
		
		public static void DeleteFile(string filename)
		{
			try
			{
				if(!string.IsNullOrEmpty(filename) && File.Exists(filename))
				{
					File.Delete(filename);
				}
			}
			catch(Exception e)
			{
				Debug.LogWarning(string.Format("DeleteFile - Caught exception: {0}", e.ToString()));
			}
		}
		
		public static string CreateHashForFile(string filename)
		{
			string ret = "";
			using(FileStream fs = new FileStream(filename, FileMode.Open))
			{
				ret = BytesToString(md5.ComputeHash(fs));
				fs.Close();
			}
			return ret.ToUpper();
		}
		
		public static string CreateHashForString(string data)
		{
			return BytesToString(md5.ComputeHash(Encoding.Default.GetBytes(data))).ToUpper();
		}
		
		public static string CreateHashForAsset(string assetFilename)
		{
			Assertion.Check(!string.IsNullOrEmpty(assetFilename));
			string ret = ETUtils.CreateHashForFile(assetFilename);
			if(!string.IsNullOrEmpty(ret))
			{
				string metaFilename = AssetDatabase.GetTextMetaDataPathFromAssetPath(assetFilename);
				if(!string.IsNullOrEmpty(metaFilename) && File.Exists(metaFilename))
				{
					ret = HashXorHash(ret, ETUtils.CreateHashForFile(metaFilename));
				}
			}
			return ret;
		}
		
		public static string HashXorHash(string x, string y)
		{
			//Debug.Log("HashXorHash - Started");
			string ret = null;
			byte[] a = !string.IsNullOrEmpty(x) ? RTUtils.HashToBytes(x) : null;
			byte[] b = !string.IsNullOrEmpty(y) ? RTUtils.HashToBytes(y) : null;
			if((a != null) && (b != null))
			{
				Assertion.Check(a.Length == b.Length);
				for(int idx = 0; idx < a.Length; ++idx)
				{
					a[idx] ^= b[idx];
				}
				ret = BytesToString(a);
			}
			else
			{
				if(a != null)
				{
					ret = x;
				}
				if(b != null)
				{
					ret = y;
				}
			}
			//Debug.Log("HashXorHash - Finished");
			return ret;
		}
		
		#endregion
		#region Implementation
		
		private static MD5 md5
		{
			get { return m_md5 ?? (m_md5 = new MD5CryptoServiceProvider()); }
		}
		
		private static bool LongSceneFilenamesAllowed
		{
			get { return (RTUtils.UnityVersion >= new System.Version("3.5.2")); }
		}
		
		private static string BytesToString(byte[] bytes)
		{
			string ret = "";
			foreach(byte b in bytes)
			{
				ret += b.ToString("x2").ToUpper();
			}
			return ret;
		}
		
		private static MD5 m_md5 = null;
		
		#endregion
	}
}
