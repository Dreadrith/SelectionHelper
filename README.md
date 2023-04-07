# SelectionHelper
Unity Editor package to make selecting certain objects easier and less tedious

Download:
<a href=https://github.com/Dreadrith/SelectionHelper/releases/download/v1.2.2/SelectionHelper_v1.2.2.unitypackage>Unity Package</a> - 
<a href=https://github.com/Dreadrith/SelectionHelper/releases/download/v1.2.2/SelectionHelper_v1.2.2.zip>Zip (VPM Compatible)</a>

SelectionHelper:
----------------
- Go to a Component on an object and Right Click > [SH] Choose Type. Right Click on a GameObject, Selection Helper > By Type > Children, Parents, or Filter Current Selection.
- Adds Selection Helper > Select Immediate Children, to GameObjects, to Select the next level of Children of currently selected GameObjects

![Type Object Selector](https://raw.githubusercontent.com/Dreadrith/SelectionHelper/main/com.dreadscripts.selectionhelper/media~/TOS.gif)

SceneObjectSelector:
--------------------
Adds a small button to the top right of the Scene view. When clicked and enabled, will show a resizable sphere on each enabled object. This is useful for quickly selecting armature bones or managing empty objects.
Right click the new button to open its settings window. Automatically ignores Dynamic Bones by default.

![Scene Object Selector](https://raw.githubusercontent.com/Dreadrith/SelectionHelper/main/com.dreadscripts.selectionhelper/media~/SOS.gif)

SaveSelection:
--------------
Adds Save\Load to Selection Helper, allows you to Save what objects were selected, and load them when needed. Loading combines current selection with loaded selection.
