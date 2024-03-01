#### Constants ####
global __var__
__var__ = {
	"guid":"89da792a-24b2-41fa-a674-20d9df2b5b6f",
	
	"name":"ICML Generator",
	"nickname":"ICML",
	"description":"Takes JSON data formated as a lists of lists and creates a InCopy Markup Language format of a table.",
	"icon": "ghContent\\Icon-TableMaker.png",

	"tabname":"Redback",
	"section":"Tables",

	"inputs":[
		{"name":"Data",		"nickname":"D",	"objectAccess":"item",	"description":"Table data formated as a JSON list of lists", },
		{"name":"Options",	"nickname":"O",	"objectAccess":"list",	"description":"Table options as a JSON dictionary"},
		{"name":"CSS",		"nickname":"C",	"objectAccess":"list",	"description":"CSS to apply styles to the table"}
	],
	"outputs":[
		{"name":"Table",	"nickname":"T",	"description":"ICML Table"}
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
        #imageLoc#ghContent\Icon-TableMaker.png
        o = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsTAAALEwEAmpwYAAAJVWlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgOS4xLWMwMDIgNzkuZjM1NGVmYywgMjAyMy8xMS8wOS0xMjo0MDoyNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczpkYz0iaHR0cDovL3B1cmwub3JnL2RjL2VsZW1lbnRzLzEuMS8iIHhtbG5zOnBob3Rvc2hvcD0iaHR0cDovL25zLmFkb2JlLmNvbS9waG90b3Nob3AvMS4wLyIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0RXZ0PSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VFdmVudCMiIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCAyNS40IChXaW5kb3dzKSIgeG1wOkNyZWF0ZURhdGU9IjIwMjQtMDItMDJUMjM6MjQ6MDErMTE6MDAiIHhtcDpNb2RpZnlEYXRlPSIyMDI0LTAyLTAyVDIzOjMyOjI0KzExOjAwIiB4bXA6TWV0YWRhdGFEYXRlPSIyMDI0LTAyLTAyVDIzOjMyOjI0KzExOjAwIiBkYzpmb3JtYXQ9ImltYWdlL3BuZyIgcGhvdG9zaG9wOkNvbG9yTW9kZT0iMyIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDpjMTE0Mzk3MC0xMjA5LWFmNGYtYjRlOC1jNTc0ZTMxZDNiYzciIHhtcE1NOkRvY3VtZW50SUQ9ImFkb2JlOmRvY2lkOnBob3Rvc2hvcDpiOWQ5NGRlMC03NTU1LTRjNDMtYTlhMC02MjgzN2NhZWQ4MDIiIHhtcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD0ieG1wLmRpZDoyMDk0ZGE3Yy0xNjY1LTRjNGUtYTI1NS0zM2YxMGRkYjA4Y2UiPiA8eG1wTU06SGlzdG9yeT4gPHJkZjpTZXE+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJjcmVhdGVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOjIwOTRkYTdjLTE2NjUtNGM0ZS1hMjU1LTMzZjEwZGRiMDhjZSIgc3RFdnQ6d2hlbj0iMjAyNC0wMi0wMlQyMzoyNDowMSsxMTowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIDI1LjQgKFdpbmRvd3MpIi8+IDxyZGY6bGkgc3RFdnQ6YWN0aW9uPSJzYXZlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDowYjgyMDMzYy1mYjk5LTY1NGMtYjgxNS1hZDdjOGZkMzJhMWEiIHN0RXZ0OndoZW49IjIwMjQtMDItMDJUMjM6MzE6MzgrMTE6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCAyNS40IChXaW5kb3dzKSIgc3RFdnQ6Y2hhbmdlZD0iLyIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6NDEyNTc2ZDktNTA5OC1mZjQ4LTg0NjUtZDAwOGY4MTI4MmE1IiBzdEV2dDp3aGVuPSIyMDI0LTAyLTAyVDIzOjMyOjI0KzExOjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgMjUuNCAoV2luZG93cykiIHN0RXZ0OmNoYW5nZWQ9Ii8iLz4gPHJkZjpsaSBzdEV2dDphY3Rpb249ImNvbnZlcnRlZCIgc3RFdnQ6cGFyYW1ldGVycz0iZnJvbSBhcHBsaWNhdGlvbi92bmQuYWRvYmUucGhvdG9zaG9wIHRvIGltYWdlL3BuZyIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0iZGVyaXZlZCIgc3RFdnQ6cGFyYW1ldGVycz0iY29udmVydGVkIGZyb20gYXBwbGljYXRpb24vdm5kLmFkb2JlLnBob3Rvc2hvcCB0byBpbWFnZS9wbmciLz4gPHJkZjpsaSBzdEV2dDphY3Rpb249InNhdmVkIiBzdEV2dDppbnN0YW5jZUlEPSJ4bXAuaWlkOmMxMTQzOTcwLTEyMDktYWY0Zi1iNGU4LWM1NzRlMzFkM2JjNyIgc3RFdnQ6d2hlbj0iMjAyNC0wMi0wMlQyMzozMjoyNCsxMTowMCIgc3RFdnQ6c29mdHdhcmVBZ2VudD0iQWRvYmUgUGhvdG9zaG9wIDI1LjQgKFdpbmRvd3MpIiBzdEV2dDpjaGFuZ2VkPSIvIi8+IDwvcmRmOlNlcT4gPC94bXBNTTpIaXN0b3J5PiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDo0MTI1NzZkOS01MDk4LWZmNDgtODQ2NS1kMDA4ZjgxMjgyYTUiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6MjA5NGRhN2MtMTY2NS00YzRlLWEyNTUtMzNmMTBkZGIwOGNlIiBzdFJlZjpvcmlnaW5hbERvY3VtZW50SUQ9InhtcC5kaWQ6MjA5NGRhN2MtMTY2NS00YzRlLWEyNTUtMzNmMTBkZGIwOGNlIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+nYt+ggAAAT9JREFUSIndlb9Lw1AUhb8rETsUh2Tq5Fjo6iIOrgrOujo4SXUqKjilS0UkLqWz+gc4VnDQQVBcdBRcHVpBovgDQaRel1RKfM8mSof2QAjvu/fdk3cIPFFVeikHIJTgh8sR10xTSDTE1utpSYb++4Xd1P8GoqqIyLihNgWcJpxj7FXVy/6PyIneTUPtwcJNsvYOTkQ5Q821cJNsvY3Biahnf5Fjgibd4G94ZIsAr7zXJ9hauuO5ZePtfYkjyuNXHnnbA8gyMnvOWpBhWPL4lStuj+M8bpAzPG6cFSjXmjzVAUbJzJ2xugPkZqgemniqEwB80NJJtsv3vJwAjOHOX7Be/ESNPLVBh75vQAH5jUuaOzmUQIBdYCFCNU9LyzYO6U9Q7RiyD6x04ckNQgk2gWK0PAAWPS2pjbf3pYroL/oCFWt964nlGLkAAAAASUVORK5CYII="
        return System.Drawing.Bitmap(System.IO.MemoryStream(System.Convert.FromBase64String(o)))

    def RunScript(self, *argv):
        global __var__
        _vars_ = dict(zip(list(map(lambda x: x["nickname"], __var__["inputs"])),argv))
        _log_ = []
        D = _vars_['D']
        O = _vars_['O']
        C = _vars_['C']
        
        import sys
        import os
        user = os.environ['USERPROFILE']
        import json
        try:    sys.path.append(r"\\sydsrv01\projects\Computational Design Group\93_Development\PackageManager\redback\py".format(**{'user':user}))
        except: pass
        try:    sys.path.append(r"E:\mydev\redback\py".format(**{'user':user}))
        except: pass
        import pyTableMaker as tm
        from pyCSSParser import parseCSS
        
        __log__ = []
        
        
        __log__.append(str(C))
        
        D = json.loads(D)
        if not O:
        	O = {"css":[]}
        else:
        	O = json.loads("".join(O))
        
        if C:
        	C = "".join(C)
        	C = parseCSS(C)
        	O["css"] = C
        print("OPTIONS",O)
        
        
        T = tm.table(D, O)
        L = __log__        
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
        return System.Guid("76655fcd-e9f6-46db-8110-dfd7eae374c6")