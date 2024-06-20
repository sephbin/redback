#!/usr/bin/env python
# -*- coding: utf-8 -*- 

"""Provides a scripting component.
	Inputs:
		x: The x script variable
		y: The y script variable
	Output:
		a: The a output variable"""

__author__ = "Andrew"
__version__ = "2022.08.05"

# from System import Array
import json
import re
#print(L)

rgb_scale = 255
cmyk_scale = 100

def hexToRGB(hex):
	hex = hex.replace("#","")
	r, g, b = hex[0]+hex[1], hex[2]+hex[3], hex[4]+hex[5]

	r = int(r, 16)
	g = int(g, 16)
	b = int(b, 16)
	return r, g, b

mmToInd = 2.8346456692913387

def recursiveBranchToTree(dictionary={}, branches=[], data=[]):
	#print("recursiveBranchToTree", dictionary, branches, data) 
	if len(branches) == 1:
		dictionary[branches[0]] = data
	else:
		index = branches.pop(0)
		if index not in dictionary:
			dictionary[index] = {}
		dictionary[index].update(recursiveBranchToTree({},branches, data))
	#print("/recursiveBranchToTree", dictionary, branches, data)
	return dictionary

def recursiveBranchToLists (lists = [], branches=[], data=[]):
	if len(branches) == 1:
		lists.append(data)
	else:
		index = branches.pop(0)
		lists.append(recursiveBranchToLists([],branches, data))
	return lists

def dictTreeToLists(dictionary):
	#print(">>", dictionary)
	if type(dictionary) == type({}):
		keys = sorted(map(lambda x: int(x), dictionary.keys()))
		values = map(lambda x: dictionary[str(x)], keys)
		outValues = []
		for v in values:
			outValues.append(dictTreeToLists(v))
		#print("<<", outValues)
		return outValues
	else:
		#print("<<<<", dictionary)
		return dictionary

def treeToList(tree):
	outList = []
	maxBranch = 0
	for path in tree.Paths:
		branch = tree.Branch(path=path)
		pathstr = path.Format("{0}",";")
		maxBranch = max(len(pathstr.split(";")), maxBranch)
		outList.append({"branch":pathstr, "data":list(branch)})
		#for index, item in enumerate(branch):
			#print(pathstr, index, item)
	outDict = {}
	outLists = []
	for index, branch in enumerate(outList):
		branch["branch"] = branch["branch"].split(";")
		outDict = recursiveBranchToTree(outDict, branch["branch"], branch["data"])
		#outLists = recursiveBranchToLists(outLists, branch["branch"], branch["data"])
		#print(index, outDict)
	outLists = dictTreeToLists(outDict)
	
	if maxBranch == 1:
		outLists = [outLists]
	return outLists

def deepFormat(formatOb, context):
	if type(formatOb) == type([]):
		for index, i in enumerate(formatOb):
			formatOb[index] = deepFormat(i,context)
	if type(formatOb) == type({}):
		for k,v in formatOb.items():
			formatOb[k] = deepFormat(v,context)
	if type(formatOb) == type(""):
		try: formatOb = formatOb.format(**context)
		except: pass
	return formatOb
def quickRemoveRows(data, classes=[]):
	#print("quickRemoveRows")
	outData = []
	for r in data:
		if type(r) != type({}):
			r = {"value":r,"data":{"class":[]}}
		willContinue = False
		for c in classes:
			if c in r["data"]["class"]:
				willContinue = True
		if willContinue:
			continue
		#if "doNotPublish" in r["data"]["class"]:
			#continue 
		outData.append(r)
	#print("#"*10,outData)
	return outData
class parent:
	def applyCSS(self, css=[]):
		def __eval__(self, search):
			try:
				ob = self.__dict__
				#print("_",search, ob)
				result = eval(search["eval"])
				#print("__",result)
				return result
			except Exception as e:
				#print("__",e)
				return False
		outStyles = {}
		for style in css:
			run = False
			for search in style["filters"]:
				function = locals()["__"+search["searchType"]+"__"]
				try:    run = function(self, search) or run
				except: pass
			if run:
				outStyles.update(style["style"])
		for k,v in outStyles.items():
			convert = False
			for i in ["width","height"]:
				if i in k.lower():
					convert = True
			if convert:
				outStyles[k] = v*mmToInd
		outStyles.update(self.__dict__)
		return outStyles

class borderParent(dict):
	def __setitem__(self, key, item):
		self.__dict__[key] = item
	def __getitem__(self, key):
		return self.__dict__[key]
	def __repr__(self):
		return repr(self.__dict__)
	def __len__(self):
		return len(self.__dict__)
	def __delitem__(self, key):
		del self.__dict__[key]
	def clear(self):
		return self.__dict__.clear()
	def copy(self):
		return self.__dict__.copy()
	def has_key(self, k):
		return k in self.__dict__
	def update(self, *args, **kwargs):
		return self.__dict__.update(*args, **kwargs)
	def keys(self):
		return self.__dict__.keys()
	def values(self):
		return self.__dict__.values()
	def items(self):
		return self.__dict__.items()
	def pop(self, *args):
		return self.__dict__.pop(*args)
	def __cmp__(self, dict_):
		return self.__cmp__(self.__dict__, dict_)
	def __contains__(self, item):
		return item in self.__dict__
	def __iter__(self):
		return iter(self.__dict__)
	def __unicode__(self):
		return unicode(repr(self.__dict__))
	def applyCSS(self, css=[]):
		def __eval__(self, search):
			border = self.__dict__
			try:
				search = eval(search["eval"])
				return search
			except exception as e:
				return False
		outStyles = {}
		for style in css:
			run = False
			for search in style["filters"]:
				function = locals()["__"+search["searchType"]+"__"]
				try:    run = bool(function(self, search)) or run
				except: pass
			if run:
				outStyles.update(style["style"])
		for k,v in outStyles.items():
			convert = False
			for i in ["width","height"]:
				if i in k.lower():
					convert = True
			if convert:
				outStyles[k] = v*mmToInd
		outStyles.update(self.__dict__)
		#print(outStyles)
		return outStyles

class border(borderParent):
	def __init__(self, table=None, yPos=0, xPos=0.5):
		self.yPos = yPos
		self.xPos = xPos
		self.xPosRev = xPos-(table.tableWidth)-1
		self.yPosRev = yPos-(table.tableHeight)-1
		self.colour = "Black"
		self.cssChanges = self.applyCSS(table.options["css"])

class column(parent):
	def __init__(self, xPos=0, table=None):
		self.table = table
		self.xPos = xPos
		self.xPosRev = xPos-(table.tableWidth)
		self.prop = {}
		if xPos == table.tableWidth-1:
			self.xPosName = "last"
		if xPos == 0:
			self.xPosName = "first"
		if "columnWidths" in self.table.options:
			index = min(self.xPos,len(self.table.options["columnWidths"])-1)
			self.columnWidth = self.table.options["columnWidths"][index]*mmToInd
		self.cssChanges = self.applyCSS(table.options["css"])
	def generateStr(self):
		#print(self.xPos)
		context = {"columnWidth": 30, "xPos": self.xPos}
		if "columnWidths" in self.table.options:
			context["columnWidth"] = self.columnWidth

		context.update(self.cssChanges)
		#string ="<ColStart:<tColAttrWidth:{columnWidth}>>".format(**context)
		for k,v in context.items():
			if v == None:
				context[k] = ""
		string = '<Column Self="u570i5a61Column{xPos}" Name="{xPos}" ClipContentToTextCell="false" SingleColumnWidth="{columnWidth}" />'.format(**context)
		#print(string)
		return string
	def __str__(self):
		return self.generateStr()

class row(parent):
	def __init__(self, yPos=0, table=None):
		self.table = table
		self.yPos = yPos
		self.yPosRev = yPos-(table.tableHeight)
		self.prop = {}
		self.rowHeight = 5*mmToInd
		if yPos == table.tableWidth-1:
			self.yPosName = "last"
		if yPos == 0:
			self.yPosName = "first"
		if "rowHeights" in self.table.options:
			index = min(self.yPos,len(self.table.options["rowHeights"])-1)
			self.rowHeight = self.table.options["rowHeights"][index]*mmToInd

		self.cssChanges = self.applyCSS(table.options["css"])
	def generateStr(self):
		#print(self.xPos)
		context = {"rowHeight": self.rowHeight, "yPos": self.yPos}
		# if "rowHeights" in self.table.options:
			# context["rowHeight"] = self.rowHeight
			#index = min(self.yPos,len(self.table.options["rowHeights"])-1)
			#context["rowHeight"] = self.table.options["rowHeights"][index]*mmToInd
		context.update(self.cssChanges)
		#string ="<ColStart:<tColAttrWidth:{columnWidth}>>".format(**context)
		string = '<Row Self="u34fi365Row{yPos}" Name="{yPos}" ClipContentToTextCell="false" SingleRowHeight="{rowHeight}" MinimumHeight="{rowHeight}" />'.format(**context)
		#print(string)
		return string
	def __str__(self):
		return self.generateStr()

class cell(parent):
	def __init__(self, data={"value":""}, xPos=0, yPos=0, table=None):
		#print("-"*100)
		try:
			data = json.loads(data)
		except Exception as e:
			pass
		if type(data) != type({}):
			data = {"value":data}

		#try:    self.above = getattr(table, "{x}-{y}".format(x=xPos,y=yPos-1))
		#except: self.above = None

		#try:    self.before = getattr(table, "{x}-{y}".format(x=xPos-1,y=yPos))
		#except: self.before = None
		self.parentTable = table
		self.hMerge = int(1)
		self.xPos = xPos
		self.yPos = yPos
		self.xPosRev = xPos-(table.tableWidth)
		self.yPosRev = yPos-(table.tableHeight)
		
		for k,v in data.items():
			setattr(self, k, v)
		if xPos == table.tableWidth-1:
			self.xPosName = "last"
		if yPos == table.tableHeight-1:
			self.yPosName = "last"
		try: self.value
		except: self.value = ""
		self.cssChanges = self.applyCSS(table.options["css"])
		self.context = self.getContext()
		self.string = self.generateStr()
		
	@property
	def above(self):
		try:    return getattr(self.parentTable, "{x}-{y}".format(x=self.xPos,y=self.yPos-1))
		except: return None
	@property
	def below(self):
		try:    return getattr(self.parentTable, "{x}-{y}".format(x=self.xPos,y=self.yPos+1))
		except: return None
	@property
	def before(self):
		try:    return getattr(self.parentTable, "{x}-{y}".format(x=self.xPos-1,y=self.yPos))
		except: return None
	@property
	def after(self):
		try:    return getattr(self.parentTable, "{x}-{y}".format(x=self.xPos+1,y=self.yPos))
		except: return None
	def getContext(self, recursive=True):
		baseContext = {"xPos":self.xPos, "yPos":self.yPos,
		"content":"", "extra":"", "paragraphStyle": "$ID/NormalParagraphStyle", "cellStyle":"[None]",
		"hAlignment": None,
		"fill": None, "cellTint":None,
		"fillString":"", "cellTintString":"" ,
		"characterStyle":"$ID/[No character style]",
		"characterColour":None, "characterColourString":"", "characterColourEscape":"",
		"borderLeft":None, "borderRight":None, "borderTop":None, "borderBottom":None,
		"borderString":"",
		"hMerge":1,
		"vMerge":1,
		"displayTemplate":"{}",
		"cellType":"TextTypeCell"
		}

		baseContext["cellWidth"] = self.parentTable.columns[self.xPos].columnWidth
		baseContext["cellHeight"] = self.parentTable.rows[self.yPos].rowHeight

		try:    baseContext["borderTop"] = list(filter(lambda i: i["yPos"]==self.yPos and i["xPos"]==self.xPos+0.5, self.parentTable.hBorders))[0].cssChanges["weight"]
		except: pass
		try:    baseContext["borderBottom"] = list(filter(lambda i: i["yPos"]==self.yPos+1 and i["xPos"]==self.xPos+0.5, self.parentTable.hBorders))[0].cssChanges["weight"]
		except: pass
		try:    baseContext["borderLeft"] = list(filter(lambda i: i["yPos"]==self.yPos+0.5 and i["xPos"]==self.xPos+0, self.parentTable.vBorders))[0].cssChanges["weight"]
		except: pass
		try:    baseContext["borderRight"] = list(filter(lambda i: i["yPos"]==self.yPos+0.5 and i["xPos"]==self.xPos+1, self.parentTable.vBorders))[0].cssChanges["weight"]
		except: pass
		context = baseContext.copy()
		context["content"] = self.value
		
		#print("baseContext",baseContext)
		context.update(self.cssChanges)
		#print("\n"*2)
		#print(context)
		# context["content"] = context["displayTemplate"].format(context["content"])
		#print(context)
		# if context["content"] == "D0043":
		#     print("-"*100)
		#     print(context)
		#     print("-"*100)
		context["paragraphStyle"] = context["paragraphStyle"].replace(":","%3a")
		#print(context["displayTemplate"])
		#print(self.value)
		try:    context["content"] = context["displayTemplate"].format(context["content"])
		except: context["content"] = context["content"]
		#if self.value == "Total":
			#print(context)

		if context["hAlignment"]:
			context["hAlignment"] = 'Justification="{hAlignment}"'.format(**context)
		if context["fill"]:
			# cC = context["fill"]
			# if type(cC) == type(""):
				# if cC.startswith("#"):
					# cC = hexToRGB(cC)
			# cC = list(map(lambda x: x/255, cC))
			#print(cC)
			#<tCellFillColor:COLOR\:RGB\:Process\:0.8901960784313725\,0.6431372549019608\,0.8509803921568627>
			#text = "<tCellFillColor:R\\={0} G\\={1} B\\={2}>".format(*context["fill"])
			#<tCellAttrLeftStrokeWeight:2><tCellAttrTopStrokeWeight:2>
			# text = "<tCellFillColor:COLOR\\:RGB\\:Process\\:{0}\\,{1}\\,{2}>".format(*cC)
			# context["fillString"] = text
			#print(context["fill"])
			if context["fill"] in self.parentTable.colourTable:
				colourID = self.parentTable.colourTable[context["fill"]]["id"]
				context["fillString"] = 'FillColor="Color/%s"'%(colourID)
		if context["characterColour"]:
			cC = context["characterColour"]
			if type(cC) == type(""):
				if cC.startswith("#"):
					cC = hexToRGB(cC)
			cC = list(map(lambda x: x/255, cC))
			text = "<cColor:COLOR\:RGB\:Process\:{0}\,{1}\,{2}>".format(*cC)
			context["characterColourString"] = text
			context["characterColourEscape"] = "<cColor:>"
		if context["cellTint"]:
			text = "<tCellAttrFillTint:{0}>".format(*[context["cellTint"]])
			context["cellTintString"] = text
		
		if "image" in context:
			#print(context)
			context["cellType"] = "GraphicTypeCell"
			context["content"] = ""
			if "imageHeight" in context:
				context["cellHeight"] = context["imageHeight"]*mmToInd
			context["extra"] = '''<Rectangle Self="u2b1" ContentType="GraphicType" StoryTitle="$ID/" ParentInterfaceChangeCount="" TargetInterfaceChangeCount="" LastUpdatedInterfaceChangeCount="" OverriddenPageItemProps="" HorizontalLayoutConstraints="FlexibleDimension FixedDimension FlexibleDimension" VerticalLayoutConstraints="FlexibleDimension FixedDimension FlexibleDimension" GradientFillStart="0 0" GradientFillLength="0" GradientFillAngle="0" GradientStrokeStart="0 0" GradientStrokeLength="0" GradientStrokeAngle="0" LocalDisplaySetting="Default" GradientFillHiliteLength="0" GradientFillHiliteAngle="0" GradientStrokeHiliteLength="0" GradientStrokeHiliteAngle="0" AppliedObjectStyle="ObjectStyle/$ID/[None]" Visible="true" Name="$ID/" ItemTransform="1 0 0 1 0 0">
				<Properties> <PathGeometry> <GeometryPathType PathOpen="false"> <PathPointArray> <PathPointType Anchor="0 0" LeftDirection="0 0" RightDirection="0 0" /> <PathPointType Anchor="0 {cellHeight}" LeftDirection="0 {cellHeight}" RightDirection="0 {cellHeight}" /> <PathPointType Anchor="{cellWidth} {cellHeight}" LeftDirection="{cellWidth} {cellHeight}" RightDirection="{cellWidth} {cellHeight}" /> <PathPointType Anchor="{cellWidth} 0" LeftDirection="{cellWidth} 0" RightDirection="{cellWidth} 0" /> </PathPointArray> </GeometryPathType> </PathGeometry> </Properties>
				<FrameFittingOption AutoFit="true" FittingOnEmptyFrame="Proportionally" />
				<Image Self="u2b4" Space="$ID/#Links_RGB" ActualPpi="300 300" EffectivePpi="500 500" ImageRenderingIntent="UseColorSettings" OverriddenPageItemProps="" LocalDisplaySetting="Default" ImageTypeName="$ID/Portable Network Graphics (PNG)" AppliedObjectStyle="ObjectStyle/$ID/[None]" ItemTransform="0.5995431265207192 0 0 0.5995431265207192 7.105427357601002e-15 9.921259842519696" ParentInterfaceChangeCount="" TargetInterfaceChangeCount="" LastUpdatedInterfaceChangeCount="" HorizontalLayoutConstraints="FlexibleDimension FixedDimension FlexibleDimension" VerticalLayoutConstraints="FlexibleDimension FixedDimension FlexibleDimension" GradientFillStart="0 0" GradientFillLength="0" GradientFillAngle="0" GradientFillHiliteLength="0" GradientFillHiliteAngle="0" Visible="true" Name="$ID/">
					<Properties> <Profile type="string">$ID/Embedded</Profile> <GraphicBounds Left="0" Top="0" Right="141.84028857480587" Bottom="141.84028857480587" /> </Properties>
					<TextWrapPreference Inverse="false" ApplyToMasterPageOnly="false" TextWrapSide="BothSides" TextWrapMode="None"> <Properties> <TextWrapOffset Top="0" Left="0" Bottom="0" Right="0" /> </Properties> <ContourOption ContourType="SameAsClipping" IncludeInsideEdges="false" ContourPathName="$ID/" /> </TextWrapPreference>
					<Link Self="u2b3" AssetURL="$ID/" AssetID="$ID/" RenditionData="Actual"
						LinkResourceURI="file:{image}"/>
				</Image>
			</Rectangle>'''
			context["extra"] = context["extra"].format(**context)

		for k,v in context.items():
			if v == None:
				context[k] = ""
		#     if not recursive:
		#         continue
		#     if k == "borderTop":
		#         if self.above:
		#             if self.above.context["borderBottom"]:
		#                 context[k] = self.above.getContext(recursive=False)["borderBottom"]
		#     if k == "borderLeft":
		#         if self.before:
		#             if self.before.context["borderRight"]:
		#                 context[k] = self.before.getContext(recursive=False)["borderRight"]
		#     if k == "borderBottom":
		#         if self.below:
		#             if self.below.context["borderTop"]:
		#                 context[k] = self.below.getContext(recursive=False)["borderTop"]
		#     if k == "borderRight":
		#         if self.after:
		#             if self.after.context["borderLeft"]:
		#                 context[k] = self.after.getContext(recursive=False)["borderLeft"]
		for r, n in [("'","&apos;"),('"',"&quot;"),("&","&amp;"),("<","&lt;"),(">","&gt;")]:
			context["content"] = context["content"].replace(r,n)
		return context
	def generateStr(self):
		context = self.context
		#template = """<CellStyle:{cellStyle}><StylePriority:0><CellStart:1,{hMerge}{borderString}{fillString}{cellTintString}<tTextCellVerticalJustification:1>><ParaStyle:{paragraphStyle}>{hAlignmentString}{characterColourString}{content}{characterColourEscape}{hAlignmentEscape}<CellEnd:>"""
		'''Graphic Cell
		<Cell Self="ue1i2a5i1" Name="1:0" RowSpan="1" ColumnSpan="1" CellType="GraphicTypeCell" TextTopInset="4" TextLeftInset="4" TextBottomInset="4" TextRightInset="4" ClipContentToTextCell="false" AppliedCellStyle="CellStyle/$ID/[None]" AppliedCellStylePriority="0" LeftInset="4" TopInset="4" RightInset="4" BottomInset="4" LeftEdgeStrokeWeight="0" RightEdgeStrokeWeight="0" TopEdgeStrokeWeight="0" BottomEdgeStrokeWeight="0" ClipContentToCell="false" LeftEdgeStrokePriority="1" RightEdgeStrokePriority="1" TopEdgeStrokePriority="1" BottomEdgeStrokePriority="1">
			<Rectangle Self="u2b1" ContentType="GraphicType" StoryTitle="$ID/" ParentInterfaceChangeCount="" TargetInterfaceChangeCount="" LastUpdatedInterfaceChangeCount="" OverriddenPageItemProps="" HorizontalLayoutConstraints="FlexibleDimension FixedDimension FlexibleDimension" VerticalLayoutConstraints="FlexibleDimension FixedDimension FlexibleDimension" GradientFillStart="0 0" GradientFillLength="0" GradientFillAngle="0" GradientStrokeStart="0 0" GradientStrokeLength="0" GradientStrokeAngle="0" LocalDisplaySetting="Default" GradientFillHiliteLength="0" GradientFillHiliteAngle="0" GradientStrokeHiliteLength="0" GradientStrokeHiliteAngle="0" AppliedObjectStyle="ObjectStyle/$ID/[None]" Visible="true" Name="$ID/" ItemTransform="1 0 0 1 0 0">
				<Properties> <PathGeometry> <GeometryPathType PathOpen="false"> <PathPointArray> <PathPointType Anchor="0 0" LeftDirection="0 0" RightDirection="0 0" /> <PathPointType Anchor="0 204.88188976377954" LeftDirection="0 204.88188976377954" RightDirection="0 204.88188976377954" /> <PathPointType Anchor="95.03937007874016 204.88188976377954" LeftDirection="95.03937007874016 204.88188976377954" RightDirection="95.03937007874016 204.88188976377954" /> <PathPointType Anchor="95.03937007874016 0" LeftDirection="95.03937007874016 0" RightDirection="95.03937007874016 0" /> </PathPointArray> </GeometryPathType> </PathGeometry> </Properties>
				<FrameFittingOption AutoFit="true" FittingOnEmptyFrame="Proportionally" />
				<Image Self="u2b4" Space="$ID/#Links_RGB" ActualPpi="300 300" EffectivePpi="500 500" ImageRenderingIntent="UseColorSettings" OverriddenPageItemProps="" LocalDisplaySetting="Default" ImageTypeName="$ID/Portable Network Graphics (PNG)" AppliedObjectStyle="ObjectStyle/$ID/[None]" ItemTransform="0.5995431265207192 0 0 0.5995431265207192 7.105427357601002e-15 9.921259842519696" ParentInterfaceChangeCount="" TargetInterfaceChangeCount="" LastUpdatedInterfaceChangeCount="" HorizontalLayoutConstraints="FlexibleDimension FixedDimension FlexibleDimension" VerticalLayoutConstraints="FlexibleDimension FixedDimension FlexibleDimension" GradientFillStart="0 0" GradientFillLength="0" GradientFillAngle="0" GradientFillHiliteLength="0" GradientFillHiliteAngle="0" Visible="true" Name="$ID/">
					<Properties> <Profile type="string">$ID/Embedded</Profile> <GraphicBounds Left="0" Top="0" Right="141.84028857480587" Bottom="141.84028857480587" /> </Properties>
					<TextWrapPreference Inverse="false" ApplyToMasterPageOnly="false" TextWrapSide="BothSides" TextWrapMode="None"> <Properties> <TextWrapOffset Top="0" Left="0" Bottom="0" Right="0" /> </Properties> <ContourOption ContourType="SameAsClipping" IncludeInsideEdges="false" ContourPathName="$ID/" /> </TextWrapPreference>
					<Link Self="u2b3" AssetURL="$ID/" AssetID="$ID/" RenditionData="Actual"
						LinkResourceURI="file:C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/organgrinder.png"
						LinkResourceFormat="$ID/Portable Network Graphics (PNG)" StoredState="Normal" LinkClassID="35906" LinkClientID="257" LinkResourceModified="false" LinkObjectModified="false" ShowInUI="true" CanEmbed="true" CanUnembed="true" CanPackage="true" ImportPolicy="NoAutoImport" ExportPolicy="NoAutoExport" LinkImportStamp="file 133436612310935410 100814" LinkImportModificationTime="2023-11-05T23:33:51" LinkImportTime="2023-11-07T11:35:23" LinkResourceSize="0~189ce" />
				</Image>
			</Rectangle>
		</Cell>
		'''
		context["borderString"] = ""
		for k, t in [('borderLeft', 'LeftEdgeStrokeWeight="{}"'), ('borderRight', 'RightEdgeStrokeWeight="{}"'), ('borderTop', 'TopEdgeStrokeWeight="{}"'), ('borderBottom', 'BottomEdgeStrokeWeight="{}"')]:
			if context[k] or context[k]==0:
				# print(k, t)
				context["borderString"] = context["borderString"] + t.format(context[k])+" "
		# print(context["borderString"])
		# print(context)

		#LeftEdgeStrokeWeight="{borderLeft}" RightEdgeStrokeWeight="{borderRight}" TopEdgeStrokeWeight="{borderTop}" BottomEdgeStrokeWeight="{borderBottom}"
		#TextTopInset="0" TextLeftInset="0" TextBottomInset="0" TextRightInset="0"
		template = """<Cell Self="u34fi365i0" Name="{xPos}:{yPos}" RowSpan="{vMerge}" ColumnSpan="{hMerge}" CellType="{cellType}" ClipContentToTextCell="false"
		AppliedCellStyle="CellStyle/Generated%3a{cellStyle}" AppliedCellStylePriority="2"
		{borderString}
		LeftEdgeStrokePriority="1" RightEdgeStrokePriority="1" TopEdgeStrokePriority="1" BottomEdgeStrokePriority="1" {fillString}>
		<ParagraphStyleRange AppliedParagraphStyle="ParagraphStyle/Generated%3a{paragraphStyle}" {hAlignment}>
		<CharacterStyleRange AppliedCharacterStyle="CharacterStyle/Generated%3a{characterStyle}">
		<Content>{content}</Content>
		</CharacterStyleRange>
		</ParagraphStyleRange>
		{extra}
		</Cell>"""
		string = template.format(**context)

		for r in range(context["hMerge"]-1):
			modContext = context.copy()
			modContext.update(context)
			# modContext["borderString"] = context["borderString"]
			string += template.format(**modContext)
		#print(string)
		#if self.mergeHorizontal > 1:
		#for r in range(self.hMerge-1):
			#string = string+"<CellStyle:[None]><StylePriority:0><CellStart:1,1><CellEnd:>"
		#print(self.yPosRev,string)
		# print(string)
		return string
	def __str__(self):
		return self.generateStr()

class table:
	def __init__(self, data=[], options={}):
		#print("-"*10)
		
		defaultOptions = {"css":[], "variables":{},"columnWidths":[30],"columnWidthsRev":[]}
		defaultOptions.update(options)
		self.options = options = deepFormat(defaultOptions, defaultOptions["variables"])
		try:    self.data = quickRemoveRows(data,[options["removeClass"]])
		except: self.data = quickRemoveRows(data)
		self.flatData = []
		#print(self.data)
		#print("flatData")
		for r in self.data:
			for c in r["value"]:
				if type(c) != type({}):
					c = {"value":c}
				self.flatData.append(c)
		#print(self.flatData)
		self.colourTable = {}

		colourId = 0
		for flatCell in self.flatData:
			#print("flatCell", flatCell)
			if "fill" in flatCell:
				if flatCell["fill"] not in self.colourTable:
					colourCode = hexToRGB(flatCell["fill"])
					self.colourTable[flatCell["fill"]] =  {"id": "GenColour"+str(colourId), "code": colourCode, "codeString": " ".join(list(map(lambda i: str(i), colourCode)))}
					colourId += 1
		for style in self.options["css"]:
			style = style["style"]
			if "fill" in style:
				if style["fill"] not in self.colourTable:
					colourCode = hexToRGB(style["fill"])
					self.colourTable[style["fill"]] =  {"id": "GenColour"+str(colourId), "code": colourCode, "codeString": " ".join(list(map(lambda i: str(i), colourCode)))}
					colourId += 1
		#print(self.colourTable)

		buildColumnWidths = [self.options["columnWidths"][-1]]*self.tableWidth
		for i, v in enumerate(self.options["columnWidths"]):
			buildColumnWidths[i] = v
		for i, v in enumerate(self.options["columnWidthsRev"]):
			i = (i+1)*-1
			buildColumnWidths[i] = v
		self.options["columnWidths"] = buildColumnWidths
		




		self.hBorders = []
		self.vBorders = []
		for yIndex in range(self.tableHeight+1):
			for xIndex in range(self.tableWidth):
				#print(xIndex,yIndex)
				self.hBorders.append(border(self,yIndex, xIndex+0.5))
		for xIndex in range(self.tableWidth+1):
			for yIndex in range(self.tableHeight):
				#print(xIndex,yIndex)
				self.vBorders.append(border(self,yIndex+0.5, xIndex))
		#print(self.hBorders)
		self.headerCount = 0
		if "headerCount" in options:
			self.headerCount = options["headerCount"]

		self.columns = list(map(lambda x: column(x,self), range(self.tableWidth)))
		self.rows = list(map(lambda x: row(x,self), range(self.tableHeight)))
		self.cells = []
		#print(self.data)
		for row_index, r in enumerate(self.data):
			#cells = []
			#print(row_index,r)
			for col_index, c in enumerate(r["value"]):
				#print(col_index, c, cell_prop)
				cell_instance = cell(c ,col_index, row_index, self)
				#print(cell_instance, cell_instance.__dict__)
				self.cells.append(cell_instance)
				setattr(self, "{x}-{y}".format(x=col_index,y=row_index), cell_instance)
			#row_instance = row(cells, row_index, self)
			#rows.append(str(row_instance))
		self.cellsString = "\n".join(list(map(lambda x: str(x), self.cells)))
		
		
		
	#@property
	#def cells(self):
		#return map(lambda x: map(lambda y: cell(y), x["data"]), self.data)
	#@property
	#def cellsDict(self):
		#out = map(lambda x: map(lambda y: dict(y), x), self.cells)
		#print(list(out))
		#return out
	#@property
	#def cellsString(self):
		#return map(lambda x: map(lambda y: str(y), x), self.cells)
	#@property
	#def rowsString(self):
		#string = map(lambda x: str(row(x)), self.cellsString)
		#string = "".join(string)
		#return string
	@property
	def colsString(self):
		strings = []
		for r in range(self.tableWidth):
			cols = str(column(r,self))
			strings.append(cols)
		strings = "\n".join(strings)
		return strings
	@property
	def rowsString(self):
		strings = []
		for r in range(self.tableHeight):
			rows = str(row(r,self))
			strings.append(rows)
		strings = "\n".join(strings)
		return strings
	@property
	def tableWidth(self):
		maxWidth = 0
		for row in self.data:
			width = len(row["value"])
			maxWidth = max(maxWidth, width)
		return maxWidth
	@property
	def tableHeight(self):
		maxd = len(self.data)
		return maxd
	@property
	def tableStartString(self):
		return '<Table Self="u34fi365" HeaderRowCount="{headerCount}" FooterRowCount="0" TextTopInset="4" TextLeftInset="4" TextBottomInset="4" TextRightInset="4" ClipContentToTextCell="false" BodyRowCount="{tableHeight}" ColumnCount="{tableWidth}" AppliedTableStyle="TableStyle/Generated%3aBasic Table" TableDirection="LeftToRightDirection">'.format(tableHeight=self.tableHeight-self.headerCount, tableWidth=self.tableWidth, headerCount=self.headerCount)
	def generateStr(self):
		#"<ASCII-WIN>\r\n<Version:17.3><FeatureSet:InDesign-Roman><ColorTable:=<Black:COLOR:RGB:Process:0,0,0><COX Black RGB:COLOR:RGB:Process:0,0,0><COX Dark Grey RGB:COLOR:RGB:Process:0.2549019607843137,0.25098039215686274,0.25882352941176473><Paper:COLOR:CMYK:Process:0,0,0,0><C\=75 M\=5 Y\=100 K\=0:COLOR:CMYK:Process:0.75,0.05,1,0>>\r\n<ParaStyle:NormalParagraphStyle><TableStyle:Table C\\\\\\: White Fill>%s%s%s<TableEnd:>"%(self.tableStartString, self.colsString, self.rowsString)
		#string = "<ASCII-WIN>\r\n<Version:16.4><FeatureSet:InDesign-Roman><ColorTable:=<Black:COLOR:CMYK:Process:0,0,0,1><Paper:COLOR:CMYK:Process:0,0,0,0>>\r\n<ParaStyle:NormalParagraphStyle><TableStyle:Table C\\\\\\: White Fill>%s%s%s<TableEnd:>"%(self.tableStartString, self.colsString, self.rowsString)		
		parastyles = []
		charastyles = []
		for cssline in self.options["css"]:
			for k,v in cssline["style"].items():
				if k == "paragraphStyle":
					parastyles.append('<ParagraphStyle Self="ParagraphStyle/Generated%3a{name}" Name="Generated:{name}" Imported="false" NextStyle="ParagraphStyle/Generated%3a{name}" SplitDocument="false" EmitCss="true" StyleUniqueId="8ad5365e-a91f-4f93-b49a-3810b988a0b0" IncludeClass="true" ExtendedKeyboardShortcut="0 0 0" EmptyNestedStyles="true" EmptyLineStyles="true" EmptyGrepStyles="true" KeyboardShortcut="0 0" AppliedLanguage="$ID/English: UK"> <Properties><BasedOn type="object">ParagraphStyle/$ID/NormalParagraphStyle</BasedOn><PreviewColor type="enumeration">Nothing</PreviewColor> </Properties> </ParagraphStyle>'.format(name=v))
				if k == "characterStyle":
					charastyles.append('<CharacterStyle Self="CharacterStyle/Generated%3a{name}" Imported="false" SplitDocument="false" EmitCss="true" StyleUniqueId="6a8c751b-7b81-4452-9747-cff8e210c306" IncludeClass="true" ExtendedKeyboardShortcut="0 0 0" KeyboardShortcut="0 0" Name="Generated:{name}"> <Properties> <BasedOn type="string">$ID/[No character style]</BasedOn> <PreviewColor type="enumeration">Nothing</PreviewColor> </Properties> </CharacterStyle>'.format(name=v))

		parastyles = "\n".join(parastyles)
		charastyles = "\n".join(charastyles)

		colours = []
		for colourKey, colour in self.colourTable.items():
			template = '''\t\t<Color Self="Color/{id}" Model="Process" Space="RGB" ColorValue="{codeString}" ColorOverride="Normal" ConvertToHsb="false" AlternateSpace="NoAlternateColor" AlternateColorValue="" Name="$ID/" ColorEditable="true" ColorRemovable="true" Visible="false" SwatchCreatorID="7937" SwatchColorGroupReference="n" />'''
			colours.append(template.format(**colour))
		colours = "\n".join(colours)


		fullDocString = """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
		<?aid style="50" type="snippet" readerVersion="6.0" featureSet="257" product="18.4(56)" ?>
		<?aid SnippetType="InCopyInterchange"?>
		<Document DOMVersion="18.0" Self="d">
{colours}
		<Ink Self="Ink/$ID/Process Cyan" Name="$ID/Process Cyan" Angle="75" ConvertToProcess="false" Frequency="70" NeutralDensity="0.61" PrintInk="true" TrapOrder="1" InkType="Normal" /> 
		<Ink Self="Ink/$ID/Process Magenta" Name="$ID/Process Magenta" Angle="15" ConvertToProcess="false" Frequency="70" NeutralDensity="0.76" PrintInk="true" TrapOrder="2" InkType="Normal" /> 
		<Ink Self="Ink/$ID/Process Yellow" Name="$ID/Process Yellow" Angle="0" ConvertToProcess="false" Frequency="70" NeutralDensity="0.16" PrintInk="true" TrapOrder="3" InkType="Normal" /> 
		<Ink Self="Ink/$ID/Process Black" Name="$ID/Process Black" Angle="45" ConvertToProcess="false" Frequency="70" NeutralDensity="1.7" PrintInk="true" TrapOrder="4" InkType="Normal" /> <Swatch Self="Swatch/None" Name="None" ColorEditable="false" ColorRemovable="false" Visible="true" SwatchCreatorID="7937" SwatchColorGroupReference="u18ColorGroupSwatch0" />
		<StrokeStyle Self="StrokeStyle/$ID/Solid" Name="$ID/Solid" />
		<CompositeFont Self="CompositeFont/$ID/[No composite font]" Name="$ID/[No composite font]"> <CompositeFontEntry Self="uae" Name="$ID/Alphabetic" RelativeSize="100" HorizontalScale="100" VerticalScale="100" CustomCharacters="ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿıŒœŠšŸŽžƒ" Locked="true" ScaleOption="false" BaselineShift="0"> <Properties> <AppliedFont type="string">Arial</AppliedFont> </Properties> </CompositeFontEntry> </CompositeFont>
		<RootCharacterStyleGroup Self="u7f"> <CharacterStyle Self="CharacterStyle/$ID/[No character style]" Imported="false" SplitDocument="false" EmitCss="true" StyleUniqueId="$ID/" IncludeClass="true" ExtendedKeyboardShortcut="0 0 0" Name="$ID/[No character style]" />
			<CharacterStyleGroup Self="CharacterStyleGroup/$ID/Generated" Name="$ID/Generated"> {charastyles} </CharacterStyleGroup>
		</RootCharacterStyleGroup>
		<RootParagraphStyleGroup Self="u7e">
		<ParagraphStyle Self="ParagraphStyle/$ID/[No paragraph style]" Name="$ID/[No paragraph style]" Imported="false" SplitDocument="false" EmitCss="true" StyleUniqueId="$ID/" IncludeClass="true" ExtendedKeyboardShortcut="0 0 0" EmptyNestedStyles="true" EmptyLineStyles="true" EmptyGrepStyles="true" FillColor="Color/Black" FontStyle="Regular" PointSize="12" HorizontalScale="100" KerningMethod="$ID/Metrics" Ligatures="true" PageNumberType="AutoPageNumber" StrokeWeight="1" Tracking="0" Composer="HL Composer" DropCapCharacters="0" DropCapLines="0" BaselineShift="0" Capitalization="Normal" StrokeColor="Swatch/None" HyphenateLadderLimit="3" VerticalScale="100" LeftIndent="0" RightIndent="0" FirstLineIndent="0" AutoLeading="120" AppliedLanguage="$ID/English: USA" Hyphenation="true" HyphenateAfterFirst="2" HyphenateBeforeLast="2" HyphenateCapitalizedWords="true" HyphenateWordsLongerThan="5" NoBreak="false" HyphenationZone="36" SpaceBefore="0" SpaceAfter="0" Underline="false" OTFFigureStyle="Default" DesiredWordSpacing="100" MaximumWordSpacing="133" MinimumWordSpacing="80" DesiredLetterSpacing="0" MaximumLetterSpacing="0" MinimumLetterSpacing="0" DesiredGlyphScaling="100" MaximumGlyphScaling="100" MinimumGlyphScaling="100" StartParagraph="Anywhere" KeepAllLinesTogether="false" KeepWithNext="0" KeepFirstLines="2" KeepLastLines="2" Position="Normal" StrikeThru="false" CharacterAlignment="AlignEmCenter" KeepLinesTogether="false" StrokeTint="-1" FillTint="-1" OverprintStroke="false" OverprintFill="false" GradientStrokeAngle="0" GradientFillAngle="0" GradientStrokeLength="-1" GradientFillLength="-1" GradientStrokeStart="0 0" GradientFillStart="0 0" Skew="0" RuleAboveLineWeight="1" RuleAboveTint="-1" RuleAboveOffset="0" RuleAboveLeftIndent="0" RuleAboveRightIndent="0" RuleAboveWidth="ColumnWidth" RuleBelowLineWeight="1" RuleBelowTint="-1" RuleBelowOffset="0" RuleBelowLeftIndent="0" RuleBelowRightIndent="0" RuleBelowWidth="ColumnWidth" RuleAboveOverprint="false" RuleBelowOverprint="false" RuleAbove="false" RuleBelow="false" LastLineIndent="0" HyphenateLastWord="true" ParagraphBreakType="Anywhere" SingleWordJustification="FullyJustified" OTFOrdinal="false" OTFFraction="false" OTFDiscretionaryLigature="false" OTFTitling="false" RuleAboveGapTint="-1" RuleAboveGapOverprint="false" RuleBelowGapTint="-1" RuleBelowGapOverprint="false" Justification="LeftAlign" DropcapDetail="1" PositionalForm="None" OTFMark="true" HyphenWeight="5" OTFLocale="true" HyphenateAcrossColumns="true" KeepRuleAboveInFrame="false" IgnoreEdgeAlignment="false" OTFSlashedZero="false" OTFStylisticSets="0" OTFHistorical="false" OTFContextualAlternate="true" UnderlineGapOverprint="false" UnderlineGapTint="-1" UnderlineOffset="-9999" UnderlineOverprint="false" UnderlineTint="-1" UnderlineWeight="-9999" StrikeThroughGapOverprint="false" StrikeThroughGapTint="-1" StrikeThroughOffset="-9999" StrikeThroughOverprint="false" StrikeThroughTint="-1" StrikeThroughWeight="-9999" MiterLimit="4" StrokeAlignment="OutsideAlignment" EndJoin="MiterEndJoin" SpanColumnType="SingleColumn" SplitColumnInsideGutter="6" SplitColumnOutsideGutter="0" KeepWithPrevious="false" SpanColumnMinSpaceBefore="0" SpanColumnMinSpaceAfter="0" OTFSwash="false" ParagraphShadingTint="20" ParagraphShadingOverprint="false" ParagraphShadingWidth="ColumnWidth" ParagraphShadingOn="false" ParagraphShadingClipToFrame="false" ParagraphShadingSuppressPrinting="false" ParagraphShadingLeftOffset="0" ParagraphShadingRightOffset="0" ParagraphShadingTopOffset="0" ParagraphShadingBottomOffset="0" ParagraphShadingTopOrigin="AscentTopOrigin" ParagraphShadingBottomOrigin="DescentBottomOrigin" ParagraphBorderTint="-1" ParagraphBorderOverprint="false" ParagraphBorderOn="false" ParagraphBorderGapTint="-1" ParagraphBorderGapOverprint="false" Tsume="0" LeadingAki="-1" TrailingAki="-1" KinsokuType="KinsokuPushInFirst" KinsokuHangType="None" BunriKinshi="true" RubyOpenTypePro="true" RubyFontSize="-1" RubyAlignment="RubyJIS" RubyType="PerCharacterRuby" RubyParentSpacing="RubyParent121Aki" RubyXScale="100" RubyYScale="100" RubyXOffset="0" RubyYOffset="0" RubyPosition="AboveRight" RubyAutoAlign="true" RubyParentOverhangAmount="RubyOverhangOneRuby" RubyOverhang="false" RubyAutoScaling="false" RubyParentScalingPercent="66" RubyTint="-1" RubyOverprintFill="Auto" RubyStrokeTint="-1" RubyOverprintStroke="Auto" RubyWeight="-1" KentenKind="None" KentenFontSize="-1" KentenXScale="100" KentenYScale="100" KentenPlacement="0" KentenAlignment="AlignKentenCenter" KentenPosition="AboveRight" KentenCustomCharacter="" KentenCharacterSet="CharacterInput" KentenTint="-1" KentenOverprintFill="Auto" KentenStrokeTint="-1" KentenOverprintStroke="Auto" KentenWeight="-1" Tatechuyoko="false" TatechuyokoXOffset="0" TatechuyokoYOffset="0" AutoTcy="0" AutoTcyIncludeRoman="false" Jidori="0" GridGyoudori="0" GridAlignFirstLineOnly="false" GridAlignment="None" CharacterRotation="0" RotateSingleByteCharacters="false" Rensuuji="true" ShataiMagnification="0" ShataiDegreeAngle="4500" ShataiAdjustTsume="true" ShataiAdjustRotation="false" Warichu="false" WarichuLines="2" WarichuSize="50" WarichuLineSpacing="0" WarichuAlignment="Auto" WarichuCharsBeforeBreak="2" WarichuCharsAfterBreak="2" OTFHVKana="false" OTFProportionalMetrics="false" OTFRomanItalics="false" LeadingModel="LeadingModelAkiBelow" ScaleAffectsLineHeight="false" ParagraphGyoudori="false" CjkGridTracking="false" GlyphForm="None" RubyAutoTcyDigits="0" RubyAutoTcyIncludeRoman="false" RubyAutoTcyAutoScale="true" TreatIdeographicSpaceAsSpace="true" AllowArbitraryHyphenation="false" BulletsAndNumberingListType="NoList" NumberingStartAt="1" NumberingLevel="1" NumberingContinue="true" NumberingApplyRestartPolicy="true" BulletsAlignment="LeftAlign" NumberingAlignment="LeftAlign" NumberingExpression="^#.^t" BulletsTextAfter="^t" ParagraphBorderLeftOffset="0" ParagraphBorderRightOffset="0" ParagraphBorderTopOffset="0" ParagraphBorderBottomOffset="0" ParagraphBorderStrokeEndJoin="MiterEndJoin" ParagraphBorderTopLeftCornerOption="None" ParagraphBorderTopRightCornerOption="None" ParagraphBorderBottomLeftCornerOption="None" ParagraphBorderBottomRightCornerOption="None" ParagraphBorderTopLeftCornerRadius="12" ParagraphBorderTopRightCornerRadius="12" ParagraphBorderBottomLeftCornerRadius="12" ParagraphBorderBottomRightCornerRadius="12" ParagraphShadingTopLeftCornerOption="None" ParagraphShadingTopRightCornerOption="None" ParagraphShadingBottomLeftCornerOption="None" ParagraphShadingBottomRightCornerOption="None" ParagraphShadingTopLeftCornerRadius="12" ParagraphShadingTopRightCornerRadius="12" ParagraphShadingBottomLeftCornerRadius="12" ParagraphShadingBottomRightCornerRadius="12" ParagraphBorderStrokeEndCap="ButtEndCap" ParagraphBorderWidth="ColumnWidth" ParagraphBorderTopOrigin="AscentTopOrigin" ParagraphBorderBottomOrigin="DescentBottomOrigin" ParagraphBorderTopLineWeight="1" ParagraphBorderBottomLineWeight="1" ParagraphBorderLeftLineWeight="1" ParagraphBorderRightLineWeight="1" ParagraphBorderDisplayIfSplits="false" MergeConsecutiveParaBorders="true" ProviderHyphenationStyle="HyphAll" DigitsType="DefaultDigits" Kashidas="DefaultKashidas" DiacriticPosition="OpentypePositionFromBaseline" CharacterDirection="DefaultDirection" ParagraphDirection="LeftToRightDirection" ParagraphJustification="DefaultJustification" ParagraphKashidaWidth="2" XOffsetDiacritic="0" YOffsetDiacritic="0" OTFOverlapSwash="false" OTFStylisticAlternate="false" OTFJustificationAlternate="false" OTFStretchedAlternate="false" KeyboardDirection="DefaultDirection"> <Properties> <Leading type="enumeration">Auto</Leading> <TabList type="list"> </TabList> <AppliedFont type="string">Arial</AppliedFont> <RuleAboveColor type="string">Text Color</RuleAboveColor> <RuleBelowColor type="string">Text Color</RuleBelowColor> <RuleAboveType type="object">StrokeStyle/$ID/Solid</RuleAboveType> <RuleBelowType type="object">StrokeStyle/$ID/Solid</RuleBelowType> <BalanceRaggedLines type="enumeration">NoBalancing</BalanceRaggedLines> <RuleAboveGapColor type="object">Swatch/None</RuleAboveGapColor> <RuleBelowGapColor type="object">Swatch/None</RuleBelowGapColor> <UnderlineColor type="string">Text Color</UnderlineColor> <UnderlineGapColor type="object">Swatch/None</UnderlineGapColor> <UnderlineType type="object">StrokeStyle/$ID/Solid</UnderlineType> <StrikeThroughColor type="string">Text Color</StrikeThroughColor> <StrikeThroughGapColor type="object">Swatch/None</StrikeThroughGapColor> <StrikeThroughType type="object">StrokeStyle/$ID/Solid</StrikeThroughType> <SpanSplitColumnCount type="enumeration">All</SpanSplitColumnCount> <ParagraphShadingColor type="object">Color/Black</ParagraphShadingColor> <ParagraphBorderColor type="object">Color/Black</ParagraphBorderColor> <ParagraphBorderGapColor type="object">Swatch/None</ParagraphBorderGapColor> <ParagraphBorderType type="object">StrokeStyle/$ID/Solid</ParagraphBorderType> <Mojikumi type="enumeration">Nothing</Mojikumi> <KinsokuSet type="enumeration">Nothing</KinsokuSet> <RubyFont type="string">$ID/</RubyFont> <RubyFontStyle type="enumeration">Nothing</RubyFontStyle> <RubyFill type="string">Text Color</RubyFill> <RubyStroke type="string">Text Color</RubyStroke> <KentenFont type="string">$ID/</KentenFont> <KentenFontStyle type="enumeration">Nothing</KentenFontStyle> <KentenFillColor type="string">Text Color</KentenFillColor> <KentenStrokeColor type="string">Text Color</KentenStrokeColor> <BulletChar BulletCharacterType="UnicodeOnly" BulletCharacterValue="8226" /> <NumberingFormat type="string">1, 2, 3, 4...</NumberingFormat> <BulletsFont type="string">$ID/</BulletsFont> <BulletsFontStyle type="enumeration">Nothing</BulletsFontStyle> <AppliedNumberingList type="object">NumberingList/$ID/[Default]</AppliedNumberingList> <NumberingRestartPolicies RestartPolicy="AnyPreviousLevel" LowerLevel="0" UpperLevel="0" /> <BulletsCharacterStyle type="object">CharacterStyle/$ID/[No character style]</BulletsCharacterStyle> <NumberingCharacterStyle type="object">CharacterStyle/$ID/[No character style]</NumberingCharacterStyle> <SameParaStyleSpacing type="enumeration">SetIgnore</SameParaStyleSpacing> </Properties> </ParagraphStyle>
		<ParagraphStyle Self="ParagraphStyle/$ID/NormalParagraphStyle" Name="$ID/NormalParagraphStyle" Imported="false" NextStyle="ParagraphStyle/$ID/NormalParagraphStyle" SplitDocument="false" EmitCss="true" StyleUniqueId="771888e2-944e-42ec-89d5-874cd945e62b" IncludeClass="true" ExtendedKeyboardShortcut="0 0 0" EmptyNestedStyles="true" EmptyLineStyles="true" EmptyGrepStyles="true" KeyboardShortcut="0 0">
			<Properties>
				<BasedOn type="string">$ID/[No paragraph style]</BasedOn>
				<AppliedFont type="string">Arial</AppliedFont>
				<PreviewColor type="enumeration">Nothing</PreviewColor>
			</Properties>
		</ParagraphStyle>
		<ParagraphStyleGroup Self="ParagraphStyleGroup/$ID/Generated" Name="$ID/Generated"> {parastyles} </ParagraphStyleGroup>
		</RootParagraphStyleGroup>
		<RootCellStyleGroup Self="u8e">
		<CellStyle Self="CellStyle/$ID/[None]" AppliedParagraphStyle="ParagraphStyle/$ID/[No paragraph style]" Name="$ID/[None]" />
		<CellStyleGroup Self="CellStyleGroup/$ID/Generated" Name="$ID/Generated">
			<CellStyle Self="CellStyle/Generated%3aHeader 1" ExtendedKeyboardShortcut="0 0 0" KeyboardShortcut="0 0" Name="Generated:Header 1"  TextTopInset="4" TextLeftInset="4" TextBottomInset="4" TextRightInset="4" TopInset="4" LeftInset="4" BottomInset="4" RightInset="4"></CellStyle>
			<CellStyle Self="CellStyle/Generated%3aNone" ExtendedKeyboardShortcut="0 0 0" KeyboardShortcut="0 0" Name="Generated:None" TextTopInset="4" TextLeftInset="4" TextBottomInset="4" TextRightInset="4" TopInset="4" LeftInset="4" BottomInset="4" RightInset="4"></CellStyle>
			<CellStyle Self="CellStyle/Generated%3aOption0" ExtendedKeyboardShortcut="0 0 0" KeyboardShortcut="0 0" Name="Generated:Option0" TextTopInset="4" TextLeftInset="4" TextBottomInset="4" TextRightInset="4" TopInset="4" LeftInset="4" BottomInset="4" RightInset="4"></CellStyle>
			<CellStyle Self="CellStyle/Generated%3aOption1" ExtendedKeyboardShortcut="0 0 0" KeyboardShortcut="0 0" Name="Generated:Option1" TextTopInset="4" TextLeftInset="4" TextBottomInset="4" TextRightInset="4" TopInset="4" LeftInset="4" BottomInset="4" RightInset="4"></CellStyle>
			<CellStyle Self="CellStyle/Generated%3aOption2" ExtendedKeyboardShortcut="0 0 0" KeyboardShortcut="0 0" Name="Generated:Option2" TextTopInset="4" TextLeftInset="4" TextBottomInset="4" TextRightInset="4" TopInset="4" LeftInset="4" BottomInset="4" RightInset="4"></CellStyle>
		</CellStyleGroup>
		</RootCellStyleGroup>
		<RootTableStyleGroup Self="u90">
		<TableStyleGroup Self="TableStyleGroup/$ID/Generated" Name="$ID/Generated">
			<TableStyle Self="TableStyle/Generated%3aBasic Table" ExtendedKeyboardShortcut="0 0 0" Name="Generated:Basic Table" KeyboardShortcut="0 0"> <Properties> <BasedOn type="string">$ID/[No table style]</BasedOn> </Properties> </TableStyle>
		</TableStyleGroup>
		</RootTableStyleGroup>
		<RootObjectStyleGroup Self="u99"> <ObjectStyle Self="ObjectStyle/$ID/[None]" EmitCss="true" IncludeClass="true" Name="$ID/[None]" AppliedParagraphStyle="ParagraphStyle/$ID/[No paragraph style]" CornerRadius="12" FillColor="Swatch/None" FillTint="-1" StrokeWeight="0" MiterLimit="4" EndCap="ButtEndCap" EndJoin="MiterEndJoin" StrokeType="StrokeStyle/$ID/Solid" LeftLineEnd="None" RightLineEnd="None" StrokeColor="Swatch/None" StrokeTint="-1" GapColor="Swatch/None" GapTint="-1" StrokeAlignment="CenterAlignment" Nonprinting="false" GradientFillAngle="0" GradientStrokeAngle="0" AppliedNamedGrid="n" TopLeftCornerOption="None" TopRightCornerOption="None" BottomLeftCornerOption="None" BottomRightCornerOption="None" TopLeftCornerRadius="12" TopRightCornerRadius="12" BottomLeftCornerRadius="12" BottomRightCornerRadius="12" CornerOption="None" ArrowHeadAlignment="InsidePath" LeftArrowHeadScale="100" RightArrowHeadScale="100"> <TransformAttributeOption TransformAttrLeftReference="PageEdgeReference" TransformAttrTopReference="PageEdgeReference" TransformAttrRefAnchorPoint="TopLeftAnchor" /> <ObjectExportOption EpubType="$ID/" SizeType="DefaultSize" CustomSize="$ID/" PreserveAppearanceFromLayout="PreserveAppearanceDefault" AltTextSourceType="SourceXMLStructure" ActualTextSourceType="SourceXMLStructure" CustomAltText="$ID/" CustomActualText="$ID/" ApplyTagType="TagFromStructure" ImageConversionType="JPEG" ImageExportResolution="Ppi300" GIFOptionsPalette="AdaptivePalette" GIFOptionsInterlaced="true" JPEGOptionsQuality="High" JPEGOptionsFormat="BaselineEncoding" ImageAlignment="AlignLeft" ImageSpaceBefore="0" ImageSpaceAfter="0" UseImagePageBreak="false" ImagePageBreak="PageBreakBefore" CustomImageAlignment="false" SpaceUnit="CssPixel" CustomLayout="false" CustomLayoutType="AlignmentAndSpacing"> <Properties> <AltMetadataProperty NamespacePrefix="$ID/" PropertyPath="$ID/" /> <ActualMetadataProperty NamespacePrefix="$ID/" PropertyPath="$ID/" /> </Properties> </ObjectExportOption> <TextFramePreference TextColumnCount="1" TextColumnGutter="12" TextColumnFixedWidth="144" UseFixedColumnWidth="false" FirstBaselineOffset="AscentOffset" MinimumFirstBaselineOffset="0" VerticalJustification="TopAlign" VerticalThreshold="0" IgnoreWrap="false" UseFlexibleColumnWidth="false" TextColumnMaxWidth="0" AutoSizingType="Off" AutoSizingReferencePoint="CenterPoint" UseMinimumHeightForAutoSizing="false" MinimumHeightForAutoSizing="0" UseMinimumWidthForAutoSizing="false" MinimumWidthForAutoSizing="0" UseNoLineBreaksForAutoSizing="false" ColumnRuleOverride="false" ColumnRuleOffset="0" ColumnRuleTopInset="0" ColumnRuleInsetChainOverride="true" ColumnRuleBottomInset="0" ColumnRuleStrokeWidth="1" ColumnRuleStrokeColor="Color/Black" ColumnRuleStrokeType="StrokeStyle/$ID/Solid" ColumnRuleStrokeTint="100" ColumnRuleOverprintOverride="false" FootnotesEnableOverrides="false" FootnotesSpanAcrossColumns="false" FootnotesMinimumSpacing="12" FootnotesSpaceBetween="6" VerticalBalanceColumns="false"> <Properties> <InsetSpacing type="list"> <ListItem type="unit">0</ListItem> <ListItem type="unit">0</ListItem> <ListItem type="unit">0</ListItem> <ListItem type="unit">0</ListItem> </InsetSpacing> </Properties> </TextFramePreference> <BaselineFrameGridOption UseCustomBaselineFrameGrid="false" StartingOffsetForBaselineFrameGrid="0" BaselineFrameGridRelativeOption="TopOfInset" BaselineFrameGridIncrement="12"> <Properties> <BaselineFrameGridColor type="enumeration">LightBlue</BaselineFrameGridColor> </Properties> </BaselineFrameGridOption> <AnchoredObjectSetting AnchoredPosition="InlinePosition" SpineRelative="false" LockPosition="false" PinPosition="true" AnchorPoint="BottomRightAnchor" HorizontalAlignment="LeftAlign" HorizontalReferencePoint="TextFrame" VerticalAlignment="BottomAlign" VerticalReferencePoint="LineBaseline" AnchorXoffset="0" AnchorYoffset="0" AnchorSpaceAbove="0" /> <TextWrapPreference Inverse="false" ApplyToMasterPageOnly="false" TextWrapSide="BothSides" TextWrapMode="None"> <Properties> <TextWrapOffset Top="0" Left="0" Bottom="0" Right="0" /> </Properties> <ContourOption ContourType="SameAsClipping" IncludeInsideEdges="false" ContourPathName="$ID/" /> </TextWrapPreference> <StoryPreference OpticalMarginAlignment="false" OpticalMarginSize="12" FrameType="TextFrameType" StoryOrientation="Horizontal" StoryDirection="LeftToRightDirection" /> <FrameFittingOption AutoFit="false" LeftCrop="0" TopCrop="0" RightCrop="0" BottomCrop="0" FittingOnEmptyFrame="None" FittingAlignment="CenterAnchor" /> <TextFrameFootnoteOptionsObject EnableOverrides="false" SpanFootnotesAcross="false" MinimumSpacingOption="12" SpaceBetweenFootnotes="6" /> </ObjectStyle> </RootObjectStyleGroup>
		<Story Self="u34f" AppliedTOCStyle="n" UserText="true" IsEndnoteStory="false" TrackChanges="false" StoryTitle="Masterplan Areas per Typology-Precinct and Lot Lables" AppliedNamedGrid="n">
		<StoryPreference OpticalMarginAlignment="false" OpticalMarginSize="12" FrameType="TextFrameType" StoryOrientation="Horizontal" StoryDirection="LeftToRightDirection" />
		<MetadataPacketPreference>
		<Properties></Properties>
		</MetadataPacketPreference>
		<InCopyExportOption IncludeGraphicProxies="true" IncludeAllResources="false" />
		<ParagraphStyleRange AppliedParagraphStyle="ParagraphStyle/$ID/NormalParagraphStyle">
		<CharacterStyleRange AppliedCharacterStyle="CharacterStyle/$ID/[No character style]">
		{tableStartString}
		<!-- DEFINE ROWS -->
		{rows}
		<!-- DEFINE COLUMNS -->
		{columns}
		<!-- DEFINE CELLS -->
		{cells}
		<!-- END TABLE -->
		</Table>
		</CharacterStyleRange>
		</ParagraphStyleRange>
		</Story>
		<ColorGroup Self="ColorGroup/[Root Color Group]" Name="[Root Color Group]" IsRootColorGroup="true"> <ColorGroupSwatch Self="u18ColorGroupSwatch0" SwatchItemRef="Swatch/None" /></ColorGroup>
		</Document>"""
		#string = """<ASCII-WIN>\r\n<Version:17.3><FeatureSet:InDesign-Roman><ColorTable:=<Black:COLOR:RGB:Process:0,0,0><Paper:COLOR:CMYK:Process:0,0,0,0><C\\=75 M\\=5 Y\\=100 K\\=0:COLOR:CMYK:Process:0.75,0.05,1,0>><ParaStyle:NormalParagraphStyle><TableStyle:Generated\\\\\\: White Fill>%s%s%s<TableEnd:>"""%(self.tableStartString, self.colsString, self.rowsString)
		context = {"tableStartString":self.tableStartString, "cells":self.cellsString, "columns":self.colsString, "rows":self.rowsString, "parastyles":parastyles,"charastyles":charastyles, "colours":colours}
		string = fullDocString.format(**context)
		return string
	def __str__(self):
		return self.generateStr()

