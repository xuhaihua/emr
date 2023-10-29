/*
 * 本文件主要用于定义视图中常用的属性，以使用户在开发中直接可以使用而无需对这些常用属性进行定义
 */

using UnityEngine;

namespace EMR
{
    public partial class Component
    {
        private bool _isPanel = true;
        public bool isPanel
        {
            get
            {
                return this.getProperty<bool>(nameof(isPanel), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._isPanel = (bool)resultValue;
                    }
                    return this._isPanel;
                });
            }

            set
            {
                this.setProperty(nameof(isPanel), value, (resultValue) =>
                {
                    this._isPanel = (bool)resultValue;
                });
            }
        }

        private string _renderMode = "";
        /// <summary>
        /// 渲染模式
        /// </summary>
        public string renderMode
        {
            get
            {
                return this.getProperty<string>(nameof(renderMode), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._renderMode = (string)resultValue;
                    }
                    return this._renderMode;
                });
            }

            set
            {
                this.setProperty(nameof(renderMode), value, (resultValue) =>
                {
                    this._renderMode = (string)resultValue;
                });
            }
        }

        private string _npcPath = "";
        /// <summary>
        /// npc
        /// </summary>
        public string npcPath
        {
            get
            {
                return this.getProperty<string>(nameof(npcPath), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._npcPath = (string)resultValue;
                    }
                    return this._npcPath;
                });
            }

            set
            {
                this.setProperty(nameof(npcPath), value, (resultValue) =>
                {
                    this._npcPath = (string)resultValue;
                });
            }
        }

        private Vector3 _npcOffset;
        /// <summary>
        /// 节点偏移量
        /// </summary>
        public Vector3 npcOffset
        {
            get
            {
                return this.getProperty<Vector3>(nameof(npcOffset), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._npcOffset = (Vector3)resultValue;
                    }
                    return this._npcOffset;
                });
            }

            set
            {
                this.setProperty(nameof(npcOffset), value, (resultValue) =>
                {
                    this._npcOffset = (Vector3)resultValue;
                });
            }
        }
        /*---------------------------------------------------定义常规属性结束---------------------------------------------------*/


        /*-------------------------------------------------定义尺寸位置属性开始-------------------------------------------------*/
        private Vector3 _offset;
        /// <summary>
        /// 节点偏移量
        /// </summary>
        public Vector3 offset
        {
            get
            {
                return this.getProperty<Vector3>(nameof(offset), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._offset = (Vector3)resultValue;
                    }
                    return this._offset;
                });
            }

            set
            {
                this.setProperty(nameof(offset), value, (resultValue) =>
                {
                    this._offset = (Vector3)resultValue;
                });
            }
        }

        private int _zIndex = 0;
        /// <summary>
        /// 层叠次序
        /// </summary>
        public int zIndex
        {
            get
            {
                return this.getProperty<int>(nameof(zIndex), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._zIndex = (int)resultValue;
                    }
                    return this._zIndex;
                });
            }

            set
            {
                this.setProperty(nameof(zIndex), value, (resultValue) =>
                {
                    this._zIndex = (int)resultValue;
                });
            }
        }

        private float _x = 0f;
        /// <summary>
        /// x坐标
        /// </summary>
        public float x
        {
            get
            {
                return this.getProperty<float>(nameof(x), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._x = (float)resultValue;
                    }
                    return this._x;
                });
            }

            set
            {
                this.setProperty(nameof(x), value, (resultValue) =>
                {
                    this._x = (float)resultValue;
                });
            }
        }

        private float _y = 0f;
        /// <summary>
        /// y坐标
        /// </summary>
        public float y
        {
            get
            {
                return this.getProperty<float>(nameof(y), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._y = (float)resultValue;
                    }
                    return this._y;
                });
            }

            set
            {
                this.setProperty(nameof(y), value, (resultValue) =>
                {
                    this._y = (float)resultValue;
                });
            }
        }

        private float _z = 0f;
        /// <summary>
        /// z坐标
        /// </summary>
        public float z
        {
            get
            {
                return this.getProperty<float>(nameof(z), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._z = (float)resultValue;
                    }
                    return this._z;
                });
            }

            set
            {
                this.setProperty(nameof(z), value, (resultValue) =>
                {
                    this._z = (float)resultValue;
                });
            }
        }

        /*
         *  以下属性与节点位置、尺寸相关
         */
        private bool _xFixed = false;
        /// <summary>
        /// x坐标是否固定
        /// </summary>
        public bool xFixed
        {
            get
            {
                return this.getProperty<bool>(nameof(xFixed), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._xFixed = (bool)resultValue;
                    }
                    return this._xFixed;
                });
            }

            set
            {
                this.setProperty(nameof(xFixed), value, (resultValue) =>
                {
                    this._xFixed = (bool)resultValue;
                });
            }
        }

        private bool _yFixed = false;
        /// <summary>
        /// y坐标是否固定
        /// </summary>
        public bool yFixed
        {
            get
            {
                return this.getProperty<bool>(nameof(yFixed), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._yFixed = (bool)resultValue;
                    }
                    return this._yFixed;
                });
            }

            set
            {
                this.setProperty(nameof(yFixed), value, (resultValue) =>
                {
                    this._yFixed = (bool)resultValue;
                });
            }
        }

        private bool _zFixed = false;
        /// <summary>
        /// z坐标是否固定
        /// </summary>
        public bool zFixed
        {
            get
            {
                return this.getProperty<bool>(nameof(zFixed), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._zFixed = (bool)resultValue;
                    }
                    return this._zFixed;
                });
            }

            set
            {
                this.setProperty(nameof(zFixed), value, (resultValue) =>
                {
                    this._zFixed = (bool)resultValue;
                });
            }
        }

        private bool _leftFixed = false;
        /// <summary>
        /// left是否固定
        /// </summary>
        public bool leftFixed
        {
            get
            {
                return this.getProperty<bool>(nameof(leftFixed), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._leftFixed = (bool)resultValue;
                    }
                    return this._leftFixed;
                });
            }

            set
            {
                this.setProperty(nameof(leftFixed), value, (resultValue) =>
                {
                    this._leftFixed = (bool)resultValue;
                });
            }
        }

        private float? _left = null;
        /// <summary>
        /// left
        /// </summary>
        public float? left
        {
            get
            {
                return this.getProperty<float?>(nameof(left), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._left = (float?)resultValue;
                    }
                    return this._left;
                });
            }

            set
            {
                this.setProperty(nameof(left), value, (resultValue) =>
                {
                    this._left = (float?)resultValue;
                });
            }
        }

        private bool _topFixed = false;
        /// <summary>
        /// top是否固定
        /// </summary>
        public bool topFixed
        {
            get
            {
                return this.getProperty<bool>(nameof(topFixed), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._topFixed = (bool)resultValue;
                    }
                    return this._topFixed;
                });
            }

            set
            {
                this.setProperty(nameof(topFixed), value, (resultValue) =>
                {
                    this._topFixed = (bool)resultValue;
                });
            }
        }

        private float? _top = null;
        /// <summary>
        /// top
        /// </summary>
        public float? top
        {
            get
            {
                return this.getProperty<float?>(nameof(top), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._top = (float?)resultValue;
                    }
                    return this._top;
                });
            }

            set
            {
                this.setProperty(nameof(top), value, (resultValue) =>
                {
                    this._top = (float?)resultValue;
                });
            }
        }

        private float? _right = null;
        /// <summary>
        /// right
        /// </summary>
        public float? right
        {
            get
            {
                return this.getProperty<float?>(nameof(right), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._right = (float?)resultValue;
                    }
                    return this._right;
                });
            }

            set
            {
                this.setProperty(nameof(right), value, (resultValue) =>
                {
                    this._right = (float?)resultValue;
                });
            }
        }

        private float? _bottom = null;
        /// <summary>
        /// bottom
        /// </summary>
        public float? bottom
        {
            get
            {
                return this.getProperty<float?>(nameof(bottom), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._bottom = (float?)resultValue;
                    }
                    return this._bottom;
                });
            }

            set
            {
                this.setProperty(nameof(bottom), value, (resultValue) =>
                {
                    this._bottom = (float?)resultValue;
                });
            }
        }

        private bool _xAngleFixed = false;
        /// <summary>
        /// xAngle是否固定
        /// </summary>
        public bool xAngleFixed
        {
            get
            {
                return this.getProperty<bool>(nameof(xAngleFixed), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._xAngleFixed = (bool)resultValue;
                    }
                    return this._xAngleFixed;
                });
            }

            set
            {
                this.setProperty(nameof(xAngleFixed), value, (resultValue) =>
                {
                    this._xAngleFixed = (bool)resultValue;
                });
            }
        }

        private float _xAngle = 0f;
        /// <summary>
        /// x轴旋转量
        /// </summary>
        public float xAngle
        {
            get
            {
                return this.getProperty<float>(nameof(xAngle), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._xAngle = (float)resultValue;
                    }
                    return this._xAngle;
                });
            }

            set
            {
                this.setProperty(nameof(xAngle), value, (resultValue) =>
                {
                    this._xAngle = (float)resultValue;
                });
            }
        }

        private bool _yAngleFixed = false;
        /// <summary>
        /// yAngle是否固定
        /// </summary>
        public bool yAngleFixed
        {
            get
            {
                return this.getProperty<bool>(nameof(yAngleFixed), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._yAngleFixed = (bool)resultValue;
                    }
                    return this._yAngleFixed;
                });
            }

            set
            {
                this.setProperty(nameof(yAngleFixed), value, (resultValue) =>
                {
                    this._yAngleFixed = (bool)resultValue;
                });
            }
        }

        private float _yAngle = 0f;
        /// <summary>
        /// y轴旋转量
        /// </summary>
        public float yAngle
        {
            get
            {
                return this.getProperty<float>(nameof(yAngle), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._yAngle = (float)resultValue;
                    }
                    return this._yAngle;
                });
            }

            set
            {
                this.setProperty(nameof(yAngle), value, (resultValue) =>
                {
                    this._yAngle = (float)resultValue;
                });
            }
        }

        private bool _zAngleFixed = false;
        /// <summary>
        /// zAngle是否固定
        /// </summary>
        public bool zAngleFixed
        {
            get
            {
                return this.getProperty<bool>(nameof(zAngleFixed), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._zAngleFixed = (bool)resultValue;
                    }
                    return this._zAngleFixed;
                });
            }

            set
            {
                this.setProperty(nameof(zAngleFixed), value, (resultValue) =>
                {
                    this._zAngleFixed = (bool)resultValue;
                });
            }
        }

        private bool _depthFixed = false;
        /// <summary>
        /// 深度是否固定
        /// </summary>
        public bool depthFixed
        {
            get
            {
                return this.getProperty<bool>(nameof(depthFixed), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._depthFixed = (bool)resultValue;
                    }
                    return this._depthFixed;
                });
            }

            set
            {
                this.setProperty(nameof(depthFixed), value, (resultValue) =>
                {
                    this._depthFixed = (bool)resultValue;
                });
            }
        }


        private float _zAngle = 0f;
        /// <summary>
        /// z轴旋转量
        /// </summary>
        public float zAngle
        {
            get
            {
                return this.getProperty<float>(nameof(zAngle), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._zAngle = (float)resultValue;
                    }
                    return this._zAngle;
                });
            }

            set
            {
                this.setProperty(nameof(zAngle), value, (resultValue) =>
                {
                    this._zAngle = (float)resultValue;
                });
            }
        }

        private bool _widthFixed = false;
        /// <summary>
        /// width是否固定
        /// </summary>
        public bool widthFixed
        {
            get
            {
                return this.getProperty<bool>(nameof(widthFixed), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._widthFixed = (bool)resultValue;
                    }
                    return this._widthFixed;
                });
            }

            set
            {
                this.setProperty(nameof(widthFixed), value, (resultValue) =>
                {
                    this._widthFixed = (bool)resultValue;
                });
            }
        }

        private float _width = 0;
        /// <summary>
        /// 宽度
        /// </summary>
        public float width
        {
            get
            {
                return this.getProperty<float>(nameof(width), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._width = (float)resultValue;
                    }
                    return this._width;
                });
            }

            set
            {
                this.setProperty(nameof(width), value, (resultValue) =>
                {
                    this._width = (float)resultValue;
                });
            }
        }

        private bool _heightFixed = false;
        /// <summary>
        /// height是否固定
        /// </summary>
        public bool heightFixed
        {
            get
            {
                return this.getProperty<bool>(nameof(heightFixed), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._heightFixed = (bool)resultValue;
                    }
                    return this._heightFixed;
                });
            }

            set
            {
                this.setProperty(nameof(heightFixed), value, (resultValue) =>
                {
                    this._heightFixed = (bool)resultValue;
                });
            }
        }

        private float _height = 0;
        /// <summary>
        /// 高度
        /// </summary>
        public float height
        {
            get
            {
                return this.getProperty<float>(nameof(height), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._height = (float)resultValue;
                    }
                    return this._height;
                });
            }

            set
            {
                this.setProperty(nameof(height), value, (resultValue) =>
                {
                    this._height = (float)resultValue;
                });
            }
        }

        private bool _panelDepthFixed = true;
        /// <summary>
        /// 平面节点深度是否固定
        /// </summary>
        public bool panelDepthFixed
        {
            get
            {
                return this.getProperty<bool>(nameof(panelDepthFixed), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._panelDepthFixed = (bool)resultValue;
                    }
                    return this._panelDepthFixed;
                });
            }

            set
            {
                this.setProperty(nameof(panelDepthFixed), value, (resultValue) =>
                {
                    this._panelDepthFixed = (bool)resultValue;
                });
            }
        }

        private bool _spaceDepthFixed = false;
        /// <summary>
        /// 空间节点深度是否固定
        /// </summary>
        public bool spaceDepthFixed
        {
            get
            {
                return this.getProperty<bool>(nameof(spaceDepthFixed), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._spaceDepthFixed = (bool)resultValue;
                    }
                    return this._spaceDepthFixed;
                });
            }

            set
            {
                this.setProperty(nameof(spaceDepthFixed), value, (resultValue) =>
                {
                    this._spaceDepthFixed = (bool)resultValue;
                });
            }
        }

        private float _depth = 0f;
        /// <summary>
        /// 深度
        /// </summary>
        public float depth
        {
            get
            {
                return this.getProperty<float>(nameof(depth), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._depth = (float)resultValue;
                    }
                    return this._depth;
                });
            }

            set
            {
                this.setProperty(nameof(depth), value, (resultValue) =>
                {
                    this._depth = (float)resultValue;
                });
            }
        }

        private string _scrollDirection = "upAndDown";
        /// <summary>
        /// 滚动方向 upAndDown | leftAndRight | both
        /// </summary>
        public string scrollDirection
        {
            get
            {
                return this.getProperty<string>(nameof(scrollDirection), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._scrollDirection = (string)resultValue;
                    }
                    return this._scrollDirection;
                });
            }

            set
            {
                this.setProperty(nameof(scrollDirection), value, (resultValue) =>
                {
                    this._scrollDirection = (string)resultValue;
                });
            }
        }
        /*-------------------------------------------------定义尺寸位置属性结束-------------------------------------------------*/


        /*---------------------------------------------------定义样式属性开始---------------------------------------------------*/
        private string _overflow = "visible";
        /// <summary>
        /// overflow
        /// </summary>
        public string overflow
        {
            get
            {
                return this.getProperty<string>(nameof(overflow), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._overflow = (string)resultValue;
                    }
                    return this._overflow;
                });
            }

            set
            {
                this.setProperty(nameof(overflow), value, (resultValue) =>
                {
                    this._overflow = (string)resultValue;
                });
            }
        }

        private float _fontSize = 14f;
        /// <summary>
        /// 文本大小
        /// </summary>
        public float fontSize
        {

            get
            {
                return this.getProperty<float>(nameof(fontSize), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._fontSize = (float)resultValue;
                    }
                    return this._fontSize;
                });
            }

            set
            {
                this.setProperty(nameof(fontSize), value, (resultValue) =>
                {
                    this._fontSize = (float)resultValue;
                });
            }
        }

        private float _borderWidth = 0f;
        /// <summary>
        /// 边框宽度
        /// </summary>
        public float borderWidth
        {
            get
            {
                return this.getProperty<float>(nameof(borderWidth), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._borderWidth = (float)resultValue;
                    }
                    return this._borderWidth;
                });
            }

            set
            {
                this.setProperty(nameof(borderWidth), value, (resultValue) =>
                {
                    this._borderWidth = (float)resultValue;
                });
            }
        }

        private float _borderRadius = 0f;
        /// <summary>
        /// 边框宽度 边框半径
        /// </summary>
        public float borderRadius
        {
            get
            {
                return this.getProperty<float>(nameof(borderRadius), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._borderRadius = (float)resultValue;
                    }
                    return this._borderRadius;
                });
            }

            set
            {
                this.setProperty(nameof(borderRadius), value, (resultValue) =>
                {
                    this._borderRadius = (float)resultValue;
                });
            }
        }

        private string _color = "255,255,255";
        /// <summary>
        /// 颜色
        /// </summary>
        public string color
        {
            get
            {
                return this.getProperty<string>(nameof(color), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._color = (string)resultValue;
                    }
                    return this._color;
                });
            }

            set
            {
                this.setProperty(nameof(color), value, (resultValue) =>
                {
                    this._color = (string)resultValue;
                });
            }
        }

        private string _backgroundImage = "";
        /// <summary>
        /// 背景
        /// </summary>
        public string backgroundImage
        {
            get
            {
                return this.getProperty<string>(nameof(backgroundImage), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._backgroundImage = (string)resultValue;
                    }
                    return this._backgroundImage;
                });
            }

            set
            {
                this.setProperty(nameof(backgroundImage), value, (resultValue) =>
                {
                    this._backgroundImage = (string)resultValue;
                });
            }
        }

        private string _backgroundColor = "";
        /// <summary>
        /// 背景色
        /// </summary>
        public string backgroundColor
        {
            get
            {
                return this.getProperty<string>(nameof(backgroundColor), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._backgroundColor = (string)resultValue;
                    }
                    return this._backgroundColor;
                });
            }

            set
            {
                this.setProperty(nameof(backgroundColor), value, (resultValue) =>
                {
                    this._backgroundColor = (string)resultValue;
                });
            }
        }

        private float _lightIntensity = 1f;
        /// <summary>
        /// 灯光强度
        /// </summary>
        public float lightIntensity
        {
            get
            {
                return this.getProperty<float>(nameof(lightIntensity), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._lightIntensity = (float)resultValue;
                    }
                    return this._lightIntensity;
                });
            }

            set
            {
                this.setProperty(nameof(lightIntensity), value, (resultValue) =>
                {
                    this._lightIntensity = (float)resultValue;
                });
            }
        }

        private string _hoverColor = "255,255,255";
        /// <summary>
        /// 是浮灯颜色
        /// </summary>
        public string hoverColor
        {
            get
            {
                return this.getProperty<string>(nameof(hoverColor), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._hoverColor = (string)resultValue;
                    }
                    return this._hoverColor;
                });
            }

            set
            {
                this.setProperty(nameof(hoverColor), value, (resultValue) =>
                {
                    this._hoverColor = (string)resultValue;
                });
            }
        }

        private string _nearLightCenterColor = "";
        /// <summary>
        /// 设置接近光center颜色
        /// </summary>
        public string nearLightCenterColor
        {
            get
            {
                return this.getProperty<string>(nameof(nearLightCenterColor), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._nearLightCenterColor = (string)resultValue;
                    }
                    return this._nearLightCenterColor;
                });
            }

            set
            {
                this.setProperty(nameof(nearLightCenterColor), value, (resultValue) =>
                {
                    this._nearLightCenterColor = (string)resultValue;
                });
            }
        }

        private string _nearLightMiddleColor = "";
        /// <summary>
        /// 设置接近光middle颜色
        /// </summary>
        public string nearLightMiddleColor
        {
            get
            {
                return this.getProperty<string>(nameof(nearLightMiddleColor), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._nearLightMiddleColor = (string)resultValue;
                    }
                    return this._nearLightMiddleColor;
                });
            }

            set
            {
                this.setProperty(nameof(nearLightMiddleColor), value, (resultValue) =>
                {
                    this._nearLightMiddleColor = (string)resultValue;
                });
            }
        }

        /// <summary>
        /// 设置接近光outer颜色
        /// </summary>
        private string _nearLightOuterColor = "";
        /// <summary>
        /// 是浮灯颜色
        /// </summary>
        public string nearLightOuterColor
        {
            get
            {
                return this.getProperty<string>(nameof(nearLightOuterColor), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._nearLightOuterColor = (string)resultValue;
                    }
                    return this._nearLightOuterColor;
                });
            }

            set
            {
                this.setProperty(nameof(nearLightOuterColor), value, (resultValue) =>
                {
                    this._nearLightOuterColor = (string)resultValue;
                });
            }
        }
        /*---------------------------------------------------定义样式属性结束---------------------------------------------------*/


        /*---------------------------------------------------定义布局属性开始---------------------------------------------------*/
        private float _characterSpace = 0f;
        /// <summary>
        /// 字符间距
        /// </summary>
        public float characterSpace
        {
            get
            {
                return this.getProperty<float>(nameof(characterSpace), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._characterSpace = (float)resultValue;
                    }
                    return this._characterSpace;
                });
            }

            set
            {
                this.setProperty(nameof(characterSpace), value, (resultValue) =>
                {
                    this._characterSpace = (float)resultValue;
                });
            }
        }

        private float _lineSpace = 0f;
        /// <summary>
        /// 行间距
        /// </summary>
        public float lineSpace
        {
            get
            {
                return this.getProperty<float>(nameof(lineSpace), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._lineSpace = (float)resultValue;
                    }
                    return this._lineSpace;
                });
            }

            set
            {
                this.setProperty(nameof(lineSpace), value, (resultValue) =>
                {
                    this._lineSpace = (float)resultValue;
                });
            }
        }

        private float _paragraphSpace = 0f;
        /// <summary>
        /// 段落间距
        /// </summary>
        public float paragraphSpace
        {
            get
            {
                return this.getProperty<float>(nameof(paragraphSpace), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._paragraphSpace = (float)resultValue;
                    }
                    return this._paragraphSpace;
                });
            }

            set
            {
                this.setProperty(nameof(paragraphSpace), value, (resultValue) =>
                {
                    this._paragraphSpace = (float)resultValue;
                });
            }
        }

        private float _wordSpace = 0f;
        /// <summary>
        /// 字间距
        /// </summary>
        public float wordSpace
        {
            get
            {
                return this.getProperty<float>(nameof(wordSpace), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._wordSpace = (float)resultValue;
                    }
                    return this._wordSpace;
                });
            }

            set
            {
                this.setProperty(nameof(wordSpace), value, (resultValue) =>
                {
                    this._wordSpace = (float)resultValue;
                });
            }
        }

        private string _textHorizontal = "left";
        /// <summary>
        /// 文本水平对齐
        /// </summary>
        public string textHorizontal
        {
            get
            {
                return this.getProperty<string>(nameof(textHorizontal), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._textHorizontal = (string)resultValue;
                    }
                    return this._textHorizontal;
                });
            }

            set
            {
                this.setProperty(nameof(textHorizontal), value, (resultValue) =>
                {
                    this._textHorizontal = (string)resultValue;
                });
            }
        }

        private string _textVertical = "top";
        /// <summary>
        /// 文本垂直对齐
        /// </summary>
        public string textVertical
        {
            get
            {
                return this.getProperty<string>(nameof(textVertical), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._textVertical = (string)resultValue;
                    }
                    return this._textVertical;
                });
            }

            set
            {
                this.setProperty(nameof(textVertical), value, (resultValue) =>
                {
                    this._textVertical = (string)resultValue;
                });
            }
        }

        private string _horizontal = "none";
        /// <summary>
        /// 自身水平对齐(浮动)方式 none | center | left | right
        /// </summary>
        public string horizontal
        {
            get
            {
                return this.getProperty<string>(nameof(horizontal), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._horizontal = (string)resultValue;
                    }
                    return this._horizontal;
                });
            }

            set
            {
                this.setProperty(nameof(horizontal), value, (resultValue) =>
                {
                    this._horizontal = (string)resultValue;
                });
            }
        }

        private string _horizontalFloat = "none";
        /// <summary>
        /// 自身水平对齐(浮动)方式 none | center | left | right
        /// </summary>
        public string horizontalFloat
        {
            get
            {
                return this.getProperty<string>(nameof(horizontalFloat), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._horizontalFloat = (string)resultValue;
                    }
                    return this._horizontalFloat;
                });
            }

            set
            {
                this.setProperty(nameof(horizontalFloat), value, (resultValue) =>
                {
                    this._horizontalFloat = (string)resultValue;
                });
            }
        }

        private string _vertical = "none";
        /// <summary>
        /// 自身垂直对齐(浮动)方式
        /// </summary>
        public string vertical
        {
            get
            {
                return this.getProperty<string>(nameof(vertical), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._vertical = (string)resultValue;
                    }
                    return this._vertical;
                });
            }

            set
            {
                this.setProperty(nameof(vertical), value, (resultValue) =>
                {
                    this._vertical = (string)resultValue;
                });
            }
        }

        private string _verticalFloat = "none";
        /// <summary>
        /// 自身垂直对齐(浮动)方式 none | center | top | bottom
        /// </summary>
        public string verticalFloat
        {
            get
            {
                return this.getProperty<string>(nameof(verticalFloat), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._verticalFloat = (string)resultValue;
                    }
                    return this._verticalFloat;
                });
            }

            set
            {
                this.setProperty(nameof(verticalFloat), value, (resultValue) =>
                {
                    this._verticalFloat = (string)resultValue;
                });
            }
        }

        private string _forward = "none";
        /// <summary>
        /// 自身z轴对齐(浮动)方式
        /// </summary>
        public string forward
        {
            get
            {
                return this.getProperty<string>(nameof(forward), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._forward = (string)resultValue;
                    }
                    return this._forward;
                });
            }

            set
            {
                this.setProperty(nameof(forward), value, (resultValue) =>
                {
                    this._forward = (string)resultValue;
                });
            }
        }

        private string _forwardFloat = "none";
        /// <summary>
        /// 自身z轴对齐(浮动)方式 none | center | top | bottom
        /// </summary>
        public string forwardFloat
        {
            get
            {
                return this.getProperty<string>(nameof(forwardFloat), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._forwardFloat = (string)resultValue;
                    }
                    return this._forwardFloat;
                });
            }

            set
            {
                this.setProperty(nameof(forwardFloat), value, (resultValue) =>
                {
                    this._forwardFloat = (string)resultValue;
                });
            }
        }

        private float _horizontalInterval = 0f;
        /// <summary>
        /// 内容水平对齐间距
        /// </summary>
        public float horizontalInterval
        {
            get
            {
                return this.getProperty<float>(nameof(horizontalInterval), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._horizontalInterval = (float)resultValue;
                    }
                    return this._horizontalInterval;
                });
            }

            set
            {
                this.setProperty(nameof(horizontalInterval), value, (resultValue) =>
                {
                    this._horizontalInterval = (float)resultValue;
                });
            }
        }

        private bool _horizontalLeftInterval = false;
        /// <summary>
        /// 水平对齐最后一个子节点是否包含右间距
        /// </summary>
        public bool horizontalLeftInterval
        {
            get
            {
                return this.getProperty<bool>(nameof(horizontalLeftInterval), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._horizontalLeftInterval = (bool)resultValue;
                    }
                    return this._horizontalLeftInterval;
                });
            }

            set
            {
                this.setProperty(nameof(horizontalLeftInterval), value, (resultValue) =>
                {
                    this._horizontalLeftInterval = (bool)resultValue;
                });
            }
        }

        private bool _horizontalRightInterval = false;
        /// <summary>
        /// 水平对齐最后一个子节点是否包含右间距
        /// </summary>
        public bool horizontalRightInterval
        {
            get
            {
                return this.getProperty<bool>(nameof(horizontalRightInterval), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._horizontalRightInterval = (bool)resultValue;
                    }
                    return this._horizontalRightInterval;
                });
            }

            set
            {
                this.setProperty(nameof(horizontalRightInterval), value, (resultValue) =>
                {
                    this._horizontalRightInterval = (bool)resultValue;
                });
            }
        }

        private string _contentHorizontal = "none";
        /// <summary>
        /// 内容水平对齐方式(只针对子节点,文本节点的对齐不在内）none | left | right | center | between
        /// </summary>
        public string contentHorizontal
        {
            get
            {
                return this.getProperty<string>(nameof(contentHorizontal), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._contentHorizontal = (string)resultValue;
                    }
                    return this._contentHorizontal;
                });
            }

            set
            {
                this.setProperty(nameof(contentHorizontal), value, (resultValue) =>
                {
                    this._contentHorizontal = (string)resultValue;
                });
            }
        }

        private float _verticalInterval = 0f;
        /// <summary>
        /// 内容垂直对齐间距
        /// </summary>
        public float verticalInterval
        {
            get
            {
                return this.getProperty<float>(nameof(verticalInterval), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._verticalInterval = (float)resultValue;
                    }
                    return this._verticalInterval;
                });
            }

            set
            {
                this.setProperty(nameof(verticalInterval), value, (resultValue) =>
                {
                    this._verticalInterval = (float)resultValue;
                });
            }
        }

        private bool _verticalTopInterval = false;
        /// <summary>
        /// 垂直对对齐第一个子节点是否包含右间距
        /// </summary>
        public bool verticalTopInterval
        {
            get
            {
                return this.getProperty<bool>(nameof(verticalTopInterval), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._verticalTopInterval = (bool)resultValue;
                    }
                    return this._verticalTopInterval;
                });
            }

            set
            {
                this.setProperty(nameof(verticalTopInterval), value, (resultValue) =>
                {
                    this._verticalTopInterval = (bool)resultValue;
                });
            }
        }

        private bool _verticalBottomInterval = false;
        /// <summary>
        /// 垂直对对齐最后一个子节点是否包含右间距
        /// </summary>
        public bool verticalBottomInterval
        {
            get
            {
                return this.getProperty<bool>(nameof(verticalBottomInterval), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._verticalBottomInterval = (bool)resultValue;
                    }
                    return this._verticalBottomInterval;
                });
            }

            set
            {
                this.setProperty(nameof(verticalBottomInterval), value, (resultValue) =>
                {
                    this._verticalBottomInterval = (bool)resultValue;
                });
            }
        }

        private string _contentVertical = "none";
        /// <summary>
        /// 内容垂直对齐方式(只针对子节点,文本节点的对齐不在内）none | top | bottom | center | between
        /// </summary>
        public string contentVertical
        {
            get
            {
                return this.getProperty<string>(nameof(contentVertical), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._contentVertical = (string)resultValue;
                    }
                    return this._contentVertical;
                });
            }

            set
            {
                this.setProperty(nameof(contentVertical), value, (resultValue) =>
                {
                    this._contentVertical = (string)resultValue;
                });
            }
        }

        private float _forwardInterval = 0f;
        /// <summary>
        /// 内容深度对齐间距
        /// </summary>
        public float forwardInterval
        {
            get
            {
                return this.getProperty<float>(nameof(forwardInterval), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._forwardInterval = (float)resultValue;
                    }
                    return this._forwardInterval;
                });
            }

            set
            {
                this.setProperty(nameof(forwardInterval), value, (resultValue) =>
                {
                    this._forwardInterval = (float)resultValue;
                });
            }
        }

        private bool _forwardBackInterval = false;
        /// <summary>
        /// 深度对齐第一个子节点是否包含右间距
        /// </summary>
        public bool forwardBackInterval
        {
            get
            {
                return this.getProperty<bool>(nameof(forwardBackInterval), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._forwardBackInterval = (bool)resultValue;
                    }
                    return this._forwardBackInterval;
                });
            }

            set
            {
                this.setProperty(nameof(forwardBackInterval), value, (resultValue) =>
                {
                    this._forwardBackInterval = (bool)resultValue;
                });
            }
        }

        private bool _forwardFrontInterval = false;
        /// <summary>
        /// 深度对齐最后一个子节点是否包含右间距
        /// </summary>
        public bool forwardFrontInterval
        {
            get
            {
                return this.getProperty<bool>(nameof(forwardFrontInterval), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._forwardFrontInterval = (bool)resultValue;
                    }
                    return this._forwardFrontInterval;
                });
            }

            set
            {
                this.setProperty(nameof(forwardFrontInterval), value, (resultValue) =>
                {
                    this._forwardFrontInterval = (bool)resultValue;
                });
            }
        }

        private string _contentForward = "none";
        /// <summary>
        /// 内容z轴对齐方式(只针对子节点,文本节点的对齐不在内）none | forward | back | center | between
        /// </summary>
        public string contentForward
        {
            get
            {
                return this.getProperty<string>(nameof(contentForward), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._contentForward = (string)resultValue;
                    }
                    return this._contentForward;
                });
            }

            set
            {
                this.setProperty(nameof(contentForward), value, (resultValue) =>
                {
                    this._contentForward = (string)resultValue;
                });
            }
        }

        private float _bendAngle = 0f;
        /// <summary>
        /// 空间弯曲
        /// </summary>
        public float bendAngle
        {
            get
            {
                return this.getProperty<float>(nameof(bendAngle), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._bendAngle = (float)resultValue;
                    }
                    return this._bendAngle;
                });
            }

            set
            {
                this.setProperty(nameof(bendAngle), value, (resultValue) =>
                {
                    this._bendAngle = (float)resultValue;
                });
            }
        }
        /*---------------------------------------------------定义布局属性结束---------------------------------------------------*/

        /*---------------------------------------------------定义刚体属性开始---------------------------------------------------*/
        public float? _mass;
        /// <summary>
        /// 刚体质量
        /// </summary>
        public float? mass
        {
            get
            {
                return this.getProperty<float?>(nameof(mass), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._mass = (float?)resultValue;
                    }
                    return this._mass;
                });
            }

            set
            {
                this.setProperty(nameof(mass), value, (resultValue) =>
                {
                    this._mass = (float?)resultValue;
                });
            }
        }

        public float? _drag;
        /// <summary>
        /// 刚体阻力
        /// </summary>
        public float? drag
        {
            get
            {
                return this.getProperty<float?>(nameof(drag), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._drag = (float?)resultValue;
                    }
                    return this._drag;
                });
            }

            set
            {
                this.setProperty(nameof(drag), value, (resultValue) =>
                {
                    this._drag = (float?)resultValue;
                });
            }
        }

        public float? _angularDrag;
        /// <summary>
        /// 刚体角阻力
        /// </summary>
        public float? angularDrag
        {
            get
            {
                return this.getProperty<float?>(nameof(angularDrag), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._angularDrag = (float?)resultValue;
                    }
                    return this._angularDrag;
                });
            }

            set
            {
                this.setProperty(nameof(angularDrag), value, (resultValue) =>
                {
                    this._angularDrag = (float?)resultValue;
                });
            }
        }

        public bool _useGravity;
        /// <summary>
        /// 是否使用重力
        /// </summary>
        public bool useGravity
        {
            get
            {
                return this.getProperty<bool>(nameof(useGravity), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._useGravity = (bool)resultValue;
                    }
                    return this._useGravity;
                });
            }

            set
            {
                this.setProperty(nameof(useGravity), value, (resultValue) =>
                {
                    this._useGravity = (bool)resultValue;
                });
            }
        }

        public bool _isKinematic;
        /// <summary>
        /// 是否使用运动学
        /// </summary>
        public bool isKinematic
        {
            get
            {
                return this.getProperty<bool>(nameof(isKinematic), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._isKinematic = (bool)resultValue;
                    }
                    return this._isKinematic;
                });
            }

            set
            {
                this.setProperty(nameof(isKinematic), value, (resultValue) =>
                {
                    this._isKinematic = (bool)resultValue;
                });
            }
        }

        public bool _rigidbodyFreezeAll;
        /// <summary>
        /// 刚体FreezeAll
        /// </summary>
        public bool rigidbodyFreezeAll
        {
            get
            {
                return this.getProperty<bool>(nameof(rigidbodyFreezeAll), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._rigidbodyFreezeAll = (bool)resultValue;
                    }
                    return this._rigidbodyFreezeAll;
                });
            }

            set
            {
                this.setProperty(nameof(rigidbodyFreezeAll), value, (resultValue) =>
                {
                    this._rigidbodyFreezeAll = (bool)resultValue;
                });
            }
        }

        public bool _rigidbodyFreezePosition;
        /// <summary>
        /// 刚体FreezePosition
        /// </summary>
        public bool rigidbodyFreezePosition
        {
            get
            {
                return this.getProperty<bool>(nameof(rigidbodyFreezePosition), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._rigidbodyFreezePosition = (bool)resultValue;
                    }
                    return this._rigidbodyFreezePosition;
                });
            }

            set
            {
                this.setProperty(nameof(rigidbodyFreezePosition), value, (resultValue) =>
                {
                    this._rigidbodyFreezePosition = (bool)resultValue;
                });
            }
        }

        public bool _rigidbodyFreezePositionX;
        /// <summary>
        /// 刚体FreezePositionX
        /// </summary>
        public bool rigidbodyFreezePositionX
        {
            get
            {
                return this.getProperty<bool>(nameof(rigidbodyFreezePositionX), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._rigidbodyFreezePositionX = (bool)resultValue;
                    }
                    return this._rigidbodyFreezePositionX;
                });
            }

            set
            {
                this.setProperty(nameof(rigidbodyFreezePositionX), value, (resultValue) =>
                {
                    this._rigidbodyFreezePositionX = (bool)resultValue;
                });
            }
        }

        public bool _rigidbodyFreezePositionY;
        /// <summary>
        /// 刚体FreezePositionY
        /// </summary>
        public bool rigidbodyFreezePositionY
        {
            get
            {
                return this.getProperty<bool>(nameof(rigidbodyFreezePositionY), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._rigidbodyFreezePositionY = (bool)resultValue;
                    }
                    return this._rigidbodyFreezePositionY;
                });
            }

            set
            {
                this.setProperty(nameof(rigidbodyFreezePositionY), value, (resultValue) =>
                {
                    this._rigidbodyFreezePositionY = (bool)resultValue;
                });
            }
        }

        public bool _rigidbodyFreezePositionZ;
        /// <summary>
        /// 刚体FreezePositionZ
        /// </summary>
        public bool rigidbodyFreezePositionZ
        {
            get
            {
                return this.getProperty<bool>(nameof(rigidbodyFreezePositionZ), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._rigidbodyFreezePositionZ = (bool)resultValue;
                    }
                    return this._rigidbodyFreezePositionZ;
                });
            }

            set
            {
                this.setProperty(nameof(rigidbodyFreezePositionZ), value, (resultValue) =>
                {
                    this._rigidbodyFreezePositionZ = (bool)resultValue;
                });
            }
        }

        public bool _rigidbodyFreezeRotation;
        /// <summary>
        /// 刚体FreezeRotation
        /// </summary>
        public bool rigidbodyFreezeRotation
        {
            get
            {
                return this.getProperty<bool>(nameof(rigidbodyFreezeRotation), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._rigidbodyFreezeRotation = (bool)resultValue;
                    }
                    return this._rigidbodyFreezeRotation;
                });
            }

            set
            {
                this.setProperty(nameof(rigidbodyFreezeRotation), value, (resultValue) =>
                {
                    this._rigidbodyFreezeRotation = (bool)resultValue;
                });
            }
        }

        public bool _rigidbodyFreezeRotationX;
        /// <summary>
        /// 刚体FreezeRotationX
        /// </summary>
        public bool rigidbodyFreezeRotationX
        {
            get
            {
                return this.getProperty<bool>(nameof(rigidbodyFreezeRotationX), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._rigidbodyFreezeRotationX = (bool)resultValue;
                    }
                    return this._rigidbodyFreezeRotationX;
                });
            }

            set
            {
                this.setProperty(nameof(rigidbodyFreezeRotationX), value, (resultValue) =>
                {
                    this._rigidbodyFreezeRotationX = (bool)resultValue;
                });
            }
        }

        public bool _rigidbodyFreezeRotationY;
        /// <summary>
        /// 刚体FreezeRotationY
        /// </summary>
        public bool rigidbodyFreezeRotationY
        {
            get
            {
                return this.getProperty<bool>(nameof(rigidbodyFreezeRotationY), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._rigidbodyFreezeRotationY = (bool)resultValue;
                    }
                    return this._rigidbodyFreezeRotationY;
                });
            }

            set
            {
                this.setProperty(nameof(rigidbodyFreezeRotationY), value, (resultValue) =>
                {
                    this._rigidbodyFreezeRotationY = (bool)resultValue;
                });
            }
        }

        public bool _rigidbodyFreezeRotationZ;
        /// <summary>
        /// 刚体FreezeRotationZ
        /// </summary>
        public bool rigidbodyFreezeRotationZ
        {
            get
            {
                return this.getProperty<bool>(nameof(rigidbodyFreezeRotationZ), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._rigidbodyFreezeRotationZ = (bool)resultValue;
                    }
                    return this._rigidbodyFreezeRotationZ;
                });
            }

            set
            {
                this.setProperty(nameof(rigidbodyFreezeRotationZ), value, (resultValue) =>
                {
                    this._rigidbodyFreezeRotationZ = (bool)resultValue;
                });
            }
        }
        /*---------------------------------------------------定义刚体属性结束---------------------------------------------------*/


        /*---------------------------------------------------定义绞链属性开始---------------------------------------------------*/
        public Vector3? _hingeJoint = null;
        /// <summary>
        /// 关节绞链
        /// </summary>
        public Vector3? hingeJoint
        {
            get
            {
                return this.getProperty<Vector3?>(nameof(hingeJoint), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._hingeJoint = (Vector3?)resultValue;
                    }
                    return this._hingeJoint;
                });
            }

            set
            {
                this.setProperty(nameof(hingeJoint), value, (resultValue) =>
                {
                    this._hingeJoint = (Vector3?)resultValue;
                });
            }
        }

        public Vector3? _hingeJointAxis = null;
        /// <summary>
        /// 关节绞链轴
        /// </summary>
        public Vector3? hingeJointAxis
        {
            get
            {
                return this.getProperty<Vector3?>(nameof(hingeJointAxis), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._hingeJointAxis = (Vector3?)resultValue;
                    }
                    return this._hingeJointAxis;
                });
            }

            set
            {
                this.setProperty(nameof(hingeJointAxis), value, (resultValue) =>
                {
                    this._hingeJointAxis = (Vector3?)resultValue;
                });
            }
        }

        public Vector3? _hingeJointConnected = null;
        /// <summary>
        /// 关节绞链连接对象
        /// </summary>
        public Vector3? hingeJointConnected
        {
            get
            {
                return this.getProperty<Vector3?>(nameof(hingeJointConnected), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._hingeJointConnected = (Vector3?)resultValue;
                    }
                    return this._hingeJointConnected;
                });
            }

            set
            {
                this.setProperty(nameof(hingeJointConnected), value, (resultValue) =>
                {
                    this._hingeJointConnected = (Vector3?)resultValue;
                });
            }
        }
        /*---------------------------------------------------定义绞链属性结束---------------------------------------------------*/


        /*---------------------------------------------------定义功能属性开始---------------------------------------------------*/
        private float _handleSize = 0.016f;
        /// <summary>
        /// 尺寸控制柄尺寸
        /// </summary>
        public float handleSize
        {
            get
            {
                return this.getProperty<float>(nameof(handleSize), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._handleSize = (float)resultValue;
                    }
                    return this._handleSize;
                });
            }

            set
            {
                this.setProperty(nameof(handleSize), value, (resultValue) =>
                {
                    this._handleSize = (float)resultValue;
                });
            }
        }

        private float _sizeBoxPadding = 0f;
        /// <summary>
        /// 尺寸控制柄尺寸
        /// </summary>
        public float sizeBoxPadding
        {
            get
            {
                return this.getProperty<float>(nameof(sizeBoxPadding), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._sizeBoxPadding = (float)resultValue;
                    }
                    return this._sizeBoxPadding;
                });
            }

            set
            {
                this.setProperty(nameof(sizeBoxPadding), value, (resultValue) =>
                {
                    this._sizeBoxPadding = (float)resultValue;
                });
            }
        }

        private bool _enableSize = false;
        /// <summary>
        /// 是否启用尺寸改变组件
        /// </summary>
        public bool enableSize
        {
            get
            {
                return this.getProperty<bool>(nameof(enableSize), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._enableSize = (bool)resultValue;
                    }
                    return this._enableSize;
                });
            }

            set
            {
                this.setProperty(nameof(enableSize), value, (resultValue) =>
                {
                    this._enableSize = (bool)resultValue;
                });
            }
        }

        private bool _enableManipulator = false;
        /// <summary>
        /// 是否启用对象操纵组件
        /// </summary>
        public bool enableManipulator
        {
            get
            {
                return this.getProperty<bool>(nameof(enableManipulator), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._enableManipulator = (bool)resultValue;
                    }
                    return this._enableManipulator;
                });
            }

            set
            {
                this.setProperty(nameof(enableManipulator), value, (resultValue) =>
                {
                    this._enableManipulator = (bool)resultValue;
                });
            }
        }

        private bool _overclip = false;
        /// <summary>
        /// 是否启用对象裁切 (空间节点对象)
        /// </summary>
        public bool overclip
        {
            get
            {
                return this.getProperty<bool>(nameof(overclip), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._overclip = (bool)resultValue;
                    }
                    return this._overclip;
                });
            }

            set
            {
                this.setProperty(nameof(overclip), value, (resultValue) =>
                {
                    this._overclip = (bool)resultValue;
                });
            }
        }

        private bool _collider = true;
        /// <summary>
        /// 事件中的Collider组件是否自动添加和回收
        /// </summary>
        public bool collider
        {
            get
            {
                return this.getProperty<bool>(nameof(collider), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._collider = (bool)resultValue;
                    }
                    return this._collider;
                });
            }

            set
            {
                this.setProperty(nameof(collider), value, (resultValue) =>
                {
                    this._collider = (bool)resultValue;
                });
            }
        }

        private bool _interactionTouchableAuto = false;
        /// <summary>
        /// 触摸事件需要的InteractionTouchable组件是否自动添加和回收
        /// </summary>
        public bool interactionTouchableAuto
        {
            get
            {
                return this.getProperty<bool>(nameof(interactionTouchableAuto), (resultValue, isSuccess) =>
                {
                    if (isSuccess)
                    {
                        this._interactionTouchableAuto = (bool)resultValue;
                    }
                    return this._interactionTouchableAuto;
                });
            }

            set
            {
                this.setProperty(nameof(interactionTouchableAuto), value, (resultValue) =>
                {
                    this._interactionTouchableAuto = (bool)resultValue;
                });
            }
        }
        /*---------------------------------------------------定义功能属性结束---------------------------------------------------*/
    }
}