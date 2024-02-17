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

import Rhino as r
import rhinoscriptsyntax as rs
import json

L = LOCATION

T = TEXT


class text:
    def __init__(self, location, content):
        self.location = location
        self.content = str(content)
        self.type = "text"

O = text(L,T)

print O.location
print O.content



