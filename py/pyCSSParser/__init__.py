#!/usr/bin/env python
# -*- coding: utf-8 -*- 

import re
test = '''*{
paragraphStyle:Table - Small;
characterStyle:Bold;
}
'''

def strip(text, characterList=[" "]):
	if type(characterList) != type([]):
		characterList = [characterList]
	for c in characterList:
		text = text.lstrip(c)
		text = text.rstrip(c)
	return text

def isfloat(text):
	text = text.replace(".","",1)
	return text.isdigit()

def checkValue(value):
	lut = {
		"isdigit": int,
		"isfloat": float
		}
	for check in [value.isdigit, isfloat]:
		try:	runCheck = check()
		except:	runCheck = check(value.lstrip("-"))
		#print(check.__name__, runCheck, value, list(value))
		if runCheck:
			value = lut[check.__name__](value)
			break
	return value

def generateAllSelector(selector, objectType):
	return "True"
def generateClassSelector(selector, objectType):
	selector = selector.lstrip(".")
	template = r'''re.search("\\b{classText}\\b", {objectType}["class"])'''.format(**{"classText":selector, "objectType":objectType})
	return template

def generateIDSelector(selector,objectType):
	selector = selector.lstrip("#")
	template = '''{objectType}["id"] == {idText}'''.format(**{"idText":selector, "objectType":objectType})
	return template

def generateSquareBracketSelector(selector, objectType):
	try:
		selector = selector.lstrip("[").rstrip("]")
		lut = [
		{"operator": "~=",	"description":"contains",					"convertToStr":True, 	"template":'''{value} in {objectType}["{key}"]'''},
		{"operator": "|=",	"description":"equal to or startswith",		"convertToStr":False,	"template":'''{objectType}["{key}"] == '{value}' or {objectType}["{key}"].startswith('{value}')'''},
		{"operator": "^=",	"description":"startswith",					"convertToStr":False,	"template":'''{objectType}["{key}"].startswith('{value}')'''},
		{"operator": "$=",	"description":"endswith",					"convertToStr":False,	"template":'''{objectType}["{key}"].endswith('{value}')'''},
		{"operator": "*=",	"description":"contains substring",			"convertToStr":False,	"template":r'''re.search("\\b{value}\\b", {objectType}["{key}"])'''},
		{"operator": "=",	"description":"equals",						"convertToStr":True,	"template":'''{objectType}["{key}"] == {value}'''},
		{"operator": ">=",	"description":"greater than or equal to",	"convertToStr":True,	"template":'''{objectType}["{key}"] >= {value}'''},
		{"operator": ">",	"description":"greater than",				"convertToStr":True,	"template":'''{objectType}["{key}"] > {value}'''},
		{"operator": "<",	"description":"less than or equal to",		"convertToStr":True,	"template":'''{objectType}["{key}"] <= {value}'''},
		{"operator": "<",	"description":"less than",					"convertToStr":True,	"template":'''{objectType}["{key}"] < {value}'''},
		]

		for check in lut:
			if check["operator"] in selector:
				#print(check)
				key, value = tuple(selector.split(check["operator"]))
				value = checkValue(value)
				if check["convertToStr"]:
					if type(value) == type(""):
						value = '"%s"'%(value)
				#print("-"*10,key,value)
				#print("-"*10,check["template"])
				execute = check["template"].format(**{"key":key,"value":value, "objectType":objectType})
				#print("+"*10,execute)
				return execute
				break
	except Exception as e:
		print("generateSquareBracketSelector",e)
	return '''False'''


def equals(a,b):
	return a == b
def parseSelector(selector):
	try:
		checks = [
		{"searchFunction": "equals",		"variables":["*"], "name": "class","runFunction":generateAllSelector},
		{"searchFunction": "startswith",	"variables":["."], "name": "class","runFunction":generateClassSelector},
		{"searchFunction": "startswith",	"variables":["#"], "name": "id", "runFunction":generateIDSelector},
		{"searchFunction": "startswith",	"variables":["["], "name": "id", "runFunction":generateSquareBracketSelector},
		]
		obType = "ob"
		for objectType in ["border"]:
			if selector.startswith(objectType):
				obType = objectType
				selector = selector.replace(objectType, "", 1)
		for check in checks:
			#print(check)
			try:	func  = getattr(selector, check["searchFunction"])
			except:
				func = globals()[check["searchFunction"]]
				check["variables"] = [selector]+check["variables"]
			
			
			runCheck = func(*check["variables"])
			if runCheck:
				selector = check["runFunction"](selector, obType)
		return selector
	except Exception as e:
		print("parseSelector",e)

def parseSelectors(selectors):
	selectors = strip(selectors)
	#print(selectors)
	selectors = {"eval":parseSelector(selectors), "searchType": "eval"}

	return {"filters":[selectors]}

def parseStyle(text):
	try:
		outDict = {}
		key, value = tuple(text.split(":"))
		key = strip(key)
		value = strip(value)
		# print(key, value)
		value = checkValue(value)
		outDict[key] = value
		return outDict
	except Exception as e:
		print("error - parseStyle |",e,"|", text)
		pass
def parseStyles(styles):
	
	try:
		pstyles = strip(styles, ["{","}"])
		pstyles = strip(pstyles)
		pstyles = pstyles.split(";")
		pstyles= list(filter(lambda x: x != "", pstyles))
		pstyles =list(map(lambda x: parseStyle(x), pstyles))
		#print("pstyles", pstyles)
		outStyles = {}
		for s in pstyles:
			outStyles.update(s)
		#print("outStyles",outStyles)
		
		return {"style":outStyles}

	except Exception as e:
		print("error - parseStyles |",e,"|", styles)

def superCSS(text):
	outList = []
	superLUT = {"removeTableBorder": ["border[yPos=0]{weight:0;}", "border[yPosRev=-1]{weight:0;}", "border[xPos=0]{weight:0;}", "border[xPosRev=-1]{weight:0;}"]}
	if text.startswith("!"):
		text = text.strip("!")
		text = text.lstrip("{").rstrip("}")
		print(text)
		if text in superLUT:
			outList = superLUT[text]

	return outList
def parseCSS(text):
	text = text.replace("\n", "")
	text = text.replace("\t", "")
	text = text.replace("}", "}█")
	text = text.lstrip().rstrip()

	lines = text.split("█")
	lines = list(filter(lambda x: x, lines))
	outList = []

	for index, line in enumerate(lines):
		appendList = superCSS(line)
		

	for line in lines:
		#print("#"*100}
		try:
			line = line.replace("{", "█{")
			selectors, styles = tuple(line.split("█"))
			outDict = {}
			selectors = parseSelectors(selectors)
			outDict.update(selectors)
			styles = parseStyles(styles)
			#print("styles",styles)
			outDict.update(styles)
			
			outList.append(outDict)
		except Exception as e:
			print("error - parseCSS",e)
			pass
		#print("#"*100)
	return outList
	# print(text)


#print(parseCSS(test))