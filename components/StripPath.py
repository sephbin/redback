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
		{"name":"Path",		"nickname":"P",	"objectAccess":"item",	"description":"Path to append, or Boolean to choose current (F)Grasshopper or (T)Rhino document.", },
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
	#print(P)
	if P == None:
		P = False
	#if not R:
	#	R = ""
	if type(P) == type(False): 
		if P:
		    DOC = Rhino.RhinoDoc.ActiveDoc.Path
		else:
		    DOC = str(self.Attributes.Owner.OnPingDocument().FilePath)
	else:
		DOC = P
	#print(DOC)
	if os.path.isdir(DOC):
		docDir = DOC
		docName = None
	else:
		docDir, docName = os.path.split(DOC)
	
	docDirs = docDir.split(os.sep)
	#print("1 docDirs",docDirs)
	if R == None:
		R = docName
	try:
		relDirs = R.split(os.sep)
	except:
		relDirs = []



	for pathStep in relDirs:
	    #print("pathstep", pathStep)
	    if pathStep == ".":
	        del docDirs[-1]

	relDirs = list(filter(lambda x: x != ".", relDirs))
	#print("relDirs",relDirs)
	#print("docDirs",docDirs)
	newDirs = docDirs+relDirs
	#print("newDirs",newDirs)
	newDirs = list(map(lambda x: x.replace(":", ":"+os.sep), newDirs))
	newDir = os.path.join(*newDirs)

	#print(newDir)
	F = newDir
	if os.path.isdir(newDir):
		D = newDir
		N = None
	else:
		D, N = os.path.split(newDir)
except Exception as e:
	#print(e)
	F = R
	D, N = os.path.split(R)