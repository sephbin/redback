#### Constants ####
global __var__
__var__ = {
	"guid":"bb0530c2-d04f-4c52-8fb1-7f81d876a193",
	
	"name":"SVG Layout",
	"nickname":"SVG Layout",
	"description":"Creates a SVG layout dictionary",
	"icon": "ghContent\\Icon-MakeJSON.png",

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
    
    def RegisterInputParams(self, pManager):
        global __var__
        for inputOb in __var__["inputs"]:
            p = GhPython.Assemblies.MarshalParam()
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
        #imageLoc#ghContent\Icon-MakeJSON.png
        o = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsTAAALEwEAmpwYAAAJCmlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgOS4xLWMwMDIgNzkuZjM1NGVmYywgMjAyMy8xMS8wOS0xMjo0MDoyNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczpkYz0iaHR0cDovL3B1cmwub3JnL2RjL2VsZW1lbnRzLzEuMS8iIHhtbG5zOnBob3Rvc2hvcD0iaHR0cDovL25zLmFkb2JlLmNvbS9waG90b3Nob3AvMS4wLyIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0RXZ0PSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VFdmVudCMiIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCAyNS40IChXaW5kb3dzKSIgeG1wOkNyZWF0ZURhdGU9IjIwMjQtMDItMTNUMTM6MDg6NDErMTE6MDAiIHhtcDpNb2RpZnlEYXRlPSIyMDI0LTAyLTEzVDEzOjEyOjU4KzExOjAwIiB4bXA6TWV0YWRhdGFEYXRlPSIyMDI0LTAyLTEzVDEzOjEyOjU4KzExOjAwIiBkYzpmb3JtYXQ9ImltYWdlL3BuZyIgcGhvdG9zaG9wOkNvbG9yTW9kZT0iMyIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDo1OGNhMmQ4NS0zNzE5LWZkNGQtOWNkOC05Y2Q0MGExODBiYmIiIHhtcE1NOkRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDpiNjlhMDE3ZS1iZDJmLTRjNDAtODhmYy1jNjk1NTExNzMwMDMiIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDo3NjYwOGEwYy1jMmZlLWYyNDYtOTc1ZS1iMTlmMDljNzc5MDciPiA8cGhvdG9zaG9wOlRleHRMYXllcnM+IDxyZGY6QmFnPiA8cmRmOmxpIHBob3Rvc2hvcDpMYXllck5hbWU9Ims6diIgcGhvdG9zaG9wOkxheWVyVGV4dD0iazp2Ii8+IDwvcmRmOkJhZz4gPC9waG90b3Nob3A6VGV4dExheWVycz4gPHhtcE1NOkhpc3Rvcnk+IDxyZGY6U2VxPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0iY3JlYXRlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDo3NjYwOGEwYy1jMmZlLWYyNDYtOTc1ZS1iMTlmMDljNzc5MDciIHN0RXZ0OndoZW49IjIwMjQtMDItMTNUMTM6MDg6NDErMTE6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCAyNS40IChXaW5kb3dzKSIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6YWM5M2IyYmYtODcxMy0wZjQ4LWJlYzktZjI3ZWVmNDVhMmQ1IiBzdEV2dDp3aGVuPSIyMDI0LTAyLTEzVDEzOjEyOjU4KzExOjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgMjUuNCAoV2luZG93cykiIHN0RXZ0OmNoYW5nZWQ9Ii8iLz4gPHJkZjpsaSBzdEV2dDphY3Rpb249ImNvbnZlcnRlZCIgc3RFdnQ6cGFyYW1ldGVycz0iZnJvbSBhcHBsaWNhdGlvbi92bmQuYWRvYmUucGhvdG9zaG9wIHRvIGltYWdlL3BuZyIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0iZGVyaXZlZCIgc3RFdnQ6cGFyYW1ldGVycz0iY29udmVydGVkIGZyb20gYXBwbGljYXRpb24vdm5kLmFkb2JlLnBob3Rvc2hvcCB0byBpbWFnZS9wbmciLz4gPHJkZjpsaSBzdEV2dDphY3Rpb249InNhdmVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOjU4Y2EyZDg1LTM3MTktZmQ0ZC05Y2Q4LTljZDQwYTE4MGJiYiIgc3RFdnQ6d2hlbj0iMjAyNC0wMi0xM1QxMzoxMjo1OCsxMTowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIDI1LjQgKFdpbmRvd3MpIiBzdEV2dDpjaGFuZ2VkPSIvIi8+IDwvcmRmOlNlcT4gPC94bXBNTTpIaXN0b3J5PiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDphYzkzYjJiZi04NzEzLTBmNDgtYmVjOS1mMjdlZWY0NWEyZDUiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6NzY2MDhhMGMtYzJmZS1mMjQ2LTk3NWUtYjE5ZjA5Yzc3OTA3IiBzdFJlZjpvcmlnaW5hbERvY3VtZW50SUQ9InhtcC5kaWQ6NzY2MDhhMGMtYzJmZS1mMjQ2LTk3NWUtYjE5ZjA5Yzc3OTA3Ii8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+k3BGiAAAAb5JREFUSIntlb9qIlEUxn93CaSwWJJiLHQhBPeyFtuYF1gkLPgCwV7QYkoLA+IDbDE+gMwDiL6Ai4FUdkkdcLmTIlhMMWYLrc82d2adVVGL7JKwBy73nMO958983z2jRISXlHcvGv1NJDgCUEodDISIqL0TuK4LwHg8ZjqdHpprZyWIFdd1Bdhr2Xs71+sH+e+w6E8pl8sUi0UAoiii3+9vvDz/8PHSqvenTz+eV/wXwAkQrCXwPI9Go0EmkyEIAiqVyvbylGohcgl8A65t8BPgzp64SrFoNBrFqhhjRGudYo7WOmGaiDDXn+tRviBRvjCPWRPlCy3rMyKSpmksYRiuBQfEGCMiIoPBQFYCzm3AurWNtVtbaeo4DqVSac2fzWYByOVyv7/S8XHPqnWLybm1h0C6A2NMUuWmLqrVqvi+L1rrpIOfX76e24olyhfu7D7Y+NC63S7tdpvlconjOAyHw+0AW3l/+z1AqRtrXti9lxzYNCo8z0vw8H0/6WCxWIiIyGQySY2K6OxTfaULs3NUNJvNpPparUan0wEgDEMAZrNZ6vzp40MPCNaqjztgzwEX48ABw079/yf/8wS/ADRpphnHIE/MAAAAAElFTkSuQmCC"
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
        return System.Guid("ac401f51-7dca-4153-8aba-0129602619d0")