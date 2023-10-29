using System.Collections.Generic;
using UnityEngine;
using EMR.Entity;

namespace EMR.Layout
{
    public class SpaceBend : ILayout
    {
        private SpaceNode _node;

        /// <summary>
        /// 曲度(弧度)
        /// </summary>
        private float _bendAngle = 0f;

        /// <summary>
        /// 曲度(角度)
        /// </summary>
        public float bendAngle
        {
            get
            {
                return this._bendAngle * (180 / Mathf.PI);
            }

            set
            {
                this._bendAngle = value *( Mathf.PI / 180 );
            }
        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="axle">作用轴</param>
        /// <param name="interval">间距</param>
        /// <param name="mode">对齐方式</param>
        /// <param name="isStartInvterval">第一个节点是否包含左间距</param>
        /// <param name="isEndInvterval">最后一个节点是否包含右间距</param>
        public SpaceBend(SpaceNode node)
        {
            this._node = node;
        }

        public void fresh()
        {
            // 阻止系统编辑阶段进入（因为这个功能行为与节点的坐标位置直接有关，在编译时它有可能有两处触发该方法的位置一处是在初始化节点设置基本属性时另一处是在布局调整阶段）
            if (this._node.component.isAssembling)
            {
                return;
            }

            // 获取空间节点列表
            List<ISpaceCharacteristic> list = new List<ISpaceCharacteristic>();
            foreach (var item in this._node.children)
            {
                if (item is ISpaceCharacteristic && item.horizontalFloat == "none" && item.verticalFloat == "none" && (item is SpaceNode && ((SpaceNode)item).forwardFloat == "none" || item is PanelRoot && ((PanelRoot)item).forwardFloat == "none" || item is SpaceMagic && ((SpaceMagic)item).forwardFloat == "none"))
                {
                    list.Add((ISpaceCharacteristic)item);
                }
            }

            // 计算弧半径
            var arcRadius = this._node.width / this._bendAngle;

            // 中心轴长度
            var axisLength = this._node.width;

            foreach (ISpaceCharacteristic item in list)
            {
                // 计算节点所在弧半径
                var radius = arcRadius + (item.z + item.offset.z);

                // 弧度不变计算弧长度
                var length = Mathf.Abs(this._bendAngle * radius);

                // 计算旋转角
                int sign = 0 - (item.x + item.offset.x) > 0 ? -1 : 1;
                var pointLength = Mathf.Abs((item.x + item.offset.x) * (length / axisLength));
                var angle = sign * (pointLength / (length / 2)) * (this._bendAngle / 2);

                var x = radius * Mathf.Sin(angle);
                var z = radius * Mathf.Cos(angle) - radius;

                item.x = x - item.offset.x;
                item.z = z - item.offset.z;

                item.yAngle = angle * (180 / Mathf.PI);
            }
        }
    }
}
