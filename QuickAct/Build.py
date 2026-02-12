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

def AutoRail(cur,body,num=0,delta=0,reverse=False,point=(0,0,0),vec=(1,0,0)):
    """从栏杆曲线及支架自动创建栏杆,返回所有支架的列表
    
    Args:
        cur:栏杆路径曲线
        body:栏杆支架的实体
        num:栏杆支柱数量
        delta:栏杆的支架和cur之间的距离
        reverse:翻转生成栏杆支架的方向
        point:body的基准点
        vec:body的方向,生成后会与cur切线方向平行
    """
    posts = [] #所有支架
    if not rs.IsCurve(cur):
        print("错误：输入的不是有效曲线")
        return None
    if reverse:
        rs.ReverseCurve(cur)
    curve_length = rs.CurveLength(cur)
    print(f"曲线长度: {curve_length:.2f} 单位")
    domain=rs.CurveDomain(cur)

    if(num>1):
        num_posts=num  #立柱数量
        post_spacing=curve_length / (num-1)
    else:
        post_spacing = 1000  # 立柱间距
        num_posts = int(math.ceil(curve_length / post_spacing)) + 1

    def M(body):#处理支架的移动
        for i in range(num_posts):
            t = domain[0]+(domain[1]-domain[0])*i/(num_posts-1)
            point_on_curve = rs.EvaluateCurve(cur, t)
            tangent = rs.CurveTangent(cur, t)
            if tangent:
                tangent=rs.VectorUnitize((tangent[0],tangent[1],0))
                cross_z = vec[0] * tangent[1] - vec[1] * tangent[0]
                dot = vec[0] * tangent[0] + vec[1] * tangent[1]
                theta = math.atan2(cross_z, dot)

                b=rs.CopyObject(body)
                rs.RotateObject(b,point,theta*180/math.pi)
                trans=[point_on_curve[0]-point[0],point_on_curve[1]-point[1],point_on_curve[2]-point[2]]
                rs.MoveObject(b,translation=trans)
                if delta:
                    trans=[delta*tangent[1],delta*-tangent[0],0]
                    rs.MoveObject(b,translation=trans)
                posts.append(b)

    M(body)
    return posts
