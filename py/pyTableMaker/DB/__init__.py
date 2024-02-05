""" Converts data to icml format for inDesign"""

__author__ = "Andrew"
__version__ = "2022.08.05"

# from System import Array
import json
import re
#print(L)

rgb_scale = 255
cmyk_scale = 100
mmToInd = 2.8346456692913387

def hexToRGB(hex):
    hex = hex.replace("#","")
    r, g, b = hex[0]+hex[1], hex[2]+hex[3], hex[4]+hex[5]

    r = int(r, 16)
    g = int(g, 16)
    b = int(b, 16)
    return r, g, b

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
            ob = self.__dict__
            search = eval(search["eval"])
            return search
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
            bord = self.__dict__
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
        self.cssChanges = self.applyCSS(table.options["css"])
    def generateStr(self):
        #print(self.xPos)
        context = {"columnWidth": 5, "xPos": self.xPos}
        if "columnWidths" in self.table.options:
            index = min(self.xPos,len(self.table.options["columnWidths"])-1)
            context["columnWidth"] = self.table.options["columnWidths"][index]*mmToInd
        context.update(self.cssChanges)
        #string ="<ColStart:<tColAttrWidth:{columnWidth}>>".format(**context)
        for k,v in context.items():
            if v == None:
                context[k] = ""
        string = '<Column Self="u570i5a61Column{xPos}" Name="{xPos}" TextTopInset="4" TextLeftInset="4" TextBottomInset="4" TextRightInset="4" ClipContentToTextCell="false" SingleColumnWidth="{columnWidth}" />'.format(**context)
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
        if yPos == table.tableWidth-1:
            self.yPosName = "last"
        if yPos == 0:
            self.yPosName = "first"
        self.cssChanges = self.applyCSS(table.options["css"])
    def generateStr(self):
        #print(self.xPos)
        context = {"columnWidth": 5, "yPos": self.yPos}
        if "columnWidths" in self.table.options:
            index = min(self.yPos,len(self.table.options["columnWidths"])-1)
            context["columnWidth"] = self.table.options["columnWidths"][index]*mmToInd
        context.update(self.cssChanges)
        #string ="<ColStart:<tColAttrWidth:{columnWidth}>>".format(**context)
        string = '<Row Self="u34fi365Row{yPos}" Name="{yPos}" TextTopInset="4" TextLeftInset="4" TextBottomInset="4" TextRightInset="4" ClipContentToTextCell="false" SingleRowHeight="14.173228346456694" MinimumHeight="14.173228346456694" />'.format(**context)
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
        "content":"", "paragraphStyle": "$ID/NormalParagraphStyle", "cellStyle":"[None]",
        "hAlignment": None,
        "fill": None, "cellTint":None,
        "fillString":"", "cellTintString":"" ,
        "characterColour":None, "characterColourString":"", "characterColourEscape":"",
        "borderLeft":0, "borderRight":0, "borderTop":0, "borderBottom":0,
        "borderString":"",
        "hMerge":1,
        "vMerge":1,
        "displayTemplate":"{}",
        }

        try:    baseContext["borderTop"] = list(filter(lambda i: i["yPos"]==self.yPos and i["xPos"]==self.xPos+0.5, self.parentTable.hBorders))[0].cssChanges["weight"]
        except: pass
        try:    baseContext["borderBottom"] = list(filter(lambda i: i["yPos"]==self.yPos+1 and i["xPos"]==self.xPos+0.5, self.parentTable.hBorders))[0].cssChanges["weight"]
        except: pass
        try:    baseContext["borderLeft"] = list(filter(lambda i: i["yPos"]==self.yPos+0.5 and i["xPos"]==self.xPos+0, self.parentTable.vBorders))[0].cssChanges["weight"]
        except: pass
        try:    baseContext["borderRight"] = list(filter(lambda i: i["yPos"]==self.yPos+0.5 and i["xPos"]==self.xPos+1, self.parentTable.vBorders))[0].cssChanges["weight"]
        except: pass
        context = baseContext.copy()
        context["content"] = context["displayTemplate"].format(self.value)
        
        #print("baseContext",baseContext)
        context.update(self.cssChanges)
        # if context["content"] == "D0043":
        #     print("-"*100)
        #     print(context)
        #     print("-"*100)
        context["paragraphStyle"] = context["paragraphStyle"].replace(":","%3a")
        #print(context["displayTemplate"])
        #print(self.value)
        try:    context["content"] = context["displayTemplate"].format(self.value)
        except: context["content"] = self.value
        #if self.value == "Total":
            #print(context)

        if context["hAlignment"]:
            context["hAlignment"] = 'Justification="{hAlignment}"'.format(**context)
        if context["fill"]:
            cC = context["fill"]
            if type(cC) == type(""):
                if cC.startswith("#"):
                    cC = hexToRGB(cC)
            cC = list(map(lambda x: x/255, cC))
            #print(cC)
            #<tCellFillColor:COLOR\:RGB\:Process\:0.8901960784313725\,0.6431372549019608\,0.8509803921568627>
            #text = "<tCellFillColor:R\\={0} G\\={1} B\\={2}>".format(*context["fill"])
            #<tCellAttrLeftStrokeWeight:2><tCellAttrTopStrokeWeight:2>
            text = "<tCellFillColor:COLOR\\:RGB\\:Process\\:{0}\\,{1}\\,{2}>".format(*cC)
            context["fillString"] = text
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
        template = """<Cell Self="u34fi365i0" Name="{xPos}:{yPos}" RowSpan="{vMerge}" ColumnSpan="{hMerge}" CellType="TextTypeCell"
        TextTopInset="4" TextLeftInset="4" TextBottomInset="4" TextRightInset="4" ClipContentToTextCell="false"
        AppliedCellStyle="{cellStyle}" AppliedCellStylePriority="2"
        LeftEdgeStrokeWeight="{borderLeft}" RightEdgeStrokeWeight="{borderRight}" TopEdgeStrokeWeight="{borderTop}" BottomEdgeStrokeWeight="{borderBottom}" LeftEdgeStrokePriority="1" RightEdgeStrokePriority="1" TopEdgeStrokePriority="1" BottomEdgeStrokePriority="1">
        <ParagraphStyleRange AppliedParagraphStyle="ParagraphStyle/{paragraphStyle}" {hAlignment}>
        <CharacterStyleRange AppliedCharacterStyle="CharacterStyle/$ID/[No character style]">
        <Content>{content}</Content>
        </CharacterStyleRange>
        </ParagraphStyleRange>
        </Cell>"""
        string = template.format(**context)
        for r in range(context["hMerge"]-1):
            modContext = context.copy()
            modContext.update(context)
            modContext["borderString"] = context["borderString"]
            string += template.format(**modContext)
        #print(string)
        #if self.mergeHorizontal > 1:
        #for r in range(self.hMerge-1):
            #string = string+"<CellStyle:[None]><StylePriority:0><CellStart:1,1><CellEnd:>"
        #print(self.yPosRev,string)
        return string
    def __str__(self):
        return self.generateStr()

class table:
    def __init__(self, data=[], options={"css":[]}):
        #print("-"*10)
        self.options = options = deepFormat(options, options["variables"])
        try:    self.data = quickRemoveRows(data,[options["removeClass"]])
        except: self.data = quickRemoveRows(data)
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
        return '<Table Self="u34fi365" HeaderRowCount="0" FooterRowCount="0" TextTopInset="4" TextLeftInset="4" TextBottomInset="4" TextRightInset="4" ClipContentToTextCell="false" BodyRowCount="{tableHeight}" ColumnCount="{tableWidth}" AppliedTableStyle="TableStyle/GH\%3a White Fill" TableDirection="LeftToRightDirection">'.format(tableHeight=self.tableHeight, tableWidth=self.tableWidth)
    def generateStr(self):
        #"<ASCII-WIN>\r\n<Version:17.3><FeatureSet:InDesign-Roman><ColorTable:=<Black:COLOR:RGB:Process:0,0,0><COX Black RGB:COLOR:RGB:Process:0,0,0><COX Dark Grey RGB:COLOR:RGB:Process:0.2549019607843137,0.25098039215686274,0.25882352941176473><Paper:COLOR:CMYK:Process:0,0,0,0><C\=75 M\=5 Y\=100 K\=0:COLOR:CMYK:Process:0.75,0.05,1,0>>\r\n<ParaStyle:NormalParagraphStyle><TableStyle:Table C\\\\\\: White Fill>%s%s%s<TableEnd:>"%(self.tableStartString, self.colsString, self.rowsString)
        #string = "<ASCII-WIN>\r\n<Version:16.4><FeatureSet:InDesign-Roman><ColorTable:=<Black:COLOR:CMYK:Process:0,0,0,1><Paper:COLOR:CMYK:Process:0,0,0,0>>\r\n<ParaStyle:NormalParagraphStyle><TableStyle:Table C\\\\\\: White Fill>%s%s%s<TableEnd:>"%(self.tableStartString, self.colsString, self.rowsString)
        parastyles = ['<DefineParaStyle:NormalParagraphStyle=<Nextstyle:NormalParagraphStyle><cFont:Minion Pro>>',
                        '<DefineParaStyle:GH\:Table\\\: H2=<Nextstyle:GH\:Table\\\: H2><cColor:COX Black RGB><cTypeface:Bold><cSize:9.000000><cAutoPairKern:Optical><pHyphenation:0><pTabRuler:28.34645669291339\,Left\,.\,0\,\;><cFont:Objektiv Mk1>>',
                        '<DefineParaStyle:GH\:Table=<Nextstyle:GH\:Table><cColor:COX Black RGB><cSize:9.000000><cLeading:20.800000><cLanguage:English\: UK><pHyphenation:0><pSpaceBefore:20.800000><pShadingTint:-1.000000><pBorderGapColor:None><pBorderCornerTLRadius:1.000000><pBorderCornerTRRadius:1.000000><pBorderCornerBLRadius:1.000000><pBorderCornerBRRadius:1.000000><pShadeCornerTLRadius:1.000000><pShadeCornerTRRadius:1.000000><pShadeCornerBLRadius:1.000000><pShadeCornerBRRadius:1.000000>>',
                        '<DefineParaStyle:GH\:Table - MT=<Nextstyle:GH\:Table - MT><cColor:COX Black RGB><cSize:9.000000><cTracking:-100><cLeading:20.800000><cLanguage:English\: UK><pHyphenation:0><pSpaceBefore:20.800000><cFont:Mark Pro MT><pShadingTint:-1.000000><pBorderGapColor:None><pBorderCornerTLRadius:1.000000><pBorderCornerTRRadius:1.000000><pBorderCornerBLRadius:1.000000><pBorderCornerBRRadius:1.000000><pShadeCornerTLRadius:1.000000><pShadeCornerTRRadius:1.000000><pShadeCornerBLRadius:1.000000><pShadeCornerBRRadius:1.000000>>',
                        '<DefineParaStyle:GH\:Table - Bold=<Nextstyle:GH\:Table - Bold><cColor:COX Black RGB><cTypeface:Bold><cSize:9.000000><cLeading:20.800000><cLanguage:English\: UK><pHyphenation:0><pSpaceBefore:20.800000><pShadingTint:-1.000000><pBorderGapColor:None><pBorderCornerTLRadius:1.000000><pBorderCornerTRRadius:1.000000><pBorderCornerBLRadius:1.000000><pBorderCornerBRRadius:1.000000><pShadeCornerTLRadius:1.000000><pShadeCornerTRRadius:1.000000><pShadeCornerBLRadius:1.000000><pShadeCornerBRRadius:1.000000>>',
                        '<DefineParaStyle:GH\:Table - MT Bold=<Nextstyle:GH\:Table - MT Bold><cColor:COX Black RGB><cTypeface:Bold><cSize:9.000000><cTracking:-100><cLeading:20.800000><cLanguage:English\: UK><pHyphenation:0><pSpaceBefore:20.800000><cFont:Mark Pro MT><pShadingTint:-1.000000><pBorderGapColor:None><pBorderCornerTLRadius:1.000000><pBorderCornerTRRadius:1.000000><pBorderCornerBLRadius:1.000000><pBorderCornerBRRadius:1.000000><pShadeCornerTLRadius:1.000000><pShadeCornerTRRadius:1.000000><pShadeCornerBLRadius:1.000000><pShadeCornerBRRadius:1.000000>>',
                        '<DefineCellStyle:GH-Import=<tCellAttrLeftInset:5.669291338582678><tCellAttrTopInset:5.669291338582678><tCellAttrRightInset:5.669291338582678><tCellAttrBottomInset:5.669291338582678><tCellStyleParaStyle:NormalParagraphStyle>>',
                        '<DefineTableStyle:GH\\\: White Fill=<tOuterLeftStrokeWeight:0><tCellOuterLeftStrokeColor:COX Dark Grey RGB><tOuterRightStrokeWeight:0><tCellOuterRightStrokeColor:COX Dark Grey RGB><tOuterTopStrokeWeight:0><tCellOuterTopStrokeColor:COX Dark Grey RGB><tOuterBottomStrokeWeight:0><tCellOuterBottomStrokeColor:COX Dark Grey RGB><tBeforeSpace:0><tAfterSpace:0><tHeaderCellStyle:\[None\]><tHeaderUseBodyCellStyle:0>>']
        parastyles = "\n".join(parastyles)

        fullDocString = """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <?aid style="50" type="snippet" readerVersion="6.0" featureSet="257" product="18.4(56)" ?>
        <?aid SnippetType="InCopyInterchange"?>
        <Document DOMVersion="18.0" Self="d">
        <Ink Self="Ink/$ID/Process Cyan" Name="$ID/Process Cyan" Angle="75" ConvertToProcess="false" Frequency="70" NeutralDensity="0.61" PrintInk="true" TrapOrder="1" InkType="Normal" /> <Ink Self="Ink/$ID/Process Magenta" Name="$ID/Process Magenta" Angle="15" ConvertToProcess="false" Frequency="70" NeutralDensity="0.76" PrintInk="true" TrapOrder="2" InkType="Normal" /> <Ink Self="Ink/$ID/Process Yellow" Name="$ID/Process Yellow" Angle="0" ConvertToProcess="false" Frequency="70" NeutralDensity="0.16" PrintInk="true" TrapOrder="3" InkType="Normal" /> <Ink Self="Ink/$ID/Process Black" Name="$ID/Process Black" Angle="45" ConvertToProcess="false" Frequency="70" NeutralDensity="1.7" PrintInk="true" TrapOrder="4" InkType="Normal" /> <Swatch Self="Swatch/None" Name="None" ColorEditable="false" ColorRemovable="false" Visible="true" SwatchCreatorID="7937" SwatchColorGroupReference="u18ColorGroupSwatch0" />
        <StrokeStyle Self="StrokeStyle/$ID/Solid" Name="$ID/Solid" />
        <RootCharacterStyleGroup Self="u7f"> <CharacterStyle Self="CharacterStyle/$ID/[No character style]" Imported="false" SplitDocument="false" EmitCss="true" StyleUniqueId="$ID/" IncludeClass="true" ExtendedKeyboardShortcut="0 0 0" Name="$ID/[No character style]" /> <CharacterStyleGroup Self="CharacterStyleGroup/$ID/Style" Name="$ID/Style"> <CharacterStyle Self="CharacterStyle/Style%3aStyle\%3a Bold" Imported="false" SplitDocument="false" EmitCss="true" StyleUniqueId="6a8c751b-7b81-4452-9747-cff8e210c306" IncludeClass="true" ExtendedKeyboardShortcut="0 0 0" KeyboardShortcut="0 0" Name="Style:Style\: Bold" FontStyle="Bold"> <Properties> <BasedOn type="string">$ID/[No character style]</BasedOn> <PreviewColor type="enumeration">Nothing</PreviewColor> </Properties> </CharacterStyle> </CharacterStyleGroup> </RootCharacterStyleGroup>
        <RootParagraphStyleGroup Self="u7e"> <ParagraphStyle Self="ParagraphStyle/$ID/NormalParagraphStyle" Name="$ID/NormalParagraphStyle" Imported="false" NextStyle="ParagraphStyle/$ID/NormalParagraphStyle" SplitDocument="false" EmitCss="true" StyleUniqueId="771888e2-944e-42ec-89d5-874cd945e62b" IncludeClass="true" ExtendedKeyboardShortcut="0 0 0" EmptyNestedStyles="true" EmptyLineStyles="true" EmptyGrepStyles="true" KeyboardShortcut="0 0"> <Properties> <BasedOn type="string">$ID/[No paragraph style]</BasedOn> <PreviewColor type="enumeration">Nothing</PreviewColor> </Properties> </ParagraphStyle> <ParagraphStyleGroup Self="ParagraphStyleGroup/$ID/GH" Name="$ID/GH"> <ParagraphStyle Self="ParagraphStyle/GH%3aTable - Small" Name="GH:Table - Small" Imported="false" NextStyle="ParagraphStyle/GH%3aTable - Small" SplitDocument="false" EmitCss="true" StyleUniqueId="8ad5365e-a91f-4f93-b49a-3810b988a0b8" IncludeClass="true" ExtendedKeyboardShortcut="0 0 0" EmptyNestedStyles="true" EmptyLineStyles="true" EmptyGrepStyles="true" KeyboardShortcut="0 0" FillColor="Color/COX Black RGB" PointSize="8" AppliedLanguage="$ID/English: UK" Hyphenation="false" SpaceBefore="20.8" ParagraphShadingTint="-1" ParagraphBorderTopLeftCornerRadius="1" ParagraphBorderTopRightCornerRadius="1" ParagraphBorderBottomLeftCornerRadius="1" ParagraphBorderBottomRightCornerRadius="1" ParagraphShadingTopLeftCornerRadius="1" ParagraphShadingTopRightCornerRadius="1" ParagraphShadingBottomLeftCornerRadius="1" ParagraphShadingBottomRightCornerRadius="1"> <Properties> <BasedOn type="string">$ID/[No paragraph style]</BasedOn> <PreviewColor type="enumeration">Nothing</PreviewColor> <Leading type="unit">20.8</Leading> <AppliedFont type="string">Arial</AppliedFont> </Properties> </ParagraphStyle> <ParagraphStyle Self="ParagraphStyle/GH%3aTable - Small - HZ-C" Name="GH:Table - Small - HZ-C" Imported="false" NextStyle="ParagraphStyle/GH%3aTable - Small - HZ-C" SplitDocument="false" EmitCss="true" StyleUniqueId="9d72d003-bc68-4cc9-aa9b-41f1f5139ac9" IncludeClass="true" ExtendedKeyboardShortcut="0 0 0" EmptyNestedStyles="true" EmptyLineStyles="true" EmptyGrepStyles="true" KeyboardShortcut="0 0" Justification="CenterAlign"> <Properties> <BasedOn type="object">ParagraphStyle/GH%3aTable - Small</BasedOn> <PreviewColor type="enumeration">Nothing</PreviewColor> </Properties> </ParagraphStyle> <ParagraphStyle Self="ParagraphStyle/GH%3aTable - Small - HZ-R" Name="GH:Table - Small - HZ-R" Imported="false" NextStyle="ParagraphStyle/GH%3aTable - Small - HZ-R" SplitDocument="false" EmitCss="true" StyleUniqueId="2eefc2f6-775f-4faf-9fdb-0c2fd9cace43" IncludeClass="true" ExtendedKeyboardShortcut="0 0 0" EmptyNestedStyles="true" EmptyLineStyles="true" EmptyGrepStyles="true" KeyboardShortcut="0 0" Justification="RightAlign"> <Properties> <BasedOn type="object">ParagraphStyle/GH%3aTable - Small</BasedOn> <PreviewColor type="enumeration">Nothing</PreviewColor> </Properties> </ParagraphStyle> </ParagraphStyleGroup> <ParagraphStyle Self="ParagraphStyle/$ID/[No paragraph style]" Name="$ID/[No paragraph style]" Imported="false" SplitDocument="false" EmitCss="true" StyleUniqueId="$ID/" IncludeClass="true" ExtendedKeyboardShortcut="0 0 0" EmptyNestedStyles="true" EmptyLineStyles="true" EmptyGrepStyles="true" FillColor="Color/Black" FontStyle="Regular" PointSize="12" HorizontalScale="100" KerningMethod="$ID/Metrics" Ligatures="true" PageNumberType="AutoPageNumber" StrokeWeight="1" Tracking="0" Composer="HL Composer" DropCapCharacters="0" DropCapLines="0" BaselineShift="0" Capitalization="Normal" StrokeColor="Swatch/None" HyphenateLadderLimit="3" VerticalScale="100" LeftIndent="0" RightIndent="0" FirstLineIndent="0" AutoLeading="120" AppliedLanguage="$ID/English: USA" Hyphenation="true" HyphenateAfterFirst="2" HyphenateBeforeLast="2" HyphenateCapitalizedWords="true" HyphenateWordsLongerThan="5" NoBreak="false" HyphenationZone="36" SpaceBefore="0" SpaceAfter="0" Underline="false" OTFFigureStyle="Default" DesiredWordSpacing="100" MaximumWordSpacing="133" MinimumWordSpacing="80" DesiredLetterSpacing="0" MaximumLetterSpacing="0" MinimumLetterSpacing="0" DesiredGlyphScaling="100" MaximumGlyphScaling="100" MinimumGlyphScaling="100" StartParagraph="Anywhere" KeepAllLinesTogether="false" KeepWithNext="0" KeepFirstLines="2" KeepLastLines="2" Position="Normal" StrikeThru="false" CharacterAlignment="AlignEmCenter" KeepLinesTogether="false" StrokeTint="-1" FillTint="-1" OverprintStroke="false" OverprintFill="false" GradientStrokeAngle="0" GradientFillAngle="0" GradientStrokeLength="-1" GradientFillLength="-1" GradientStrokeStart="0 0" GradientFillStart="0 0" Skew="0" RuleAboveLineWeight="1" RuleAboveTint="-1" RuleAboveOffset="0" RuleAboveLeftIndent="0" RuleAboveRightIndent="0" RuleAboveWidth="ColumnWidth" RuleBelowLineWeight="1" RuleBelowTint="-1" RuleBelowOffset="0" RuleBelowLeftIndent="0" RuleBelowRightIndent="0" RuleBelowWidth="ColumnWidth" RuleAboveOverprint="false" RuleBelowOverprint="false" RuleAbove="false" RuleBelow="false" LastLineIndent="0" HyphenateLastWord="true" ParagraphBreakType="Anywhere" SingleWordJustification="FullyJustified" OTFOrdinal="false" OTFFraction="false" OTFDiscretionaryLigature="false" OTFTitling="false" RuleAboveGapTint="-1" RuleAboveGapOverprint="false" RuleBelowGapTint="-1" RuleBelowGapOverprint="false" Justification="LeftAlign" DropcapDetail="1" PositionalForm="None" OTFMark="true" HyphenWeight="5" OTFLocale="true" HyphenateAcrossColumns="true" KeepRuleAboveInFrame="false" IgnoreEdgeAlignment="false" OTFSlashedZero="false" OTFStylisticSets="0" OTFHistorical="false" OTFContextualAlternate="true" UnderlineGapOverprint="false" UnderlineGapTint="-1" UnderlineOffset="-9999" UnderlineOverprint="false" UnderlineTint="-1" UnderlineWeight="-9999" StrikeThroughGapOverprint="false" StrikeThroughGapTint="-1" StrikeThroughOffset="-9999" StrikeThroughOverprint="false" StrikeThroughTint="-1" StrikeThroughWeight="-9999" MiterLimit="4" StrokeAlignment="OutsideAlignment" EndJoin="MiterEndJoin" SpanColumnType="SingleColumn" SplitColumnInsideGutter="6" SplitColumnOutsideGutter="0" KeepWithPrevious="false" SpanColumnMinSpaceBefore="0" SpanColumnMinSpaceAfter="0" OTFSwash="false" ParagraphShadingTint="20" ParagraphShadingOverprint="false" ParagraphShadingWidth="ColumnWidth" ParagraphShadingOn="false" ParagraphShadingClipToFrame="false" ParagraphShadingSuppressPrinting="false" ParagraphShadingLeftOffset="0" ParagraphShadingRightOffset="0" ParagraphShadingTopOffset="0" ParagraphShadingBottomOffset="0" ParagraphShadingTopOrigin="AscentTopOrigin" ParagraphShadingBottomOrigin="DescentBottomOrigin" ParagraphBorderTint="-1" ParagraphBorderOverprint="false" ParagraphBorderOn="false" ParagraphBorderGapTint="-1" ParagraphBorderGapOverprint="false" Tsume="0" LeadingAki="-1" TrailingAki="-1" KinsokuType="KinsokuPushInFirst" KinsokuHangType="None" BunriKinshi="true" RubyOpenTypePro="true" RubyFontSize="-1" RubyAlignment="RubyJIS" RubyType="PerCharacterRuby" RubyParentSpacing="RubyParent121Aki" RubyXScale="100" RubyYScale="100" RubyXOffset="0" RubyYOffset="0" RubyPosition="AboveRight" RubyAutoAlign="true" RubyParentOverhangAmount="RubyOverhangOneRuby" RubyOverhang="false" RubyAutoScaling="false" RubyParentScalingPercent="66" RubyTint="-1" RubyOverprintFill="Auto" RubyStrokeTint="-1" RubyOverprintStroke="Auto" RubyWeight="-1" KentenKind="None" KentenFontSize="-1" KentenXScale="100" KentenYScale="100" KentenPlacement="0" KentenAlignment="AlignKentenCenter" KentenPosition="AboveRight" KentenCustomCharacter="" KentenCharacterSet="CharacterInput" KentenTint="-1" KentenOverprintFill="Auto" KentenStrokeTint="-1" KentenOverprintStroke="Auto" KentenWeight="-1" Tatechuyoko="false" TatechuyokoXOffset="0" TatechuyokoYOffset="0" AutoTcy="0" AutoTcyIncludeRoman="false" Jidori="0" GridGyoudori="0" GridAlignFirstLineOnly="false" GridAlignment="None" CharacterRotation="0" RotateSingleByteCharacters="false" Rensuuji="true" ShataiMagnification="0" ShataiDegreeAngle="4500" ShataiAdjustTsume="true" ShataiAdjustRotation="false" Warichu="false" WarichuLines="2" WarichuSize="50" WarichuLineSpacing="0" WarichuAlignment="Auto" WarichuCharsBeforeBreak="2" WarichuCharsAfterBreak="2" OTFHVKana="false" OTFProportionalMetrics="false" OTFRomanItalics="false" LeadingModel="LeadingModelAkiBelow" ScaleAffectsLineHeight="false" ParagraphGyoudori="false" CjkGridTracking="false" GlyphForm="None" RubyAutoTcyDigits="0" RubyAutoTcyIncludeRoman="false" RubyAutoTcyAutoScale="true" TreatIdeographicSpaceAsSpace="true" AllowArbitraryHyphenation="false" BulletsAndNumberingListType="NoList" NumberingStartAt="1" NumberingLevel="1" NumberingContinue="true" NumberingApplyRestartPolicy="true" BulletsAlignment="LeftAlign" NumberingAlignment="LeftAlign" NumberingExpression="^#.^t" BulletsTextAfter="^t" ParagraphBorderLeftOffset="0" ParagraphBorderRightOffset="0" ParagraphBorderTopOffset="0" ParagraphBorderBottomOffset="0" ParagraphBorderStrokeEndJoin="MiterEndJoin" ParagraphBorderTopLeftCornerOption="None" ParagraphBorderTopRightCornerOption="None" ParagraphBorderBottomLeftCornerOption="None" ParagraphBorderBottomRightCornerOption="None" ParagraphBorderTopLeftCornerRadius="12" ParagraphBorderTopRightCornerRadius="12" ParagraphBorderBottomLeftCornerRadius="12" ParagraphBorderBottomRightCornerRadius="12" ParagraphShadingTopLeftCornerOption="None" ParagraphShadingTopRightCornerOption="None" ParagraphShadingBottomLeftCornerOption="None" ParagraphShadingBottomRightCornerOption="None" ParagraphShadingTopLeftCornerRadius="12" ParagraphShadingTopRightCornerRadius="12" ParagraphShadingBottomLeftCornerRadius="12" ParagraphShadingBottomRightCornerRadius="12" ParagraphBorderStrokeEndCap="ButtEndCap" ParagraphBorderWidth="ColumnWidth" ParagraphBorderTopOrigin="AscentTopOrigin" ParagraphBorderBottomOrigin="DescentBottomOrigin" ParagraphBorderTopLineWeight="1" ParagraphBorderBottomLineWeight="1" ParagraphBorderLeftLineWeight="1" ParagraphBorderRightLineWeight="1" ParagraphBorderDisplayIfSplits="false" MergeConsecutiveParaBorders="true" ProviderHyphenationStyle="HyphAll" DigitsType="DefaultDigits" Kashidas="DefaultKashidas" DiacriticPosition="OpentypePositionFromBaseline" CharacterDirection="DefaultDirection" ParagraphDirection="LeftToRightDirection" ParagraphJustification="DefaultJustification" ParagraphKashidaWidth="2" XOffsetDiacritic="0" YOffsetDiacritic="0" OTFOverlapSwash="false" OTFStylisticAlternate="false" OTFJustificationAlternate="false" OTFStretchedAlternate="false" KeyboardDirection="DefaultDirection"> <Properties> <Leading type="enumeration">Auto</Leading> <TabList type="list"> </TabList> <AppliedFont type="string">Minion Pro</AppliedFont> <RuleAboveColor type="string">Text Color</RuleAboveColor> <RuleBelowColor type="string">Text Color</RuleBelowColor> <RuleAboveType type="object">StrokeStyle/$ID/Solid</RuleAboveType> <RuleBelowType type="object">StrokeStyle/$ID/Solid</RuleBelowType> <BalanceRaggedLines type="enumeration">NoBalancing</BalanceRaggedLines> <RuleAboveGapColor type="object">Swatch/None</RuleAboveGapColor> <RuleBelowGapColor type="object">Swatch/None</RuleBelowGapColor> <UnderlineColor type="string">Text Color</UnderlineColor> <UnderlineGapColor type="object">Swatch/None</UnderlineGapColor> <UnderlineType type="object">StrokeStyle/$ID/Solid</UnderlineType> <StrikeThroughColor type="string">Text Color</StrikeThroughColor> <StrikeThroughGapColor type="object">Swatch/None</StrikeThroughGapColor> <StrikeThroughType type="object">StrokeStyle/$ID/Solid</StrikeThroughType> <SpanSplitColumnCount type="enumeration">All</SpanSplitColumnCount> <ParagraphShadingColor type="object">Color/Black</ParagraphShadingColor> <ParagraphBorderColor type="object">Color/Black</ParagraphBorderColor> <ParagraphBorderGapColor type="object">Swatch/None</ParagraphBorderGapColor> <ParagraphBorderType type="object">StrokeStyle/$ID/Solid</ParagraphBorderType> <Mojikumi type="enumeration">Nothing</Mojikumi> <KinsokuSet type="enumeration">Nothing</KinsokuSet> <RubyFont type="string">$ID/</RubyFont> <RubyFontStyle type="enumeration">Nothing</RubyFontStyle> <RubyFill type="string">Text Color</RubyFill> <RubyStroke type="string">Text Color</RubyStroke> <KentenFont type="string">$ID/</KentenFont> <KentenFontStyle type="enumeration">Nothing</KentenFontStyle> <KentenFillColor type="string">Text Color</KentenFillColor> <KentenStrokeColor type="string">Text Color</KentenStrokeColor> <BulletChar BulletCharacterType="UnicodeOnly" BulletCharacterValue="8226" /> <NumberingFormat type="string">1, 2, 3, 4...</NumberingFormat> <BulletsFont type="string">$ID/</BulletsFont> <BulletsFontStyle type="enumeration">Nothing</BulletsFontStyle> <AppliedNumberingList type="object">NumberingList/$ID/[Default]</AppliedNumberingList> <NumberingRestartPolicies RestartPolicy="AnyPreviousLevel" LowerLevel="0" UpperLevel="0" /> <BulletsCharacterStyle type="object">CharacterStyle/$ID/[No character style]</BulletsCharacterStyle> <NumberingCharacterStyle type="object">CharacterStyle/$ID/[No character style]</NumberingCharacterStyle> <SameParaStyleSpacing type="enumeration">SetIgnore</SameParaStyleSpacing> </Properties> </ParagraphStyle> </RootParagraphStyleGroup>
        <RootCellStyleGroup Self="u8e">
        <CellStyle Self="CellStyle/GH-Import" ExtendedKeyboardShortcut="0 0 0" TextTopInset="5.669291338582678" TextLeftInset="5.669291338582678" TextBottomInset="5.669291338582678" TextRightInset="5.669291338582678" AppliedParagraphStyle="ParagraphStyle/$ID/NormalParagraphStyle" TopInset="5.669291338582678" LeftInset="5.669291338582678" BottomInset="5.669291338582678" RightInset="5.669291338582678" FillColor="Color/C=100 M=0 Y=0 K=0" KeyboardShortcut="0 0" Name="GH-Import"><Properties><BasedOn type="string">$ID/[None]</BasedOn></Properties></CellStyle>
        <CellStyle Self="CellStyle/$ID/[None]" AppliedParagraphStyle="ParagraphStyle/$ID/[No paragraph style]" Name="$ID/[None]" /> </RootCellStyleGroup>
        <RootTableStyleGroup Self="u90"> <TableStyle Self="TableStyle/GH\%3a White Fill" ExtendedKeyboardShortcut="0 0 0" Name="GH\: White Fill" TopBorderStrokeWeight="0" TopBorderStrokeColor="Color/COX Dark Grey RGB" LeftBorderStrokeWeight="0" LeftBorderStrokeColor="Color/COX Dark Grey RGB" BottomBorderStrokeWeight="0" BottomBorderStrokeColor="Color/COX Dark Grey RGB" RightBorderStrokeWeight="0" RightBorderStrokeColor="Color/COX Dark Grey RGB" SpaceBefore="0" SpaceAfter="0" HeaderRegionSameAsBodyRegion="false" HeaderRegionCellStyle="CellStyle/$ID/[None]" KeyboardShortcut="0 0"> <Properties> <BasedOn type="string">$ID/[No table style]</BasedOn> </Properties> </TableStyle> </RootTableStyleGroup>
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
        #string = """<ASCII-WIN>\r\n<Version:17.3><FeatureSet:InDesign-Roman><ColorTable:=<Black:COLOR:RGB:Process:0,0,0><Paper:COLOR:CMYK:Process:0,0,0,0><C\\=75 M\\=5 Y\\=100 K\\=0:COLOR:CMYK:Process:0.75,0.05,1,0>><ParaStyle:NormalParagraphStyle><TableStyle:GH\\\\\\: White Fill>%s%s%s<TableEnd:>"""%(self.tableStartString, self.colsString, self.rowsString)
        context = {"tableStartString":self.tableStartString, "cells":self.cellsString, "columns":self.colsString, "rows":self.rowsString}
        string = fullDocString.format(**context)
        return string
    def __str__(self):
        return self.generateStr()
