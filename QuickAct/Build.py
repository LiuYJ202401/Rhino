# coding: utf-8
import rhinoscriptsyntax as rs
import scriptcontext as sc
import numpy as np 
import math

def Build():
    z=np.arange(0,10,1)
    x=np.sin(z)
    y=np.cos(z)
    line=rs.AddInterpCurve([(xx,yy,zz)for xx,yy,zz in zip(x,y,z)])
    circle = rs.AddCircle((0,0,0), 5)
    cylinder = rs.ExtrudeCurve(circle, line)

def CreateSteps(width,stepnum,stepdepth,stepheight):
    """从宽度、步数、步深、步高创建楼梯"""
    floorHeight=stepheight
    lines=[rs.AddLine((0,i*stepdepth,i*stepheight),(0,i*stepdepth,(i+1)*stepheight))for i in range(stepnum)]+[rs.AddLine((0,i*stepdepth,(i+1)*stepheight),(0,(i+1)*stepdepth,(i+1)*stepheight))for i in range(stepnum)]
    lines+=[rs.AddPolyline([(0,(stepnum)*stepdepth,(stepnum)*stepheight),(0,(stepnum)*stepdepth,(stepnum)*stepheight-floorHeight),(0,stepdepth,0),(0,0,0)])]
    step=rs.JoinCurves(lines)
    rs.DeleteObjects(lines)
    cemian=rs.AddPlanarSrf(step)
    lt=rs.ExtrudeSurface(cemian,rs.AddLine((0,0,0),(width,0,0)))
    return lt