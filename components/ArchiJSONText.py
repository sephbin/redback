#### Constants ####
global __var__
__var__ = {
	"guid":"983d9511-87a8-479f-a688-dcef879bd7dc",
	
	"name":"Text Object",
	"nickname":"Text Object",
	"description":"Creates a text object that can be parsed by archJSON",
	"icon": "ghContent\\Icon-TextObject.png",

	"tabname":"Redback",
	"section":"SVG",

	"inputs":[
		{"name":"Plane",		"nickname":"P",	"objectAccess":"item",	"description":"", },
		{"name":"Text",			"nickname":"T",	"objectAccess":"item",	"description":"", },
	],
	"outputs":[
		{"name":"Layout",		"nickname":"O",	"description":"archJSON Text Object"},
	]
}
__author__ = "Andrew.Butler"
__version__ = "2024.02.02"
#### Constants ####

import Rhino as r
import rhinoscriptsyntax as rs
import json

class archJSONText:
    def __init__(self, location, content):
        self.location = location
        self.content = str(content)
    def __str__(self):
        return ("ArchJSON String: "+self.content)

O = archJSONText(P,T)