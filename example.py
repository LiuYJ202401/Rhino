# coding: utf-8
import rhinoscriptsyntax as rs
import scriptcontext as sc
import numpy as np 
import math

# 得到线
points=[(0,0,0),(1000,0,0),(1000,1000,0),(900,1000,0),(900,100,0),(0,100,0),(0,0,0)]
l=rs.AddPolyline(points)# 多点生成封闭线段
c=rs.AddCurve(points)

path = rs.AddCurve([(0, 0, 0), (5, 5, 5), (10, 0, 10)])# 创建一条空间曲线作为路径

#得到面
m0=rs.ExtrudeCurvePoint(rs.AddCircle((0,0,0),100),(0,0,100))# 从曲线挤出到一点形成锥型面
m1=rs.ExtrudeCurve(c, path)# 沿路径从曲线挤出面
m2=rs.ExtrudeCurveStraight(l,(0,0,0),(0,0,100))#从封闭平面曲线挤出面

ml=rs.AddPlanarSrf(l)#平面封闭线生成面

#得到体
bool1=rs.CapPlanarHoles(m1)#给挤出面封口成为体
bool2=rs.CapPlanarHoles(m2)

b1=rs.ExtrudeSurface(m0,rs.AddLine((0,0,0),(0,0,100)))#沿曲线从曲面挤出体
b3=rs.BooleanDifference(m1,m2)# m1-m2 注意m已经封口为体了