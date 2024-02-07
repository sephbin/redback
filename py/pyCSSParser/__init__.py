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

def generateAllSelector(selector):
	return "True"
def generateClassSelector(selector):
	selector = selector.lstrip(".")
	template = r'''re.search("\\b{classText}\\b", ob["class"])'''.format(**{"classText":selector})
	return template

def generateIDSelector(selector):
	selector = selector.lstrip("#")
	template = '''ob["id"] == {idText}'''.format(**{"idText":selector})
	return template

def simpleSelector(selector):
	selector = selector.lstrip(".")
	selector = selector.lstrip("#")

	template = r'''re.search("\b{classText}\b", ob["class"])'''.format({"classText":selector})

	return template
def equals(a,b):
	return a == b
def parseSelector(selector):
	try:
		checks = [
		{"searchFunction": "equals", "variables":["*"], "name": "class","runFunction":generateAllSelector},
		{"searchFunction": "startswith", "variables":["."], "name": "class","runFunction":generateClassSelector},
		{"searchFunction": "startswith", "variables":["#"], "name": "id", "runFunction":generateIDSelector},
		]
		for check in checks:
			#print(check)
			try:	func  = getattr(selector, check["searchFunction"])
			except:
				func = globals()[check["searchFunction"]]
				check["variables"] = [selector]+check["variables"]
			
			runCheck = func(*check["variables"])
			if runCheck:
				selector = check["runFunction"](selector)
		return selector
	except Exception as e:
		print("parseSelector",e)

def parseSelectors(selectors):
	selectors = strip(selectors)
	selectors = {"eval":parseSelector(selectors), "searchType": "eval"}

	return {"filters":[selectors]}

def parseStyle(text):
	try:
		outDict = {}
		key, value = tuple(text.split(":"))
		key = strip(key)
		value = strip(value)
		# print(key, value)
		lut = {
		"isdigit": int,
		"isfloat": float
		}
		for check in [value.isdigit, isfloat]:
			try:	runCheck = check()
			except:	runCheck = check(value)
			# print(key, check.__name__, runCheck, value)
			if runCheck:
				value = lut[check.__name__](value)
				break
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
def parseCSS(text):
	text = text.replace("\n", "")
	text = text.replace("}", "}█")
	text = text.lstrip().rstrip()

	lines = text.split("█")
	lines = list(filter(lambda x: x, lines))
	outList = []
	for line in lines:
		print("#"*100)
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
		print("#"*100)
	return outList
	# print(text)


print(parseCSS(test))