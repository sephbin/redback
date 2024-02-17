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

import rhinoscriptsyntax as rs
import Rhino as r




class image:
    def __init__(self, rec, img):
        p = rs.CurvePoints(rec)
        self.location = rs.PlaneFromPoints(p[0],p[1],p[2])
        self.bounds = rs.BoundingBox(rec,self.location,False)
        self.width = self.bounds[2][0]
        self.height = self.bounds[2][1]
        self.img = str(img)
        self.type = "image"

O = image(REC,IMG)