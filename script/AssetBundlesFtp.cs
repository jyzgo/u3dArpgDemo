using System;
using System.Collections;
using System.IO;
using System.Net;
using UnityEngine;
using InJoy.AssetBundles;

namespace InJoy.AssetBundles
{
	public static class AssetBundlesFtp
	{
		private static bool _isLocal = true;

		public static bool isLocal	//local and remote ftp have different usernames and passwords
		{
			get { return _isLocal; }
			set 
			{
				if (value)
				{
					_userName = "locals3";
					_password = "locals3";
					FCDownloadManager.URL_SERVER_ROOT = FCDownloadManager.URL_SERVER_ROOT_DEV;
				}
				else
				{
					_userName = "ftp_user";
					_password = "ftp_user_123";
					FCDownloadManager.URL_SERVER_ROOT = FCDownloadManager.URL_SERVER_ROOT_TEST;
				}
				_isLocal = value;
			}
		}

		private static string _userName, _password;

		#region ftp commands

		public static void UploadFiles(FileInfo[] fileInfos, string ftpAddress = null)
		{
			string targetFtpAddress = FCDownloadManager.UploadLocalS3BuildBundleTagAddress.Replace("http://", "ftp://");
			foreach(FileInfo targetFileInfo in fileInfos)
			{
				//FileInfo targetFileInfo = new FileInfo(targetFileName);
				Uri targetURI = new Uri(targetFtpAddress +"/"+ targetFileInfo.Name);
				FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(targetURI);
				ftpRequest.Credentials = new NetworkCredential(_userName, _password);
				ftpRequest.KeepAlive = false;
				ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
				ftpRequest.UseBinary = true;
				ftpRequest.ContentLength = targetFileInfo.Length;
				byte[] buff = new byte[2048];
				int contentLen;
				FileStream fs = targetFileInfo.OpenRead();
				try
				{
					Debug.Log("FTP Uploader. Uploading file "+ targetFileInfo.FullName);
					Stream strm = ftpRequest.GetRequestStream();
					contentLen = fs.Read(buff, 0, buff.Length);
					while(contentLen != 0)
					{
						strm.Write(buff, 0 , contentLen);
						contentLen = fs.Read(buff, 0, buff.Length);
					}
					strm.Close();
					fs.Close();
					Debug.Log("\t\tFTP Uploader. Finished uploading file "+ targetFileInfo.FullName);
				}
				catch(Exception e)
				{
					Debug.LogError("FTP upload file " + targetFileInfo.FullName + "failed!" + e.StackTrace);
				}
			}
		}
		#endregion
		
		#region ftp commands
		public static void UploadJSONFile(string fileName, string fileContent, string ftpAddress = null)
		{	
			string targetFtpAddress = "";
			if(ftpAddress == null)
			{
				targetFtpAddress = FCDownloadManager.UploadLocalS3BuildJSONAddress.Replace("http://", "ftp://");
			}
			else
			{
				targetFtpAddress = ftpAddress.Replace("http://","ftp://");
			}

			//FileInfo targetFileInfo = new FileInfo(targetFileName);
			byte[] contentByte = System.Text.Encoding.Default.GetBytes(fileContent);
			
			Uri targetURI = new Uri(targetFtpAddress +"/"+ fileName);
			FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(targetURI);
			//ftpRequest.Credentials = new NetworkCredential("locals3", "locals3");
			ftpRequest.Credentials = new NetworkCredential(_userName, _password);
			ftpRequest.KeepAlive = false;
			ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
			ftpRequest.UseBinary = true;
			ftpRequest.ContentLength = contentByte.Length;
			try
			{
				Stream strm = ftpRequest.GetRequestStream();
				strm.Write(contentByte, 0, contentByte.Length);
				strm.Close();
			}
			catch(Exception e)
			{
				Debug.LogError("failed!" + e.StackTrace);
			}
			Debug.Log(string.Format("FTP Uploader: Json file {0} uploaded to: {1}", fileName, targetURI));
		}
		
		public static void CheckBuildTagAndAssetBundleTag()
		{
			string buildTagFile = string.Format("Assets/Resources/{0}.txt", InJoy.UnityBuildSystem.BuildInfo.k_build_tag);

			string bundleTagFile = string.Format("Assets/Resources/{0}.txt", InJoy.UnityBuildSystem.BuildInfo.k_bundle_tag);

			if (!File.Exists(buildTagFile))
			{
				Stream file = File.Create(buildTagFile);
				StreamWriter sw = new StreamWriter(file);
				sw.Write(UnityEngine.SystemInfo.deviceName);
				sw.Close();
				file.Close();
				InJoy.UnityBuildSystem.BuildInfo.buildTag = UnityEngine.SystemInfo.deviceName;
			}

			if (!File.Exists(bundleTagFile))
			{
				Stream file = File.Create(bundleTagFile);
				StreamWriter sw = new StreamWriter(file);
				sw.Write(UnityEngine.SystemInfo.deviceName);
				sw.Close();
				file.Close();
				InJoy.UnityBuildSystem.BuildInfo.assetbundleUploadTag = UnityEngine.SystemInfo.deviceName;
			}
		}
		
		public static void CheckUploadFolders()
		{
			CheckRemoteFolder(FCDownloadManager.UploadLocalS3BuildJSONAddress.Replace("http://", "ftp://"));
			CheckRemoteFolder((FCDownloadManager.UploadLocalS3BuildJSONAddress+"/AssetBundles").Replace("http://", "ftp://"));
			CheckRemoteFolder(FCDownloadManager.UploadLocalS3BuildBundleTagAddress.Replace("http://", "ftp://"));
		}
		
		public static void CheckRemoteFolder(string path)
		{
			try
			{
				Uri targetURI = new Uri(path);
				FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(targetURI);
				ftpRequest.Credentials = new NetworkCredential(_userName, _password);
				ftpRequest.KeepAlive = false;
				ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
				ftpRequest.UseBinary = true;
				FtpWebResponse response = ftpRequest.GetResponse() as FtpWebResponse;
				response.Close();
			}
			catch (Exception ex)
			{
				Debug.LogWarning(string.Format("FTP uploader. Remote folder creation failed. Folder = {0}, error = {1}", 
				                               path, ex.Message));
			}
			
		}
		#endregion
		
		#region generate JSON and upload
		public static void GenerateJSONAndUpload(DirectoryInfo targetFolder, string ftpAddress = null)
		{	
			FileInfo[] VersionFileInfos = targetFolder.GetFiles("*.version");
			Hashtable jsonFile = new Hashtable();
			jsonFile.Add("AssetBundlesTag", InJoy.DynamicContentPipeline.DynamicContent.Impl.AssetBundleUploadTag);
			string prefixPath = FCDownloadManager.UploadLocalDynamicInfoURLPrefix;
			ArrayList targetVersionFiles = new ArrayList();
			foreach(FileInfo curFileInfo in VersionFileInfos)
			{
				targetVersionFiles.Add(prefixPath + "/" + curFileInfo.Name);
			}
			jsonFile.Add("AssetBundlesUrls", targetVersionFiles);
			//jsonFile.Add("AssetBundlesSizes", GenerateIndexSizeMap("Assets/StreamingAssets/AssetBundles"));
			UploadJSONFile("DynamicContentInfo.json", InJoy.Utils.FCJson.jsonEncode(jsonFile));
		}
		
		public static Hashtable GenerateIndexSizeMap(string targetFolderPath)
		{
			Hashtable indexSizeHashtable = new Hashtable();
			
			Index[] indices = Builder.GetIndexInstances();
			Debug.LogWarning("Builder.GetIndexInstances():" + indices.Length);
			foreach(Index idx in indices) {
				Debug.LogWarning("index name:" + idx.m_filename);
			}
			DirectoryInfo targetFolderInfo = new DirectoryInfo(targetFolderPath);
			FileInfo[] indexFileInfos = targetFolderInfo.GetFiles("*.version");
			Debug.LogWarning("indexFileInfos:" + indexFileInfos.Length);
			foreach(FileInfo curIndexFileInfo in indexFileInfos)
			{
				long curIndexSize = 0;
				Index targetIndex = null;
				Debug.LogWarning("index file info:" + curIndexFileInfo.Name);
				foreach(Index curIndex in indices)
				{
					if(curIndex.m_filename+".version" == curIndexFileInfo.Name)
					{
						targetIndex = curIndex;
						break;
					}
				}
				
				foreach(Index.AssetBundle curAssetBundle in targetIndex.m_assetBundles)
				{
					curIndexSize += curAssetBundle.Size;
				}
				indexSizeHashtable.Add(curIndexFileInfo.Name, curIndexSize);
			}
			return indexSizeHashtable;
		}
		
		public static void GenerateIndexSizeMapToFile(string outputFilePath)
		{
			string fileContent = InJoy.Utils.FCJson.jsonEncode(GenerateIndexSizeMap("Assets/StreamingAssets/AssetBundles"));
			byte[] fileContentByte = System.Text.Encoding.UTF8.GetBytes(fileContent);
			FileStream fs = File.OpenWrite(outputFilePath);
			fs.Write(fileContentByte, 0, fileContentByte.Length);
			fs.Flush();
			fs.Close();
			
		}
		#endregion
	}
}

