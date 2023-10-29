using System.Collections.Generic;

namespace EMR
{
    /// <summary>
    /// 属性缓冲记录
    /// </summary>
    internal class AttributeCatchRecord
    {
        public string name = "";
        public object value = null;

        /// <summary>
        /// 1 代表来自属性、0 代表来自样式
        /// </summary>
        public int from = 0;
    }

    /// <summary>
    /// 属性缓冲
    /// </summary>
    internal class AttributeCatch
    {
        /// <summary>
        /// 节点标识类属性缓冲列表
        /// </summary>
        private List<AttributeCatchRecord> _markList = new List<AttributeCatchRecord>();

        /// <summary>
        /// 参数类
        /// </summary>
        private List<AttributeCatchRecord> _paramList = new List<AttributeCatchRecord>();

        /// <summary>
        /// 尺寸缓冲列表
        /// </summary>
        private List<AttributeCatchRecord> _sizeList = new List<AttributeCatchRecord>();

        /// <summary>
        /// 位置缓冲列表 (其中包含rotation)
        /// </summary>
        private List<AttributeCatchRecord> _positionList = new List<AttributeCatchRecord>();

        /// <summary>
        /// 节点布局属性缓冲列表
        /// </summary>
        private List<AttributeCatchRecord> _layoutList = new List<AttributeCatchRecord>();

        /// <summary>
        /// 其它
        /// </summary>
        private List<AttributeCatchRecord> _otherList = new List<AttributeCatchRecord>();

        /// <summary>
        /// 比较器
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int compareHandler(AttributeCatchRecord a, AttributeCatchRecord b)
        {
            if (a.name != b.name)
            {
                return a.name.CompareTo(b.name);
            }
            else if (a.from != b.from)
            {
                return a.from - b.from;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 节点标识类属性缓冲列表
        /// </summary>
        public List<AttributeCatchRecord> markList
        {
            get
            {
                this._markList.Sort(compareHandler);
                return this._markList;
            }
        }

        /// <summary>
        /// 参数类缓冲列表
        /// </summary>
        public List<AttributeCatchRecord> paramList
        {
            get
            {
                this._paramList.Sort(compareHandler);
                return this._paramList;
            }
        }

        /// <summary>
        /// 尺寸缓冲列表
        /// </summary>
        public List<AttributeCatchRecord> sizeList
        {
            get
            {
                this._sizeList.Sort(compareHandler);
                return this._sizeList;
            }
        }

        /// <summary>
        /// 位置缓冲列表 (其中包含rotation)
        /// </summary>
        public List<AttributeCatchRecord> positionList
        {
            get
            {
                this._positionList.Sort(compareHandler);
                return this._positionList;
            }
        }

        /// <summary>
        /// 节点布局属性缓冲列表
        /// </summary>
        public List<AttributeCatchRecord> layoutList
        {
            get
            {
                this._layoutList.Sort(compareHandler);
                return this._layoutList;
            }
        }

        /// <summary>
        /// 其它
        /// </summary>
        public List<AttributeCatchRecord> otherList
        {
            get
            {
                this._otherList.Sort(compareHandler);
                return this._otherList;
            }
        }

        private Dictionary<string, AttributeCatchRecord> indexTable = new Dictionary<string, AttributeCatchRecord>();

        /// <summary>
        /// 向缓冲添加属性
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="from"></param>
        public void add(string name, object value, int from)
        {
            List<AttributeCatchRecord> targetList = null;

            // 标识类
            if (name == "id" || name == "name" || name == "isPanel" || name == "renderMode" || name == "interactionTouchableAuto" || name == "collider")
            {
                targetList = _markList;
            }

            // 参数类(这里用于存放一些功能设备属性的前置参数，这些参数属性单独设置不会驱动具体功能，只是其它功能的参数)
            else if (name == "xFixed" || name == "yFixed" || name == "zFixed" || name == "xAngleFixed" || name == "yAngleFixed" || name == "zAngleFixed" || name == "leftFixed" || name == "topFixed" ||  name == "horizontalInterval" || name == "verticalInterval" || name == "forwardInterval" || name == "horizontalLeftInterval" || name == "horizontalRightInterval" || name == "verticalTopInterval" || name == "verticalBottomInterval" || name == "forwardBackInterval" || name == "forwardFrontInterval" || name == "handleSize" || name == "sizeBoxPadding")
            {
                targetList = _paramList;
            }

            // 尺寸类
            else if (name == "width" || name == "height" || name == "depth")
            {
                targetList = _sizeList;
            }

            // 位置类
            else if (name == "offset" || name == "zIndex" || name == "x" || name == "y" || name == "z" || name == "xAngle" || name == "yAngle" || name == "zAngle" || name == "top" || name == "left" || name == "right" || name == "bottom")
            {
                targetList = _positionList;
            }

            // 布局类
            else if (name == "horizontalFloat" || name == "verticalFloat" || name == "forwardFloat" || name == "contentHorizontal" || name == "contentVertical" || name == "contentForward" || name == "overflow")
            {
                targetList = _layoutList;
            }

            else
            {
                targetList = _otherList;
            }

            // 如果在索引表内不存在name的属性则创建并向索引表添加该属性记录
            var indexRecord = indexTable.ContainsKey(name) ? indexTable[name] : null;
            if (indexRecord == null)
            {
                var record = new AttributeCatchRecord
                {
                    name = name,
                    value = value,
                    from = from
                };
                targetList.Add(record);

                indexTable.Add(name, record);
            }

            // 如果索引表内存在该属性记录并且优先级大于或等于原记录时修改该记录
            if (indexRecord != null && indexRecord.from <= from)
            {
                indexRecord.value = value;
            }
        }

        /// <summary>
        /// 向缓冲添加属性
        /// </summary>
        /// <param name="record"></param>
        public void add(AttributeCatchRecord record)
        {
            this.add(record.name, record.value, record.from);
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void clear()
        {
            this._markList.Clear();
            this._paramList.Clear();
            this._sizeList.Clear();
            this._positionList.Clear();
            this._layoutList.Clear();
            this._otherList.Clear();
        }
    }
}
