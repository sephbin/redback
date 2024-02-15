#### Constants ####
global __var__
__var__ = {
	"guid":"33f7772a-c9ff-4356-915e-6a0b99bf4fcc",
	
	"name":"Read JSON",
	"nickname":"Read JSON",
	"description":"Reads keys from a JSON string. It can use javascript style key notation (a.b.c)",
	"icon": "ghContent\\Icon-ReadJSON.png",

	"tabname":"Redback",
	"section":"JSON",

	"inputs":[
		{"name":"JSON",			"nickname":"J",	"objectAccess":"list",	"description":"JSON object to read", },
		{"name":"Key",			"nickname":"K",	"objectAccess":"item",	"description":"Key to access from JSON", },
	],
	"outputs":[
		{"name":"Value",	"nickname":"V",	"description":"Returned value from JSON"}
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
        #imageLoc#ghContent\Icon-ReadJSON.png
        o = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsSAAALEgHS3X78AAAAcElEQVRIiWP8//8/Ay0BE01NZ2BgYIExGBkZqeqV////MzLQ1QdINsPZjIyMGGKEAEwPTgvQFeASI9sHb2VU4GzhJ3cwxAgBmB4YoHkcwPMBrVLRMIzkUR+M+oBkMOoDgo6huQ/oV5rSCtDWBwwMDAAl2EFvQnsgLgAAAABJRU5ErkJggg=="
        return System.Drawing.Bitmap(System.IO.MemoryStream(System.Convert.FromBase64String(o)))

    def RunScript(self, *argv):
        global __var__
        _vars_ = dict(zip(list(map(lambda x: x["nickname"], __var__["inputs"])),argv))
        _log_ = []
        J = _vars_['J']
        K = _vars_['K']
                
        import json
        def deepGet(key, object):
            try:
                keylist = key.split(".")
                for ki, k in enumerate(keylist):
                    try: k = int(k)
                    except: pass
                    print(k,type(k))
                    if ki < len(keylist)-1:
                        if type(k) == type("") and k not in object:
                            object[k] = {}
                        object = object[k]
                    else:
                        print("last",k, object)
                        returnOb = object[k]
                        return returnOb
            except Exception as e:
                print(e)
                return None
                
                
        out = []
        for o in J:
            o = json.loads(o)
            apob = deepGet(K,o)
            print(apob)
            if type(apob) == type([]) or type(apob) == type({}):
                apob = json.dumps(apob)
            out.append(apob)
                
        V = out        
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
        return System.Guid("23f1f3c3-0b4d-4e0a-9267-6555e328f978")