using UnityEngine;
using System.Collections;
using System;

public class LootCollider : MonoBehaviour
{
    private LootMove _move = null;
    public LootMove Move
    {
        set { _move = value; }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == FCConst.LAYER_GROUND)
        {
            _move.StartPickup();
        }else if(collision.gameObject.layer == FCConst.LAYER_WALL
			||collision.gameObject.layer == FCConst.LAYER_WALL_AIR)
		{
			_move.StopHorizontalMove();
		}
    }
}
