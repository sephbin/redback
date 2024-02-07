import sys
sys.path.append("..") # Adds higher directory to python modules path.
import pyTableMaker as tm
from pyCSSParser import parseCSS

data = [[{"value": "A", "class": "A"}, {"value": "B", "class": "A"}, {"value": "C", "class": "A"}, {"value": "D", "class": "A"}], [{"value": "E", "class": "B"}, {"value": "F", "class": "B"}, {"value": "G", "class": "B"}, {"value": "H", "class": "B"}], [{"value": "I", "class": "C"}, {"value": "J", "class": "C"}, {"value": "K", "class": "C"}, {"value": "L", "class": "C"}], [{"value": "M", "class": "D"}, {"value": "N", "class": "D"}, {"value": "O", "class": "D"}, {"value": "P", "class": "D"}]]

css ='''
*{paragraphStyle:Table - Small; }
.A{characterStyle:Bold;}
'''

options = {"columnWidths": [7,14],"columnWidthsRev": [7], "variables": {"line-thick": 2, "line-medium": 1, "line-thin": 0.5 }, "headerCount":1, "css":parseCSS(css)}
print(options["css"])
table = tm.table(data, options)

# print(table)


with open("testing.icml", "wb") as file:
	file.write(str(table).encode('utf-8'))