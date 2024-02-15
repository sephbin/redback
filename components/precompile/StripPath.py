#### Constants ####
global __var__
__var__ = {
	"guid":"41bdcdeb-7482-4608-873f-59490830db42",
	
	"name":"Relative Path",
	"nickname":"Rel Path",
	"description":"Creates a relative path to Rhino/GH document",
	"icon": "ghContent\\Icon-RelativePath.png",

	"tabname":"Redback",
	"section":"Files",

	"inputs":[
		{"name":"Path",		"nickname":"P",	"objectAccess":"item",	"description":"Path to append, or Boolean to choose current (F)Grasshopper or (T)Rhino document.", },
		{"name":"Relative Path",			"nickname":"R",	"objectAccess":"item",	"description":"Relative Path string", },
	],
	"outputs":[
		{"name":"Full path",		"nickname":"F",	"description":"Full Path"},
		{"name":"Directory path",	"nickname":"D",	"description":"Directory Path"},
		{"name":"File name",		"nickname":"N",	"description":"File name"},
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
        o = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsSAAALEgHS3X78AAAA/ElEQVRIiWP8//8/Ay0B41sZFQEGBoYEBgaGADz2HGBgYFgg9Pj2A1LdArIApNmeCLUfQY4Qenz7AKkWEB1GfxkYPt7692sCkco/WD99OIEkC0DgW3Icw38+Xrxq/nz6xPBs9lwQcyELKYaDAPenzwxM7q541XDIyjJwa2sx3C4ojifZB//Z2Rh++nkx/JWRxqnm47HjDDprVjIclZZnINkHjD9/MXCs3sDA5ubCwKytiVXNvyfPGX6uXgdRT6oPSAHv/v9lYKKV4SAgxMhMWwtAYNSCUQtGLRguFhykofkHQcU1qLIHVfoODAwMoAYAtcAGUEOBts0WBgYGAMH6SksO3FJrAAAAAElFTkSuQmCC"
        return System.Drawing.Bitmap(System.IO.MemoryStream(System.Convert.FromBase64String(o)))

    def RunScript(self, *argv):
        global __var__
        _vars_ = dict(zip(list(map(lambda x: x["nickname"], __var__["inputs"])),argv))
        _log_ = []
        P = _vars_['P']
        R = _vars_['R']
        
        import os
        import Rhino
        import scriptcontext as sc
        try:
        	#print(P)
        	if P == None:
        		P = False
        	#if not R:
        	#	R = ""
        	if type(P) == type(False): 
        		if P:
        		    DOC = Rhino.RhinoDoc.ActiveDoc.Path
        		else:
        		    DOC = str(self.Attributes.Owner.OnPingDocument().FilePath)
        	else:
        		DOC = P
        	#print(DOC)
        	if os.path.isdir(DOC):
        		docDir = DOC
        		docName = None
        	else:
        		docDir, docName = os.path.split(DOC)
        	
        	docDirs = docDir.split(os.sep)
        	#print("1 docDirs",docDirs)
        	if R == None:
        		R = docName
        	try:
        		relDirs = R.split(os.sep)
        	except:
        		relDirs = []
        
        
        
        	for pathStep in relDirs:
        	    #print("pathstep", pathStep)
        	    if pathStep == ".":
        	        del docDirs[-1]
        
        	relDirs = list(filter(lambda x: x != ".", relDirs))
        	#print("relDirs",relDirs)
        	#print("docDirs",docDirs)
        	newDirs = docDirs+relDirs
        	#print("newDirs",newDirs)
        	newDirs = list(map(lambda x: x.replace(":", ":"+os.sep), newDirs))
        	newDir = os.path.join(*newDirs)
        
        	#print(newDir)
        	F = newDir
        	if os.path.isdir(newDir):
        		D = newDir
        		N = None
        	else:
        		D, N = os.path.split(newDir)
        except Exception as e:
        	#print(e)
        	F = R
        	D, N = os.path.split(R)        
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
        return System.Guid("6ea2b6d8-ce51-4e8a-80f0-cc0286a7dfb0")