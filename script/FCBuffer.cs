using UnityEngine;
using System.Collections;

[System.Serializable]
public class FCBuffer
{
	public string _bufferName;
	public BufferInfo[] _bufferInfoList;
	[System.Serializable]
	public class BufferInfo
	{
		public FC_BUFFER_ATTRIBUTES  _bufferAtt;
		public int                   _bufferIdx;
	}
}
