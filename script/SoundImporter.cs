using UnityEditor;
using UnityEngine;
using System.Collections;

public class SoundImporter : AssetPostprocessor
{
	//const string k_sfx_root = "Resources/Audio/sfx";
	
	//const string k_music_root = "StreamingAssets/";

	void OnPreprocessAudio()
    {
        AudioImporter audioImporter = assetImporter as AudioImporter;

        if (audioImporter)
        {
            if (audioImporter.assetPath.ToLower().EndsWith(".wav"))
            {
                audioImporter.format = AudioImporterFormat.Compressed;
				if (audioImporter.assetPath.ToLower().Contains("ui/"))
				{
					audioImporter.threeD = false;
				}
				else
				{
					audioImporter.threeD = true;
				}
                audioImporter.forceToMono = false;
                audioImporter.loadType = AudioImporterLoadType.CompressedInMemory;
				audioImporter.hardware = true;
				audioImporter.compressionBitrate = 128000;
            }
            else if (audioImporter.assetPath.ToLower().EndsWith(".mp3"))
			{
				audioImporter.threeD = false;
				audioImporter.loadType = AudioImporterLoadType.StreamFromDisc;
				audioImporter.hardware = true;
				audioImporter.compressionBitrate = 128000;
			}
        }
    }
}
