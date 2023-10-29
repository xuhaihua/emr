using UnityEngine;
using EMR.Struct;

public delegate void MovementCallback();

public class Movement
{
    // 当前时间
    private float currentTime = 0f;

    // 开始量
    private Vector3 start = new Vector3(0, 0, 0);

    // 结束量
    private Vector3 end = new Vector3(0, 0, 0);

    // 曲线
    private ICurve curve;

    // 总时长
    private float time = 0f;

    private Vector3 _data = new Vector3(0, 0, 0);

    public bool _isFinish = false;

    private MovementCallback callback;

    public Movement(Vector3 start, Vector3 end, float time, MotionCurve curveType, MovementCallback callback = null)
    {
        // 相关参数设定
        this.start = start;
        this.end = end;
        this.time = time;
        this.curve = BuiltInCurve.GetCurve(curveType);
        this.callback = callback;
    }

    public bool isFinish
    {
        get
        {
            return this._isFinish;
        }
    }

    public Vector3 data
    {
        get
        {
            // 计算当前时间
            this.currentTime += Time.deltaTime;

            // 计算当前时间对应curveValueList中的索引
            var timeScale = this.currentTime / time;

            if(timeScale <= 1)
            {
                var scale = this.curve.Evaluate(timeScale);

                var startX = this.start.x;
                var startY = this.start.y;
                var startZ = this.start.z;

                var endX = this.end.x;
                var endY = this.end.y;
                var endZ = this.end.z;

                var xData = startX + scale * (endX - startX);
                var yData = startY + scale * (endY - startY);
                var zData = startZ + scale * (endZ - startZ);

                this._data = new Vector3(xData, yData, zData);
            } else
            {
                this._data = this.end;

                if(this.callback != null && !this.isFinish)
                {
                    this.callback();
                }

                this._isFinish = true;
            }

            return this._data;
            
        }
    }
}
