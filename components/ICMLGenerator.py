#### Constants ####
global __var__
__var__ = {
	"guid":"89da792a-24b2-41fa-a674-20d9df2b5b6f",
	
	"name":"ICML Generator",
	"nickname":"ICML",
	"description":"Takes JSON data formated as a lists of lists and creates a InCopy Markup Language format of a table.",
	"icon": "ghContent\\Icon-TableMaker.png",

	"tabname":"Redback",
	"section":"Tables",

	"inputs":[
		{"name":"Data",		"nickname":"D",	"objectAccess":"item", "objectType":"String",	"description":"Table data formated as a JSON list of lists", },
		{"name":"Options",	"nickname":"O",	"objectAccess":"list",	"description":"Table options as a JSON dictionary"},
		{"name":"CSS",		"nickname":"C",	"objectAccess":"list",	"description":"CSS to apply styles to the table"}
	],
	"outputs":[
		{"name":"Table",	"nickname":"T",	"description":"ICML Table"}
	]
}
__author__ = "Andrew.Butler"
__version__ = "2024.02.02"
#### Constants ####

import sys
import os
user = os.environ['USERPROFILE']
import json
try:    sys.path.append(r"\\sydsrv01\projects\Computational Design Group\93_Development\PackageManager\redback\py".format(**{'user':user}))
except: pass
try:    sys.path.append(r"E:\mydev\redback\py".format(**{'user':user}))
except: pass
import pyTableMaker as tm
from pyCSSParser import parseCSS

__log__ = []


__log__.append(str(C))

D = json.loads(D)
if not O:
	O = {"css":[]}
else:
	O = json.loads("".join(O))

if C:
	C = "".join(C)
	C = parseCSS(C)
	O["css"] = C
print("OPTIONS",O)


T = tm.table(D, O)
L = __log__