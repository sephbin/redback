import sys
sys.path.append("..") # Adds higher directory to python modules path.
import pyTableMaker as tm
from pyCSSParser import parseCSS

data = [[
	{"value": "A A A", "class": "A"},
	{"value": "A A A", "class": "A"},
	{"value": "A A A", "class": "A"},
	{"value": "A A A", "class": "A"}],
[
	{"value": "A A A", "class": "B"},
	{"value": "A_K_A", "class": "B"},
	{"value": "A A A", "class": "B"},
	{"value": "A A A", "class": "B"}
], [
	{"value": "A A A", "class": "C"},
	{"value": "A A A", "class": "C"},
	{"value": "A K A", "class": "C"},
	{"value": "A A A", "class": "C"}
], [
	{"value": "A A A", "class": "D"},
	{"value": "K A A", "class": "D"},
	{"value": "A A A", "class": "D"},
	{"value": "A A A", "class": "D"}
]]

css ='''
*{paragraphStyle:Table - Small; }
[xPos=0]{characterStyle:Test;}
[value|=K]{characterStyle:Test2;}
[value=A_K_A]{characterStyle:Test2;}
.A{characterStyle:Bold;}
'''

options = {"columnWidths": [17,21],"columnWidthsRev": [17], "variables": {"line-thick": 2, "line-medium": 1, "line-thin": 0.5 }, "headerCount":1, "css":parseCSS(css)}
#print(options["css"])
table = tm.table(data, options)

# print(table)


with open("testing.icml", "wb") as file:
	file.write(str(table).encode('utf-8'))