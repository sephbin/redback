#### Constants ####
global __var__
__var__ = {
	"guid":"d276cba6-c7bd-4860-8d09-1c6b23a620d4",
	
	"name":"Geo2Ar",
	"nickname":"Wrapper",
	"description":"Wraps and joins text with selected data characters",
	"icon": "ghContent\\Icon-Geo2ArchiJSON.png",

	"tabname":"Redback",
	"section":"ArchJSON",

	"inputs":[
		{"name":"Geometry",			"nickname":"G",	"objectAccess":"list", "objectType":"GenericObject"	,"description":"List of Geometry to convert", },
		{"name":"Properties",		"nickname":"P",	"objectAccess":"list", "objectType":"String"		,"description":"List of Properties per geometry", },
	],
	"outputs":[
		{"name":"JSON",	"nickname":"J",	"description":"ArchiJSON representing the geometry"},
		#{"name":"CHECKGEOM", "nickname":"CHECKGEOM", "description":"ArchiJSON representing the geometry"},
		#{"name":"LOG", "nickname":"LOG", "description":"ArchiJSON representing the geometry"}
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
        #imageLoc#ghContent\Icon-Geo2ArchiJSON.png
        o = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsSAAALEgHS3X78AAACmklEQVRIibVWzWsTURD/zebDthSTKgRRtBrwVkXwKqGCf0A9KHrrSetJD6b1Vr2poaCnVm+iCNqLniM0jV4UFxTqVczBIIi2qUWTbPaNzPPtstls6ubQgWXnTWbmN59vQ8yMnaRk2HeVipMJsuZdVpNBeYKsisvqVoFLlUHi6crgNc0+ZfDFESuNbHIYKUpoucMuNjp/8Fu1YYGWTvHdKwMDvKHZRQwlZsaP5ZHJ50CZXWg9+dSlLCD1dmMgEAumLAo8c/DoIeydmkDyeA7caPUoS1b70xmIrtjEBpCa67Lkc/4PqtaINBAQ0RWbOAC6ydLQbGpYC5x3dajaJtw+AB5Ivd2IlYE/RdJQp/wljo3f/DhkeToJothGg5AP0FSd2GYytgNlIEskI+iywndn679GomsRleMA6D0wI7ei+8Au9iRHsC+9u0e5xj8w3XyENa7rMzP7dSWiBwAumeNlZn7oZ2DWf02cWyA9JVH0rG1r50NIyZLcCKm8ArBs+NueUE9RlYpZABPCKzC+tX9FXhWbqqnPTTjzzHwn6J2ZxfkyEcnVMObJvSZPm/cqgNMt7lTkSqi1fupH+CY7ZRu1BaM3hggiIk9ud2Ugu2PeUqqsy+ojgLMFLm0E3byn0jnD5iNrCJw07/VwBt4VLJk8BnAVwIfgfUNEcwCehyMMkW2cnzGl6mqyRD0OYBTAluFXqlS8Z3oUpPU+AEKfgwcrwEu0Lw0/GpDrbFZx/S2A86FSRJVIHtsbYR9A6l3g0hSAIwBkQr5uE2W/DHpKZ0UoHQZwAcABc74P4IQpY7/mRmWiqeebDOAFgIwsrjQ99A32Ip+Tpm+zyX4fogCuScQAbobHVNbfzLpEGM5GyiOO5YqQrf4HvKN/WwD8BdfTDaT/2nTFAAAAAElFTkSuQmCC"
        return System.Drawing.Bitmap(System.IO.MemoryStream(System.Convert.FromBase64String(o)))

    def RunScript(self, *argv):
        global __var__
        _vars_ = dict(zip(list(map(lambda x: x["nickname"], __var__["inputs"])),argv))
        _log_ = []
        G = _vars_['G']
        P = _vars_['P']
        		
        import rhinoscriptsyntax as rs
        import Rhino as rh
        import scriptcontext as sc
        import Grasshopper as g
        import ghpythonlib as ghp
        import json
        #LOG = []
        CHECKGEOM = []
        #document = sc.doc
        #sc.doc = rh.RhinoDoc.ActiveDoc
        #sc.doc = ghdoc
        
        
        def Point(geom,prop=None):
        	pt = rs.coerce3dpoint(geom)
        	pt = [pt[0],pt[1],pt[2]]
        	if prop:
        		pt = {"type":"Feature","geometry":{'coordinates':pt, 'type': 'Point'}}
        		pt['properties'] = json.loads(prop)
        		return pt
        	else:
        		return pt
        def convertToRPT (geom):
        	pt = rh.Geometry.Point3d(geom[0],geom[1],geom[2])
        	return pt
        def PointCollection(geom,prop=None):
        	t = geom
        	pts = map(lambda g: Point(g), t)
        	crv = {"type":"Feature","geometry":{'coordinates':pts, 'type': 'PointCollection'}}
        	if prop:
        		crv['properties'] = json.loads(prop)
        	#print(crv)
        	return crv
        	
        def CurveCollection(geom,prop=None):
        	t = geom
        	#print('geom', geom)
        	crvs = map(lambda g: Curve(g), t)
        	#print("CURVE COLLECTION--- ", crvs)
        	crvCol = {"type":"Feature","geometry":{'coordinates':crvs, 'type': 'CurveCollection'}}
        	if prop:
        		crvCol['properties'] = json.loads(prop)
        	return crvCol
        	
        #    t = rs.CurvePoints(crvs)
        #    tc = rs.ClosedCurveOrientation(crvs)
        #    if tc == 0: tc = False
        #    else: tc = True
        #    tdeg = rs.CurveDegree(crvs)
        #    tcPo = len(t)
        #    #print("tdeg: ",tdeg)
        #    #print("tcPo: ",tcPo)
        #    pts = map(lambda g: Point(g), t)
        #    crv = {"type":"Feature","geometry":{'coordinates':pts, 'type': 'CurveCollection','closed':tc}}
        #    if tdeg == 1 and tc: del crv['geometry']['coordinates'][-1]
        #    if tdeg == 1 and tcPo == 2  : crv['geometry']['type']='Line'
        #    if tdeg >= 2                : crv['geometry']['type']='Spline'
        #    if tdeg == 2 and tcPo == 3  :
        #        #print("3ARC")
        #        crv['geometry']['type']='Arc'
        #    if tdeg == 2 and tcPo == 5  :
        #        #print("5ARC")
        #        arc = rs.AddArc3Pt(pts[0],pts[4],pts[2])
        #        pt0 = convertToRPT(pts[0])
        #        pt2 = convertToRPT(pts[2])
        #        pt4 = convertToRPT(pts[4])
        #        arc = rh.Geometry.Arc(startPoint = pt0, pointOnInterior = pt2, endPoint = pt4)
        #        crv['geometry']['type']='Arc'
        #        #print(crv['geometry']['coordinates'])
        #        del crv['geometry']['coordinates'][3]
        #        del crv['geometry']['coordinates'][1]
        #    if prop:
        #        crv['properties'] = json.loads(prop)
        #    #print(crvcollection)
        #    return crvcollection
        
        def Plane(geom,prop=None):
        	pln = {"type":"Feature","geometry":{'coordinates':[geom.XAxis.X,geom.YAxis.X,geom.ZAxis.X,geom.OriginX,geom.XAxis.Y,geom.YAxis.Y,geom.ZAxis.Y,geom.OriginY,geom.XAxis.Z,geom.YAxis.Z,geom.ZAxis.Z,geom.OriginZ,0,0,0,1], 'type': 'Xform'}}
        	if prop:
        		pln['properties'] = json.loads(prop)
        	return pln
        def Text(geom,prop=None):
        	#print("TEXT")
        	text = Plane(geom.location,prop)
        	text['geometry']['type']="Text"
        	text['geometry']['content']=geom.content
        	return text
        def Image(geom,prop=None):
        	#print("Image")
        	##print(dir(geom))
        	image = Plane(geom.location,prop)
        	image['geometry']['type']="Image"
        	image['geometry']['image']=geom.img
        	image['geometry']['width']=geom.width
        	image['geometry']['height']=geom.height
        	return image
        def Curve(geom,prop=None):
        	#print("Curve def")
        	#print(geom, type(geom))
        	if type(geom).__name__ == "Line":
        		geom = ghp.components.RebuildCurve(geom,1,3,False)
        	if type(geom).__name__ == "Circle":
        		geom = geom.ToNurbsCurve()
        	try:
        		t = rs.CurvePoints(geom)
        	except:
        		t = []
        	
        	#print(t)
        	#check if curve closed
        	tc = rs.ClosedCurveOrientation(geom)
        	if tc == 0: tc = False
        	else: tc = True
        	##print(dir(geom))
        	#split = rs.ExplodeCurves(geom, True)
        	##print(rs.CurveDegree(geom))
        	#split = ghp.components.Explode(,True)
        	##print("Split",split)
        	tdeg = rs.CurveDegree(geom)
        	tcPo = len(t)
        	isArc = rs.IsArc(geom)
        	isCirc = rs.IsCircle(geom)
        	
        	##print("isCirc: ",isCirc)
        	pts = map(lambda g: Point(g), t) #object to points
        	#print(pts)
        	crv = {"type":"Feature","geometry":{'coordinates':pts, 'type': 'Spline','closed':tc, "deg":tdeg}}
        	#print("tdeg: "+str(tdeg))
        	#print("tcPo: "+str(tcPo))
        	#print(crv)
        	#print("isArc",isArc)
        	#print("----------------------------------")
        	if tdeg == 1 and tc  : del crv['geometry']['coordinates'][-1]
        	if tdeg == 1 and tcPo == 2  : crv['geometry']['type']='Line'
        	if tdeg >= 2                : crv['geometry']['type']='Spline'
        	if tdeg == 2 and tcPo == 3  :
        		#print("3ARC")
        		crv['geometry']['deg']=tdeg
        		# crv['geometry']['type']='Arc'
        	#if tdeg == 2 and tcPo == 5  :
        		#arc = rs.AddArc3Pt(pts[0],pts[4],pts[2])
        		#pt0 = convertToRPT(pts[0])
        		#pt2 = convertToRPT(pts[2])
        		#pt4 = convertToRPT(pts[4])
        		#arc = rh.Geometry.Arc(startPoint = pt0, pointOnInterior = pt2, endPoint = pt4)
        		
        		##print(crv['geometry']['coordinates'])
        		#del crv['geometry']['coordinates'][3]
        		#del crv['geometry']['coordinates'][1]
        	if isArc:
        		cpt = rs.ArcCenterPoint(geom)
        		testCrvPts = rs.CurvePoints(geom)
        		testCrvPts.append(testCrvPts[0])
        		testCrv = rs.AddPolyline(testCrvPts)
        		try:    isBig = bool(rs.PointInPlanarClosedCurve(cpt, testCrv))
        		except: isBig = False
        		cpt = Point(cpt)
        		crd = rs.ArcRadius(geom)
        		cang = rs.ArcAngle(geom)
        		##print("cang",cang)
        		crv['geometry']['type']='Arc'
        		crv['geometry']['radius'] = crd
        		crv['geometry']['isBig'] = isBig
        		crv['geometry']['center'] = cpt
        		crv['geometry']['angle'] = cang
        	if isCirc:
        		cpt = Point(rs.CircleCenterPoint(geom))
        		crd = rs.CircleRadius(geom)
        		crv['geometry']['type']='Circle'
        		crv['geometry']['coordinates'] = cpt
        		crv['geometry']['radius'] = crd
        	if prop:
        		crv['properties'] = json.loads(prop)
        	#if not isArc and crv["geometry"]['type'] == "Arc":
        		#crv['geometry']['type']='Spline'
        #    if tdeg >= 2:
        	#print (crv)
        	return crv
        def PolyCurve(geom, prop=None):
        	##print(geom)
        	
        	#print("PolyCurve def")
        	#print(geom, type(geom))
        	if str(type(geom)) != "<type 'List[object]'>":
        		geom = [geom]
        	polyCurveParts = []
        	#for d in dir(geom):
        	#    #print(d)
        
        	for part in geom:
        		#print(part)
        		tc = rs.ClosedCurveOrientation(part)
        		part = part.Explode()
        		curves = list(map(lambda x: Curve(x, {})["geometry"], part))
        		polycrvPart = {"type":"Feature","geometry":{'coordinates':curves, 'type': 'PolyCurvePart','closed':tc}}
        		polyCurveParts.append(polycrvPart)
        	polycrv = {"type":"Feature","geometry":{'coordinates':polyCurveParts, 'type': 'PolyCurve'}}
        	if prop:
        		polycrv['properties'] = json.loads(prop)
        	return polycrv
        
        	return crvCol
        otype = {
        		"PolyCurve": Curve,
        		"Line": Curve,
        		"LineCurve": Curve,
        		"NurbsCurve": Curve,
        		"ArcCurve": Curve,
        		"Circle": Curve,
        		"PolylineCurve": Curve,
        		"0": "Unknown object",
        		"1": Point,
        		"2": "PointCloud",
        		"3": PointCollection,
        		"4": Curve,
        		"5": Plane,
        		"6": Text,
        		"7": CurveCollection,
        		"8": "Surface",
        		"9": Image,
        		"16": "Polysurface",
        		"32": "Mesh",
        		"256": "Light",
        		"512": "Annotation",
        		"4096": "Instance reference",
        		"8192": "Text dot",
        		"16384": "Grip",
        		"32768": "Detail",
        		"65536": "Hatch",
        		"131072": "Morph control",
        		"134217728": "Cage",
        		"268435456": "Phantom",
        		"536870912": "Clipping plane",
        		"1073741824": "Extrusion"}
        def Convert(geom,prop):
        	#print("START CONVERT")
        	#print("SC",geom, type(geom))
        	ot = otype["1"]
        	try:
        		#print(type(geom))
        		#print("-"*100)
        		ot = otype[str(type(geom).__name__)]
        		try:
        			if ot.__name__ == "Curve":
        				try:
        					ngeom = rs.coercegeometry(geom)
        					ngeom = ghp.components.Explode(geom, True)
        					ngeom = ghp.components.JoinCurves(geom["segments"],True)
        					geom = ngeom
        				except Exception as e:
        					pass
        				#print("after explode",geom)
        				othertype = str(type(geom))
        				##print("othertype", othertype)
        				if othertype == "<type 'PolyCurve'>":
        					ot = PolyCurve
        					##print("POLYCURVE")
        				if othertype == "<type 'Line'>":
        					ot = Curve
        		except Exception as e:
        			#print("e",e)
        			pass
        		
        		
        	except Exception as e:
        		#print("1:", e)
        		#ot = otype[type(geom)]
        		othertype = str(type(geom))
        		print("othertype" ,othertype)
        		if othertype == "<type 'List[object]'>":
        			"object is in a List"
        			innertype = str(type(geom[0]))
        			#print("innertype",innertype,)
        			#if innertype=="<type 'PolylineCurve'>":
        				##print("PCrv")
        				#ot = otype["7"]
        			#if innertype=="<type 'PolyCurve'>":
        				##print("PolyCrv")
        				#ot = PolyCurve
        			if "Curve" in innertype:
        				#print("PCrv")
        				ot = otype["7"]
        			if innertype=="<type 'Point3d'>":
        				#print("PT3D")
        				ot = otype["3"]
        		if othertype == "<type 'Plane'>":
        			ot = otype["5"]
        		othertype = str(geom.__class__.__name__)
        		#print("othertype" ,othertype)
        		if othertype == "archJSONText":
        			ot = otype["6"]
        		if othertype == "archJSONImage":
        			ot = otype["9"]
        		if othertype == "<type 'instance'>":
        			if geom.type == "text":
        				ot = otype["6"]
        			if geom.type == "image":
        				ot = otype["9"]
        	#print("OBJECT TYPE ---- ", ot)
        	#print("GEOM ---- ", geom,type(geom))
        	return(ot(geom,prop)
        		)
        
        
        fillP = ["{}"]*len(G)
        for i, prop in enumerate(P):
        	fillP[i] = prop
        
        #print(G)
        P = map(lambda g,p: Convert(g,p), G,P)
        
        	
        J = json.dumps({"type":"FeatureCollection", "features":P})
        
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
        return System.Guid("32784c79-830e-4b8c-9fee-8ba09545a480")