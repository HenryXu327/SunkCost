using System;
using UnityEngine;

namespace Dungeon
{
    [Serializable]
    public class Room
    {
        public Transform tile;
        public Transform preRoom;
        public Connector connector;

        public Room(Transform tile, Transform preRoom)
        {
            this.tile = tile;
            this.preRoom = preRoom;
        }
    }
}