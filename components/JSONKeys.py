#### Constants ####
global __var__
__var__ = {
	"guid":"c2790a28-9f47-43da-a0c7-538cae087e63",
	
	"name":"JSON Keys",
	"nickname":"JSON Keys",
	"description":"Returns all surface keys from a JSON string",
	"icon": "ghContent\\Icon-JSONKeys.png",

	"tabname":"Redback",
	"section":"JSON",

	"inputs":[
		{"name":"JSON",			"nickname":"J",	"objectAccess":"item",	"description":"JSON object to read", },
	],
	"outputs":[
		{"name":"Keys",	"nickname":"K",	"description":"Surface level keys."}
	]
}
__author__ = "Andrew.Butler"
__version__ = "2024.02.02"
#### Constants ####
        
import json
    
K = list(json.loads(J))