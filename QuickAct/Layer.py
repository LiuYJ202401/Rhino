# coding: utf-8
import rhinoscriptsyntax as rs
import scriptcontext as sc
import numpy as np 
import math

def JumpTo(target="默认值"):
    """跳转至目标图层，锁定其余所有图层"""
    all_layers = rs.LayerNames()
    if not all_layers:
        print("文档中未找到任何图层。")
        return None
    if not rs.IsLayer(target):
        rs.AddLayer(target)
        print(f"已创建目标图层: '{target}'")
    
    rs.CurrentLayer(target)
    print(f"已切换到工作图层: '{target}'")
    locked_count = 0
    for layer in all_layers:
        if layer != target and not rs.LayerLocked(layer):
            rs.LayerLocked(layer, True)
            locked_count += 1
    print(f"已锁定其他 {locked_count} 个图层。")






def Unlock_All_Layers():
    """解锁模型中的所有图层"""
    all_layers = rs.LayerNames()
    unlocked_count = 0
    for layer in all_layers:
        if rs.LayerLocked(layer):
            rs.LayerLocked(layer, False)
            unlocked_count += 1
    print(f"已解锁 {unlocked_count} 个图层。")