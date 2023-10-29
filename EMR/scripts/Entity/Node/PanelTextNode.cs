using UnityEngine;
using EMR.Common;
using EMR.Struct;
using TMPro;

namespace EMR.Entity
{
    public class PanelTextNode
    {
        /// <summary>
        /// 所在的平面节点
        /// </summary>
        private PanelNode _node;

        /// <summary>
        /// 表演者对象(一个空节点文本组件的容器)
        /// </summary>
        public GameObject parasitifer;

        /// <summary>
        /// 文本组件
        /// </summary>
        private TMPro.TextMeshPro textComponent;

        /// <summary>
        /// 文本组件
        /// </summary>
        /// <param name="node">文本所关联的平面节点</param>
        public PanelTextNode(PanelNode node = null)
        {
            this.parasitifer = new GameObject();
            this.parasitifer.transform.name = "text object";
            this.textComponent = this.parasitifer.AddComponent<TMPro.TextMeshPro>();
            

            EMR.Space.mainService.next(() =>
            {
                this.textComponent.font = Resources.Load<TMP_FontAsset>("Fonts/msyhl");
                //this.parasitifer.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/TextMeshPro") as Material;
                return true;
            });

            if (node != null)
            {
                this.node = node;
            }
        }

        #region 基本属性
        /// <summary>
        /// 所在平面节点
        /// </summary>
        public PanelNode node
        {
            get
            {
                return this._node;
            }

            set
            {
                this._node = value;

                this.parasitifer.transform.SetParent(this._node.parasitifer.transform);
                this.fresh();
            }
        }

        /// <summary>
        /// 文本
        /// </summary>
        /// <returns></returns>
        public string text
        {
            get
            {
                return this.textComponent.text;
            }

            set
            {
                this.textComponent.text = value;
                this.parasitifer.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/TextMeshPro") as Material;
            }
        }
        #endregion

        #region 文本相关样式设置方法
        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="color">文本颜色</param>
        public void setColor(Color color)
        {
            this.textComponent.color = color;
            this.parasitifer.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/TextMeshPro") as Material;
        }

        /// <summary>
        /// 设置尺寸
        /// </summary>
        /// <param name="data">文本大小</param>
        public void setSize(float data)
        {
            this.textComponent.fontSize = data * 13f / 1000f;
            this.parasitifer.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/TextMeshPro") as Material;
        }

        /// <summary>
        /// 设置水平对齐模式
        /// </summary>
        /// <param name="mode">对齐模式</param>
        public void setHorizontalAlignment(TMPro.HorizontalAlignmentOptions mode)
        {
            this.textComponent.horizontalAlignment = mode;
            this.parasitifer.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/TextMeshPro") as Material;
        }

        /// <summary>
        /// 设置垂直对齐模式
        /// </summary>
        /// <param name="mode">对齐模式</param>
        public void setVerticalAlignment(TMPro.VerticalAlignmentOptions mode)
        {
            this.textComponent.verticalAlignment = mode;
            this.parasitifer.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/TextMeshPro") as Material;
        }

        /// <summary>
        /// 设置字符间距
        /// </summary>
        /// <param name="data">间距</param>
        public void setCharacterSpace(float data)
        {
            this.textComponent.characterSpacing = data;
            this.parasitifer.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/TextMeshPro") as Material;
        }

        /// <summary>
        /// 设置行间距
        /// </summary>
        /// <param name="data">间距</param>
        public void setLineSpace(float data)
        {
            this.textComponent.lineSpacing = data;
            this.parasitifer.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/TextMeshPro") as Material;
        }

        /// <summary>
        /// 设置段落间距
        /// </summary>
        /// <param name="data">间距</param>
        public void setParagraphSpace(float data)
        {
            this.textComponent.paragraphSpacing = data;
            this.parasitifer.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/TextMeshPro") as Material;
        }

        /// <summary>
        /// 设置字间距
        /// </summary>
        /// <param name="data">间距</param>
        public void setWordSpace(float data)
        {
            this.textComponent.wordSpacing = data;
            this.parasitifer.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/TextMeshPro") as Material;
        }
        #endregion

        #region 文本节点相关功能方法
        /// <summary>
        /// 文本刷新
        /// </summary>
        public void fresh()
        {
            // 节点不在panelRoot下时不进行处理
            var panelRoot = this._node.panelRoot;
            if (panelRoot == null)
            {
                return;
            }

            this.parasitifer.transform.localScale = new Vector3(0f, 0f, 0f);
            this.parasitifer.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

            var width = Space.Unit.unitToScaleForGameObject(this.parasitifer, 1000f, Axle.right);
            var height = Space.Unit.unitToScaleForGameObject(this.parasitifer, 1000f, Axle.up);
           
            if (!Utils.equals(width, 0) && !Utils.equals(height, 0) && !float.IsInfinity(width) && !float.IsInfinity(height))
            {
                this.parasitifer.transform.localScale = new Vector3(width, height, 0.00001f);

                var rectTransform = this.parasitifer.GetComponent<RectTransform>();

                // 获取和设置本地尺寸
                var w = Space.Unit.unitToScaleForGameObject(this.parasitifer, this._node.width, Axle.right) / this.parasitifer.transform.localScale.x;
                var h = Space.Unit.unitToScaleForGameObject(this.parasitifer, this._node.height, Axle.up) / this.parasitifer.transform.localScale.y;

                if (float.IsNaN(w) && float.IsInfinity(w) || float.IsNaN(w) && float.IsInfinity(w))
                {
                    return;
                }

                var localSize = new Vector3(w, h, 0f);
                rectTransform.sizeDelta = localSize;
                
                // 获取和设置本地坐标
                rectTransform.anchoredPosition3D = new Vector3(0f, 0f, -Space.Unit.unitToScaleForGameObject(this.parasitifer, 0.01f / 2 + this._node.depth / 2, Axle.forward));

                /*
                 *  以下逻辑用于设置文本样式
                 */
                this.setColor(Utils.stringToColor(this._node.color));
                this.setSize(this._node.fontSize);

                if (this._node.textHorizontal == "left")
                {
                    this.setHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left);
                }
                if (this._node.textHorizontal == "center")
                {
                    this.setHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center);
                }
                if (this._node.textHorizontal == "flush")
                {
                    this.setHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Flush);
                }
                if (this._node.textHorizontal == "Geometry")
                {
                    this.setHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Geometry);
                }
                if (this._node.textHorizontal == "Justified")
                {
                    this.setHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Justified);
                }
                if (this._node.textHorizontal == "Right")
                {
                    this.setHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Right);
                }

                if (this._node.textVertical == "top")
                {
                    this.setVerticalAlignment(TMPro.VerticalAlignmentOptions.Top);
                }
                if (this._node.textVertical == "baseline")
                {
                    this.setVerticalAlignment(TMPro.VerticalAlignmentOptions.Baseline);
                }
                if (this._node.textVertical == "capline")
                {
                    this.setVerticalAlignment(TMPro.VerticalAlignmentOptions.Capline);
                }
                if (this._node.textVertical == "geometry")
                {
                    this.setVerticalAlignment(TMPro.VerticalAlignmentOptions.Geometry);
                }
                if (this._node.textVertical == "middle")
                {
                    this.setVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle);
                }
                if (this._node.textVertical == "bottom")
                {
                    this.setVerticalAlignment(TMPro.VerticalAlignmentOptions.Bottom);
                }

                this.setCharacterSpace(this._node.characterSpace);
                this.setLineSpace(this._node.lineSpace);
                this.setWordSpace(this._node.wordSpace);
                this.setParagraphSpace(this._node.paragraphSpace);

                // 刷新文本材质
                this.parasitifer.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/TextMeshPro") as Material;

                // 样式调整后可能会引响尺寸故在此对parent scroll节点进行视口刷新
                PanelNode scrollNode = null;
                var count = 0;
                scrollNode = this._node.getOverflow() == NodeOverflow.scroll ? this._node : PanelNode.findParentScrollNode(this._node);
                if (scrollNode != null)
                {
                    scrollNode.service.next(() =>
                    {
                        bool result = false;

                        count++;
                        if (count == 2)
                        {
                            EMR.Plugin.PanelScroll.viewFresh(scrollNode, true, false);
                            count = 0;
                            result = true;
                        }

                        return result;
                    });
                }
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void destory()
        {
            if (this._node.textNodeList.Count > 0)
            {
                this._node.textNodeList.Remove(this._node.textNodeList[0]);
            }
            GameObject.Destroy(this.parasitifer);
        }
        #endregion
    }
}
