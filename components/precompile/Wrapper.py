#### Constants ####
global __var__
__var__ = {
	"guid":"8e12d3d2-9c02-4af4-8931-ae8a51372bd0",
	
	"name":"Text Wrapper",
	"nickname":"Wrapper",
	"description":"Wraps and joins text with selected data characters",
	"icon": "ghContent\\Icon-Wrapper.png",

	"tabname":"Redback",
	"section":"JSON",

	"inputs":[
		{"name":"Content",			"nickname":"C",	"objectAccess":"list",	"description":"Text content to wrap", },
		{"name":"Wrap Character",	"nickname":"W",	"objectAccess":"item",	"description":"Character that will wrap the text", },
		{"name":"Join Character",	"nickname":"J",	"objectAccess":"item",	"description":"Character that will join the text, leave empty to wrap each individually", },
	],
	"outputs":[
		{"name":"Wrapped Text",	"nickname":"W",	"description":"Wrapped Text"}
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
        #imageLoc#ghContent\Icon-Wrapper.png
        o = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsSAAALEgHS3X78AAABA0lEQVRIie2UwW0CMRBF3yLuiTZXrHDAZ7YDKIESSAWhBOiADiAdQAdQAcl5OYCWK06owGjQLELIhJUgkRLtSNaMPfPn2/6WI+89t5gz9lHgcZZ+hdpUQovO2KEz1uuYOWOTQE0iOeBThmAKEThj68ArMAVeAJkvnLHdkxqJF5obAHPBKPbqCfKi9zhLx0CiZCNprM1HwJvk4iztA7Mz7NGCGjhjV8Cz7iwH94AHjXdAfiVtoAWs4ywtTCDCyU47gMRNTc3Vt9R/ACLuBBiHhL75FV2z6pEpiu7K5L2PuPRM72l/n+D3RKaA0Nta4+CfNstvm+YCU4pcEpQE/4TgZ/8iYA9kxVrpb6NLcQAAAABJRU5ErkJggg=="
        return System.Drawing.Bitmap(System.IO.MemoryStream(System.Convert.FromBase64String(o)))

    def RunScript(self, *argv):
        global __var__
        _vars_ = dict(zip(list(map(lambda x: x["nickname"], __var__["inputs"])),argv))
        _log_ = []
        C = _vars_['C']
        W = _vars_['W']
        J = _vars_['J']
                
        lut = {
        	'"': {'t':'"{value}"'},
        	"'": {'t':"'{value}'"},
        	'{': {'t':'{{{value}}}'},
        	'}': {'t':'{{{value}}}'},
        	'{}': {'t':'{{{value}}}'},
        	'[': {'t':'[{value}]'},
        	']': {'t':'[{value}]'},
        	'[]': {'t':'[{value}]'},
        	'(': {'t':'({value})'},
        	')': {'t':'({value})'},
        	'()': {'t':'({value})'},
        }
        
        def wrapText(text, wrap):
        	if wrap in lut:
        		template = lut[wrap]["t"]
        		print(template)
        		return template.format(value=text)
        	else:
        		return wrap+text+wrap
        
        if J != None:
        	C = [J.join(C)]
        
        outText = list(map(lambda x: wrapText(x, W), C))
        
        
        W = outText        
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
        return System.Guid("63ba3d63-89dd-4cf6-baf8-d1d863769cd5")