__author__ = "Andrew.Butler"
__version__ = "2024.02.02"
#### Constants ####
		
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

def errorLog(e, log=[]):
	import os, sys
	exc_type, exc_obj, exc_tb = sys.exc_info()
	other = sys.exc_info()[0].__name__
	fname = os.path.split(exc_tb.tb_frame.f_code.co_filename)[1]
	errorType = str(exc_type)
	errob = {"isError": True, "error":str(e), "errorType":errorType, "function":fname, "line":exc_tb.tb_lineno, "log":log}
	return errob
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
	pts = list(map(lambda g: Point(g), t))
	crv = {"type":"Feature","geometry":{'coordinates':pts, 'type': 'PointCollection'}}
	if prop:
		crv['properties'] = json.loads(prop)
	#print(crv)
	return crv
	
def CurveCollection(geom,prop=None):
	print('geom', geom)
	#print(geom.__class__.__name__)
	
	#crvs = list(map(lambda g: func(g), geom))
	crvs = []
	for g in geom:
		func = Curve
		print(g.__class__.__name__)
		if "PolyCurve" in g.__class__.__name__:
			func = PolyCurve
		crvs.append(func(g))
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
	print("Curve def")
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
	pts = list(map(lambda g: Point(g), t)) #object to points
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
		print("\n"+"#"*5)
		print(" "*5, geom, type(geom))
		print(dir(geom.Arc))
		try:
			cpt = geom.Arc.Center
			print(cpt, type(cpt))
			testCrvPts = rs.CurvePoints(geom)
			testCrvPts.append(testCrvPts[0])
			testCrv = rs.AddPolyline(testCrvPts)
			try:    isBig = bool(rs.PointInPlanarClosedCurve(cpt, testCrv))
			except: isBig = False
			cpt = Point(cpt)
			crd = geom.Arc.Radius

			cang = geom.Arc.Angle
			##print("cang",cang)
			crv['geometry']['type']='Arc'
			crv['geometry']['radius'] = crd
			crv['geometry']['isBig'] = isBig
			crv['geometry']['center'] = cpt
			crv['geometry']['angle'] = cang
		except Exception as e:
			print(errorLog(e))
			pass
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
	print("START CONVERT")
	#print("SC",geom, type(geom))
	ot = otype["1"]
	try:
		print(type(geom))
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
				print("othertype", othertype)

				if 'PolyCurve' in othertype:
					ot = PolyCurve
					##print("POLYCURVE")
				if 'Line' in othertype:
					ot = Curve
		except Exception as e:
			print(errorLog(e))
			pass
		
		
	except Exception as e:
		print(errorLog(e), "not simple geometry")
		#print("1:", e)
		#ot = otype[type(geom)]
		othertype = str(type(geom))
		print("othertype" ,othertype)
		if "list" in othertype:
			"object is in a List"
			innertype = str(type(geom[0]))
			print(innertype)
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
	print("OBJECT TYPE ---- ", ot)
	print("GEOM ---- ", geom,type(geom), prop)
	
	return(ot(geom,prop))
