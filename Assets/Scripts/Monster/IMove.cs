using UnityEngine;

namespace Monster
{
    /// <summary>
    /// 速度接口
    /// </summary>
    public interface IMove
    {
        void SetVelocity(Vector3 velocity);

        void MoveTo(Vector3 velocity, Transform target);
        
        void SetSpeed(float speed);
    }
}