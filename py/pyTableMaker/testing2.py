import sys
sys.path.append("..") # Adds higher directory to python modules path.
import pyTableMaker as tm
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

dataTable = [
[{"value":"Code"},
{"value":"Location"},
{"value":"Product"},
{"value":"Finish"},
{"value":"Image"},
{"value":"Supplier"},
{"value":"Rev"}],
]
for row_index, row in enumerate(dataOut):
	# if row_index > 7:
		# break
	rowData = [{"value":row["Code"]},{"value":row["Description"]},
	{"value":row["Description"]},
	{"value":row["Description"]},
	{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/ojo.png"},{"value":row["Comments"]},{"value":row["Revision"]}]
	# print(rowData)
	dataTable.append(rowData)


css = [
    {"filters": [{"eval": "True", "searchType": "eval"} ], "style": {
    "paragraphStyle": "Generated%3aTable\%3a Body", "cellStyle": "Generated%3aNone",
    
    }},
	# {"filters": [{"eval": "bord", "searchType": "eval"} ], "style": {"weight":"{line-thin}"} },
    {"filters": [{"eval": "ob['yPos'] in [0]", "searchType": "eval"} ], "style": {"hAlignment": "CenterAlign", "paragraphStyle":"Generated%3aTable\%3a H1", "cellStyle": "Generated%3aHeader 1"} },
    {"filters": [{"eval": "ob['xPos'] in [0]", "searchType": "eval"} ], "style": {"hAlignment": "CenterAlign"} },
    {"filters": [{"eval": "ob['xPosRev'] in [-1]", "searchType": "eval"} ], "style": {"hAlignment": "CenterAlign"} },

	{"filters": [{"eval": "bord['xPos'] in [0]", "searchType": "eval"} ], "style": {"weight":0} },
	{"filters": [{"eval": "bord['xPosRev'] in [-1]", "searchType": "eval"} ], "style": {"weight":0} },
	# {"filters": [{"eval": "bord['yPos'] in [0]", "searchType": "eval"} ], "style": {"weight":0} },
	# {"filters": [{"eval": "bord['yPos'] in [1]", "searchType": "eval"} ], "style": {"weight":"{line-thick}"} },
	]



table = tm.table(dataTable, {"css":css, "columnWidths":[21.743,43.75,83.75,83.75,69.375,65.625,21.743],"headerCount":1})


with open(r"C:\Users\Andrew.Butler\Cox Architecture Pty Ltd\223007.00 - Qiddiya (Neybor) [+dBA] - Documents\Design Technology (BIM)\Specifications Schedules\Finishes\testing.icml", "wb") as file:
	file.write(str(table).encode("utf-8"))