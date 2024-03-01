#### Constants ####
global __var__
__var__ = {
	"guid":"bb0530c2-d04f-4c52-8fb1-7f81d876a193",
	
	"name":"SVG Layout",
	"nickname":"SVG Layout",
	"description":"Creates a SVG layout dictionary",
	"icon": "ghContent\\Icon-SVGLayout.png",

	"tabname":"Redback",
	"section":"SVG",

	"inputs":[
		{"name":"Plane",		"nickname":"P",	"objectAccess":"item",	"description":"", },
		{"name":"Dimensions",	"nickname":"D",	"objectAccess":"item",	"description":"", },
		{"name":"Scale",		"nickname":"S",	"objectAccess":"item",	"description":"", },
		{"name":"Location",		"nickname":"L",	"objectAccess":"item",	"description":"", },
	],
	"outputs":[
		{"name":"Layout",		"nickname":"L",	"description":"JSON Layout"},
		{"name":"Rectangle",	"nickname":"R",	"description":"Page Border"},
	]
}
__author__ = "Andrew.Butler"
__version__ = "2024.02.02"
#### Constants ####
from ghpythonlib.componentbase import dotnetcompiledcomponent as component
import Grasshopper, GhPython
import System
import Rhino
import rhinoscriptsyntax as rs

class MyComponent(component):
    def __new__(cls):
        global __var__
        instance = Grasshopper.Kernel.GH_Component.__new__(cls,
            __var__["name"], __var__["nickname"], __var__["description"], __var__["tabname"], __var__["section"])
        return instance
    
    def get_ComponentGuid(self):
        global __var__
        return System.Guid(__var__["guid"])
    
    def SetUpParam(self, p, name, nickname, description):
        p.Name = name
        p.NickName = nickname
        p.Description = description
        p.Optional = True
        #print(p.Name, p.Nickname)
    
    def RegisterInputParams(self, pManager):
        global __var__
        for inputOb in __var__["inputs"]:
            try:    pfunc = getattr(Grasshopper.Kernel.Parameters, "Param_"+inputOb["objectType"])
            except Exception as e:
                #print(e)
                pfunc = getattr(GhPython.Assemblies, "MarshalParam")
            p = pfunc()
            self.SetUpParam(p, inputOb["name"], inputOb["nickname"], inputOb["description"])
            access = getattr(Grasshopper.Kernel.GH_ParamAccess, inputOb["objectAccess"])
            p.Access = access
            self.Params.Input.Add(p)
        
    
    def RegisterOutputParams(self, pManager):
        global __var__
        for outputOb in __var__["outputs"]:
            p = Grasshopper.Kernel.Parameters.Param_GenericObject()
            self.SetUpParam(p, outputOb["name"], outputOb["nickname"], outputOb["description"])
            self.Params.Output.Add(p)
        
    
    def SolveInstance(self, DA):
        global __var__
        args = []
        for r in range(len(__var__["inputs"])):
            #key = p+str(r)
            args.append(self.marshal.GetInput(DA, r))
        result = self.RunScript(*args)
        #p0 = self.marshal.GetInput(DA, 0)
        #p1 = self.marshal.GetInput(DA, 1)
        #p2 = self.marshal.GetInput(DA, 2)
        #result = self.RunScript(p0, p1, p2)

        if result is not None:
            for r in range(len(__var__["outputs"])):
                self.marshal.SetOutput(result[r], DA, r, True)
        
    def get_Internal_Icon_24x24(self):
        #imageLoc#ghContent\Icon-SVGLayout.png
        o = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsSAAALEgHS3X78AAACYklEQVRIid2Vz2sTURDHv7ObmBptk2xq44/GeEgE0WJ6EykaEXoRsSjFQj3k0LPgVUURD55ErdTfFhTspWARsYiI6F+gp6LdisLGQ4JJs2tJdpu8HS/ZksZNqLQ96MDwlpnh833zhveWmBnradK60v8LAc9awgrRRBBAsgIesMADEtMwrWbIhWhiF4AUgGRt3W+BC2VmXztJ37Zoas9fdVCIJuphKQABJ2eBc3kWukKyEiQJAK4DQNMOzDsP+liII+a9RxE2jH0ADrvVWeBslkVwE4jDJLfVwrqiqUGgbgbmw/HTUjg8RJ3hHnl3IuZNHfIYg8Ngw3DdwC+2s1kWES+RvJ1knxdUn77pfFBpdCzmGzw1J22NLImJmc9oBnfAVbC5gzx2O0l+F/2QoqnFZUe0+PzFlLf/6Akwk95/HHbmhyu4AkaQpJ9d5OmUXXvDE0VT00sd1M+gfOP2zsVXr8+KL7MjAAIMlBfYNhywDOjd5An4qeX16VU09ZOrgGOFaCKYY/GuyKJX1GJhkvUukgN/FC+3D4qmpuoDrltRNLWYZ2EIABLzfFzyYgVw+M4MTTXGWt4DvVopZy0zJPv8ZnzDxrZmdfLePfBfOl/2HjxwqzHX6jC/FyuLBgDMWKU5i+1pt6K2kTQ6Jp/B1jITbvmmAguialq2HQFwJV0s9GzLfD0G4BwAHQCoowObH9+F//IFgIjtXO6qK4iZXX0y3HVxPBBKNsbz3fGkfnJoVmgZdsx68/ZjM05TgVZeGh2L1aDMzFweu9+3pgKOm08nrlkvp9+3qlnVc70S+/d/mesu8BsAB70gWjTvAQAAAABJRU5ErkJggg=="
        return System.Drawing.Bitmap(System.IO.MemoryStream(System.Convert.FromBase64String(o)))

    def RunScript(self, *argv):
        global __var__
        _vars_ = dict(zip(list(map(lambda x: x["nickname"], __var__["inputs"])),argv))
        _log_ = []
        P = _vars_['P']
        D = _vars_['D']
        S = _vars_['S']
        L = _vars_['L']
        
        import json
        import scriptcontext as sc
        import rhinoscriptsyntax as rs
        import Rhino
        
        
        
        millimeters = sc.doc.ModelUnitSystem.Millimeters
        currentUnits = sc.doc.ModelUnitSystem
        
        
        #### This assumes base units as millimeters
        
        unitRatio = Rhino.RhinoMath.UnitScale(millimeters, currentUnits)
        
        pageLUT = {
        "A5":		(210,148),
        "A5 (L)":	(210,148),
        "A5 (P)":	(148,210),
        "A4":		(297,210),
        "A4 (L)":	(297,210),
        "A4 (P)":	(210,297),
        "A3":		(420,297),
        "A3 (L)":	(420,297),
        "A3 (P)":	(297,420),
        "A2":		(594,420),
        "A2 (L)":	(594,420),
        "A2 (P)":	(420,594),
        "A1":		(841,594),
        "A1 (L)":	(841,594),
        "A1 (P)":	(594,841),
        "A0":		(1189,841),
        "A0 (L)":	(1189,841),
        "A0 (P)":	(841,1189),
        }
        
        plane = P
        location = L
        scale = S
        
        DIM = D
        if not DIM:
        	DIM = "A3 (L)"
        if location == None:
        	location = True
        if scale == None:
        	scale = "1:1"
        
        pageScale = float(scale.split(":")[1])/float(scale.split(":")[0])
        
        if DIM in pageLUT:
        	width, height = pageLUT[DIM]
        elif DIM.replace(",","").isdigit():
        	width, height = tuple(DIM.split(","))
        
        
        combScale = unitRatio*pageScale
        swidth = float(width)*combScale
        sheight = float(height)*combScale
        
        print(width, height)
        
        
        #print(dir(plane))
        #print(plane.PointAt.__doc__)
        print(location)
        if location:
        	positionedPlaneOrig = plane.PointAt(swidth/-2.0,sheight/-2.0,0)
        	svgPlaneOrig = plane.PointAt(swidth/-2.0,sheight/2.0,0)
        else:
        	positionedPlaneOrig = plane.PointAt(0,-sheight,0)
        	svgPlaneOrig = plane.PointAt(0,0,0)
        print(positionedPlaneOrig)
        plane.Origin = positionedPlaneOrig
        #print(rs.AddRectangle.__doc__)
        R = rs.AddRectangle(plane,swidth,sheight )
        plane.Origin = svgPlaneOrig
        p = plane
        
        L = json.dumps({"plane":[[p.Origin[0],p.Origin[1],p.Origin[2]],[p.XAxis[0],p.XAxis[1],p.XAxis[2]],[p.YAxis[0],p.YAxis[1],p.YAxis[2]]], "page":[width,height], "scale":combScale})        
        returnTuple = []
        for output in __var__["outputs"]:
            varName = output["nickname"]
            returnTuple.append(locals()[varName])
        returnTuple = tuple(returnTuple)
        return returnTuple

import GhPython
import System

class AssemblyInfo(GhPython.Assemblies.PythonAssemblyInfo):
    def get_AssemblyName(self):
        return "Python"
    
    def get_AssemblyDescription(self):
        return """"""

    def get_AssemblyVersion(self):
        return "0.1"

    def get_AuthorName(self):
        return ""
    
    def get_Id(self):
        return System.Guid("fcae8761-8363-46dc-a534-256db0228115")