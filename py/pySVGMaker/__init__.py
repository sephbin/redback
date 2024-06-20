__author__ = "Andrew.Butler"
__version__ = "2024.02.02"
#### Constants ####

import rhinoscriptsyntax as rs
import sys
import json
import inspect
import Rhino as r
from string import Template

LOG = []
#The inputs to this node will be stored as a list in the IN variables.
#ARCHIJSON = J
#LAYOUT = L
#CSS = C

function_mappings = {
		'FeatureCollection':featurecollection,
		'Feature': feature,
		'Circle':circle,
		'Arc':arc,
		'Polyline': polyline,
		'Spline': spline,
		'PolyCurve': polycurve,
		'CurveCollection': curvecollection,
		'Line': line,
		'Text': text,
		'Image':image,
}


def errorLog(e, log=[]):
	import os, sys
	exc_type, exc_obj, exc_tb = sys.exc_info()
	other = sys.exc_info()[0].__name__
	fname = os.path.split(exc_tb.tb_frame.f_code.co_filename)[1]
	errorType = str(exc_type)
	errob = {"isError": True, "error":str(e), "errorType":errorType, "function":fname, "line":exc_tb.tb_lineno, "log":log}
	return errob

def checkRB(ob):
	if str(type(ob)) == "<type 'str'>":
		return json.loads(ob)
	else:
		return ob

def featurecollection(o):
	global obarray
	try:
		#print ('o', o)
		for i in o["features"]:
			#print ('i', i)
			#print("FEATURES---- ", i)
			fCall = function_mappings[i['geometry']['type']]

			props = {}
			try:
				props = i['properties']
			except:
				pass
			li = []
			li.append(i)
			#print(li)
			obarray.append(fCall(li,props))
	except Exception as e:
		print(errorLog(e))

def feature(o):
	pass
	
def point(o,type="standard"):
	#print(o)
	global lPlane
	global layout
	pt = r.Geometry.Point3d(o[0],o[1],o[2])
	remap = r.Geometry.Plane.RemapToPlaneSpace(lPlane, pt)
	rpt = remap[1]
	rpt = [rpt[0]/layout['scale'],-1*rpt[1]/layout['scale'],rpt[2]/layout['scale']]
	ptx = str(remap[1][0]/layout['scale'])
	pty = str(remap[1][1]/layout['scale']*-1)
	pt = "%s, %s" %(ptx, pty)
	ppt = "%s %s" %(ptx, pty)
	ptypes = {
		"standard": "%s, %s" %(ptx, pty),
		"plane": [rpt[0]/layout['scale'],-1*rpt[1]/layout['scale'],rpt[2]/layout['scale']],
		"line": [float(ptx),float(pty),0],
		"path": "%s %s" %(ptx, pty)
	}
	return ptypes[type]
	

def isCClockwise(pts):
	total = 0
	#print(pts)
	for index in range(len(pts)):
		p1 = pts[index]
		p2 = pts[(index+1) % len(pts)]
		
		f = (p2[0]-p1[0])*((p2[1]*-1)+(p1[1]*-1))
		total += f
		#print("t",total, p1,p2)
	
	cclockwise = total < 0
	#print("cclockwise:",cclockwise)
	return cclockwise

def arc(o, props, collection = False, outType=None, closed = False, skipFirst = False, baseOb = None):
	#print(o)
	#<path d="M 80 80 A 45 45, 0, 0, 0, 125 125">
	#print("\n"+"#"*5, o)
	#print("#"*5, baseOb)

	if baseOb:
		if type(baseOb) != type([]):
			baseOb = [baseOb]
		o = baseOb
	try:
		arcList = []
		arcCoordsList = []
		for items in o:
			#print("#"*6, items)
			if "geometry" in items:
				items = items['geometry']

			pts = list(map(lambda i: point(i,'line'), items['coordinates']))

			radius = items["radius"]
			angle = items["angle"]
			cclockwise = isCClockwise(pts)
			isBig = items["isBig"]
			largeCircleFlag = str(int(cclockwise))
			
			testflag = False
			largeCircleFlag = True
			if not isBig and cclockwise:
				testflag = False
				largeCircleFlag = False
			if not isBig and not cclockwise:
				testflag = True
				largeCircleFlag = False
			if isBig and not cclockwise:
				testflag = True
				largeCircleFlag = True
			if isBig and cclockwise:
				testflag = False
				largeCircleFlag = True
			largeCircleFlag = str(int(largeCircleFlag))
			testflag = str(int(testflag))
			
			#print(largeCircleFlag,testflag)
			propString = []
			for k in props:
				propString.append(str(k)+'=\"'+str(props[k])+'\"')
			pString = " ".join(propString)
			# pString = pString.split('="')
			# print("arc pstring", pString)
			# pString = pString[0] + '= "fill:none;' + pString[1]
			coordString = ["M %.2f %.2f"%(pts[0][0],pts[0][1])]
			if skipFirst:
				coordString= []
			coordString = coordString + ['A %.2f %.2f, 0, %s, %s, %.2f %.2f'%(radius,radius,largeCircleFlag,testflag,pts[-1][0],pts[-1][1])]
			arcCoords = "".join(coordString)
			#print("@"*6, arcCoords)
			arc = '<path d="%s" %s/>'%(arcCoords,pString)
			#pline = NurbsCurve.ByControlPoints(polarray,1,o['properties']['closed'])
			arcList.append(arc)
			arcCoordsList.append(arcCoords)

		arcList = " ".join(arcList)
		arcCoordsList = " ".join(arcCoordsList)
		
		if "coordinates" in outType:
			return arcCoordsList
		else:
			return arcList
	except Exception as e:
		print(errorLog(e))
		return ""
def line(o, props, collection = False):
	#<polyline points="118.85,155.3 118.85,180.7" class="FOPActual" zindex="10"/>
	try:
		ptList = []
		for items in o:
			items = items['geometry']
			#print(o)
			pts = list(map(lambda i: point(i, "line"), items['coordinates']))
			propString = []
			for k in props:
				#print(k)
				#print(props[k])
				propString.append(str(k)+'=\"'+str(props[k])+'\"')
			pString = " ".join(propString)
			pline = '<line x1="'+str(pts[0][0])+'" y1="'+str(pts[0][1])+'" x2="'+str(pts[1][0])+'" y2="'+str(pts[1][1])+'" '+pString+'/>'
			#pline = NurbsCurve.ByControlPoints(polarray,1,o['properties']['closed'])
			
			ptList.append(pline)
		ptList = " ".join(ptList)
		return ptList
	except Exception as e:
		print(errorLog(e))
		return ""
def polyline(o, props, closed = False ,collection = False):
	#<polyline points="118.85,155.3 118.85,180.7" class="FOPActual" zindex="10"/>
	ptList = []
	pStringList = []
	#print(o)
	for cords in o:
		closed = cords['geometry']['closed']
		#print("CORDS ---", cords)
		polarray = []
		for p in cords['geometry']['coordinates']:
			pt = point(p)
			polarray.append(pt)
		
		pts = " ".join(polarray)
		propString = []
	
		for k in props:
			propString.append(str(k)+'=\"'+str(props[k])+'\"')
		pString = " ".join(propString)
		if collection == True:
			if closed:
				pts = "M %s Z" % (pts)
			else: pts = "M %s" % (pts)
		ptList.append(pts)
		pStringList.append(pString)
	pStringList = " ".join(pStringList)
	if not closed:
		if pString:
			pString = pString.split('="')
			pString = pString[0] + '= "fill:none;' + pString[1]
	ptList = " ".join(ptList)
	if collection == True: 
		pline = '<path d=\"'+ptList+'\"  '+ pString +'/>'
	else:
		if closed:
			pline = '<polygon points=\"'+ptList+'\" '+pStringList+'/>'
		else:
			pline = '<polyline points=\"'+ptList+'\" '+pString+'/>'
			#pline = NurbsCurve.ByControlPoints(polarray,1,o['properties']['closed'])

	return pline
	
def curvecollection(o, props):
	#<path d="M10 10 H 90 V 90 H 10 L 10 10"/>
	print("CURVE COLLECTION")
	try:
		o = o[0]["geometry"]["coordinates"]
		#print(o)
		# in types in o["coordinates"]
		typeDic = {}
		for items in  o:
			# print ("curvecollection ", items)
			groupType = items['geometry']['type']
			if str(groupType) in typeDic:
				typeDic[str(groupType)].append(items)
			else:
				typeDic[str(groupType)] = [items]
		#print("GROUP BY TYPES --", typeDic)
		
		ptListCombined = []
		for typeKey in typeDic:
			fCall = function_mappings[str(typeKey)] #get group type
			print(fCall)
			#typeDicList.append(typeDic[str(typeKey)])
			ptList = fCall(typeDic[str(typeKey)], props, outType = "flatcoordinates")
			print(ptList)
			ptListCombined.append(ptList)
		
		ptListCombined = ' '.join(ptListCombined)
		propString = []
		for k in props:
			propString.append(str(k)+'=\"'+str(props[k])+'\"')
		pString = " ".join(propString)
		
		ptListCombined = '<path d="'+ptListCombined+'" '+pString+'/>'
		return ptListCombined
	except Exception as e:
		print(errorLog(e))
		return ""
   
def avgpts(p1, p2, w1=0.5, w2=0.5):
	p3 = []
	#LOG.append(" "*20+"avgpts"+str([p1,p2,w1,w2]))
	for p1p, p2p in zip(p1, p2):
		p3.append((p1p*w1)+(p2p*w2))
	#LOG.append(" "*20+"avgpts"+str([p3]))
	return p3
def spline_1deg(initCoords, props, outType=None, closed = True, skipFirst = False, baseOb = None):
	try:
		polarray = []
		#print("spline_1deg",initCoords)
		initCoords = [initCoords]
		knotPtsList = initCoords
		beizerPtsList = []
		coordStringList = []
		#LOG.append(" knotPtsList --- "+str(knotPtsList))#########################################################
		
		for sPts in initCoords:
			coordString = []
			start = sPts.pop(0)
			start = point(start, 'line')
			start = start[:-1]
			start = list(map(lambda x: "{:.2f}".format(float(x)), start))
			if not skipFirst:
				coordString.append(" M"+",".join(start))
			for pts in sPts:
				pts = point(pts, 'line')
				pts = list(map(lambda x: "{:.2f}".format(float(x)), pts))
				pts = pts[:-1]
				coordString.append(" L"+",".join(pts))

			coordString = "".join(coordString)
			#LOG.append(" coordString --- "+str(coordString))#########################################################
			if closed:
				coordString = coordString+"Z"
			coordStringList.append(coordString)
		#LOG.append(" coordStringList --- "+str(coordStringList))#########################################################
		propString = []
		for k in props:
			propString.append(str(k)+'=\"'+str(props[k])+'\"')
		pString = " ".join(propString)
		coordStringList = " ".join(coordStringList)
		pline = '<path d="'+coordStringList+'" '+pString+'/>'
		#pline = NurbsCurve.ByControlPoints(polarray,1,o['properties']['closed'])
		#LOG.append("pline"+str(pline))#########################################################
		#print("bp",bezierPts)    
		if "coordinates" in outType:
			return coordStringList
		else:
			return pline
	except Exception as e:
		print(e)
		LOG.append(e)
		return "<>"
		pass

def spline_2deg(initCoords, props, outType=None, closed = True, skipFirst = False, baseOb = None):
	polarray = []
	
	#print("SPLINE COORDS -- ", initCoords)
	initCoords = [initCoords]
	knotPtsList = []
	beizerPtsList = []
	
	for index, p in enumerate(initCoords):
		for indvPts in p:
			
			pnt = point(indvPts, 'line')
			pt = [pnt[0],pnt[1]]
			polarray.append(pt)
			polarray_len = len(polarray)
		bezierPts = []
		knotPts = []
		#print("polLen",polarray_len)
		if closed:
			#print("closed")
			for index in range(polarray_len-1):
				pt1 = polarray[index % len(polarray)]
				pt2 = polarray[(index+1) % len(polarray)]
				pt3 = polarray[(index+2) % len(polarray)]
				
				pt4 = avgpts(pt1,pt2,(1.0/6.0),(5.0/6.0))
				pt5 = avgpts(pt2,pt3,(5.0/6.0),(1.0/6.0))
				#print("par",pt1,pt2,pt3,pt4)
				bezierPts.append(pt4)
				bezierPts.append(pt5)
				a = bezierPts
			#fpoint = bezierPts.pop(0)
			#bezierPts.append(fpoint)
			# fpoint = bezierPts.pop(0)
			bezierPts = bezierPts[:-2]
			for index in range(int(len(bezierPts)/2.0)):
				index = index*2
				pt1 = bezierPts[(index+1) % len(bezierPts)]
				pt2 = bezierPts[(index+2) % len(bezierPts)]
				#print("avg",index,index+1,pt1,pt2)
				knotPts.append(avgpts(pt1,pt2))
			#bezierPts.append(fpoint)
			#bezierPts = bezierPts[-1:]+bezierPts[:-1]
			knotPts = knotPts[-1:]+knotPts[:-1]
			knotPts = knotPts+[knotPts[0]]
		else:
			mode = "normal"
			if len(polarray) == 3:
				mode = "3pt"
			#print("_"*10+mode)
			#print("p",len(polarray),polarray)
			#if mode == "normal":
				#apPt = avgpts(polarray[0],polarray[1],1.0/3.0,2.0/3.0)
				#bezierPts.append(apPt)
			if mode == "3pt":
				apPt1 = avgpts(polarray[0],polarray[1],(1.0/3.0),(2.0/3.0))
				apPt2 = avgpts(polarray[1],polarray[2],(2.0/3.0),(1.0/3.0))
				bezierPts.append(apPt1)
				bezierPts.append(apPt2)
			if mode == "normal":
				for index in range(int((polarray_len-1)/2)):
					index = index*2
					#print("index",index)
					p1 = avgpts(polarray[index+0],polarray[index+1],(1.0/3.0),(2.0/3.0))
					p2 = avgpts(polarray[index+1],polarray[index+2],(2.0/3.0),(1.0/3.0))
					#print("bez",p1)
					bezierPts.append(p1)
					bezierPts.append(p2)
			if mode == "normal":
				bezierPts.append(avgpts(polarray[-1],polarray[-2],1.0/3.0,2.0/3.0))
			#print(len(bezierPts),bezierPts)
			knotPts = [polarray[0]]+knotPts
			if mode == "normal":
				for index in range(int((len(bezierPts)-2)/2.0)):
					index = (index*2)+1
					p1 = bezierPts[index]
					p2 = bezierPts[index+1]
					p3 = avgpts(p1,p2)
					#print("kp",index,p1,p2)
					knotPts.append(p3)
			knotPts.append(polarray[-1])
		knotPtsList.append(knotPts)
		beizerPtsList.append(bezierPts)
	#print("knotPtsList",knotPtsList)
	#print("beizerPtsList",beizerPtsList)
	coordStringList = []
	#print(len(knotPts),knotPts)
	#print(len(bezierPts),bezierPts)
	for sPts in range(len(knotPtsList)):
		splineCoords = []
		#print('SPTS == ', sPts)
		knotPts = knotPtsList[sPts]
		bezierPts = beizerPtsList[sPts]
		#print("KNOTS --- ", knotPts)
		
		for index, knotPt in enumerate(knotPts):
			rspoints = [knotPt]
			try:
				if index == 0:  rspoints = [knotPt, bezierPts[index*2], bezierPts[(index*2)+1]]
				else:           rspoints = [knotPt, bezierPts[(index*2)+1]]
			except: pass
			for rspoint in rspoints:
				for coord in rspoint:
					splineCoords.append("%.2f"%(coord))
		coordString= []
		if not skipFirst:
			coordString= ["M"]

		coordString = coordString+list(" , ".join(splineCoords))
		delimIndex = 0
		for index, char in enumerate(coordString):
			#print(index, char)
			if char == ",":
				if delimIndex == 1:
					if not skipFirst:
						coordString[index] = "C"
					else:
						coordString[index] = "~C"

				if (delimIndex-7) % 4 == 0 and delimIndex >= 7:
					coordString[index] = "S"
				delimIndex += 1
		coordString = "".join(coordString)
		#print("-"*20, coordString)
		if closed:
			coordString = coordString+"Z"
		coordStringList.append(coordString)
	propString = []
	for k in props:
		propString.append(str(k)+'=\"'+str(props[k])+'\"')
	pString = " ".join(propString)
	coordStringList = "".join(coordStringList)
	if skipFirst:
		coordStringList = coordStringList.split("~")[1]
	pline = '<path d="'+coordStringList+'" '+pString+'/>'
	#print(pline)
	#pline = NurbsCurve.ByControlPoints(polarray,1,o['properties']['closed'])
	if "coordinates" in outType:
		#print("coordStringList"+"__"+str(coordStringList))
		return coordStringList
	else:
		return pline
	
def spline_3deg(initCoords, props, outType=None, closed = True, skipFirst = False, baseOb = None):
	try:
		#<polyline points="118.85,155.3 118.85,180.7" class="FOPActual" zindex="10"/>
		polarray = []
		#LOG.append("CLOSED ---"+str(closed))#########################################################
		#LOG.append("SPLINE COORDS -- "+str(initCoords['coords']))####################################
		initCoords = [initCoords]
		
		knotPtsList = []
		beizerPtsList = []
		for indvCoords in initCoords:
			polarray = []
			#LOG.append("SPLINE COORD -- "+str(indvCoords))###########################################
			for index, p in enumerate(indvCoords):
				pnt = point(p, type = 'line')
				#LOG.append("   pnt ----- "+str(pnt))
				
				pt = [pnt[0],pnt[1]]
				#print('pnt', pnt)
				#print('pt', pt)
				polarray.append(pt)
			polarray_len = len(polarray)
			bezierPts = []
			knotPts = []
			#LOG.append("  polarray ---- "+str(polarray))
			if closed:
				for index in range(polarray_len-1):
					pt1 = polarray[index]
					pt2 = polarray[(index+1) % len(polarray)]
					pt3 = avgpts(pt1,pt2,float(2.0/3.0),float(1.0/3.0))
					pt4 = avgpts(pt1,pt2,float(1.0/3.0),float(2.0/3.0))
					#print("par",pt1,pt2,pt3,pt4)
					#LOG.append(" "*10+"pt3,pt4 ----- "+str([pt3,pt4]))
					bezierPts.append(pt3)
					bezierPts.append(pt4)
				
				fpoint = bezierPts.pop(0)
				bezierPts.append(fpoint)
				#print(bezierPts)
				#LOG.append("   bezierPts ----- "+str(bezierPts))
				#LOG.append("   runningKnots")
				for index in range(int(len(bezierPts)/2.0)-1):
					index = index*2
					pt1 = bezierPts[index]
					pt2 = bezierPts[(index+1) % len(bezierPts)]
					#print("avg",index,index+1,pt1,pt2)
					avgresult = avgpts(pt1,pt2)
					#LOG.append("   avgresult ----- "+str(avgresult))
					knotPts.append(avgresult)
				fpoint = bezierPts.pop(0)
				bezierPts.append(fpoint)
				#print("bpts", bezierPts)
			else:
				mode = "normal"
				if len(polarray) == 4:
					mode = "4pt"
				#LOG.append("  mode ---- "+str(mode))
				if mode == "normal":
					bezierPts.append(avgpts(polarray[1],polarray[2]))
				for index in range(polarray_len-5):
					bezierPts.append(avgpts(polarray[index+2],polarray[index+3],float(2.0/3.0),float(1.0/3.0)))
					bezierPts.append(avgpts(polarray[index+2],polarray[index+3],float(1.0/3.0),float(2.0/3.0)))
				if mode == "normal":
					bezierPts.append(avgpts(polarray[-2],polarray[-3]))
				#LOG.append("   adding to knotPts ----- "+str(polarray[0])+str(knotPts))
				knotPts = [polarray[0]]+knotPts
				if mode == "normal":
					for index in range(int(len(bezierPts)/2.0)):
						index = index*2
						p1 = bezierPts[index]
						p2 = bezierPts[index+1]
						p3 = []
						for p1p, p2p in zip(p1, p2):
							p3.append((p1p*(1.0/2.0))+(p2p*(1.0/2.0)))
						knotPts.append(p3)
				knotPts.append(polarray[-1])
				bezierPts = [polarray[1]]+bezierPts
				bezierPts.append(polarray[-2])
		
				#print('SPLINE OBJECT --', splineObject)
			beizerPtsList.append(bezierPts)
			knotPtsList.append(knotPts)
		
		coordStringList = []
		#LOG.append(" knotPtsList --- "+str(knotPtsList))#########################################################
		for sPts in range(len(knotPtsList)):
			splineCoords = []
			#print(sPts)
			knotPts = knotPtsList[sPts]
			bezierPts = beizerPtsList[sPts]
			#print("KNOTS --- ", knotPts)
			for index, knotPt in enumerate(knotPts):
				#print(index)
				#print(len(knotPts))
				rspoints = [knotPt]
				try:
					if index == len(knotPts)-1: pass
					elif index == 0:  rspoints = [knotPt, bezierPts[index*2], bezierPts[(index*2)+1]]
					else:           rspoints = [knotPt, bezierPts[(index*2)+1]]
				except: pass
				#print("RS POINTS --- ", rspoints)
				for rspoint in rspoints:
					for coord in rspoint:
						splineCoords.append("%.2f"%(coord))
			#print (splineCoords)
			# #LOG.append(" splineCoords --- "+str(splineCoords))#########################################################
			coordString = list(" , ".join(splineCoords))
			if not skipFirst:
				coordString = ["M"]+list(" , ".join(splineCoords))
			
			delimIndex = 0
			for index, char in enumerate(coordString):
				if char == ",":
					if delimIndex == 1:
						coordString[index] = "C"
					if (delimIndex-7) % 4 == 0 and delimIndex >= 7:
						coordString[index] = "S"
					delimIndex += 1
			coordString = "".join(coordString)
			#LOG.append(" coordString --- "+str(coordString))#########################################################
			if closed:
				coordString = coordString+"Z"
			coordStringList.append(coordString)
		#LOG.append(" coordStringList --- "+str(coordStringList))#########################################################
		propString = []
		for k in props:
			propString.append(str(k)+'=\"'+str(props[k])+'\"')
		pString = " ".join(propString)
		coordStringList = " ".join(coordStringList)
		pline = '<path d="'+coordStringList+'" '+pString+'/>'
		#pline = NurbsCurve.ByControlPoints(polarray,1,o['properties']['closed'])
		#LOG.append("pline"+str(pline))#########################################################
		#print("bp",bezierPts)    

		if "coordinates" in outType:
			return coordStringList
		else:
			return pline
	except Exception as e:
		print(errorLog(e))
		return ""

def spline(o, props, outType=None, closed = True, skipFirst = False, baseOb = None, collection = False):
	#outType = None, "coordinates", 
	#print("\n"+"#"*5+"spline", o)
	try:
		
		splineFuncs = {
		"spline_1deg" : spline_1deg,
		"spline_2deg" : spline_2deg,
		"spline_3deg" : spline_3deg,
		"spline_arc"  : arc,
		}
		splineObject = ""
		
		for segmentIndex, coords in enumerate(o):
			#print("\n"+"#"*5+"coords", coords)
			if "geometry" in coords:
				coords = coords["geometry"]
			deg = coords["deg"]
			#print("coords", deg)
			func = splineFuncs["spline_"+str(deg)+"deg"]
			if coords["type"].lower() == "arc":
				func = splineFuncs["spline_arc"]
			
			skipFirst = True
			if segmentIndex == 0:
				skipFirst = False
			if collection:
				skipFirst = False
				outType = "coordinates"
			print(func)
			appendSpline = func(coords["coordinates"], props, outType = outType, closed = coords['closed'], skipFirst=skipFirst, baseOb=coords)
			
			#print("appendSpline", appendSpline)
			splineObject = splineObject+appendSpline
			#print("splineObject",splineObject)

			#try:
				#coords = coords['geometry']
			#except:
				#pass
			#if coords['deg'] == 1:
				#initCoords1d['coords'].append(coords['coordinates'])
				#initCoords1d['closed'] = coords['closed']        
			#if coords['deg'] == 2:
				#initCoords2d['coords'].append(coords['coordinates'])
				#initCoords2d['closed'] = coords['closed']        
			#if coords['deg'] == 3:
				#initCoords3d['coords'].append(coords['coordinates'])
				#initCoords3d['closed'] = coords['closed']

			#print(initCoords3d)
		
		#if len(initCoords1d['coords']) >= 1:
			#print ("1 degree")
			#splineObject = spline_1deg(initCoords1d, props, outType, closed = initCoords1d['closed'])

		#if len(initCoords2d['coords']) >= 1:
			#print ("2 degree")
			#splineObject = spline_2deg(initCoords2d, props, outType, closed = initCoords2d['closed'])
			
		#elif len(initCoords3d['coords']) >= 1:
			#print ("3 degree")
			#splineObject = spline_3deg(initCoords3d, props, outType, closed = initCoords3d['closed'])
		#knotPts = splineObject['knotPts']
		#bezierPts = splineObject['bezierPts']

		if "coordinates" in outType:
			propString = []
			for k in props:
				propString.append(str(k)+'=\"'+str(props[k])+'\"')
			pString = " ".join(propString)
			splineObject = "".join(splineObject)
			pline = '<path d="'+splineObject+'" '+pString+'/>'
			if "flat" in outType:
				return splineObject
			return pline
		return splineObject
	except Exception as e:
		LOG.append("spline: "+str(errorLog(e)))
		print(errorLog(e))
		return ""
def polycurve(o, props, outType="coordinates", collection = False):
	#print(o)
	try:
		parts = []
		for part in o:
			part = part["geometry"]["coordinates"]
			polycurvePath = []
			for curve_i, curve in enumerate(part):
				#for curveInfo in curve['geometry']["coordinates"]:
				#print(curve)
				svgCurve = spline(curve["geometry"]["coordinates"],{}, outType = outType)
				polycurvePath.append(svgCurve)
		polycurvePath = ' '.join(polycurvePath)
		

		return polycurvePath
	except Exception as e:
		LOG.append("polycurve"+str(errorLog(e)))
		print(errorLog(e))
		return ""

	
def circPath(cx,cy,r, cclockwise):
	#NOTE : Assumes direction pattern is T/F
	#"M" + cx + "," + cy + "m" + (r) + ",0" + "a" + r + "," + r + " 0 1,1 " + (-r * 2) + ",0" + "a" + r + "," + r + " 0 1,1 " + (r * 2) + ",0"
	circPath = "M %.2f, %.2f m %.2f, 0 a %.2f, %.2f 0 1,%s %.2f, 0 a %.2f, %.2f 0 1, %s %.2f, 0"%(cx, cy, r, r, r, cclockwise, -r*2, r, r, cclockwise, r*2)
	return circPath


def circle(o, props, collection = False):
	global layout
	#<circle cx="50" cy="50" r="50"/>
	
	circList = []
	for index, coords in enumerate(o):
		coords = coords['geometry']
		#print(coords['coordinates'])
		pt = point(coords['coordinates'], 'line')
		propString = []
		for k in props:
			#print(k)
			#print(props[k])
			propString.append(str(k)+'=\"'+str(props[k])+'\"')
		pString = " ".join(propString)
		
		#pline = NurbsCurve.ByControlPoints(polarray,1,o['properties']['closed'])
		if collection:
			cclockwise = index%2
			#print (cclockwise)
			circ = circPath(pt[0],pt[1],coords['radius']/layout['scale'], cclockwise)
			#print (circ)
			#circ = ''.join([str(item) for item in circ])
		else:
			circ = '<circle cx="%.2f" cy="%.2f" r="%.2f" %s/>'%(pt[0],pt[1],coords['radius']/layout['scale'],pString)
		circList.append(circ)
	print (circList)
	
	circList = " ".join(circList)
	if collection:
		circList = '<path d= "%s" %s/>'%(circList, pString)
	return circList

def text(o, props):
	#<text class="CLASS" zindex="500" transform="matrix(1 0 0 1 210 148.5)">TEXT</text> 
	propString = []
	for k in props:
		propString.append(str(k)+'=\"'+str(props[k])+'\"')
	pString = " ".join(propString)
	gm = o['coordinates']
	planeorigin = [gm[3],gm[7],gm[11]]
	planeremap = point(planeorigin,False)
	matrix = [str(gm[0]), str(gm[1]),str(gm[4]),str(gm[5]), str(planeremap[0]), str(planeremap[1])]
	matrixtext = " ".join(matrix)
	text = '<text '+pString+' transform="matrix(%s)">%s</text>'%(matrixtext,o['content'])
	#print(text)
	return text
def image(o, props):
	global layout
	global lPlane
	#<image width="132.543857" height="132.543857" transform="matrix(-0.173648 -0.984808 0.984808 -0.173648 28.869475 131.454367)" xlink:href="K:\Computational Design Group\20.png"'"/>
	propString = []
	for k in props:
		propString.append(str(k)+'=\"'+str(props[k])+'\"')
	pString = " ".join(propString)
	gm = o['coordinates']
	yvec = [gm[1]*o['height'],gm[5]*o['height'],gm[9]*o['height']]
	planeorigin = [gm[3]+yvec[0],gm[7]+yvec[1],gm[11]+yvec[2]]
	planeremap = point(planeorigin,False)
	imgPlane = r.Geometry.Plane(r.Geometry.Point3d(gm[3],gm[7],gm[11]),r.Geometry.Vector3d(gm[0],gm[4],gm[8]),r.Geometry.Vector3d(gm[1],gm[5],gm[9]))
	worldPlane = r.Geometry.Plane(r.Geometry.Point3d(0,0,0),r.Geometry.Vector3d(1,0,0),r.Geometry.Vector3d(0,1,0))
	transform = r.Geometry.Transform.PlaneToPlane(lPlane,worldPlane)
	imgPlane.Transform(transform)
	npln = [imgPlane.XAxis.X,imgPlane.YAxis.X,imgPlane.ZAxis.X,imgPlane.OriginX,imgPlane.XAxis.Y,imgPlane.YAxis.Y,imgPlane.ZAxis.Y,imgPlane.OriginY,imgPlane.XAxis.Z,imgPlane.YAxis.Z,imgPlane.ZAxis.Z,imgPlane.OriginZ,0,0,0,1]
	matrix = [str(npln[0]), str(npln[1]),str(npln[4]),str(npln[5]), str(planeremap[0]), str(planeremap[1])]
	matrixtext = " ".join(matrix)
	text = '<image transform="matrix(%s)" width="%s" height="%s" xlink:href="%s"/>'%(matrixtext,o['width']/layout['scale'],o['height']/layout['scale'],o['image'])
	#print(text)
	return text



#ARCHIJSON = list(map(lambda u: checkRB(u),ARCHIJSON))
def archiJsonToSVG(ARCHIJSON=[{}], LAYOUT={}, CSS=""):
	if type(ARCHIJSON) == type({}):
		ARCHIJSON = [ARCHIJSON]
	if type(CSS) != type([]):
		CSS = [CSS]
	array = []
	global obarray
	global lPlane
	global layout
	obarray = []
	layout = json.loads(LAYOUT)
	lPlane = layout['plane']
	lOrigin = r.Geometry.Point3d(lPlane[0][0],lPlane[0][1],lPlane[0][2])
	lXAx = r.Geometry.Vector3d(lPlane[1][0],lPlane[1][1],lPlane[1][2])
	lYAx = r.Geometry.Vector3d(lPlane[2][0],lPlane[2][1],lPlane[2][2])
	lPlane = r.Geometry.Plane(lOrigin,lXAx,lYAx)
	try:
		for i in ARCHIJSON:
			if i["type"]:
				fCall = function_mappings[i['type']]
				fCall(i)
			else:
				fCall = function_mappings[i['geometry']['type']]
				obarray.append(fCall(i['geometry']))

		#OUT = log
		template =Template("""<?xml version="1.0" encoding="utf-8"?>
		<!-- Generator: Grasshopper 1.0.0, Redback Plugin. SVG Version: 6.00 Build 0) Made by Andrew Butler at Cox Architecture -->
		<!DOCTYPE svg PUBLIC "-//W3C//DTD SVG 1.1//EN" "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd">
		<svg version="1.1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" x="0px" y="0px"
		width="${width}mm" height="${height}mm" viewBox="0 0 ${width} ${height}" xml:space="preserve">
		<style type="text/css">
		$css
		</style>
		$svg
		</svg>""")
		#print(layout)
		svgdict = {'svg':"\n".join(obarray),'width':layout['page'][0],'height':layout['page'][1],'css':"\n".join(CSS)}
		svgfull = template.substitute(svgdict)
		return svgfull
		#SVG = svgfull.split("\n")
		#S = SVG
		#L = log


	except Exception as e:
		print(errorLog(e))
		LOG.append("based error: "+str(errorLog(e)))
		S = None
		pass
