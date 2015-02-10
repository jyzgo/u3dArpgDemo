using UnityEditor;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using System.Net;

using InJoy.AssetBundles;
using InJoy.AssetBundles.Internal;

using Window = AssetBundlesShellWindow;

/// <summary>
/// Instantiates and displays a window with asset properties,
/// showing what asset goes what asset bundle in.
/// </summary>
public class AssetBundlesShellWindow : EditorWindow
{
	#region Interface - UI - MenuItem
	
	/// <summary>
	/// Creates the instance.
	/// </summary>
	/// <returns>
	/// The instance.
	/// </returns>
	[MenuItem ("Window/AssetBundles")]
	public static Window GetInstance()
	{
		if(m_instance != null)
		{
			m_instance.Focus();
			return m_instance;
		}
		else
		{
			m_defaultColor = GUI.color;
			m_instance = ((Window)EditorWindow.GetWindow(typeof(Window))).Init();
			return m_instance;
		}
	}
	
	// TODO: should other item placement be chosen, i.e. not "File/" ?
//	/// <summary>
//	/// Menu item to build everything and run built player.
//	/// </summary>
//	[MenuItem ("File/Build Bundles and Run"/*, false*/)]
//	public static void BuildAllAndRun()
//	{
//	}
	
	#endregion
	#region Implementation - UI - EditorWindow
	
	public void OnInspectorUpdate()
	{
		if(EnsuresInstance())
		{
			DelayedState state = m_delayedState;
			Index index = m_delayedIndexToProcess;
			m_delayedIndexToProcess = null;
			try
			{
				switch(state)
				{
				case DelayedState.None:
					Repaint();
					break;
				case DelayedState.SaveIndex:
					SaveIndex(index);
					break;
				case DelayedState.BuildIndex:
					BuildAssetBundlesToStreamingAssets(index);
					break;
				case DelayedState.BuildPlayerOnly:
					BuildPlayer();
					break;
				case DelayedState.BuildAll:
					BuildAll();
					break;
				default:
					Assertion.Check(false); // SANITY CHECK
					break;
				}
			}
			finally
			{
				// try-finally block was used in order to finish "Build" states,
				// if compile-time errors occurred during build-time.
				// Without "finally" piece build would be started again (!)
				m_delayedState = DelayedState.None; // waited until the end, because OnGUI could be invoked during building
			}
		}
		if(m_delayedState == DelayedState.Destroy)
		{
			Close();
			GetInstance();
		}
	}
	
	public void OnGUI()
	{
		if(EnsuresInstance())
		{
			switch(m_delayedState)
			{
			case DelayedState.None:
				m_scrollPosRoot = BeginGroup(m_scrollPosRoot);
				DisplayRootLayout();
				EndGroup();
				break;
			default:
				GUILayout.Label(string.Format("Got request \"{0}\". Processing...", m_delayedState));
				GUILayout.Label(string.Format("Please wait."));
				GUILayout.Label(string.Format("(Left {0} item(s) to do)", (Builder.ItemsToDo > 0) ? ("approx " + Builder.ItemsToDo.ToString()) : "unknown number of"));
				break;
			}
		}
	}
	
	public void OnSelectionChange()
	{
		if(EnsuresInstance())
		{
			switch(m_delayedState)
			{
			case DelayedState.None:
				m_selectedIds = Selection.instanceIDs ?? new int[]{};
				break;
			default:
				// nothing to do
				break;
			}
		}
	}
	
	#endregion
	#region Implementation - UI - Window
	
	private GUIStyle MaskGUIStyle
	{
		get
		{
			if(m_maskGUIStyle != null)
			{
				return m_maskGUIStyle[(m_guiGroupLevel > 0) ? 1 : 0];
			}
			else
			{
				m_maskGUIStyle = new GUIStyle[2];
				m_maskGUIStyle[0] = new GUIStyle(GUI.skin.textField); // default
				m_maskGUIStyle[1] = new GUIStyle(GUI.skin.textField); // custom
				m_maskGUIStyle[1].margin.left += kMaskHorOffset;
			}
			return m_maskGUIStyle[(m_guiGroupLevel > 0) ? 1 : 0];
		}
	}
	
	private GUIStyle ScrollViewGUIStyle
	{
		get
		{
			if(m_scrollViewGUIStyle != null)
			{
				return m_scrollViewGUIStyle[(m_guiGroupLevel > 0) ? 1 : 0];
			}
			else
			{
				m_scrollViewGUIStyle = new GUIStyle[2];
				m_scrollViewGUIStyle[0] = new GUIStyle(GUI.skin.scrollView); // default
				m_scrollViewGUIStyle[1] = new GUIStyle(GUI.skin.scrollView); // custom
				m_scrollViewGUIStyle[1].margin.left += kScrollViewHorOffset;
			}
			return m_scrollViewGUIStyle[(m_guiGroupLevel > 0) ? 1 : 0];
		}
	}
	
	private GUIStyle ToggleGUIStyle
	{
		get
		{
			if(m_toggleGUIStyle != null)
			{
				return m_toggleGUIStyle[(m_guiGroupLevel > 0) ? 1 : 0];
			}
			else
			{
				m_toggleGUIStyle = new GUIStyle[2];
				m_toggleGUIStyle[0] = new GUIStyle(GUI.skin.toggle); // default
				m_toggleGUIStyle[1] = new GUIStyle(GUI.skin.toggle); // custom
				m_toggleGUIStyle[1].margin.left += kToggleHorOffset;
			}
			return m_toggleGUIStyle[(m_guiGroupLevel > 0) ? 1 : 0];
		}
	}
	
	private GUIStyle VerticalGUIStyle
	{
		get
		{
			if(m_verticalGUIStyle != null)
			{
				return m_verticalGUIStyle[(m_guiGroupLevel > 0) ? 1 : 0];
			}
			else
			{
				m_verticalGUIStyle = new GUIStyle[2];
				// HACK: consistent default style was not found
				m_verticalGUIStyle[0] = new GUIStyle(GUI.skin.scrollView); // default
				m_verticalGUIStyle[1] = new GUIStyle(GUI.skin.scrollView); // custom
				m_verticalGUIStyle[1].margin.left += kVerticalGroupHorOffset;
			}
			return m_verticalGUIStyle[(m_guiGroupLevel > 0) ? 1 : 0];
		}
	}
	
	private bool EnsuresInstance()
	{
		bool ret = false;
		if(m_isInitialised)
		{
			ret = (this == m_instance);
			if(!ret)
			{
				Debug.Log("It looks like Unity has recompiled the AssetBundles window. Restored it");
				m_delayedState = DelayedState.Destroy;
			}
		}
		return ret;
	}
	
	private bool CheckLastUIElementOnClick(ref int mouseButton, ref int clickCount)
	{
		bool ret = false;
		if(Event.current.isMouse)
		{
			if(GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
			{
				mouseButton = Event.current.button;
				clickCount = Event.current.clickCount;
				ret = true;
			}
		}
		return ret;
	}
	
	private bool IsScroolBarEnabled(int guiGroupLevel)
	{
		return (((options.m_scrollBarsMask >> ((guiGroupLevel - 1) / 2)) & 1) == 1);
	}
	
	private Vector2 BeginGroup(Vector2 scrollPosition)
	{
		Vector2 ret = scrollPosition;
		Assertion.Check(m_guiGroupLevel >= 0);
		if(m_guiGroupLevel >= 0)
		{
			switch(m_guiGroupLevel)
			{
			// hardcoded, it should be consistent with EndGroup cases
			case 1:
			case 3:
			case 5:
				if(IsScroolBarEnabled(m_guiGroupLevel))
				{
					ret = EditorGUILayout.BeginScrollView(scrollPosition, ScrollViewGUIStyle);
					break;
				}
				else
				{
					goto default;
				}
			default:
				EditorGUILayout.BeginVertical(VerticalGUIStyle);
				break;
			}
			++m_guiGroupLevel;
		}
		return ret;
	}
	
	private void EndGroup()
	{
		Assertion.Check(m_guiGroupLevel > 0);
		if(m_guiGroupLevel > 0)
		{
			--m_guiGroupLevel;
			switch(m_guiGroupLevel)
			{
			// hardcoded, it should be consistent with BeginGroup cases
			case 1:
			case 3:
			case 5:
				if(IsScroolBarEnabled(m_guiGroupLevel))
				{
					EditorGUILayout.EndScrollView();
					break;
				}
				else
				{
					goto default;
				}
			default:
				EditorGUILayout.EndVertical();
				break;
			}
		}
	}
	
	private void PushGUIEnabled(bool enabled)
	{
		m_guiEnabled.Push(GUI.enabled);
		GUI.enabled = enabled;
	}
	
	private void PopGUIEnabled()
	{
		try
		{
			GUI.enabled = m_guiEnabled.Pop();
		}
		catch(Exception)
		{
		}
	}
	
	private void PushGUIColor(Color color)
	{
		m_guiColor.Push(GUI.color);
		GUI.color = color;
	}
	
	private void PopGUIColor()
	{
		try
		{
			GUI.color = m_guiColor.Pop();
		}
		catch(Exception)
		{
		}
	}
	
	private struct AssetDesc
	{
		public AssetDesc(Index.AssetBundle.Asset asset, Color color, int? idx)
		{
			m_asset = asset;
			m_color = color;
			m_idx = idx;
		}
		
		public Index.AssetBundle.Asset asset
		{
			private set { m_asset = value; }
			get {return m_asset; }
		}
		
		public Color color
		{
			private set { m_color = value; }
			get {return m_color; }
		}
		
		public int? idx
		{
			private set { m_idx = value; }
			get {return m_idx; }
		}
		
		private Index.AssetBundle.Asset m_asset;
		private Color m_color;
		private int? m_idx;
	}
	
	private struct AssetBundleDesc
	{
		public AssetBundleDesc(Index.AssetBundle assetBundle, Color color, int? idx)
		{
			m_assetBundle = assetBundle;
			m_color = color;
			m_idx = idx;
		}
		
		public Index.AssetBundle assetBundle
		{
			private set { m_assetBundle = value; }
			get {return m_assetBundle; }
		}
		
		public Color color
		{
			private set { m_color = value; }
			get {return m_color; }
		}
		
		public int? idx
		{
			private set { m_idx = value; }
			get {return m_idx; }
		}
		
		private Index.AssetBundle m_assetBundle;
		private Color m_color;
		private int? m_idx;
	}
	
	private IEnumerable<AssetDesc> GetAssetDesc(Index.AssetBundle assetBundle)
	{
		Assertion.Check(assetBundle != null);
		if(assetBundle != null)
		{
			bool showChanges = options.m_shouldChangesBeShown && (m_curDiffIndex != null) && (m_curDiffIndex.m_indexToCompareWith != null);
			Index.AssetBundle abC = assetBundle;
			Index.AssetBundle abD = null;
			int idxC = 0;
			int idxD = 0;
			if(showChanges)
			{
				// find asset bundle to diff with
				foreach(Index.AssetBundle ab in m_curDiffIndex.m_indexToCompareWith.m_assetBundles)
				{
					if(assetBundle.m_filename.Equals(ab.m_filename))
					{
						abD = ab;
						break;
					}
				}
				showChanges &= (abD != null); // changes would be shown only if asset bundle is found
			}
			while(true)
			{
				Index.AssetBundle.Asset asset = null;
				Color color = m_defaultColor;
				int? idx = null;
				
				// we would try to display assets from the both indices step by step
				
				// first goes asset from the old index
				if(showChanges && (idxD < abD.m_assets.Count))
				{
					asset = abD.m_assets[idxD];
					color = kColor_ItemMissed;
					idx = null;
					++idxD;
				}
				
				if(idxC < abC.m_assets.Count)
				{
					// second goes asset from the actual index
					if(asset != null)
					{
						// find asset from the actual index related to asset from the old index
						for(int j = idxC; j < abC.m_assets.Count; ++j)
						{
							Index.AssetBundle.Asset assetC = abC.m_assets[j];
							bool areFilenamesTheSame = asset.m_filename.Equals(assetC.m_filename);
							bool areGuidsTheSame = asset.m_guid.Equals(assetC.m_guid);
							bool areAssetsTheSame = areFilenamesTheSame || areGuidsTheSame;
							
							if(areAssetsTheSame) // found
							{
								bool areHashesTheSame = (string.IsNullOrEmpty(asset.m_hash) && string.IsNullOrEmpty(assetC.m_hash)) || (!string.IsNullOrEmpty(asset.m_hash) && !string.IsNullOrEmpty(assetC.m_hash) && asset.m_hash.Equals(assetC.m_hash));
								
								if(areFilenamesTheSame)
								{
									// display skipped assets and move pointer
									for(int k = idxC; k < j; ++k)
									{
										yield return new AssetDesc(abC.m_assets[k], kColor_ItemAdded, k);
									}
									idxC = j;
									
									// means, asset has been found in both indices
									asset = abC.m_assets[idxC];
									if(areGuidsTheSame)
									{
										color = areHashesTheSame ? m_defaultColor : kColor_ItemModified;
									}
									else
									{
										color = kColor_ItemModified; //color = areHashesTheSame ? kColor_ItemMoved : kColor_ItemMovedModified;
									}
									idx = idxC;
									++idxC;
								}
								else
								{
									color = kColor_ItemMissed; //color = areHashesTheSame ? kColor_ItemMoved : kColor_ItemMovedModified;
								}
								break;
							}
						}
					}
					else
					{
						// no assets from the old index
						asset = abC.m_assets[idxC];
						color = showChanges ? kColor_ItemAdded : m_defaultColor;
						idx = idxC;
						++idxC;
					}
				}
				
				if(asset != null)
				{
					// asset to display has been found
					yield return new AssetDesc(asset, color, idx);
				}
				else
				{
					// all assets from both indices have been processed. Finishing
					yield break;
				}
			}
		}
		else
		{
			yield break;
		}
	}
	
	private IEnumerable<AssetBundleDesc> GetAssetBundleDesc(Index index)
	{
		Assertion.Check(index != null);
		if(index != null)
		{
			m_curDiffIndex = index;
			Index indexC = index;
			Index indexD = indexC.m_indexToCompareWith;
			int idxC = 0;
			int idxD = 0;
			bool showChanges = options.m_shouldChangesBeShown && (indexD != null);
			while(true)
			{
				Index.AssetBundle assetBundle = null;
				Color color = m_defaultColor;
				int? idx = null;
				
				// we would try to display asset bundles from the both indices in two steps
				
				// first goes asset bundle from the actual index
				if(idxC < indexC.m_assetBundles.Count)
				{
					assetBundle = indexC.m_assetBundles[idxC];
					color = showChanges ? kColor_ItemAdded : m_defaultColor;
					idx = idxC;
					++idxC;
				}
				
				if(showChanges && (idxD < indexD.m_assetBundles.Count))
				{
					if(assetBundle != null)
					{
						// find asset bundle from the old index related to asset bundle from the actual index
						for(int j = idxD; j < indexD.m_assetBundles.Count; ++j)
						{
							if(assetBundle.m_filename.Equals(indexD.m_assetBundles[j].m_filename))
							{
								// means, asset bundle has been found in both indices
								color = (idx == j) ? m_defaultColor : kColor_ItemMoved;
								indexD.m_assetBundles[j].m_wasDiffProcessed = true;
								break;
							}
						}
					}
					else
					{
						// no asset bundles from the actual index anymore
						while(idxD < indexD.m_assetBundles.Count)
						{
							if(indexD.m_assetBundles[idxD].m_wasDiffProcessed)
							{
								// skip those are already proccessed
								indexD.m_assetBundles[idxD].m_wasDiffProcessed = false;
								++idxD;
							}
							else
							{
								// process left asset bundles
								assetBundle = indexD.m_assetBundles[idxD];
								color = kColor_ItemMissed;
								idx = null;
								++idxD;
								break;
							}
						}
					}
				}
				
				if(assetBundle != null)
				{
					// asset bundle to display has been found
					yield return new AssetBundleDesc(assetBundle, color, idx);
				}
				else
				{
					// all asset bundles from both indices have been processed. Finishing
					m_curDiffIndex = null;
					yield break;
				}
			}
		}
		else
		{
			yield break;
		}
	}
	
	private void DisplayAssetLayout(ref Index.AssetBundle.Asset asset)
	{
		m_showDependencies = EditorGUILayout.Foldout(m_showDependencies, "Required files");
		if(m_showDependencies)
		{
			if(string.IsNullOrEmpty(m_assetToCollectDependencies) || !m_assetToCollectDependencies.Equals(asset.m_filename))
			{
				using(ETUtils.ProgressBar pb = new ETUtils.ProgressBar("Collect dependencies", null))
				{
					m_dependencies = AssetDatabase.GetDependencies(new string[]{asset.m_filename});
					m_assetToCollectDependencies = asset.m_filename;
					
					// sort by file extension, then by filename
					for(int i = 0; i < m_dependencies.Length; ++i)
					{
						pb.Progress = ((float)1 + ((float)i) / m_dependencies.Length) / 2;
						int idxFirst = i;
						for(int j = i + 1; j < m_dependencies.Length; ++j)
						{
							int cmp = Path.GetExtension(m_dependencies[j]).CompareTo(Path.GetExtension(m_dependencies[idxFirst]));
							if(cmp < 0)
							{
								idxFirst = j;
							}
							else if(cmp == 0)
							{
								cmp = Path.GetFileNameWithoutExtension(m_dependencies[j]).CompareTo(Path.GetFileNameWithoutExtension(m_dependencies[idxFirst]));
								if(cmp < 0)
								{
									idxFirst = j;
								}
							}
						}
						if(i != idxFirst)
						{
							string temp = m_dependencies[i];
							m_dependencies[i] = m_dependencies[idxFirst];
							m_dependencies[idxFirst] = temp;
						}
					}
				}
			}
			m_scrollPosDependencies = BeginGroup(m_scrollPosDependencies);
			foreach(string filename in m_dependencies)
			{
				GUILayout.Label(Path.GetFileName(filename));
				int mouseButton = 0;
				int clickCount = 0;
				if(CheckLastUIElementOnClick(ref mouseButton, ref clickCount) && (mouseButton == 0))
				{
					EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(filename));
				}
			}
			EndGroup();
		}
	}
	
	private void DisplayAssetBundleLayout(ref Index.AssetBundle assetBundle)
	{
		//if(asset bundle filename is editable)
		{
			const string kControlName = "AssetBundleFilename";
			bool isRenaming = false;
			if(assetBundle.m_editableFilename == null)
			{
				assetBundle.m_editableFilename = assetBundle.m_filename;
			}
			isRenaming = !assetBundle.m_filename.ToLower().Equals(assetBundle.m_editableFilename.ToLower());
			string format = (isRenaming ? "* {0}" : "{0}");
			GUI.SetNextControlName(kControlName);
			assetBundle.m_editableFilename = EditorGUILayout.TextField(string.Format(format, "Filename"), assetBundle.m_editableFilename);
			isRenaming = !assetBundle.m_filename.ToLower().Equals(assetBundle.m_editableFilename.ToLower());
			if(isRenaming)
			{
				if(!GUI.GetNameOfFocusedControl().Equals(kControlName)) // finished editing
				{
					assetBundle.m_filename = assetBundle.m_editableFilename;
					m_delayedState = DelayedState.SaveIndex;
				}
			}
		}
		//if(asset bundles could be compressed or not)
		{
			bool allowOption = true;
			
			PushGUIEnabled(RTUtils.UncompressedAssetBundlesAllowed && allowOption);
			bool? wasSet = assetBundle.m_isCompressed;
			bool? isSet = wasSet;
			string[] types = new string[]{"Default", "Compressed", "Uncompressed"};
			switch(EditorGUILayout.Popup("Compressibility", wasSet.HasValue ? (wasSet.Value ? 1 : 2 ) : 0, types))
			{
			case 0:
				isSet = null;
				break;
			case 1:
				isSet = true;
				break;
			case 2:
				isSet = false;
				break;
			default:
				Assertion.Check(false);
				break;
			}
			if(wasSet != isSet)
			{
				assetBundle.m_isCompressed = isSet;
				m_delayedState = DelayedState.SaveIndex;
			}
			PopGUIEnabled();
		}
		m_showAssets = EditorGUILayout.Foldout(m_showAssets, "Assets");
		if(m_showAssets)
		{
			m_scrollPosAssets = BeginGroup(m_scrollPosAssets);
			foreach(AssetDesc assetDesc in GetAssetDesc(assetBundle))
			{
				Index.AssetBundle.Asset asset = assetDesc.asset;
				Color color = assetDesc.color;
				string format = "";
				if(!ETUtils.IsAssetFilenameValid(asset.m_filename))
				{
					color = kColor_ItemCorrupted;
					format = "(TooLong!) ";
				}
				if(string.IsNullOrEmpty(asset.m_hash) && !File.Exists(asset.m_filename)) // instead of Directory.Exists(asset.m_filename)
				{
					format += "Folder: [ {0} ]";
				}
				else
				{
					format += "{0}";
				}
				
				EditorGUILayout.BeginHorizontal();
				bool wasOpened = (assetDesc.idx == m_idxAsset);
				PushGUIColor(color);
				bool isOpened = EditorGUILayout.Foldout(wasOpened, string.Format(format, Path.GetFileName(asset.m_filename)));
				PopGUIColor();
				
				bool willBeRemoved = false;
				if(assetDesc.idx.HasValue)
				{
					if(GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
					{
						isOpened = false;
						willBeRemoved = true;
					}
				}
				
				EditorGUILayout.EndHorizontal();
				
				if(assetDesc.idx.HasValue)
				{
					int idx = assetDesc.idx.Value;
					if(!wasOpened && isOpened) // just opened folded section
					{
						m_idxAsset = idx;
					}
					if(wasOpened && !isOpened) // just closed folded section
					{
						m_idxAsset = -1;
					}
					if(isOpened)
					{
						m_scrollPosAsset = BeginGroup(m_scrollPosAsset);
						DisplayAssetLayout(ref asset);
						EndGroup();
					}
					if(willBeRemoved)
					{
						if(Directory.Exists(asset.m_filename))
						{
							if(ETUtils.DialogBox("Remove directory from asset bundles", "Reference to the directory will be removed from asset bundles. Should references to files from the directory be removed from asset bundles, too?", "Remove directory only", "Remove all"))
							{
								// do nothing (reference to directory would be removed later)
							}
							else
							{
								// remove files only (reference to directory would be removed later)
								bool wereAssetsMessedUp = false;
								for(int i = idx + 1; i < assetBundle.m_assets.Count; ++i)
								{
									if(assetBundle.m_assets[i].m_filename.IndexOf(asset.m_filename) == 0)
									{
										assetBundle.m_assets.RemoveAt(i);
										--i;
										wereAssetsMessedUp = true;
									}
								}
								if(wereAssetsMessedUp)
								{
									// close subview
									m_idxAsset = -1;
								}
							}
						}
						assetBundle.m_assets.RemoveAt(idx);
						if(m_idxAsset > idx)
							--m_idxAsset;
						--idx;
						if(assetBundle.m_assets.Count == 0) // TODO: in case of empty directories, too
						{
							assetBundle.m_type = Index.AssetBundle.Type.None;
						}
						m_delayedState = DelayedState.SaveIndex;
					}
				}
			}
			EndGroup();
		}
		//EditorGUILayout.Separator();
		
		// display bottom "buttons parade" line 
		EditorGUILayout.BeginHorizontal();
		{
			if(GUILayout.Button("Add selected assets", GUILayout.ExpandWidth(false)))
			{
				if(m_selectedIds == null)
				{
					OnSelectionChange(); // forced selection update
				}
				bool succeeded;
				bool wereAssetsMessedUp;
				Builder.UpdateSpecifiedAssetBundle(assetBundle, m_selectedIds, out succeeded, out wereAssetsMessedUp);
				if(succeeded)
				{
					m_delayedState = DelayedState.SaveIndex;
				}
				if(wereAssetsMessedUp)
				{
					// means asset order has been changed, so close subview
					m_idxAsset = -1;
				}
			}
		}
		EditorGUILayout.EndHorizontal();
		//EditorGUILayout.Separator();
	}
	
	private void DisplayIndexLayout(ref Index index)
	{
		//if index filename is editable
		{
			const string kControlName = "IndexFilename";
			bool isRenaming = false;
			if(index.m_editableFilename == null)
			{
				index.m_editableFilename = index.m_filename;
			}
			isRenaming = !index.m_filename.ToLower().Equals(index.m_editableFilename.ToLower());
			string format = (isRenaming ? "* {0}" : "{0}");
			GUI.SetNextControlName(kControlName);
			index.m_editableFilename = EditorGUILayout.TextField(string.Format(format, "Filename"), index.m_editableFilename);
			isRenaming = !index.m_filename.ToLower().Equals(index.m_editableFilename.ToLower());
			if(isRenaming)
			{
				if(!GUI.GetNameOfFocusedControl().Equals(kControlName)) // finished editing
				{
					index.m_filename = index.m_editableFilename;
					m_delayedState = DelayedState.SaveIndex;
				}
			}
		}
		m_showAssetBundles = EditorGUILayout.Foldout(m_showAssetBundles, "Asset bundles");
		if(m_showAssetBundles)
		{
			m_scrollPosAssetBundles = BeginGroup(m_scrollPosAssetBundles);
			foreach(AssetBundleDesc assetBundleDesc in GetAssetBundleDesc(index))
			{
				Index.AssetBundle assetBundle = assetBundleDesc.assetBundle;
				Color color = assetBundleDesc.color;
				string format = "";
				if(assetBundle.m_assets.Count == 0)
				{
					if(color != kColor_ItemMissed)
					{
						color = kColor_ItemCorrupted;
					}
					format = "(Empty!) ";
				}
				format += "{0}";
				
				EditorGUILayout.BeginHorizontal();
				bool wasOpened = (assetBundleDesc.idx == m_idxAssetBundle);
				PushGUIColor(color);
				bool isOpened  = EditorGUILayout.Foldout(wasOpened, string.Format(format, Path.GetFileName(assetBundle.m_filename)));
				PopGUIColor();
				
				bool willBeDropped = false;
				bool willBeRemoved = false;
				if(assetBundleDesc.idx.HasValue)
				{
					int idx = assetBundleDesc.idx.Value;
					string pfx = ((m_idxDraggingAssetBundle >= 0) ? ((idx == m_idxDraggingAssetBundle) ? "H" : "H") : "Me");
					if((index.m_assetBundles.Count >= 2) && GUILayout.Button(pfx, GUILayout.ExpandWidth(false)))
					{
						if(m_idxDraggingAssetBundle >= 0) // already dragging?
						{
							if(m_idxDraggingAssetBundle == idx) // dropping back?
							{
								m_idxDraggingAssetBundle = -1; // canceled dragging
							}
							else
							{
								willBeDropped = true;
							}
						}
						else
						{
							m_idxDraggingAssetBundle = idx;
						}
					}
					if(GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
					{
						if(ETUtils.DialogBox("Removing asset bundle", string.Format("Are you sure you want to delete asset bundle \"{0}\"?", Path.GetFileName(assetBundle.m_filename))))
						{
							isOpened = false;
							willBeRemoved = true;
						}
					}
				}
				
				EditorGUILayout.EndHorizontal();
				
				if(assetBundleDesc.idx.HasValue)
				{
					int idx = assetBundleDesc.idx.Value;
					if(!wasOpened && isOpened) // just opened folded section
					{
						m_idxAssetBundle = idx;
						m_idxAsset = -1;
						assetBundle.m_editableFilename = null;
					}
					if(wasOpened && !isOpened) // just closed folded section
					{
						m_idxAssetBundle = -1;
					}
					if(isOpened)
					{
						m_scrollPosAssetBundle = BeginGroup(m_scrollPosAssetBundle);
						DisplayAssetBundleLayout(ref assetBundle);
						EndGroup();
					}
					if(willBeDropped)
					{
						Index.AssetBundle draggingAssetBundle = index.m_assetBundles[m_idxDraggingAssetBundle];
						index.m_assetBundles.RemoveAt(m_idxDraggingAssetBundle);
						index.m_assetBundles.Insert(idx, draggingAssetBundle);
						if(m_idxAssetBundle >= 0)
						{
							if(m_idxAssetBundle == m_idxDraggingAssetBundle)
							{
								m_idxAssetBundle = idx;
							}
							else
							{
								if((m_idxDraggingAssetBundle < m_idxAssetBundle) && (m_idxAssetBundle <= idx))
								{
									--m_idxAssetBundle;
								}
								if((idx <= m_idxAssetBundle) && (m_idxAssetBundle < m_idxDraggingAssetBundle))
								{
									++m_idxAssetBundle;
								}
							}
						}
						m_idxDraggingAssetBundle = -1;
						m_delayedState = DelayedState.SaveIndex;
					}
					if(willBeRemoved)
					{
						index.m_assetBundles.RemoveAt(idx);
						if(m_idxAssetBundle > idx)
						{
							--m_idxAssetBundle;
						}
						--idx;
						m_delayedState = DelayedState.SaveIndex;
					}
				}
			}
			EndGroup();
		}
		//EditorGUILayout.Separator();
		
		// display bottom "buttons parade" line 
		EditorGUILayout.BeginHorizontal();
		{
			if(GUILayout.Button("Add asset bundle", GUILayout.ExpandWidth(false)))
			{
				CreateAndAddAssetBundle(index);
			}
		}
		EditorGUILayout.EndHorizontal();
		//EditorGUILayout.Separator();
	}
	
	private void DisplayRootLayout()
	{
		EditorGUILayout.BeginHorizontal();
		m_showIndices = EditorGUILayout.Foldout(m_showIndices, "Indices");
		// TODO: do we need following buttons to update, build, remove ALL indices?
		//if(GUILayout.Button("Update", GUILayout.ExpandWidth(false)))
		//{
		//	UpdateAssetBundles(null);
		//}
		//if(GUILayout.Button("Build", GUILayout.ExpandWidth(false)))
		//{
		//	CleanProjectSilently();
		//	BuildAssetBundlesToStreamingAssets(null);
		//}
		EditorGUILayout.EndHorizontal();
		if(m_showIndices)
		{
			m_scrollPosIndices = BeginGroup(m_scrollPosIndices);
			for(int idx = 0; idx < m_indices.Count; ++idx)
			{
				Index index = m_indices[idx];
				EditorGUILayout.BeginHorizontal();
				bool wasOpened = (idx == m_idxIndex);
				bool isOpened  = EditorGUILayout.Foldout(wasOpened, Path.GetFileName(index.m_filename));
				bool willBeRemoved = false;
				if(GUILayout.Button("Update", GUILayout.ExpandWidth(false)))
				{
					UpdateAssetBundles(index);
					m_delayedState = DelayedState.SaveIndex;
				}
				if(GUILayout.Button("Build", GUILayout.ExpandWidth(false)))
				{
					// do not build the player right here
					// delay it until the very end of OnGUI instead
					m_delayedState = DelayedState.BuildIndex;
					m_delayedIndexToProcess = index;
				}
				if(GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
				{
					if(ETUtils.DialogBox("Removing index", string.Format("Are you sure you want to delete index \"{0}\" including all its asset bundles?", Path.GetFileName(index.m_filename))))
					{
						isOpened = false;
						willBeRemoved = true;
					}
				}
				EditorGUILayout.EndHorizontal();
				if(!wasOpened && isOpened) // just opened folded section
				{
					m_idxIndex = idx;
					m_idxAssetBundle = -1;
					m_idxAsset = -1;
					m_idxDraggingAssetBundle = -1;
					index.m_editableFilename = null;
				}
				if(wasOpened && !isOpened) // just closed folded section
				{
					m_idxIndex = -1;
				}
				if(isOpened)
				{
					m_scrollPosIndex = BeginGroup(m_scrollPosIndex);
					DisplayIndexLayout(ref index);
					EndGroup();
				}
				if(willBeRemoved)
				{
					string fileToDelete = index.m_xmlFilename;
					string prevToDelete = (index.m_indexToCompareWith != null) ? index.m_indexToCompareWith.m_xmlFilename : null;
					m_indices.RemoveAt(idx);
					if(m_idxIndex > idx)
					{
						--m_idxIndex;
					}
					--idx;
					if(!string.IsNullOrEmpty(fileToDelete) && File.Exists(fileToDelete))
					{
						ETUtils.DeleteFile(fileToDelete);
						ETUtils.DeleteFile(fileToDelete + ".meta");
						if(!string.IsNullOrEmpty(prevToDelete))
						{
							ETUtils.DeleteFile(prevToDelete);
						}
						AssetDatabase.Refresh();
					}
				}
				if(m_delayedState == DelayedState.SaveIndex)
				{
					if(m_delayedIndexToProcess == null) // if index to save is not specified explicitly, use current
					{
						m_delayedIndexToProcess = index;
					}
				}
			}
			EndGroup();
		}
		//EditorGUILayout.Separator();
		
		// display bottom "buttons parade" line - level 1
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Add index", GUILayout.ExpandWidth(false)))
		{
			CreateAndAddIndex();
		}
		if(GUILayout.Button("Log asset dependencies", GUILayout.ExpandWidth(false)))
		{
			Analyzer.LogAssetDependencies();
		}
		EditorGUILayout.EndHorizontal();
		//EditorGUILayout.Separator();
		
		// display bottom "buttons parade" line - level 2
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Clean project", GUILayout.ExpandWidth(false)))
		{
			if(ETUtils.DialogBox("Clean project", "This will delete all generated asset bundles from the folder StreamingAssets in your project. Are you sure?"))
			{
				CleanProjectSilently();
			}
		}
		if(GUILayout.Button("Build Player only", GUILayout.ExpandWidth(false)))
		{
			// do not build the player right here
			// delay it until the very end of OnGUI instead
			m_delayedState = DelayedState.BuildPlayerOnly;
			m_delayedIndexToProcess = null;
		}
		if(GUILayout.Button("Build All", GUILayout.ExpandWidth(false)))
		{
			// do not build the player right here
			// delay it until the very end of OnGUI instead
			m_delayedState = DelayedState.BuildAll;
			m_delayedIndexToProcess = null;
		}
		EditorGUILayout.Space(); //GUILayout.FlexibleSpace();
		EditorGUILayout.BeginVertical();
		bool newValue = false;
		bool shouldBeSaved = false;
		newValue = EditorGUILayout.Foldout(options.m_showBuildOptions, "Shell options");
		shouldBeSaved |= (options.m_showBuildOptions != newValue);
		options.m_showBuildOptions = newValue;
		if(options.m_showBuildOptions)
		{
			// "setup scrollbars"
			int mask = EditorGUILayout.MaskField("Scrollbars", options.m_scrollBarsMask, new string[]{"Indices", "Asset bundles", "Assets"}, MaskGUIStyle);
			shouldBeSaved |= (options.m_scrollBarsMask != mask);
			options.m_scrollBarsMask = mask;
			// "display changes"
			newValue = GUILayout.Toggle(options.m_shouldChangesBeShown, "Display changes", ToggleGUIStyle);
			shouldBeSaved |= (options.m_shouldChangesBeShown != newValue);
			options.m_shouldChangesBeShown = newValue;
			// "show window"
			newValue = GUILayout.Toggle(options.m_shouldPlayerBeShown, "Show player on success", ToggleGUIStyle);
			shouldBeSaved |= (options.m_shouldPlayerBeShown != newValue);
			options.m_shouldPlayerBeShown = newValue;
			// "run player"
			newValue = GUILayout.Toggle(options.m_shouldPlayerBeRun, "Run player on success", ToggleGUIStyle);
			shouldBeSaved |= (options.m_shouldPlayerBeRun != newValue);
			options.m_shouldPlayerBeRun = newValue;
			// "clean cache"
			newValue = GUILayout.Toggle(options.m_cleanCache, "Clean cache on app launch in Editor", ToggleGUIStyle);
			shouldBeSaved |= (options.m_cleanCache != newValue);
			options.m_cleanCache = newValue;
		}
		if(shouldBeSaved)
		{
			options.Save();
		}

		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();

		//upload
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();

		if(GUILayout.Button("upload bundles", GUILayout.ExpandWidth(false)))
		{
//			AssetBundlesFtp.GenerateIndexSizeMapToFile("assetBundlesSizeFile.txt");
			AssetBundlesFtp.CheckBuildTagAndAssetBundleTag();
			AssetBundlesFtp.CheckUploadFolders();
			DirectoryInfo targetFolder = new DirectoryInfo("Assets/StreamingAssets/AssetBundles");
			AssetBundlesFtp.GenerateJSONAndUpload(targetFolder);
			AssetBundlesFtp.UploadFiles(targetFolder.GetFiles("*.unity3d"));
			AssetBundlesFtp.UploadFiles(targetFolder.GetFiles("*.version"));
		}

		if(GUILayout.Button("upload server post", GUILayout.ExpandWidth(false)))
		{
			AssetBundlesFtp.CheckBuildTagAndAssetBundleTag();
			StreamReader result = new StreamReader(new FileStream(FCDownloadManager.UploadServerpostName, FileMode.Open));
			string test = result.ReadToEnd();
			AssetBundlesFtp.UploadJSONFile("serverpost_" + InJoy.UnityBuildSystem.BuildInfo.ServerPostTag + ".json", test, FCDownloadManager.UploadLocalS3ServerpostJSONAddress);
		}

		AssetBundlesFtp.isLocal = GUILayout.Toggle(AssetBundlesFtp.isLocal,
		                                           AssetBundlesFtp.isLocal ? "Local S3" : "Ali Cloud", ToggleGUIStyle);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
	}
	
	#endregion
	#region Implementation
	
	[XmlTypeAttribute("DefaultShellOptions")]
	public class Options
	{
		[XmlAttribute("Expanded")]
		public bool m_showBuildOptions;
		[XmlElement("ScrollBarsMask")]
		public int m_scrollBarsMask;
		[XmlElement("ShowDiff")]
		public bool m_shouldChangesBeShown;
		[XmlElement("ShowProductOnBuild")]
		public bool m_shouldPlayerBeShown;
		[XmlElement("RunProductOnBuild")]
		public bool m_shouldPlayerBeRun;
		[XmlElement("CleanCacheOnRunInEditor")]
		public bool m_cleanCache;

		public static Options Load()
		{
			Options ret = null;
			if(File.Exists(FILENAME))
			{
				try
				{
					using(Stream stream = new FileStream(FILENAME, FileMode.Open))
					{
						ret = xmlSerializer.Deserialize(stream) as Options;
					}
				}
				catch(Exception e)
				{
					Debug.LogWarning(string.Format("Load - Caught exception: {0}", e.ToString()));
				}
				if(ret == null)
				{
					try
					{
						// load by use of old format
						ret = new Options();
						using(Stream stream = new FileStream(FILENAME, FileMode.Open))
						{
							if(stream.Length > 0)
							{
								byte[] bytes = new byte[stream.Length];
								stream.Read(bytes, 0, bytes.Length);
								int idx = 0;
								if(bytes[0] == 2)
								{
									++idx; ret.m_shouldChangesBeShown = ((idx < bytes.Length) && (bytes[idx] > 0));
									++idx; ret.m_shouldPlayerBeShown  = ((idx < bytes.Length) && (bytes[idx] > 0));
									++idx; ret.m_shouldPlayerBeRun    = ((idx < bytes.Length) && (bytes[idx] > 0));
									++idx; ret.m_cleanCache           = ((idx < bytes.Length) && (bytes[idx] > 0));
								}
								else if(bytes[0] == 1)
								{
									++idx; ret.m_shouldChangesBeShown = ((idx < bytes.Length) && (bytes[idx] > 0));
									++idx; ret.m_shouldPlayerBeShown  = ((idx < bytes.Length) && (bytes[idx] > 0));
									++idx; ret.m_shouldPlayerBeRun    = ((idx < bytes.Length) && (bytes[idx] > 0));
								}
								else if(bytes[0] == 0)
								{
									++idx; ret.m_shouldPlayerBeShown = ((idx < bytes.Length) && (bytes[idx] > 0));
									++idx; ret.m_shouldPlayerBeRun   = ((idx < bytes.Length) && (bytes[idx] > 0));
								}
							}
						}
						ret.Save();
					}
					catch(Exception e)
					{
						Debug.LogWarning(string.Format("Load - Caught exception: {0}", e.ToString()));
						ret = null;
					}
				}
			}
			if(ret == null)
			{
				ret = new Options();
			}
			return ret;
		}
		
		public void Save()
		{
			try
			{
				ETUtils.CreateDirectoryForFile(FILENAME);
				using(Stream stream = new FileStream(FILENAME, FileMode.Create))
				{
					using(XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
					{
						xmlSerializer.Serialize(xmlWriter, this);
					}
				}
			}
			catch(Exception e)
			{
				Debug.LogWarning(string.Format("Save - Caught exception: {0}", e.ToString()));
			}
			finally
			{
				Load();
				
				// HACK: tricky way to allow run-time scripts to detect,
				// whether cache should be cleaned or not.
				string filename = Builder.SOURCE_OF_INDICES + "/DefaultShellSettings_CleanCache~";
				if(m_cleanCache)
				{
					ETUtils.CreateDirectoryForFile(filename);
					using(Stream stream = new FileStream(filename, FileMode.OpenOrCreate))
					{
					}
				}
				else
				{
					ETUtils.DeleteFile(filename);
				}
			}
		}
		
		public Options()
		{
			m_scrollBarsMask       = -1;
			m_showBuildOptions     = false;
			m_shouldChangesBeShown = false;
			m_shouldPlayerBeShown  = false;
			m_shouldPlayerBeRun    = false;
			m_cleanCache           = true;
		}
		
		private static XmlWriterSettings xmlWriterSettings
		{
			get
			{
				if(m_xmlWriterSettings != null)
				{
					return m_xmlWriterSettings;
				}
				else
				{
					m_xmlWriterSettings = new XmlWriterSettings();
					m_xmlWriterSettings.Encoding           = Encoding.UTF8;
					m_xmlWriterSettings.Indent             = true;
					m_xmlWriterSettings.IndentChars        = "\t";
					m_xmlWriterSettings.NewLineChars       = "\r\n"; // CR LF on every platform
					m_xmlWriterSettings.NewLineHandling    = NewLineHandling.None;
					m_xmlWriterSettings.OmitXmlDeclaration = true;
					return m_xmlWriterSettings;
				}
			}
		}
		
		private static XmlSerializer xmlSerializer
		{
			get
			{
				if(m_xmlSerializer != null)
				{
					return m_xmlSerializer;
				}
				else
				{
					m_xmlSerializer = new XmlSerializer(typeof(Options));
					return m_xmlSerializer;
				}
			}
		}
		
		private const string FILENAME = Builder.SOURCE_OF_INDICES + "/DefaultShellSettings~";
		
		// to serialize/deserialize instances
		private static XmlWriterSettings m_xmlWriterSettings = null;
		private static XmlSerializer     m_xmlSerializer     = null;
	}

	private enum DelayedState
	{
		None,
		SaveIndex,
		BuildIndex,
		BuildPlayerOnly,
		BuildAll,
		Destroy,
	}
	
	private static Options options
	{
		get { return m_options; }
	}
	
	private static string GetPlayerDefaultOutputFolder()
	{
		const string kDefaultOutputFolder = "Build";
		return Directory.Exists(kDefaultOutputFolder) ? kDefaultOutputFolder : "";
	}
	
	private static string GetPlayerDefaultName()
	{
		return "";
	}
	
	private static string GetPlayerExtension()
	{
#if UNITY_WEBPLAYER
		string fileExtension = "unity3d";
#elif UNITY_ANDROID
		string fileExtension = "apk";
#elif UNITY_IPHONE
		string fileExtension = "";
#elif UNITY_STANDALONE_OSX
		string fileExtension = "app";
#elif UNITY_STANDALONE_WIN
		string fileExtension = "exe";
#else
		string fileExtension = "unity3d";
#endif
		return fileExtension;
	}
	
	private static BuildTarget GetPlayerTarget()
	{
#if UNITY_WEBPLAYER
#error AssetBundles doesn't support current platform
#elif UNITY_ANDROID
		BuildTarget buildTarget = BuildTarget.Android;
#elif UNITY_IPHONE
		BuildTarget buildTarget = BuildTarget.iPhone;
#elif UNITY_STANDALONE_OSX
		BuildTarget buildTarget = BuildTarget.StandaloneOSXIntel;
#elif UNITY_STANDALONE_WIN
		BuildTarget buildTarget = BuildTarget.StandaloneWindows;
#else
#error AssetBundlesBuilder doesn't support current platform
#endif
		return buildTarget;
	}
	
	private static BuildTarget GetAssetBundleTarget()
	{
		return GetPlayerTarget();
	}
	
	private static BuildOptions GetGeneralOptions()
	{
		BuildOptions buildOptions = BuildOptions.None;
		if(EditorUserBuildSettings.development)
		{
			buildOptions |= BuildOptions.Development;
		}
		if(EditorUserBuildSettings.connectProfiler)
		{
			buildOptions |= BuildOptions.ConnectWithProfiler;
		}
		if(EditorUserBuildSettings.allowDebugging)
		{
			buildOptions |= BuildOptions.AllowDebugging;
		}
		return buildOptions;
	}
	
	private static BuildOptions GetPlayerOptions()
	{
		BuildOptions buildOptions = GetGeneralOptions();
		if(options.m_shouldPlayerBeShown)
		{
			buildOptions |= BuildOptions.ShowBuiltPlayer;
		}
		if(options.m_shouldPlayerBeRun)
		{
			buildOptions |= BuildOptions.AutoRunPlayer;
		}
		return buildOptions;
	}
	
	private static BuildOptions GetAssetBundleOptions()
	{
		return GetGeneralOptions();
	}
	
	private static void CheckForPreviousVersion()
	{
		const string kDeprecatedIndex = "Assets/Editor/Conf/AssetBundlesBuilderDatabase.xml";
		string indexFilename = ETUtils.CombinePaths(Builder.SOURCE_OF_INDICES, Guid.NewGuid().ToString() + ".xml");
		Index temp = Builder.index;
		Builder.index = null;
		bool is_040 = File.Exists(kDeprecatedIndex);
		bool is_050 = (Builder.GetIndexInstances().Length > 0);
		Builder.index = temp;
		if(is_040)
		{
			if(is_050)
			{
				if(ETUtils.DialogBox("AssetBundles", "Found deprecated assets-to-bundles distribution file. Should it be removed from the project?"))
				{
					ETUtils.DeleteFile(kDeprecatedIndex);
					ETUtils.DeleteFile(kDeprecatedIndex + ".meta");
					AssetDatabase.Refresh();
				}
			}
			else
			{
				if(ETUtils.DialogBox("AssetBundles", "Found deprecated assets-to-bundles distribution file. Should it be updated to the current version of the component?"))
				{
					ETUtils.CreateDirectoryForFile(indexFilename);
					File.Copy(kDeprecatedIndex, indexFilename, true);
					ETUtils.DeleteFile(kDeprecatedIndex);
					ETUtils.DeleteFile(kDeprecatedIndex + ".meta");
					AssetDatabase.Refresh();
				}
			}
		}
	}
	
	private Window ReloadAllIndices()
	{
		using(ETUtils.ProgressBar pb = new ETUtils.ProgressBar("Reloading all indices", null))
		{
			m_indices = new List<Index>();
			m_indices.Clear();
			Index temp = Builder.index;
			Builder.index = null;
			Index[] indices = Builder.GetIndexInstances();
			Builder.index = temp;
			for(int idx = 0; idx < indices.Length; ++idx)
			{
				pb.Progress = ((float)idx) / indices.Length;
				if(indices[idx] != null)
				{
					m_indices.Add(indices[idx]);
				}
			}
		}
		return this;
	}
	
	private void TryToExpandTheOnlySections()
	{
		if(m_showIndices)
		{
			if(m_indices.Count == 1)
			{
				m_idxIndex = 0; // expand the only index
				if(m_showAssetBundles)
				{
					if(m_indices[m_idxIndex].m_assetBundles.Count == 1)
					{
						m_idxAssetBundle = 0; // expand the only asset bundle
					}
				}
			}
		}
	}
	
	private Window Init()
	{
		CheckForPreviousVersion();
		ReloadAllIndices();
		TryToExpandTheOnlySections();
		m_options = Options.Load();
		m_isInitialised = true;
		return this;
	}
	
	private void CreateAndAddIndex()
	{
		if(ETUtils.DialogBox("Add index", "This will create totally new index. Are you sure?"))
		{
			using(ETUtils.ProgressBar pb = new ETUtils.ProgressBar("Adding index", "Creating index..."))
			{
				Index index = Index.CreateInstance();
				index.m_filename = Guid.NewGuid().ToString();
				try
				{
					ETUtils.CreateDirectory(Builder.SOURCE_OF_INDICES);
					using(Stream fs = new FileStream(ETUtils.CombinePaths(Builder.SOURCE_OF_INDICES, index.m_filename + ".xml"), FileMode.CreateNew))
					{
						index.SaveInstance(fs);
					}
				}
				catch(Exception e)
				{
					Debug.LogError(string.Format("CreateAndAddIndex - Caught exception: {0}", e.ToString()));
				}
				AssetDatabase.Refresh();
				ReloadAllIndices();
				m_idxIndex = -1;
				TryToExpandTheOnlySections();
			}
		}
	}
	
	private void CreateAndAddAssetBundle(Index index)
	{
		Assertion.Check(index != null);
		if(index != null)
		{
			if(ETUtils.DialogBox("Add asset bundle", "This will create totally new asset bundle. Are you sure?"))
			{
				Index.AssetBundle assetBundle = new Index.AssetBundle();
				assetBundle.m_filename = Guid.NewGuid().ToString();
				index.m_assetBundles.Add(assetBundle);
				m_delayedState = DelayedState.SaveIndex;
				m_idxAssetBundle = -1;
				TryToExpandTheOnlySections();
			}
		}
	}
	
	private void UpdateAssetBundles(Index index)
	{
		using(ETUtils.ProgressBar pb = new ETUtils.ProgressBar("Updating asset bundles", null))
		{
			Index temp = Builder.index;
			Builder.index = index;
			Builder.UpdateAssetBundles();
			Builder.index = temp;
			// TODO: save index on change only, i.e. Index originalIndex = Index.DuplicateInstance(index);
			//if(!originalIndex.Equals(index))
			//{
			//	pb.Progress = (float)1;
			//	if(ETUtils.DialogBox("Update asset bundles", "Index has been modified. Do you want to save changes?"))
			//	{
			//		m_delayedState = DelayedState.SaveIndex;
			//		//m_delayedIndexToProcess = indexToModify;
			//	}
			//}
		}
	}
	
	private void SaveIndex(Index index)
	{
		Assertion.Check(index != null);
		Assertion.Check(!string.IsNullOrEmpty(index.m_xmlFilename));
		if(!string.IsNullOrEmpty(index.m_xmlFilename))
		{
			// save plain distribution
			string strippedIndexFilename = index.m_xmlFilename;
			Index strippedIndex = Index.DuplicateAndStripInstance(index);
			using(Stream fs = new FileStream(strippedIndexFilename, FileMode.Create))
			{
				strippedIndex.SaveInstance(fs);
			}
			
			// save advanced data
			string filledIndexFilename = Path.Combine(Path.GetDirectoryName(index.m_xmlFilename), ".#" + Path.GetFileNameWithoutExtension(index.m_xmlFilename) + ".cur");
			Index filledIndex = Index.DuplicateInstance(index);
			filledIndex.m_strippedIndexHash = ETUtils.CreateHashForFile(strippedIndexFilename);
			using(Stream fs = new FileStream(filledIndexFilename, FileMode.Create))
			{
				filledIndex.SaveInstance(fs);
			}
		}
	}
	
	private void CleanProjectSilently()
	{
		using(ETUtils.ProgressBar pb = new ETUtils.ProgressBar("Cleaning the project", null))
		{
			ETUtils.DeleteDirectory(Builder.TARGET_IN_STREAMING_ASSETS);
			ETUtils.CreateDirectory(Builder.TARGET_IN_STREAMING_ASSETS);
			AssetDatabase.Refresh();
		}
	}
	
	private string GenerateBuildDetails(BuildTarget buildTarget, BuildOptions buildOptions)
	{
		string target = buildTarget.ToString();
		string options = buildOptions.ToString();
		return string.Format("Platform: {0}\nOptions: {1}", target, options);
	}
	
	private void BuildAssetBundlesToStreamingAssets(Index index)
	{
		bool succeeded = true;
		if(succeeded)
		{
			if(EditorApplication.isPlaying)
			{
				succeeded = false;
				ETUtils.MessageBox("Warning", "Cannot build asset bundles while application is playing in Editor. Please stop first");
			}
		}
		if(succeeded)
		{
			if(index != null)
			{
				succeeded = ETUtils.DialogBox("Build asset bundles", string.Format("This will build asset bundles for the index \"{0}\". Are you sure?\n\n{1}", index.m_filename, GenerateBuildDetails(GetAssetBundleTarget(), GetAssetBundleOptions())));
			}
		}
		if(succeeded)
		{
			using(ETUtils.ProgressBar pb = new ETUtils.ProgressBar("Building asset bundles", null))
			{
				ETUtils.CreateDirectory(Builder.TARGET_IN_STREAMING_ASSETS);
				Index temp = Builder.index;
				Builder.index = index;
				Builder.BuildAssetBundles(Builder.TARGET_IN_STREAMING_ASSETS, GetAssetBundleTarget(), GetAssetBundleOptions());
				Builder.index = temp;
				AssetDatabase.Refresh();
			}
		}
	}
	
	private void BuildPlayer()
	{
		if(EditorApplication.isPlaying)
		{
			ETUtils.MessageBox("Warning", "Cannot build player while application is playing in Editor. Please stop first");
		}
		else
		{
			string filename = EditorUtility.SaveFilePanel("Save Player to", GetPlayerDefaultOutputFolder(), GetPlayerDefaultName(), GetPlayerExtension());
			if(!string.IsNullOrEmpty(filename))
			{
				using(ETUtils.ProgressBar pb = new ETUtils.ProgressBar("Building player", null))
				{
					Index temp = Builder.index;
					Builder.index = null;
					Builder.BuildPlayer(filename, GetPlayerTarget(), GetPlayerOptions());
					Builder.index = temp;
				}
			}
		}
	}
	
	private void BuildAll()
	{
		if(EditorApplication.isPlaying)
		{
			ETUtils.MessageBox("Warning", "Cannot build asset bundles nor player while application is playing in Editor. Please stop first");
		}
		else if(ETUtils.DialogBox("Build all", string.Format("This will build all asset bundles packs. Also this will build the player with scenes from build settings except for those, which are found in asset bundles. Built asset bundles packs will be included within the player. Are you sure?\n\n{0}", GenerateBuildDetails(GetPlayerTarget(), GetPlayerOptions()))))
		{
			using(ETUtils.ProgressBar pb = new ETUtils.ProgressBar("Building all", null))
			{
				CleanProjectSilently();
				BuildAssetBundlesToStreamingAssets(null);
				BuildPlayer();
			}
		}
	}
	
	#endregion
	#region Implementation - members
	
	// main instances
	private static Window m_instance = null;
	private static Options m_options = Options.Load();
	private bool m_isInitialised = false;
	private List<Index> m_indices = null;
	
	// to display scroll view(s) in the window
	private const int kGUIHorOffset = 32;
	private const int kMaskHorOffset = kGUIHorOffset;
	private const int kScrollViewHorOffset = kGUIHorOffset;
	private const int kToggleHorOffset = kGUIHorOffset;
	private const int kVerticalGroupHorOffset = kGUIHorOffset;
	private GUIStyle[] m_maskGUIStyle = null;
	private GUIStyle[] m_scrollViewGUIStyle = null;
	private GUIStyle[] m_toggleGUIStyle = null;
	private GUIStyle[] m_verticalGUIStyle = null;
	private int m_guiGroupLevel = 0;
	private Vector2 m_scrollPosRoot         = new Vector2(0, 0);
	private Vector2 m_scrollPosIndices      = new Vector2(0, 0);
	private Vector2 m_scrollPosIndex        = new Vector2(0, 0);
	private Vector2 m_scrollPosAssetBundles = new Vector2(0, 0);
	private Vector2 m_scrollPosAssetBundle  = new Vector2(0, 0);
	private Vector2 m_scrollPosAssets       = new Vector2(0, 0);
	private Vector2 m_scrollPosAsset        = new Vector2(0, 0);
	private Vector2 m_scrollPosDependencies = new Vector2(0, 0);
	
	// to show and hide content in the window
	private int m_idxIndex       = -1;
	private int m_idxAssetBundle = -1;
	private int m_idxAsset       = -1;
	private bool m_showIndices      = true;
	private bool m_showAssetBundles = true;
	private bool m_showAssets       = false;
	private bool m_showDependencies = false;
	
	// to cache dependencies
	private string m_assetToCollectDependencies = null;
	private string[] m_dependencies = null;
	
	// to track selection
	private int[] m_selectedIds = null;
	
	// to delay actions. For example, Unity cannot build player in OnGUI
	private DelayedState m_delayedState = DelayedState.None;
	private Index m_delayedIndexToProcess = null;
	
	// to change the order of asset bundles
	private int m_idxDraggingAssetBundle = -1;
	
	// to paint GUI pretty
	private static Color m_defaultColor = Color.clear;
	private readonly Color kColor_ItemCorrupted     = Color.red;
	private readonly Color kColor_ItemMissed        = Color.gray;
	private readonly Color kColor_ItemModified      = new Color(0.62f, 0.62f, 1.0f);
	private readonly Color kColor_ItemAdded         = Color.green;
	private readonly Color kColor_ItemMoved         = Color.magenta;
//	private readonly Color kColor_ItemMovedModified = Color.magenta;
	private Stack<bool> m_guiEnabled = new Stack<bool>();
	private Stack<Color> m_guiColor = new Stack<Color>();
	
	// to track differences between two indices
	private Index m_curDiffIndex = null;
	
	#endregion
	
	public static void BuildAssetBundleByIndex()
	{
//		const string kDistributionsDirectory = "Assets/Editor/Conf/AssetBundles";
//		string[] distributions = Directory.Exists(kDistributionsDirectory) ? Directory.GetFiles(kDistributionsDirectory, "*.xml", SearchOption.TopDirectoryOnly) : new string[]{};
//		if(distributions.Length == 0)
//		{
//			return;
//		}
//		Index[] allIndexContents = new Index[distributions.Length];
		
//		for(int i = 0; i < distributions.Length; i++)
//		{
//			using(FileStream fs = new FileStream(distributions[i], FileMode.Open))
//			{
//				allIndexContents[i] = Index.LoadInstance(fs);
//			}
//		}
		//string[] fileNames = {"index_world_china"};//Environment.GetCommandLineArgs();
		Console.WriteLine("Begin to Build Special Index!!!");
		string[] args = Environment.GetCommandLineArgs();
		//string[] args = {"+IndexNames","index_world_greece_2,index_world_china_1"};
		string[] fileNames = null;
		for(int i = 0;i < args.Length;i++)
		{
			if(args[i] == "+IndexNames" && args.Length > i + 1)
			{
				string allIndexName = args[i + 1];
				fileNames = allIndexName.Split(",".ToCharArray());
				break;
			}
		}
		
		foreach(string targetFileName in fileNames)
		{
			Console.WriteLine(targetFileName + " is Found in List!");
		}
		Index[] allIndexContents = Builder.GetIndexInstances();
		
		Builder.AssetBundlesCompressionOverriding temp = Builder.OverrideAssetBundlesCompression;
		Builder.OverrideAssetBundlesCompression = Builder.AssetBundlesCompressionOverriding.Compressed;
		
		foreach(string curFileName in fileNames)
		{
			Console.WriteLine(curFileName + " is Processing!");
			Index curTargetIndexFile = null;
			foreach(Index curTestIndexFile in allIndexContents)
			{
				if(curTestIndexFile.m_filename == curFileName)
				{
					curTargetIndexFile = curTestIndexFile;
					break;
				}
			}
			
			if (curTargetIndexFile == null)
			{
				return;
			}
			string targetOutputFolder = Builder.TARGET_IN_STREAMING_ASSETS;
			ETUtils.CreateDirectory(targetOutputFolder);
			Builder.index = curTargetIndexFile;
			Builder.BuildAssetBundles(targetOutputFolder, GetAssetBundleTarget(), GetAssetBundleOptions());
		}
		Builder.OverrideAssetBundlesCompression = temp;
		Builder.index = null; // clear index to clear Builder.GetIndexInstances().
		AssetBundlesFtp.GenerateIndexSizeMapToFile("assetBundlesSizeFile.txt");
	}
}
