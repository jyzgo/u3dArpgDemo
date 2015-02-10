#define FPS_COUNTER_ENABLED

using UnityEngine;
using System.Collections;
using System;

/* counts fps, frametime, memory: current/average/worst, plus debug info */
/* defines: FPS_COUNTER_ENABLED -- to disable, FPS_COUNTER_MINIMAL -- to reduce to simple fps counting */
public class FpsCounter: MonoBehaviour
{
	/* mode of showing: show as block in corner or as text */
	public enum Mode { Block, Text } ; public Mode mode = Mode.Text ;
	/* alignment */
	public TextAnchor align = TextAnchor.UpperCenter ;
	/* recount fps every */
	public float updateInterval = 0.25f ;
	
	/* for text mode: */
	/* text's color */
	public Color color = new Color(.7f,.7f,.7f) ;
	/* padding from corners of screen, use zero to show it right on corner of screen */
	public int padding = 5 ;
	/* show frame time in addition to fps */
	public bool showFrameTime = true ;
	/* show full or brief fps/etc info */
	public bool showFullFpsInfo = false ;
	/* format for number's output, try "0." or "0.0" */
	public string numbersFormat = "0." ;
	
	/* block mode: */
	/* scaling of block */
	public float blockScale = 2.0f ;
	/* color of fps block's text/background */
	public Color blockTextColor = Color.magenta ;
	public Color blockBackgroundColor = Color.black ;
	
	/* for showing debug info */
	public delegate string DebugInfoProc () ;
	public DebugInfoProc debugInfo { get ; set ; }
	public bool showDebugInfo = true ;
	/* offset from upper-left corner */
	public float debugInfoOffsetX = 30 ;
	public float debugInfoOffsetY = 60 ;
	
	#if !FPS_COUNTER_ENABLED
	public void Reset () {}
	public string GetText ( bool full ) { return "(disabled)" ; }
	#else
	/* time of last tick */
	float lastTime ;
	/* total ticks passed */
	int ticksTotal ;
	/* frames since last tick */
	int framesCount ;
	/* total time spent since start */
	float timeTotal ;
	/* total frames since start */
	float framesTotal ;
	/* frame time since last tick and maximal */
	float frameTime ;
	float frameTimeMax ;
	/* memory in kilobytes since last tick, max and total sum */
	long memory ;
	long memoryMax ;
	long memorySum ;
	/* last reported text */
	string text = "" ;
	/* background texture for block */
	Texture2D texture ;
	/* cached debug text */
	string debugText = "" ;
	
	public float FrameTime {
		get {return frameTime;}
	}
	public long Memory {
		get {return memoryMax;}
	}
	
	/* reset counters for each level */
	void OnLevelWasLoaded ( int level )
	{
		Reset () ;
	}
	
	/* reset all counters, start from new */
	public void Reset ()
	{
		lastTime = Time.realtimeSinceStartup ;
		ticksTotal = 0 ;
		framesCount = 0 ;
		timeTotal = 0 ;
		framesTotal = 0 ;
		frameTime = 0 ;
		frameTimeMax = int.MinValue ;
		memory = 0 ;
		memoryMax = int.MinValue ;
		memorySum = 0 ;
	}
	
	void Update ()
	{
		framesCount++ ;
		
		/* new tick? recount */
		float time = Time.realtimeSinceStartup ;
		float timePassed = time - lastTime ;
		if ( timePassed >= updateInterval )
		{
			/* increment/update */
			ticksTotal++ ;
			frameTime = ( time - lastTime ) / framesCount ;
			frameTimeMax = Mathf.Max ( frameTimeMax, frameTime ) ;
			timeTotal += timePassed ;
			framesTotal += framesCount ;
			//#if !FPS_COUNTER_MINIMAL
			memory = NativeUtils.GetCurrentMemoryBytes() ;
			memory /= 1024 ;
			memoryMax = memory>memoryMax? memory: memoryMax ;
			memorySum += memory ;
			//#endif
			
			/* reset tick counter */
			lastTime = time ;
			framesCount = 0 ;
			
			/* update fps/etc and debug info */
			#if !FPS_COUNTER_MINIMAL
			UpdateTexts ( showFullFpsInfo ) ;
			#endif
		}
	}
	
	/* update text with fps/ft/mem and debug info */
	void UpdateTexts ( bool full )
	{
		/* update text */
		text = "" ;
		string f = ":"+numbersFormat ;
		if ( full )
		{
			text += string.Format (
				"{0"+f+"} fps ({1"+f+"} avg, {2"+f+"} wst)",
				1/frameTime,
				framesTotal/timeTotal,
				1/frameTimeMax ) ;
			if ( showFrameTime )
			{
				text += string.Format (
					", {0"+f+"} ft ({1"+f+"} avg, {2"+f+"} wst)",
					1000*frameTime,
					1000*timeTotal/framesTotal,
					1000*frameTimeMax ) ;
			}
			if ( memory > 0 )
			{
				text += string.Format (
					", {0"+f+"} mem ({1"+f+"} avg, {2"+f+"} wst)",
					memory/1024.0f,
					memorySum/ticksTotal/1024.0f,
					memoryMax/1024.0f ) ;
			}
		}
		else
		{
			text += string.Format ( "{0"+f+"}", 1/frameTime ) ;
			if ( showFrameTime )
			{	text += string.Format ( ", {0"+f+"}", 1000*frameTime ) ; }
			if ( memory > 0 )
			{	text += string.Format ( ", {0"+f+"}", memory/1024.0f ) ; }
		}
		
		/* update debug info */
		debugText = "" ;
		if ( showDebugInfo && debugInfo != null )
		{
			Delegate[] infos = (debugInfo as System.Delegate).GetInvocationList() ;
			for ( int i = 0 ; i < infos.Length ; i++ )
			{	debugText += (infos[i] as DebugInfoProc)()+"\n" ; }
		}
	}
	
	/* get text manually with info about fps/ft/mem */
	public string GetText ( bool full )
	{	UpdateTexts ( full ) ; return text ; }
	
	#if !FPS_COUNTER_MINIMAL
	void OnGUI ()
	{
#if !(DEVELOPMENT_BUILD || UNITY_EDITOR)
		return;
#endif

		/* text for fps */
		if ( text.Length > 0 )
		{
			/* text mode */
			if ( mode == Mode.Text )
			{
				float width  = 400 * blockScale ;
				float height = 16 * blockScale ;
				Rect rect = new Rect (
					( align == TextAnchor.UpperCenter || align == TextAnchor.MiddleCenter || align == TextAnchor.LowerCenter )? Screen.width/2-width/2:
					( align == TextAnchor.UpperRight || align == TextAnchor.MiddleRight || align == TextAnchor.LowerRight )? Screen.width-width: 0,
					( align == TextAnchor.MiddleLeft || align == TextAnchor.MiddleCenter || align == TextAnchor.MiddleRight )? Screen.height/2-height/2:
					( align == TextAnchor.LowerLeft || align == TextAnchor.LowerCenter || align == TextAnchor.LowerRight )? Screen.height-height: 0,
					width, height ) ;
				if ( !texture )
				{	texture = new Texture2D(1,1) ; texture.SetPixel(0,0, new Color(0.0f, 0.0f, 0.0f, 0.5f)) ; texture.Apply () ; }
				GUI.DrawTexture(rect, texture);
				
				
				Color lastcolor = GUI.contentColor ; GUI.contentColor = Color.white ;
				GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Label")) ;
				style.alignment = align ;
				GUI.Label (new Rect(padding,padding,Screen.width-padding*2,Screen.height-padding*2),text,style);
				GUI.contentColor = lastcolor;
			}
			/* block with background mode */
			else
			{
				float width  = 32 * blockScale ;
				float height = 36 * blockScale ;
				Rect rect = new Rect (
					( align == TextAnchor.UpperCenter || align == TextAnchor.MiddleCenter || align == TextAnchor.LowerCenter )? Screen.width/2-width/2:
					( align == TextAnchor.UpperRight || align == TextAnchor.MiddleRight || align == TextAnchor.LowerRight )? Screen.width-width: 0,
					( align == TextAnchor.MiddleLeft || align == TextAnchor.MiddleCenter || align == TextAnchor.MiddleRight )? Screen.height/2-height/2:
					( align == TextAnchor.LowerLeft || align == TextAnchor.LowerCenter || align == TextAnchor.LowerRight )? Screen.height-height: 0,
					width, height ) ;
				if ( !texture )
				{	texture = new Texture2D(1,1) ; texture.SetPixel(0,0,blockBackgroundColor) ; texture.Apply () ; }
				GUI.DrawTexture(rect, texture);
				Color lastcolor = GUI.contentColor;
				GUI.contentColor = blockTextColor;
				Matrix4x4 lastMatrix = GUI.matrix;
				Vector3 scaleVector = new Vector3(blockScale, blockScale, 1.0f); 
				GUI.matrix = Matrix4x4.TRS (new Vector3(rect.x, rect.y, 0), Quaternion.identity, scaleVector);
				string str = (1/frameTime).ToString ("00") + "\n" + (memory/1024).ToString("000");
				GUILayout.Label (str);
				GUI.matrix = lastMatrix;
				GUI.contentColor = lastcolor;
			}
		}
		
		/* text for debug info */
		if ( debugText.Length > 0 )
		{
			Color lastcolor = GUI.contentColor ; GUI.contentColor = color ;
			GUI.Label ( new Rect ( debugInfoOffsetX, debugInfoOffsetY, Screen.width-padding-debugInfoOffsetX, Screen.height-padding-debugInfoOffsetY ), debugText ) ;
			GUI.contentColor = lastcolor ;
		}
	}
	#endif
	#endif
}
