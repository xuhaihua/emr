# EMR
EMR 全称 Easy Mixed Reality 其中 Easy 指的是快速、敏捷、容易理解，它使用类似 HTML 的方式带你进入空间开发
## Example
### 视图文档
```
<Root>

    <!--地球-->
    <SpaceNode  x="0" y="0" z="1500" width="500" height="500" depth="500" npcPath="sources/model/Earth/Prefab/earth"></SpaceNode>

    <SpaceNode id="moonContainer" x="0" y="0" z="1500" width="800" height="800" depth="800" >
        <!--月亮-->
        <SpaceNode  x="0" y="0" z="-400" width="60" height="60" depth="60" npcPath="sources/model/Moon/Prefab/moon"></SpaceNode>

        <!--轨道-->
        <PanelRoot width="800" height="800" xAngle="90" lightIntensity="1" borderWidth="1" borderRadius="200" hoverColor="0,0,255" backgroundColor="0,0,0" renderMode="additive" ></PanelRoot>
    </SpaceNode>
</Root>
```
### 脚本
```
// mounted 生命周期
protected override void mounted()
{
    // 通过Id获取空间内的指定节点
    var moonContainer = this.getNodeById<SpaceNode>("moonContainer");

    // 旋转月球
    moonContainer.rotateTo(new RotationData(0, 360 * 3000, 0), 60 * 60 * 5, MotionCurve.Linear);
}
```
<img width="100%" src="http://mms1.baidu.com/it/u=478280279,2859243190&fm=253&app=138&f=JPEG?w=670&h=473" />

