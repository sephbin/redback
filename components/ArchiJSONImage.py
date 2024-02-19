#### Constants ####
global __var__
__var__ = {
	"guid":"53cc467c-f4e5-4782-ac66-a8ce3084f988",
	
	"name":"Text Object",
	"nickname":"Text Object",
	"description":"Creates a text object that can be parsed by archJSON",
	"icon": "ghContent\\Icon-TextObject.png",

	"tabname":"Redback",
	"section":"SVG",

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