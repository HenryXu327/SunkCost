using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
    public class Connector : MonoBehaviour
    {
        public Vector2 size = new Vector2(4, 4);
        public bool isConnected = false;

        private void OnDrawGizmos()
        {
            Gizmos.color = isConnected ? Color.green : Color.red;
            Vector2 halfSize = size / 2;
            Vector3 center = transform.position + transform.up * halfSize.y;
            Vector3 rightUp = center + transform.right * halfSize.x + transform.up * halfSize.y;
            Vector3 leftUp = center - transform.right * halfSize.x + transform.up * halfSize.y;
            Vector3 rightDown = center + transform.right * halfSize.x - transform.up * halfSize.y;
            Vector3 leftDown = center - transform.right * halfSize.x - transform.up * halfSize.y;
            Gizmos.DrawLine(rightUp, leftUp);
            Gizmos.DrawLine(leftUp, leftDown);
            Gizmos.DrawLine(leftDown, rightDown);
            Gizmos.DrawLine(rightDown, rightUp);

            float factor = 0.7f;
            Gizmos.color = Color.green * factor;
            Gizmos.DrawLine(rightUp, center);
            Gizmos.DrawLine(leftUp, center);
            Gizmos.DrawLine(leftDown, center);
            Gizmos.DrawLine(rightDown, center);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(center, center + transform.forward); ;
        }
    }
}