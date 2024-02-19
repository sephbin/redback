#### Constants ####
global __var__
__var__ = {
	"guid":"c2790a28-9f47-43da-a0c7-538cae087e63",
	
	"name":"JSON Keys",
	"nickname":"JSON Keys",
	"description":"Returns all surface keys from a JSON string",
	"icon": "ghContent\\Icon-JSONKeys.png",

	"tabname":"Redback",
	"section":"JSON",

	"inputs":[
		{"name":"JSON",			"nickname":"J",	"objectAccess":"item",	"description":"JSON object to read", },
	],
	"outputs":[
		{"name":"Keys",	"nickname":"K",	"description":"Surface level keys."}
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
        #imageLoc#ghContent\Icon-JSONKeys.png
        o = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsSAAALEgHS3X78AAABUklEQVRIibVWy03DQBB9IO6R9r7CHPYMVEBKoANMBykhVICpANNBOiB0YJ/3ErT3VVKB0SRjZMKOPxPlSZb8Wb/3ZmZn7IumaXBOXGm4o3WPAO74qABsAZQm+O3x2kkRMHEB4FpY8mKCX6oEonU5gPcRSz9M8PkkgWjdHMDnKCcHvJngF1ME1gAeEo++OudUj1nn+sYEvxkscrQuE8hhgp/3mNjX63LQPpCNWJMCRYQxAlrsjan6oEW0rq+A1dgIKqX+aIFCQb4DsBoUiNaVAJ4UAot2bIgCJ5BTk5XtRbLISvJvdr7q3vwnIJDXNC0BvNKsAbDkbUiNtqGCmuCTm+HPqEiQ77j9aybLJCIJvzUQyDN2fAsgn0qOVkBICzkvePQ+m+A12/WQokRH1ux+xuSl8P4gpFFBhGtOi5q8LwLCvSbnx5AajT57J5MTzvvbAuAH62GH1rTTziYAAAAASUVORK5CYII="
        return System.Drawing.Bitmap(System.IO.MemoryStream(System.Convert.FromBase64String(o)))

    def RunScript(self, *argv):
        global __var__
        _vars_ = dict(zip(list(map(lambda x: x["nickname"], __var__["inputs"])),argv))
        _log_ = []
        J = _vars_['J']
                
        import json
            
        K = list(json.loads(J))        
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
        return System.Guid("0f6cba5f-386a-45fd-8878-c3baa4c86e98")