#### Constants ####
global __var__
__var__ = {
	"guid":"9bf0fbf6-1cb9-49e0-b0a0-0ced95dcf5f9",
	
	"name":"Write File",
	"nickname":"Write",
	"description":"Saves data to file",
	"icon": "ghContent\\Icon-WriteFile.png",

	"tabname":"Redback",
	"section":"Util",

	"inputs":[
		{"name":"Run",			"nickname":"R",	"objectAccess":"item",	"description":"", },
		{"name":"Content",		"nickname":"C",	"objectAccess":"list",	"description":"", },
		{"name":"Folder",		"nickname":"P",	"objectAccess":"item",	"description":"", },
		{"name":"Name",			"nickname":"N",	"objectAccess":"item",	"description":"", },
	],
	"outputs":[
		{"name":"Filepath",	"nickname":"F",	"description":"Absolute path to saved file"}
	]
}
__author__ = "Andrew.Butler"
__version__ = "2024.02.02"
#### Constants ####

import os
from json import dumps
def check(o):
    if str(type(o)) == "<type 'dict'>":
        return dumps(o)
    else:
        return o
F = None
if R:
    con = list(map(lambda u: check(u), C))
    conwrite = "\n".join(con)
    directory = P+"\\"+N
    fileDir, _t = os.path.split(directory)
    if not os.path.isdir(fileDir):
    	os.makedirs(fileDir)
    F = directory
    with open(directory,'wb') as f:
        f.write(conwrite.encode("UTF-8"))
