import sys
sys.path.append("..") # Adds higher directory to python modules path.
import pyTableMaker as tm
from pyCSSParser import parseCSS
import openpyxl


wb = openpyxl.load_workbook(r"C:\Users\Andrew.Butler\Cox Architecture Pty Ltd\223007.00 - Qiddiya (Neybor) [+dBA] - Documents\Design Technology (BIM)\Specifications Schedules\Finishes\Finishes Schedule - Data.xlsx", data_only=True)
ws = wb.active


dataOut = []
headers = []
for row_index, row in enumerate(ws.iter_rows()):
	apDict = {}
	for cell_index, cell in enumerate(row):
		if row_index == 0:
			headers.append(cell.value)
			continue
		try:	apDict[headers[cell_index]] = cell.value
		except:	apDict[headers[cell_index]] = cell.value
	if row_index != 0:
		dataOut.append(apDict)
# print(dataOut)

dataTable = [[{"value":""}, {"value":"Precinct Area"}, {"value": "eGaming", "fill": "#B2DFDB"}, {"value": "Entertainment", "fill": "#B39DDB"}, {"value": "Hotel", "fill": "#FFCC80"}, {"value": "HQ", "fill": "#4CAF50"}, {"value": "Office", "fill": "#81C784"}, {"value": "Parking", "fill": "#90A4AE"}, {"value": "Plaza", "fill": "#F48FB1"}, {"value": "ProTeam", "fill": "#8BDA79"}, {"value": "Residential", "fill": "#FFF59D"}, {"value": "Retail", "fill": "#EF9A9A"}, {"value": "Services", "fill": "#BDBDBD"}, {"value": "Sports", "fill": "#E6EE9C"}, {"value": "Transport", "fill": "#9FA8DA"}, "GFA"], ["Alternate History\nCC-005", 65289, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 47818, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 6745, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 54563, "hAlignment": "Right"}], ["High Fantasy\nCC-006", 74924, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 17745, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 43027, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 1345, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 9947, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 72064, "hAlignment": "Right"}], ["Cyberpunk\nCC-010", 72850, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 30713, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 28742, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 50358, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 70934, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 5328, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 186075, "hAlignment": "Right"}], [{"displayTemplate": "{:,}", "value": "Total", "hAlignment": "Right"}, 213063, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 48458, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 71769, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 50358, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 1345, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 47818, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 70934, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 22020, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 312702, "hAlignment": "Right"}], [{"value": "Brief", "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 92422, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 101815, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 49116, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 93000, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 47501, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 54222, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 71317, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 27000, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, " "], [{"value": "Difference", "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": -92422, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": -101815, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": -658, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": -21231, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 2857, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 1345, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": -6404, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": -383, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": -4980, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"displayTemplate": "{:,}", "value": 0, "hAlignment": "Right"}, {"value": 0, "hAlignment": "Right"}, {"value":" "}]]

# for row_index, row in enumerate(dataOut):
# 	# if row_index > 7:
# 		# break
# 	rowData = [{"value":row["Code"]},{"value":row["Description"]},
# 	{"value":row["Description"]},
# 	{"value":row["Description"]},
# 	{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/ojo.png"},{"value":row["Comments"]},{"value":row["Revision"]}]
# 	# print(rowData)
# 	dataTable.append(rowData)


css = '''
	*{weight:0.5;}
	!{removeTableBorder}

	border[yPos=1]{weight:1;}
	border[yPosRev=-4]{weight:1;}
	border[yPosRev=-2]{weight:1;}
	border[xPos=1]{weight:1;}
	border[xPos=2]{weight:1;}
	border[xPosRev=-2]{weight:1;}
	[value=0]{characterStyle: Hello; fill:#FF0000; borderRight:2; displayTemplate:HELLO;}

	'''

css = parseCSS(css)
#print(css)

table = tm.table(dataTable, {"css":css, "columnWidths": [20,15,15,20,15],"columnWidthsRev": [], "variables": {}})


with open(r"testing.icml", "wb") as file:
	file.write(str(table).encode("utf-8"))