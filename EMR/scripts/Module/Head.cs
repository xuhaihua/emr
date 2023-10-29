using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EMR.Module
{
    public class Head
    {
        private readonly UnityEngine.Camera _camera = UnityEngine.Camera.main;

        /// <summary>
        /// x坐标
        /// </summary>
        public float x
        {
            get
            {
                return _camera.transform.position.x;
            }
        }

        /// <summary>
        /// y坐标
        /// </summary>
        public float y
        {
            get
            {
                return _camera.transform.position.y;
            }
        }

        /// <summary>
        /// z坐标
        /// </summary>
        public float z
        {
            get
            {
                return _camera.transform.position.z;
            }
        }


        /// <summary>
        /// x轴旋转量
        /// </summary>
        public float xAngle
        {
            get
            {
                return _camera.transform.eulerAngles.x;
            }
        }

        /// <summary>
        /// y轴旋转量
        /// </summary>
        public float yAngle
        {
            get
            {
                return _camera.transform.eulerAngles.y;
            }
        }

        /// <summary>
        /// z轴旋转量
        /// </summary>
        public float zAngle
        {
            get
            {
                return _camera.transform.eulerAngles.z;
            }
        }
    }
}
