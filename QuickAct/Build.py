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