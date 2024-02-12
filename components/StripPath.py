#### Constants ####
global __var__
__var__ = {
	"guid":"41bdcdeb-7482-4608-873f-59490830db42",
	
	"name":"Relative Path",
	"nickname":"Rel Path",
	"description":"Creates a relative path to Rhino/GH document",
	"icon": "ghContent\\Icon-RelativePath.png",

	"tabname":"Redback",
	"section":"Util",

	"inputs":[
		{"name":"Document",		"nickname":"D",	"objectAccess":"item",	"description":"Boolean to choose (F)Grasshopper or (T)Rhino", },
		{"name":"Relative Path",			"nickname":"R",	"objectAccess":"item",	"description":"Relative Path string", },
	],
	"outputs":[
		{"name":"Full path",		"nickname":"F",	"description":"Full Path"},
		{"name":"Directory path",	"nickname":"D",	"description":"Directory Path"},
		{"name":"File name",		"nickname":"N",	"description":"File name"},
	]
}
__author__ = "Andrew.Butler"
__version__ = "2024.02.02"
#### Constants ####

import os
import Rhino
import scriptcontext as sc
try:
	if D:
	    DOC = Rhino.RhinoDoc.ActiveDoc.Path
	else:
	    DOC = str(sc.doc.Component.Attributes.Owner.OnPingDocument().FilePath)

	print(DOC)
	docDir, docName = os.path.split(DOC)
	docDirs = docDir.split(os.sep)
	relDirs = R.split(os.sep)



	for pathStep in relDirs:
	    if pathStep == ".":
	        del docDirs[-1]

	relDirs = list(filter(lambda x: x != ".", relDirs))
	newDirs = docDirs+relDirs
	print(newDirs)
	newDir = os.path.join(*newDirs)


	F = newDir
	D, N = os.path.split(newDir)
except:
	F = R
	D, N = os.path.split(R)