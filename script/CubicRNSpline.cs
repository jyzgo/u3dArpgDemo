using UnityEngine;
using System.Collections;

public class CubicRNSpline : MonoBehaviour {
	
	public string _pathName ="";
	public Color _pathColor = Color.blue;
	protected int _keyCount = 128;
	public Vector3[] _nodes;
	public float _pathScale  = 2.0f;
	public float _stepLength = 1f;
	
	public bool _reDraw = false;
	public class RNS_Node
	{
		public Vector3 position;	//Pi
		public Vector3 velocity;	//Vi
		public float distance;	//Length((P(i+1) - Pi))
		public Vector3 subP;		//(P(i+1) - Pi) / distance
	}
	
	RNS_Node[]  m_node;
	
	float[,]  m_HGx;
	float[,]  m_HGy;
	float[,]  m_HGz;

	float[]  m_totalLength; //m_totalLength[0] = 0 , always
	
	int		  m_nodeNum;
//    bool m_hasCreated;
	
	void Awake()
	{
//		m_hasCreated = false;
		//CreateLineData();
	}
	
	void CreateLineData()
	{
		_keyCount = (_nodes.Length+1)*3;
		m_node = new RNS_Node[_keyCount];
	
		m_HGx = new float[_keyCount-1,4];
		m_HGy = new float[_keyCount-1,4];
		m_HGz = new float[_keyCount-1,4];
		m_totalLength = new float[_keyCount];
		
		Vector3 posSrc, posDest;
		posSrc =  _nodes[1] - _nodes[0];
		posDest =   _nodes[_nodes.Length-1] - _nodes[_nodes.Length-2];
		float[] vA = null;
		vA = new float[(_nodes.Length-2)*3];
		for(int i =1; i< _nodes.Length-1; i++)
		{
			vA[(i-1)*3+0] = _nodes[i].x;
			vA[(i-1)*3+1] = _nodes[i].y;
			vA[(i-1)*3+2] = _nodes[i].z;
		}
		Create(vA.Length/3, vA, posSrc, posDest, _stepLength);
	}
	
	public void CreateSimplePath(Vector3 startPos, Vector3 endPos, Vector3 startSpeed, Vector3 endSpeed)
	{
		_nodes = null;
		m_node = null;
		m_HGx = null;
	    m_HGy = null;
		m_HGz = null;

		m_totalLength = null;
		_nodes = new Vector3[4];
		m_nodeNum = 0;
		_nodes[0] = startPos - startSpeed;
		_nodes[1] = startPos;
		_nodes[2] = endPos;
		_nodes[3] = endSpeed + endPos;
		//Debug.Log(endSpeed);
		ForceRereashPath();
	}

	public float GetTotalLength()
	{
		return m_totalLength[m_nodeNum-1];
	}
	public float GetTotalLength(int idx)
	{
		//Assertion.Check(idx >= 0 && idx < m_nodeNum);
		return m_totalLength[idx];
	}
	public int  GetNodeNum()
	{
		return m_nodeNum;
	}
	
	public void ForceRereashPath()
	{
		CreateLineData();
	}
	
	// 
	// @Parameters:
	// @numNode  the number of nodes
	// @route    the node coodinates: N0x,N0y,N0z, N1x,N1y, N1z, ... , Nnx,Nny, Nnz. (n = numNode)
	// @pStartV  the director of velocity for N0. (unit vector)
	// @pEndV    the director of velocity for Nn. (unit vector)
	// 
	public void Create(int numNode, float[] route,Vector3 pStartV, Vector3 pEndV, float maxNodeLength)
	{
		//Assertion.Check(numNode <= MAX_NODE_NUM && numNode >=2);
		m_nodeNum = numNode;
		float[] p = route;
		int i;
		int routeIdx = 0;
		m_totalLength[0] = 0;
		for (i=0; i<_keyCount; i++)
		{
			m_node[i] = new RNS_Node();
		}
		for (i=0; i<m_nodeNum; i++)
		{
			
			m_node[i].position.x = p[routeIdx++];
			m_node[i].position.y = p[routeIdx++];
			m_node[i].position.z = p[routeIdx++];
			if (i > 0 && 
				m_node[i].position.x == m_node[i-1].position.x  && 
				m_node[i].position.y == m_node[i-1].position.y  &&
				m_node[i].position.z == m_node[i-1].position.z
				)
			{
				if (m_nodeNum > 2)
				{
					m_nodeNum --;
					//				//Assertion.Check(m_nodeNum >= 2);
					i--;
					continue;
	
				}
				else
				{
					m_node[i].position.x += 0.1f;
				}
			}
	
			if (i > 0)
			{
				m_node[i-1].subP = m_node[i].position - m_node[i-1].position;
	
				m_node[i-1].distance = m_node[i-1].subP.magnitude;
				if (m_node[i-1].distance == 0)
				{
					m_node[i-1].distance = (0.1f);
				}
				// 			//Assertion.Check(m_node[i-1].distance > 0);
				m_node[i-1].subP =m_node[i-1].subP / m_node[i-1].distance;
				m_totalLength[i] = m_totalLength[i-1] + m_node[i-1].distance;
	
				if (maxNodeLength > 0 && m_node[i-1].distance > maxNodeLength )
				{
					//insert new node
					float curDist = m_node[i-1].distance;
					Vector3 curDir =  m_node[i-1].subP;
					int j = (int)(curDist / maxNodeLength + 1);
					for (int k=0; k<j; k++)
					{
						m_node[i + k -1].distance = curDist / j;
						m_node[i + k].position = m_node[i-1].position + curDir * ((((float)(k+1))/j)*curDist) ;
						m_node[i + k -1].subP = curDir;
						m_totalLength[i+k] = m_totalLength[i+k-1] + m_node[i+k-1].distance;
					}
					i += j -1;
					m_nodeNum += j -1;
					//Assertion.Check(m_nodeNum <= MAX_NODE_NUM);
				}
	
	
			}
		}
		if (pStartV != Vector3.zero)
		{
			m_node[0].velocity = pStartV;
		}
		else
		{
			m_node[0].velocity = m_node[0].subP;
		}
		if (pEndV != Vector3.zero)
		{
			m_node[m_nodeNum-1].velocity = pEndV;
		}
		else
		{
			m_node[m_nodeNum-1].velocity = m_node[m_nodeNum - 2].subP;
		}
	
		m_node[0].velocity.x *= _pathScale;
		m_node[0].velocity.y *= _pathScale;
		m_node[0].velocity.z *= _pathScale;
		m_node[m_nodeNum-1].velocity.x *= _pathScale;
		m_node[m_nodeNum-1].velocity.y *= _pathScale;
		m_node[m_nodeNum-1].velocity.z *= _pathScale;
	
		for (i=1; i<m_nodeNum-1; i++)
		{
			Vector3 tmp = m_node[i-1].subP + m_node[i].subP;
			m_node[i].velocity = tmp.normalized;
		}
	
		for (i = 0; i< m_nodeNum -1; i++)
		{
			Vector3 v1 = m_node[i].velocity * m_node[i].distance;
			Vector3 v2 =m_node[i+1].velocity * m_node[i].distance;
			CreateBy2Node(m_node[i].position,m_node[i+1].position, 
				v1,v2, m_HGx , m_HGy, m_HGz, i);
		}
	
//		m_hasCreated = true;
	
	}
	
	
	//p(t) = tHG = [t^3  t^2  t 1] HG
	//     | 2  -2   1   1 |
	//     |-3   3  -2  -1 |
	//  H= | 0   0   1   0 |
	//     | 1   0   0   0 |
	//
	//  G =[ P0  P1  V0  V1]T
	void CreateBy2Node(Vector3 P0,Vector3 P1,Vector3 V0,Vector3 V1, float[,] HGx,  float[,] HGy,  float[,] HGz,int arrayIdx)
	{
		HGx[arrayIdx, 0] =  2 * P0.x - 2 * P1.x +     V0.x  + V1.x;
		HGx[arrayIdx, 1] = -3 * P0.x + 3 * P1.x - 2 * V0.x  - V1.x;
		HGx[arrayIdx, 2] =                            V0.x;
		HGx[arrayIdx, 3] =      P0.x;
	
		HGy[arrayIdx, 0] =  2 * P0.y - 2 * P1.y +     V0.y  + V1.y;
		HGy[arrayIdx, 1] = -3 * P0.y + 3 * P1.y - 2 * V0.y  - V1.y;
		HGy[arrayIdx, 2] =                            V0.y;
		HGy[arrayIdx, 3] =      P0.y;
	
		HGz[arrayIdx, 0] =  2 * P0.z - 2 * P1.z +     V0.z  + V1.z;
		HGz[arrayIdx, 1] = -3 * P0.z + 3 * P1.z - 2 * V0.z  - V1.z;
		HGz[arrayIdx, 2] =                            V0.z;
		HGz[arrayIdx, 3] =      P0.z;
	}
	
	
	//  t  [0, GLT_FIXED_ONE]
	public float GetP(float t,float[,] HG, int segIndex)
	{
		//Assertion.Check(m_hasCreated);
		//Assertion.Check(t >=0 && t <= 1.1f);
		return (((((HG[segIndex, 0]*t) + HG[segIndex, 1] )* t) + HG[segIndex, 2])* t) + HG[segIndex, 3];
	}
	
	public  float GetV(float t,float[,] HG, int segIndex)
	{
		//Assertion.Check(m_hasCreated);
		//Assertion.Check(t >=0 && t <= 1.1f);
		return (((HG[segIndex, 0]* t * 3) + HG[segIndex, 1] * 2)* t) + HG[segIndex, 2];
	
	}
	
	
	public void GetPosByTotalTime(float t, ref Vector3 pos,ref Vector3 Vel)  //[0, 1.0f]
	{
		if(t>1)
		{
			t = 1;
		}
		float fGepTime = 1.0f / (m_nodeNum-1);
		int nGapIndex = (int)(t / fGepTime);
		float fStartTime = (t - nGapIndex*fGepTime) * (m_nodeNum-1);
		GetPos(fStartTime, nGapIndex, ref pos.x, ref pos.y, ref pos.z);
		GetVel(fStartTime, nGapIndex, ref Vel.x, ref Vel.y, ref Vel.z);
		
	}
	
	public void GetPosByTotalTime(float t, ref Vector3 pos)  //[0, 1.0f]
	{
		if(t>1)
		{
			t = 1;
		}
		float fGepTime = 1.0f / (m_nodeNum-1);
		int nGapIndex = (int)(t / fGepTime);
		float fStartTime = (t - nGapIndex*fGepTime) * (m_nodeNum-1);
		GetPos(fStartTime, nGapIndex, ref pos.x, ref pos.y, ref pos.z);
		
	}
	
	//  t  [0, GLT_FIXED_ONE]
	public void GetPos(float t, int segIndex,ref float outX,ref float outY,ref float outZ)
	{
		//Assertion.Check(segIndex >=0 && segIndex < m_nodeNum -1);
		outX = GetP(t, m_HGx, segIndex);
		outY = GetP(t, m_HGy, segIndex);
		outZ = GetP(t, m_HGz, segIndex);
	}
	
	
	public void GetVel(float t, int segIndex, ref Vector3 v)
	{
		GetVel(t, segIndex, ref v.x, ref v.y, ref v.z);
	}
	void GetVel(float t, int segIndex, ref float outX, ref float outY, ref float outZ)
	{
		//Assertion.Check(segIndex >=0 && segIndex < m_nodeNum -1);
		outX = GetV(t, m_HGx, segIndex);
		outY = GetV(t, m_HGy, segIndex);
		outZ = GetV(t, m_HGz, segIndex);
	}
	
	
	
	//  l  [0, totalLength]
	void GetPosAndVelByLength(float l, ref float outX, ref float outY, ref float outZ, ref float outVx, ref float outVy, ref float outVz)
	{
		//Assertion.Check(l >=0 && l <= GetTotalLength());
		int i=1;
		while (m_totalLength[i] <  l)
		{
			i++;
		}
		float t = l - m_totalLength[i-1];
		t =(t/m_node[i-1].distance);
		if(t <0 )t = 0.0f;
		if(t > 1.0f)t = 1.0f;
		GetPos(t, i-1, ref outX, ref outY, ref outZ);
		GetVel(t, i-1, ref outVx, ref outVy, ref outVz);
	}
	

	void OnDrawGizmosSelected()
	{
		
		if(_nodes != null && _nodes.Length >=4)
		{
			Vector3 posSrc, posDest;
			Gizmos.color= _pathColor;
			posSrc = transform.position;
			posDest = Vector3.zero;
			
			int i;
	
			float t = 0; 
			const int SEGMENT = 100;
			float dt = 1.0f / SEGMENT;
			Gizmos.DrawLine(_nodes[0],_nodes[1]);
			Gizmos.DrawLine(_nodes[_nodes.Length-2], _nodes[_nodes.Length-1]);
			for (i=0; i< SEGMENT-1; i++)
			{
				GetPosByTotalTime(t, ref posSrc);
				t+=dt;
				GetPosByTotalTime(t, ref posDest);
				Gizmos.DrawLine(posSrc,posDest);
			}
		}
		
	}
}
