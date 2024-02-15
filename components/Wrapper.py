#### Constants ####
global __var__
__var__ = {
	"guid":"8e12d3d2-9c02-4af4-8931-ae8a51372bd0",
	
	"name":"Text Wrapper",
	"nickname":"Wrapper",
	"description":"Wraps and joins text with selected data characters",
	"icon": "ghContent\\Icon-Wrapper.png",

	"tabname":"Redback",
	"section":"JSON",

	"inputs":[
		{"name":"Content",			"nickname":"C",	"objectAccess":"list",	"description":"Text content to wrap", },
		{"name":"Wrap Character",	"nickname":"W",	"objectAccess":"item",	"description":"Character that will wrap the text", },
		{"name":"Join Character",	"nickname":"J",	"objectAccess":"item",	"description":"Character that will join the text, leave empty to wrap each individually", },
	],
	"outputs":[
		{"name":"Wrapped Text",	"nickname":"W",	"description":"Wrapped Text"}
	]
}
__author__ = "Andrew.Butler"
__version__ = "2024.02.02"
#### Constants ####
        
lut = {
	'"': {'t':'"{value}"'},
	"'": {'t':"'{value}'"},
	'{': {'t':'{{{value}}}'},
	'}': {'t':'{{{value}}}'},
	'{}': {'t':'{{{value}}}'},
	'[': {'t':'[{value}]'},
	']': {'t':'[{value}]'},
	'[]': {'t':'[{value}]'},
	'(': {'t':'({value})'},
	')': {'t':'({value})'},
	'()': {'t':'({value})'},
}

def wrapText(text, wrap):
	if wrap in lut:
		template = lut[wrap]["t"]
		print(template)
		return template.format(value=text)
	else:
		return wrap+text+wrap

if J != None:
	C = [J.join(C)]

outText = list(map(lambda x: wrapText(x, W), C))


W = outText