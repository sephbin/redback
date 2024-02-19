#### Constants ####
global __var__
__var__ = {
	"guid":"53cc467c-f4e5-4782-ac66-a8ce3084f988",
	
	"name":"Image Object",
	"nickname":"Image Object",
	"description":"Creates an image object that can be parsed by archJSON",
	"icon": "ghContent\\Icon-ImageObject.png",

	"tabname":"Redback",
	"section":"ArchJSON",

	"inputs":[
		{"name":"Rectangle",		"nickname":"R",	"objectAccess":"item",	"description":"", },
		{"name":"Image",			"nickname":"I",	"objectAccess":"item",	"description":"", },
	],
	"outputs":[
		{"name":"Image Object",		"nickname":"O",	"description":"archJSON Text Object"},
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
        #imageLoc#ghContent\Icon-ImageObject.png
        o = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsSAAALEgHS3X78AAABdklEQVRIibWWvU7DMBSFv1RdWoZWldhqUaR6hjegD8DKzsaIipihsFYCZhb6CAzswBuwd6BS55oulAVh5DSJ3NQkaUKu5J97Y5/jY1u+8bTWlGmVUtGBqqnO6luRjJvFp1cEMI5VsRy/9TxPFyk2VqQgYA67V0UUAJcWFphDDg7aVIPQz1Jm7W5n1u4OZ+1u08IZLGGXfu5DVkKOgHfgHPhQQh66xhW5RUcx/+K/CX6yDCpCMLT638B1bgIlZEcJ2bRjrenYAO4CJ8B2azp+cs2tuoIx8H3gBZgoIXut6XhukUyA+6T5iQqCVT8CDWDPEMWVpFnaFhnwHcsPSXpKyDcl5DxQuLkCJeQdcOD4ZEieg7aRpspJoIQ8Bk6TVmZZIskaQSD5ISN4aH+ej0vBaEPwFZJ40HVNzdWbO+JZbG2enzLtJHH7tXjNCe5bv1aPLoafvAxBv1Y3z6s2bfBs5y421spzHSYJrbVXpNhY0RaVaeX+VQC/YEz5qWBh6ZoAAAAASUVORK5CYII="
        return System.Drawing.Bitmap(System.IO.MemoryStream(System.Convert.FromBase64String(o)))

    def RunScript(self, *argv):
        global __var__
        _vars_ = dict(zip(list(map(lambda x: x["nickname"], __var__["inputs"])),argv))
        _log_ = []
        R = _vars_['R']
        I = _vars_['I']
        
        import rhinoscriptsyntax as rs
        import Rhino as r
        
        
        
        
        class archJSONImage:
            def __init__(self, rec, img):
                p = rs.CurvePoints(rec)
                self.location = rs.PlaneFromPoints(p[0],p[1],p[2])
                self.bounds = rs.BoundingBox(rec,self.location,False)
                self.width = self.bounds[2][0]
                self.height = self.bounds[2][1]
                self.img = str(img)
            def __str__(self):
                return ("ArchJSON Image")
        
        O = archJSONImage(R,I)        
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
        return System.Guid("488c812c-3f55-4c62-bd20-6f674e08f8d1")