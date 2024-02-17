#### Constants ####
global __var__
__var__ = {
	"guid":"bb0530c2-d04f-4c52-8fb1-7f81d876a193",
	
	"name":"SVG Layout",
	"nickname":"SVG Layout",
	"description":"Creates a SVG layout dictionary",
	"icon": "ghContent\\Icon-SVGLayout.png",

	"tabname":"Redback",
	"section":"SVG",

	"inputs":[
		{"name":"Plane",		"nickname":"P",	"objectAccess":"item",	"description":"", },
		{"name":"Dimensions",	"nickname":"D",	"objectAccess":"item",	"description":"", },
		{"name":"Scale",		"nickname":"S",	"objectAccess":"item",	"description":"", },
		{"name":"Location",		"nickname":"L",	"objectAccess":"item",	"description":"", },
	],
	"outputs":[
		{"name":"Layout",		"nickname":"L",	"description":"JSON Layout"},
		{"name":"Rectangle",	"nickname":"R",	"description":"Page Border"},
	]
}
__author__ = "Andrew.Butler"
__version__ = "2024.02.02"
#### Constants ####

import json
import scriptcontext as sc
import rhinoscriptsyntax as rs
import Rhino



millimeters = sc.doc.ModelUnitSystem.Millimeters
currentUnits = sc.doc.ModelUnitSystem


#### This assumes base units as millimeters

unitRatio = Rhino.RhinoMath.UnitScale(millimeters, currentUnits)

pageLUT = {
"A5":		(210,148),
"A5 (L)":	(210,148),
"A5 (P)":	(148,210),
"A4":		(297,210),
"A4 (L)":	(297,210),
"A4 (P)":	(210,297),
"A3":		(420,297),
"A3 (L)":	(420,297),
"A3 (P)":	(297,420),
"A2":		(594,420),
"A2 (L)":	(594,420),
"A2 (P)":	(420,594),
"A1":		(841,594),
"A1 (L)":	(841,594),
"A1 (P)":	(594,841),
"A0":		(1189,841),
"A0 (L)":	(1189,841),
"A0 (P)":	(841,1189),
}

plane = P
location = L
scale = S

DIM = D
if not DIM:
	DIM = "A3 (L)"
if location == None:
	location = True
if scale == None:
	scale = "1:1"

pageScale = float(scale.split(":")[1])/float(scale.split(":")[0])

if DIM in pageLUT:
	width, height = pageLUT[DIM]
elif DIM.replace(",","").isdigit():
	width, height = tuple(DIM.split(","))


combScale = unitRatio*pageScale
swidth = float(width)*combScale
sheight = float(height)*combScale

print(width, height)


#print(dir(plane))
#print(plane.PointAt.__doc__)
print(location)
if location:
	positionedPlaneOrig = plane.PointAt(swidth/-2.0,sheight/-2.0,0)
	svgPlaneOrig = plane.PointAt(swidth/-2.0,sheight/2.0,0)
else:
	positionedPlaneOrig = plane.PointAt(0,-sheight,0)
	svgPlaneOrig = plane.PointAt(0,0,0)
print(positionedPlaneOrig)
plane.Origin = positionedPlaneOrig
#print(rs.AddRectangle.__doc__)
R = rs.AddRectangle(plane,swidth,sheight )
plane.Origin = svgPlaneOrig
p = plane

L = json.dumps({"plane":[[p.Origin[0],p.Origin[1],p.Origin[2]],[p.XAxis[0],p.XAxis[1],p.XAxis[2]],[p.YAxis[0],p.YAxis[1],p.YAxis[2]]], "page":[width,height], "scale":combScale})