using System.Collections.Generic;
using UnityEngine;
using EMR.Common;
using EMR.Event;
using UnityEngine.Events;

namespace EMR.Entity
{
    public class NPC
    {
        #region 基本字段
        /// <summary>
        /// 宽比
        /// </summary>
        private float npcWidthScale = 0f;

        /// <summary>
        /// 高比
        /// </summary>
        private float npcHeightScale = 0f;

        /// <summary>
        /// 深度比
        /// </summary>
        private float npcDepthScale = 0f;

        /// <summary>
        /// npc x轴偏移比例
        /// </summary>
        private float xNPCOffsetScale = 0f;

        /// <summary>
        /// npc y轴偏移比例
        /// </summary>
        private float yNPCOffsetScale = 0f;

        /// <summary>
        /// npc z轴偏移比例
        /// </summary>
        private float zNPCOffsetScale = 0f;

        /// <summary>
        /// 安全盒
        /// </summary>
        private GameObject secureBox = new GameObject();

        /// <summary>
        /// npc所在宿主
        /// </summary>
        private ISizeFeature _host;


        /// <summary>
        /// npc 游戏对象 (表演者)
        /// </summary>
        public GameObject parasitifer = null;
        #endregion

        /// <summary>
        /// npc字典
        /// </summary>
        public static Dictionary<GameObject, NPC> npcDictionary = new Dictionary<GameObject, NPC>();

        /// <summary>
        /// npc事件集合
        /// </summary>
        public Dictionary<string, NPCEvent> events = new Dictionary<string, NPCEvent>();

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="npc">npc游戏对象</param>
        /// <param name="node">npc所在节点</param>
        public NPC(GameObject npc, ISizeFeature node)
        {
            this.parasitifer = npc;
            this._host = node;

            this.parasitifer = GameObject.Instantiate(npc, null, false);

            // 计算npc单位尺寸比
            this.parasitifer.transform.localScale = new Vector3(1f, 1f, 1f);
            this.parasitifer.transform.localPosition = new Vector3(0f, 0f, 0f);

            var bounds = this.parasitifer.CalculateBounds();

            this.npcWidthScale = bounds.size.x;
            this.npcHeightScale = bounds.size.y;
            this.npcDepthScale = bounds.size.z;

            // 计算npc单位中心点偏移
            this.xNPCOffsetScale = (-bounds.center).x;
            this.yNPCOffsetScale = (-bounds.center).y;
            this.zNPCOffsetScale = (-bounds.center).z;

            // 将安全盒加入表演者之下
            this.secureBox.transform.SetParent(((Node)this._host).parasitifer.transform);
            this.secureBox.transform.localScale = new Vector3(0f, 0f, 0f);
            this.secureBox.transform.localPosition = new Vector3(0f, 0f, 0f);
            this.secureBox.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

            // 将npc加入全安盒之下
            this.parasitifer.transform.SetParent(this.secureBox.transform);
            this.parasitifer.transform.localScale = new Vector3(1f, 1f, 1f);
            this.parasitifer.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

            npcDictionary.Add(this.parasitifer, this);

            this.fresh();
        }

        #region 基本属性
        private Vector3 _offset = new Vector3(0f, 0f, 0f);
        /// <summary>
        /// 空间偏移量
        /// </summary>
        public virtual Vector3 offset
        {
            get
            {
                return this._offset;
            }

            set
            {

                this._offset = value;

                var offsetX = Space.Unit.unitToScaleForGameObject(this.secureBox, value.x, Struct.Axle.right);
                var offsetY = Space.Unit.unitToScaleForGameObject(this.secureBox, value.y, Struct.Axle.up);
                var offsetZ = Space.Unit.unitToScaleForGameObject(this.secureBox, value.z, Struct.Axle.forward);

                var x = this.secureBox.transform.localPosition.x + offsetX;
                var y = this.secureBox.transform.localPosition.y + offsetY;
                var z = this.secureBox.transform.localPosition.z + offsetZ;



                this.secureBox.transform.localPosition = new Vector3(x, y, z);
            }
        }
        #endregion

        #region 事件
        /// <summary>
        /// 注册npc事件 (unity动画与EMR通讯解偶)
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="callback">事件处理方法</param>
        public void AddListener(string name, UnityAction<NPCEventData> callback)
        {
            if (!events.ContainsKey(name))
            {
                events.Add(name, new NPCEvent());
            }

            events[name].AddListener(callback);
        }

        /// <summary>
        /// 注销事件
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="callback">事件处理方法</param>
        public void RemoveListener(string name, UnityAction<NPCEventData> callback)
        {
            if (events.ContainsKey(name) && events[name].listenerCount > 0)
            {
                events[name].RemoveListener(callback);
            }

            if(events.ContainsKey(name) && events[name].listenerCount == 0)
            {
                events.Remove(name);
            }
        }

        /// <summary>
        /// 抛出npc事件 (unity动画与EMR通讯解偶)
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="data">事件数据</param>
        public virtual void emit(string name, NPCEventData eventData)
        {
            if (events.ContainsKey(name))
            {
                eventData.target = this;
                eventData.host = (Node)this._host;

                events[name].Invoke(eventData);
            }
        }


        /// <summary>
        /// 事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="name">事件名称</param>
        /// <param name="eventData">NPCEventData对象</param>
        public static void emit(GameObject sender, string name, NPCEventData eventData)
        {
            if (npcDictionary.ContainsKey(sender))
            {
                var npc = npcDictionary[sender];
                npc.emit(name, eventData);
            }
        }
        #endregion

        #region 相关方法
        /*--------------------------------------------------定义npc行为方法开始--------------------------------------------------*/
        /// <summary>
        /// 执行npc动作
        /// </summary>
        /// <param name="name">动作名称</param>
        /// <param name="data">动作传入参数</param>
        public void action(string name, float data)
        {
            var anim = this.parasitifer.GetComponent<Animator>();
            if(anim != null)
            {
                anim.SetFloat(name, data);
            }
        }

        /// <summary>
        /// 执行npc动作
        /// </summary>
        /// <param name="name">动作名称</param>
        /// <param name="data">动作传入参数</param>
        public void action(string name, int data)
        {
            var anim = this.parasitifer.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetInteger(name, data);
            }
        }

        /// <summary>
        /// 执行npc动作
        /// </summary>
        /// <param name="name">动作名称</param>
        /// <param name="data">动作传入参数</param>
        public void action(string name, bool data)
        {
            var anim = this.parasitifer.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetBool(name, data);
            }
        }

        /// <summary>
        /// 执行npc动作
        /// </summary>
        /// <param name="name">动作名称</param>
        /// <param name="data">动作传入参数</param>
        public void action(string name)
        {
            var anim = this.parasitifer.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger(name);
            }
        }

        /// <summary>
        /// 执行npc动作
        /// </summary>
        /// <typeparam name="T">MonoBehaviour</typeparam>
        /// <param name="name">MonoBehaviour 中的方法名</param>
        /// <param name="paramList">方法的参数列表</param>
        public void action<T>(string name, object[] paramList = null) where T : MonoBehaviour
        {
            var component = this.parasitifer.GetComponent<T>();
            var methodInfo = Utils.getMethod(name, component);
            if(methodInfo != null)
            {
                methodInfo.Invoke(component, paramList);
            }
        }

        /// <summary>
        /// 执行npc动作
        /// </summary>
        /// <param name="name">动作名称</param>
        /// <param name="data">动作传入参数</param>
        public void action(int name, float data)
        {
            var anim = this.parasitifer.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetFloat(name, data);
            }
        }

        /// <summary>
        /// 执行npc动作
        /// </summary>
        /// <param name="name">动作名称</param>
        /// <param name="data">动作传入参数</param>
        public void action(int name, int data)
        {
            var anim = this.parasitifer.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetInteger(name, data);
            }
        }

        /// <summary>
        /// 执行npc动作
        /// </summary>
        /// <param name="name">动作名称</param>
        /// <param name="data">动作传入参数</param>
        public void action(int name, bool data)
        {
            var anim = this.parasitifer.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetBool(name, data);
            }
        }

        /// <summary>
        /// 执行npc动作
        /// </summary>
        /// <param name="name">动作名称</param>
        /// <param name="data">动作传入参数</param>
        public void action(int name)
        {
            var anim = this.parasitifer.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger(name);
            }
        }
        /*--------------------------------------------------定义npc行为方法结束--------------------------------------------------*/

        /*------------------------------------------------定义刷新相关支撑方法开始-----------------------------------------------*/
        /// <summary>
        /// 计算最短边尺寸
        /// </summary>
        /// <returns></returns>
        private void computeNodeMinSize(Vector3 vector, ref float num, ref int index)
        {
            float min = vector.x;
            index = 0;
            if (Utils.noExceed(vector.y, min))
            {
                min = vector.y;
                index = 1;
            }
            if (Utils.noExceed(vector.z, min))
            {
                min = vector.z;
                index = 2;
            }

            num = min;
        }

        /// <summary>
        /// 计算尺寸
        /// </summary>
        /// <returns></returns>
        private Vector3 computeSize()
        {
            float[] size = { this._host.width, this._host.height, this._host.depth };
            float[] list = { this.npcWidthScale, this.npcHeightScale, this.npcDepthScale };
            List<float[]> middleResult = new List<float[]>();
            for (var i = 0; i < list.Length; i++)
            {
                for (var j = i + 1; j < list.Length; j++)
                {
                    float[] temp = new float[] { float.NaN, float.NaN, float.NaN };
                    var value = size[j] * list[i] / list[j];
                    if (Utils.noExceed(value, list[i]))
                    {
                        temp[i] = value;
                        temp[j] = size[j];
                        middleResult.Add(temp);
                    }

                    temp = new float[] { float.NaN, float.NaN, float.NaN };
                    value = size[i] * list[j] / list[i];
                    if (Utils.noExceed(value, list[j]))
                    {
                        temp[i] = size[i];
                        temp[j] = value;
                        middleResult.Add(temp);
                    }

                    temp = new float[] { float.NaN, float.NaN, float.NaN };
                    var min = size[i];
                    if (Utils.noUnder(min, size[j]))
                    {
                        min = size[j];
                    }

                    if (Utils.noUnder(list[i], list[j]))
                    {
                        temp[i] = min;
                        temp[j] = min * list[j] / list[i];
                    }
                    else
                    {
                        temp[i] = min * list[i] / list[j];
                        temp[j] = min;
                    }
                    middleResult.Add(temp);
                }
            }

            List<float[]> resultList = new List<float[]>();
            for (var i = 0; i < middleResult.Count; i++)
            {
                var temp = middleResult[i];
                for (var j = 0; j < temp.Length; j++)
                {
                    if (float.IsNaN(temp[j]))
                    {
                        float value;
                        if (j != 0)
                        {
                            value = temp[j - 1] * list[j] / list[j - 1];
                        }
                        else
                        {
                            value = temp[j + 1] * list[j] / list[j + 1];
                        }

                        if (Utils.noExceed(value, size[j]))
                        {
                            temp[j] = value;
                            resultList.Add(temp);
                        }
                        break;
                    }
                }
            }

            var max = 0f;
            Vector3 result = new Vector3(float.NaN, float.NaN, float.NaN);
            for (var i = 0; i < resultList.Count; i++)
            {
                var temp = resultList[i];
                var value = temp[0] * temp[1] * temp[2];
                if (Utils.noExceed(max, value))
                {
                    max = value;
                    result = new Vector3(temp[0], temp[1], temp[2]);
                }
            }

            return result;
        }
        /*------------------------------------------------定义刷新相关支撑方法结束-----------------------------------------------*/

        /// <summary>
        /// 尺寸刷新
        /// </summary>
        public void fresh()
        {
            if (!Utils.equals(this._host.width, 0f) && !Utils.equals(this._host.height, 0f) && !Utils.equals(this._host.depth, 0f))
            {
                var sizeVector = this.computeSize();
                float num = 0f;
                int index = 0;
                computeNodeMinSize(sizeVector, ref num, ref index);

                float[] list = { this.npcWidthScale, this.npcHeightScale, this.npcDepthScale };

                num = num / list[index];

                secureBox.transform.localScale = new Vector3(Space.Unit.unitToScaleForGameObject(this.secureBox.gameObject, num, Struct.Axle.right), Space.Unit.unitToScaleForGameObject(this.secureBox.gameObject, num, Struct.Axle.up), Space.Unit.unitToScaleForGameObject(this.secureBox.gameObject, num, Struct.Axle.forward));

                var x = Space.Unit.unitToScaleForGameObject(((Node)this._host).parasitifer, this.xNPCOffsetScale / this.npcWidthScale * sizeVector.x, Struct.Axle.right) / ((Node)this._host).parasitifer.transform.localScale.x;
                var y = Space.Unit.unitToScaleForGameObject(((Node)this._host).parasitifer, this.yNPCOffsetScale / this.npcHeightScale * sizeVector.y, Struct.Axle.up) / ((Node)this._host).parasitifer.transform.localScale.y;
                var z = Space.Unit.unitToScaleForGameObject(((Node)this._host).parasitifer, this.zNPCOffsetScale / this.npcDepthScale * sizeVector.z, Struct.Axle.up) / ((Node)this._host).parasitifer.transform.localScale.z;

                if (!float.IsNaN(x) && !float.IsInfinity(x) && !float.IsNaN(y) && !float.IsInfinity(y) && !float.IsNaN(z) && !float.IsInfinity(z))
                {
                    this.secureBox.transform.localPosition = new Vector3(x, y, z);

                    this.offset = this._offset;
                }
            }
        }

        public void destory()
        {
            GameObject.Destroy(this.secureBox);
        }
        #endregion
    }
}

