#### Constants ####
global __var__
__var__ = {
	"guid":"9bf0fbf6-1cb9-49e0-b0a0-0ced95dcf5f9",
	
	"name":"Write File",
	"nickname":"Write",
	"description":"Saves data to file",
	"icon": "ghContent\\Icon-WriteFile.png",

	"tabname":"Redback",
	"section":"Files",

	"inputs":[
		{"name":"Run",			"nickname":"R",	"objectAccess":"item",	"description":"", },
		{"name":"Content",		"nickname":"C",	"objectAccess":"list",	"description":"", },
		{"name":"Folder",		"nickname":"P",	"objectAccess":"item",	"description":"", },
		{"name":"Name",			"nickname":"N",	"objectAccess":"item",	"description":"", },
	],
	"outputs":[
		{"name":"Filepath",	"nickname":"F",	"description":"Absolute path to saved file"}
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
        #imageLoc#ghContent\Icon-WriteFile.png
        o = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsSAAALEgHS3X78AAAAyklEQVRIiWP8//8/Ay0BE01NZ2BgYIEx3smqkuuVhUKPbyfgkqSGD+LfyaouoKUFeC2hZhxgtYTakYxhCS1SEYoltEqm8TAGC351CMCsrcnA5u4K5v85fpLh9/GTROmD52R8+YDV0pyBd9USFLGPHn4Mf69ex2mw0OPbjAzEBhGLpTmGGMw3hADNiwqiLPi1czdRYtgAUZEMCmtQmMOCBWQ4vvBHBkRFMjmApEimBBAVREKPb2MVfyerSlDv4EhFI9uCId6qYGBgAACDJUMuZsuj+AAAAABJRU5ErkJggg=="
        return System.Drawing.Bitmap(System.IO.MemoryStream(System.Convert.FromBase64String(o)))

    def RunScript(self, *argv):
        global __var__
        _vars_ = dict(zip(list(map(lambda x: x["nickname"], __var__["inputs"])),argv))
        _log_ = []
        R = _vars_['R']
        C = _vars_['C']
        P = _vars_['P']
        N = _vars_['N']
        
        import os
        from json import dumps
        def check(o):
            if str(type(o)) == "<type 'dict'>":
                return dumps(o)
            else:
                return o
        F = None
        if R:
            con = list(map(lambda u: check(u), C))
            conwrite = "\n".join(con)
            directory = P+"\\"+N
            fileDir, _t = os.path.split(directory)
            if not os.path.isdir(fileDir):
            	os.makedirs(fileDir)
            F = directory
            with open(directory,'wb') as f:
                f.write(conwrite.encode("UTF-8"))
        
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
        return System.Guid("e0b42af9-1693-4dcc-bdca-aa6158474621")