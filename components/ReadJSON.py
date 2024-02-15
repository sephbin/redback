#### Constants ####
global __var__
__var__ = {
	"guid":"33f7772a-c9ff-4356-915e-6a0b99bf4fcc",
	
	"name":"Read JSON",
	"nickname":"Read JSON",
	"description":"Reads keys from a JSON string. It can use javascript style key notation (a.b.c)",
	"icon": "ghContent\\Icon-ReadJSON.png",

	"tabname":"Redback",
	"section":"JSON",

	"inputs":[
		{"name":"JSON",			"nickname":"J",	"objectAccess":"list",	"description":"JSON object to read", },
		{"name":"Key",			"nickname":"K",	"objectAccess":"item",	"description":"Key to access from JSON", },
	],
	"outputs":[
		{"name":"Value",	"nickname":"V",	"description":"Returned value from JSON"}
	]
}
__author__ = "Andrew.Butler"
__version__ = "2024.02.02"
#### Constants ####
        
import json
def deepGet(key, object):
    try:
        keylist = key.split(".")
        for ki, k in enumerate(keylist):
            try: k = int(k)
            except: pass
            print(k,type(k))
            if ki < len(keylist)-1:
                if type(k) == type("") and k not in object:
                    object[k] = {}
                object = object[k]
            else:
                print("last",k, object)
                returnOb = object[k]
                return returnOb
    except Exception as e:
        print(e)
        return None
        
        
out = []
for o in J:
    o = json.loads(o)
    apob = deepGet(K,o)
    print(apob)
    if type(apob) == type([]) or type(apob) == type({}):
        apob = json.dumps(apob)
    out.append(apob)
        
V = out