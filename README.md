# EMR
EMR uses a web based architecture for designing and developing the metaverse
## Example
### View Document
```
<Root>

    <!--earth-->
    <SpaceNode  x="0" y="0" z="1500" width="500" height="500" depth="500" npcPath="sources/model/Earth/Prefab/earth"></SpaceNode>

    <SpaceNode id="moonContainer" x="0" y="0" z="1500" width="800" height="800" depth="800" >
        <!--moon-->
        <SpaceNode  x="0" y="0" z="-400" width="60" height="60" depth="60" npcPath="sources/model/Moon/Prefab/moon"></SpaceNode>

        <!--track-->
        <PanelRoot width="800" height="800" xAngle="90" lightIntensity="1" borderWidth="1" borderRadius="200" hoverColor="0,0,255" backgroundColor="0,0,0" renderMode="additive" ></PanelRoot>
    </SpaceNode>
</Root>
```
### Script
```
// mounted life cycle
protected override void mounted()
{
    // get node
    var moonContainer = this.getNodeById<SpaceNode>("moonContainer");

    // rotating the Moon
    moonContainer.rotateTo(new RotationData(0, 360 * 3000, 0), 60 * 60 * 5, MotionCurve.Linear);
}
```
<img width="100%" src="http://mms1.baidu.com/it/u=478280279,2859243190&fm=253&app=138&f=JPEG?w=670&h=473" />
<p>
    <b>More content can be found in doc/index.html</b>
</p>

