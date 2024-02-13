#### Constants ####
global __var__
__var__ = {
	"guid":"0e4b6507-e8be-483f-8d55-21c261e5bbcb",
	
	"name":"Create JSON",
	"nickname":"JSON",
	"description":"Creates a JSON string from a list of matching keys and values",
	"icon": "ghContent\\Icon-MakeJSON.png",

	"tabname":"Redback",
	"section":"JSON",

	"inputs":[
		{"name":"Keys",		"nickname":"K",	"objectAccess":"list",	"description":"List of Keys", },
		{"name":"Values",	"nickname":"V",	"objectAccess":"list",	"description":"List of Values", },
	],
	"outputs":[
		{"name":"JSON",		"nickname":"J",	"description":"JSON Output"},
	]
}
__author__ = "Andrew.Butler"
__version__ = "2024.02.02"
#### Constants ####

import json

def convertObs(ob):
	try:
		ob = json.loads(ob)
		return ob
	except:
		return ob

V = list(map(lambda x: convertObs(x),V))

outdict = dict(zip(K,V))
J = json.dumps(outdict)