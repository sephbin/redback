#### Constants ####
global __var__
__var__ = {
	"guid":"3f9c365f-cccc-469a-9d4e-72a0b3c94343",
	
	"name":"Debug",
	"nickname":"Debug",
	"description":"Debug",
	"icon": "ghContent\\Icon-RelativePath.png",

	"tabname":"Redback",
	"section":"Debug",

	"inputs":[
		{"name":"E",		"nickname":"E",	"objectAccess":"Exec",	"description":"E", },
	],
	"outputs":[
		{"name":"LOG",		"nickname":"L",	"description":"LOG"},
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
        #imageLoc#ghContent\Icon-RelativePath.png
        o = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAWCAYAAADafVyIAAAACXBIWXMAAAsSAAALEgHS3X78AAAA1klEQVRIiWN8w9AjwMDAkMDAwODAwMAAYmMDDcL/iw/gkMMLQBaANNoToXYjyCHC/4s/kGrBfxLUgyy5QKTaD8L/iyeQZAGTAh8DR4IOQXX/Pvxk+DHhLIi5kIVYw8EaH3xiYDYQZWAU4MCrjlmBj4HFQJThS8KOeFKDiChf/D7wmIFvfxjDW8ZeBpJ8wAD1xZ8Lrxg4Coxxqvm14Q6cTbIPiAUgn4Icw0QLwxmgPgUBmlkAA6MWjFowasFwseAgDc0/CCquA6CVPoimFgDV2wcYGBgWAADYoj+ZEL2zKwAAAABJRU5ErkJggg=="
        return System.Drawing.Bitmap(System.IO.MemoryStream(System.Convert.FromBase64String(o)))

    def RunScript(self, *argv):
        global __var__
        _vars_ = dict(zip(list(map(lambda x: x["nickname"], __var__["inputs"])),argv))
        _log_ = []
        E = _vars_['E']
        
        import os
        import Rhino
        import scriptcontext as sc
        L = []
        eval(E)        
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
        return System.Guid("335ee0ea-1cc4-4610-aad7-fd1a52d83360")