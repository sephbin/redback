#### Constants ####
global __var__
__var__ = {
	"guid":"0e4b6507-e8be-483f-8d55-21c261e5bbcb",
	
	"name":"Create JSON",
	"nickname":"JSON",
	"description":"Creates a JSON string from a list of matching keys and values",
	"icon": "ghContent\\Icon-MakeJSON.png",

	"tabname":"Redback",
	"section":"JSON",

	"inputs":[
		{"name":"Keys",		"nickname":"K",	"objectAccess":"list",	"description":"List of Keys", },
		{"name":"Values",	"nickname":"V",	"objectAccess":"list",	"description":"List of Values", },
	],
	"outputs":[
		{"name":"JSON",		"nickname":"J",	"description":"JSON Output"},
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
        K = _vars_['K']
        V = _vars_['V']
        
        import json
        
        def convertObs(ob):
        	try:
        		ob = json.loads(ob)
        		return ob
        	except:
        		return ob
        
        V = list(map(lambda x: convertObs(x),V))
        
        outdict = dict(zip(K,V))
        J = json.dumps(outdict)        
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
        return System.Guid("f1808b63-4ceb-48c2-be1f-a140552c9c45")