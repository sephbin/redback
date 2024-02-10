"""Provides a scripting component.
    Inputs:
        DATA: The x script variable
        OPT: The y script variable
        CSS: The y script variable
    Output:
        a: The a output variable"""
        
var = {"guid":"89da792a-24b2-41fa-a674-20d9df2b5b6f","name":"ICML Generator","nickname":"ICML",
"description":"Takes JSON data formated as a lists of lists and creates a InCopy Markup Language format of a table.",
"tabname":"Redback","section":"Table",
"inputs":[
{"name":"Data","nickname":"D","description":"Table data formated as a JSON list of lists", "objectAccess":"item"},
{"name":"Options","nickname":"O","description":"Table options as a JSON dictionary", "objectAccess":"list"},
{"name":"CSS","nickname":"C","description":"CSS to apply styles to the table", "objectAccess":"list"}
]
"outputs":[
{"name":"Table","nickname":"T","description":"ICML Table"},
],
}
__author__ = "Andrew.Butler"
__version__ = "2024.02.02"

from ghpythonlib.componentbase import dotnetcompiledcomponent as component
import Grasshopper, GhPython
import System
import Rhino
import rhinoscriptsyntax as rs

class MyComponent(component):
    def __new__(cls):
        instance = Grasshopper.Kernel.GH_Component.__new__(cls,
            var["name"], var["nickname"], var["description"], var["tabname"], var["section"])
        return instance
    
    def get_ComponentGuid(self):
        return System.Guid(var["guid"])
    
    def SetUpParam(self, p, name, nickname, description):
        p.Name = name
        p.NickName = nickname
        p.Description = description
        p.Optional = True
    
    def RegisterInputParams(self, pManager):
        for inputOb in var["inputs"]:
            p = GhPython.Assemblies.MarshalParam()
            self.SetUpParam(p, inputOb["name"], inputOb["nickname"], inputOb["description"])
            access = getattr(Grasshopper.Kernel.GH_ParamAccess, inputOb["objectAccess"])
            self.Params.Input.Add(p)


        #p = GhPython.Assemblies.MarshalParam()
        #self.SetUpParam(p, "DATA", "DATA", "The x script variable")
        #p.Access = Grasshopper.Kernel.GH_ParamAccess.item
        #self.Params.Input.Add(p)
        #
        #p = GhPython.Assemblies.MarshalParam()
        #self.SetUpParam(p, "OPT", "OPT", "Script input OPT.")
        #p.Access = Grasshopper.Kernel.GH_ParamAccess.list
        #self.Params.Input.Add(p)
        #
        #p = GhPython.Assemblies.MarshalParam()
        #self.SetUpParam(p, "CSS", "CSS", "Script input CSS.")
        #p.Access = Grasshopper.Kernel.GH_ParamAccess.list
        #self.Params.Input.Add(p)
        
    
    def RegisterOutputParams(self, pManager):
        p = Grasshopper.Kernel.Parameters.Param_GenericObject()
        self.SetUpParam(p, "TABLE", "TABLE", "Script output TABLE.")
        self.Params.Output.Add(p)
        
    
    def SolveInstance(self, DA):
        p0 = self.marshal.GetInput(DA, 0)
        p1 = self.marshal.GetInput(DA, 1)
        p2 = self.marshal.GetInput(DA, 2)
        result = self.RunScript(p0, p1, p2)

        if result is not None:
            self.marshal.SetOutput(result, DA, 0, True)
        
    def get_Internal_Icon_24x24(self):
        o = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAASDSURBVEhLnZQLUJRVFMcXvmUXRN47I4OQOk5NM2YSBGalmWYDueKEjizrg0DK5GXaSNhUoERINcLwEHeIQClCVEhwyEcSomBMJINoRKiIgMhrP2B3eazgv+/c3YJ1khjOzH/m7P3uOb9zzz17RU8ye5lM7ujkzDs4OGAqOcqcedprDJu+OTrN4ZNV53C9eQTV9UM496sWJ8s1yD0zgNTjPBJy+hCd1g15yCkGMYZN36i6kkoNlvhmwO05BTzlh7H+o068vc8g8v323seanc3sJMaw6RsFqYr78dRiBdwW+WOBuxIBcV0IPNDNRP6mz7rgG9E0c8D+b9V4yf8IFnoosVyhQlBSL4K/6mMif2tiD9ZG3Zw5IPxQL7Yk9LCEoclq7EjlsTPdIPJpbV3U9f8BcF5yc85buCR3YdOErKzmmfz+Rx9kDzLtyhpEeGa/ALiG+ZaWWCwSmcjTQsR7cCK5yIzz4pMzK3Hr3ijq/hzGpWtDKLuqw4kKLY5d0EJVpkVKiRZJxTrEHtchJt+g6Dwt9uRosC6ihp1g9E4BhmrjMHBWgd6jS3AmyAKeYpFaOII7Lv+uw4rQu3gzso1NBl0e9ZdaQFVSIkr66YlhxBWNMJFPa/KwKgboz7eFOnc2elXW6E6zwu1PJOwkDFB6aRAr32uFz652Nn40IXSJ1GdqBVVLCeNLRnHwp4dM5NOa77sVDDBQ6AT+mA36sgRAuhX+ipFOAPJK+/G6EUAzTmO4Ob4Dythb2BzXhG0HGhH8+U1sT2hA6Bf1CIqrxaa91fALq8AyvxTMd3XSa36cBz5PAHxjjZ6MWfjjw0knSP2+zwSwYV8rViqzH7nOddFTdVNpgZtMf/rgwkeaIhcTQEPkJEBsZrdJi3zCb2Cui4u+vDAWY90VGOsswVhHHsbb0jHemoDx29EYa9wBfV0ARqpWQVf2NAYK7ExaVPf+JEBEYqfJJa8KqWXV6VsKoDlpB12JA4bPOmLkZyeMlsuYyKc1+kZ7Hr/k34ItJgCB0W14Nfgu3gi7h7V7OrBcWc0AI42ZQmW20BTZs0RDZY4sKYl8llz4RntYe7KF9mTOQleKJa4qJwFWh7bg5W0t/97DUn/DZAzVJaD/OxsDRKhSW2wP7WmjBJ/W6BvtUedMVP/gkCWubBQbAGbmXrzrC0fx7OpfsGjNRTzvcwHuvucZQHtlNwuk6qgFlGyw0A7qPFt0ZdmgI202WpKs0RwnTE20JeojpajdLmHV57/G0R9NeCGEp8LsP54KAgyeD2JV0dGpv3SJVNV0RMnZU/EkIwB/6i12ZOorAwkTwv/wDDQXt2C4IQ0Pu2pARnuNYdM3CurN9Wb9pEsjEI0fzbgqWArlMg5H3pGgIUoyc8CD1AW4nyhB55dSdH4tZTBS4FIO/h4cAr05XN4gnjmgLd4G7fst0B5vgY4ECYORDivEULxojowAMUpfMZ8ZYI7Mni/cKkHrxxzuxIjRtFuMG2Ec6kI41Cg5VG3kULneHOkrpHAW9hrDpm8ymb2cAqm6qUR7aK8x7DETif4GWQPKTm78ggMAAAAASUVORK5CYII="
        return System.Drawing.Bitmap(System.IO.MemoryStream(System.Convert.FromBase64String(o)))

    def RunScript(self, DATA, OPT, CSS):
        import sys
        import os
        user = os.environ['USERPROFILE']
        import json
        sys.path.append(r"\\sydsrv01\projects\Computational Design Group\93_Development\PackageManager\redback\py".format(**{'user':user}))
        import pyTableMaker as tm
        from pyCSSParser import parseCSS
        DATA = json.loads(DATA)
        if not OPT:
            OPT = {"css":[]}
        else:
            OPT = json.loads("".join(OPT))

        if CSS:
            CSS = "".join(CSS)
            CSS = parseCSS(CSS)
            OPT["css"] = CSS
        print("OPTIONS",OPT)

        TABLE = tm.table(DATA, OPT)
        # return outputs if you have them; here I try it for you:
        return TABLE


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
        return System.Guid("5c4d3d13-8708-4146-a504-f01347f02049")