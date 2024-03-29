#### Constants ####
global __var__
__var__ = {
	"guid":"983d9511-87a8-479f-a688-dcef879bd7dc",
	
	"name":"Text Object",
	"nickname":"Text Object",
	"description":"Creates a text object that can be parsed by archJSON",
	"icon": "ghContent\\Icon-TextObject.png",

	"tabname":"Redback",
	"section":"ArchJSON",

	"inputs":[
		{"name":"Plane",		"nickname":"P",	"objectAccess":"item",	"description":"", },
		{"name":"Text",			"nickname":"T",	"objectAccess":"item",	"description":"", },
	],
	"outputs":[
		{"name":"Text Object",		"nickname":"O",	"description":"archJSON Text Object"},
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
        #imageLoc#ghContent\Icon-TextObject.png
        o = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsSAAALEgHS3X78AAAA3ElEQVRIiWP8//8/Ay0BC8zsIi5uuE19374ykmMnNjOYkBX0ffsKphkZGf+Tg5HNgAEWZE4RFzeY/ugZQFagNRzYBTcDBlDiAOSKtzIqZBkOA8JP7jD8//8fHsRMBHVQCOA+QI4gX15huKmbP7+Fs2uEpcB0y9tnDITU4o1kp1eP4ZgBS8SRohbFAlgEgcIQhpHFyVFLVEYDRf4+MVkGfTZ2uBh6ZOICNI/kUQtGLRi1gJ6lKS6AXMpiK00JVa9E+QBWQoLKHxBGFiMEiLIAveREFiMEaNtsYWBgAAA5sY2tq3QwZQAAAABJRU5ErkJggg=="
        return System.Drawing.Bitmap(System.IO.MemoryStream(System.Convert.FromBase64String(o)))

    def RunScript(self, *argv):
        global __var__
        _vars_ = dict(zip(list(map(lambda x: x["nickname"], __var__["inputs"])),argv))
        _log_ = []
        P = _vars_['P']
        T = _vars_['T']
        
        import Rhino as r
        import rhinoscriptsyntax as rs
        import json
        
        class archJSONText:
            def __init__(self, location, content):
                self.location = location
                self.content = str(content)
            def __str__(self):
                return ("ArchJSON String: "+self.content)
        
        O = archJSONText(P,T)        
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
        return System.Guid("01f8d0e5-0f24-4e98-8b76-933f36282a26")